using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services;

public sealed class DeviceConnectionService(
    IAdbService adbService,
    ISettingsService settings,
    IDeviceDiscoveryService discoveryService,
    IAdbConfigService adbConfig) : IDeviceConnectionService
{
    private const int AdbPort = 5555;
    private const int SettleAttempts = 6;
    private static readonly TimeSpan SettleInterval = TimeSpan.FromMilliseconds(500);

    public bool IsAdbAvailable() => adbConfig.IsAdbAvailable(null);

    public async Task<ConnectionResult> ConnectAsync(string ipAddress, string? deviceName = null)
    {
        var (device, output) = await ReconnectAsync(ipAddress, CancellationToken.None);

        // macOS grants local network access per app. An adb server started by another process, or left
        // running after Shield Commander exited, gets "No route to host" until it is restarted by this app.
        if (device is null && output.Contains("No route to host", StringComparison.OrdinalIgnoreCase))
        {
            await adbService.KillServerAsync();
            (device, output) = await ReconnectAsync(ipAddress, CancellationToken.None);
        }

        return device?.State switch
        {
            AdbDeviceState.Online => OnConnected(ipAddress, deviceName ?? device.DeviceName),
            AdbDeviceState.Unauthorized or AdbDeviceState.Offline => new ConnectionResult(ConnectionStatus.AwaitingAuthorization),
            _ => new ConnectionResult(ConnectionStatus.Failed, Error: output),
        };
    }

    public async Task<ConnectionResult> WaitForAuthorizationAsync(string ipAddress, CancellationToken ct, string? deviceName = null)
    {
        try
        {
            for (var i = 0; i < 60; i++)
            {
                await Task.Delay(2000, ct);

                var device = await FindDeviceAsync(ipAddress);

                // A pending authorization shows as unauthorized; only re-establish a dropped or stale transport
                if ((device?.State is null or AdbDeviceState.Offline) && i % 5 == 4)
                {
                    (device, _) = await ReconnectAsync(ipAddress, ct);
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
        settings.AddOrUpdateDevice(ipAddress, deviceName);
        return new ConnectionResult(ConnectionStatus.Connected, deviceName);
    }

    private async Task<(ShieldDevice? Device, string Output)> ReconnectAsync(string ipAddress, CancellationToken ct)
    {
        // adb keeps a stale offline transport after the device sleeps or drops off the network,
        // and "adb connect" then reports "already connected" without reconnecting
        if ((await FindDeviceAsync(ipAddress))?.State == AdbDeviceState.Offline)
        {
            await adbService.DisconnectAsync(ToSerial(ipAddress));
        }

        // Exit code and output are unreliable here ("failed to authenticate" still connects),
        // so the device list is the source of truth
        var result = await adbService.ConnectAsync(ipAddress, AdbPort);

        var device = await FindDeviceAsync(ipAddress);
        for (var i = 1; i < SettleAttempts && device?.State == AdbDeviceState.Offline; i++)
        {
            await Task.Delay(SettleInterval, ct);
            device = await FindDeviceAsync(ipAddress);
        }

        return (device, string.IsNullOrEmpty(result.Output) ? result.Error : result.Output);
    }

    private async Task<ShieldDevice?> FindDeviceAsync(string ipAddress)
    {
        var serial = ToSerial(ipAddress);
        var devices = await adbService.GetDevicesAsync();
        return devices.FirstOrDefault(d => d.IpAddress == serial);
    }

    private static string ToSerial(string ipAddress) => $"{ipAddress}:{AdbPort}";

    public async Task DisconnectAsync(string ipAddress)
    {
        await adbService.DisconnectAsync(ipAddress);
    }

    public async Task DisconnectAllAsync()
    {
        await adbService.DisconnectAllAsync();
    }

    public async Task<List<ShieldDevice>> GetConnectedDevicesAsync()
    {
        var devices = await adbService.GetDevicesAsync();
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
