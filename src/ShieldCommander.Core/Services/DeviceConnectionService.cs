using ShieldCommander.Core.Models;
using ShieldCommander.Core.Services.Commands;
using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Services;

public sealed class DeviceConnectionService(
    IAdbExecutor executor,
    IAdbRunner runner,
    ISettingsService settings,
    IDeviceDiscoveryService discoveryService,
    IAdbConfigService adbConfig) : IDeviceConnectionService
{
    private const int AdbPort = 5555;
    private const int SettleAttempts = 6;
    private static readonly TimeSpan SettleInterval = TimeSpan.FromMilliseconds(500);
    private static readonly DevicesQuery Devices = new();
    private static readonly KillServerCommand KillServer = new();
    private static readonly DisconnectDeviceCommand DisconnectAll = new();

    public bool IsAdbAvailable() => adbConfig.IsAdbAvailable(null);

    public async Task<ConnectionResult> ConnectAsync(string ipAddress, string? deviceName = null)
    {
        var (device, output) = await ReconnectAsync(ipAddress, allowServerRestart: true, CancellationToken.None);

        return device?.State switch
        {
            AdbDeviceState.Online => OnConnected(ipAddress, deviceName ?? device.DeviceName),
            AdbDeviceState.Unauthorized or AdbDeviceState.Offline => new ConnectionResult(ConnectionStatus.AwaitingAuthorization),
            _ => new ConnectionResult(ConnectionStatus.Failed, Error: output),
        };
    }

    public async Task<ConnectionResult> WaitForAuthorizationAsync(string ipAddress, CancellationToken ct, string? deviceName = null)
    {
        // Restart the adb server at most once per wait, so an unreachable device doesn't restart it every reconnect
        var serverRestartAllowed = true;

        try
        {
            for (var i = 0; i < 60; i++)
            {
                await Task.Delay(2000, ct);

                var device = await FindDeviceAsync(ipAddress, ct);

                // A pending authorization shows as unauthorized; only re-establish a dropped or stale transport
                if ((device?.State is null or AdbDeviceState.Offline) && i % 5 == 4)
                {
                    (device, _) = await ReconnectAsync(ipAddress, serverRestartAllowed, ct);
                    serverRestartAllowed = false;
                }

                if (device?.State == AdbDeviceState.Online)
                {
                    return OnConnected(ipAddress, deviceName ?? device.DeviceName);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // User cancelled
        }

        return new ConnectionResult(ConnectionStatus.Failed);
    }

    private ConnectionResult OnConnected(string ipAddress, string? deviceName)
    {
        runner.OpenSession(ToSerial(ipAddress));
        settings.AddOrUpdateDevice(ipAddress, deviceName);
        return new ConnectionResult(ConnectionStatus.Connected, deviceName);
    }

    private async Task<(ShieldDevice? Device, string Output)> ReconnectAsync(string ipAddress, bool allowServerRestart, CancellationToken ct)
    {
        var (device, output) = await ConnectOnceAsync(ipAddress, ct);

        // macOS grants local network access per app. An adb server started by another process, or left
        // running after Shield Commander exited, gets "No route to host" until it is restarted by this app.
        if (allowServerRestart && device is null && IsLocalNetworkDenied(output))
        {
            await executor.ExecuteAsync(KillServer, ct);
            (device, output) = await ConnectOnceAsync(ipAddress, ct);
        }

        return (device, output);
    }

    private static bool IsLocalNetworkDenied(string adbOutput) =>
        OperatingSystem.IsMacOS() && adbOutput.Contains("No route to host", StringComparison.OrdinalIgnoreCase);

    private async Task<(ShieldDevice? Device, string Output)> ConnectOnceAsync(string ipAddress, CancellationToken ct)
    {
        // adb keeps a stale offline transport after the device sleeps or drops off the network,
        // and "adb connect" then reports "already connected" without reconnecting
        if ((await FindDeviceAsync(ipAddress, ct))?.State == AdbDeviceState.Offline)
        {
            var disconnect = new DisconnectDeviceCommand(ToSerial(ipAddress));
            await executor.ExecuteAsync(disconnect, ct);
        }

        // Exit code and output are unreliable here ("failed to authenticate" still connects),
        // so the device list is the source of truth
        var connect = new ConnectDeviceCommand(ipAddress, AdbPort);
        var result = await executor.ExecuteAsync(connect, ct);

        var device = await FindDeviceAsync(ipAddress, ct);
        for (var i = 1; i < SettleAttempts && device?.State == AdbDeviceState.Offline; i++)
        {
            await Task.Delay(SettleInterval, ct);
            device = await FindDeviceAsync(ipAddress, ct);
        }

        return (device, string.IsNullOrEmpty(result.Output) ? result.Error : result.Output);
    }

    private async Task<ShieldDevice?> FindDeviceAsync(string ipAddress, CancellationToken ct)
    {
        var serial = ToSerial(ipAddress);
        var devices = await executor.QueryAsync(Devices, ct);
        return devices.FirstOrDefault(d => d.IpAddress == serial);
    }

    private static string ToSerial(string ipAddress) => $"{ipAddress}:{AdbPort}";

    public event Action? SessionLost
    {
        add => runner.SessionLost += value;
        remove => runner.SessionLost -= value;
    }

    public void CloseSession() => runner.CloseSession();

    public async Task DisconnectAsync(string ipAddress)
    {
        var disconnect = new DisconnectDeviceCommand(ipAddress);
        await executor.ExecuteAsync(disconnect);
    }

    public async Task DisconnectAllAsync()
    {
        await executor.ExecuteAsync(DisconnectAll);
    }

    public async Task<List<ShieldDevice>> GetConnectedDevicesAsync()
    {
        var devices = await executor.QueryAsync(Devices);
        return devices.Where(d => d.State == AdbDeviceState.Online).ToList();
    }

    public IReadOnlyList<SavedDevice> GetSavedDevices() => settings.SavedDevices;

    public void AddOrUpdateDevice(string ipAddress, string? deviceName) => settings.AddOrUpdateDevice(ipAddress, deviceName);

    public void RemoveDevice(string ipAddress) => settings.RemoveDevice(ipAddress);

    public void SetAutoConnect(string ipAddress, bool autoConnect) => settings.SetAutoConnect(ipAddress, autoConnect);

    public SavedDevice? GetAutoConnectDevice() => settings.SavedDevices.FirstOrDefault(d => d.AutoConnect);

    public List<DeviceSuggestion> GetSavedSuggestions()
    {
        return settings.SavedDevices
            .OrderByDescending(d => d.LastConnected)
            .Select(d => new DeviceSuggestion
            {
                IpAddress = d.IpAddress,
                DisplayName = d.DeviceName,
                Source = "Saved",
            })
            .ToList();
    }

    public async Task<List<DeviceSuggestion>> ScanForSuggestionsAsync(IReadOnlySet<string> existingIps, CancellationToken ct = default)
    {
        try
        {
            var devices = await discoveryService.ScanAsync(cancellationToken: ct);
            return devices
                .Where(d => !existingIps.Contains(d.IpAddress))
                .Select(d => new DeviceSuggestion
                {
                    IpAddress = d.IpAddress,
                    DisplayName = d.DisplayName,
                    Source = "Discovered",
                })
                .ToList();
        }
        catch
        {
            return [];
        }
    }
}
