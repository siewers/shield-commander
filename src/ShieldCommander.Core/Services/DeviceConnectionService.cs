using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services;

public sealed class DeviceConnectionService(
    IAdbService adbService,
    ISettingsService settings,
    IDeviceDiscoveryService discoveryService,
    IAdbConfigService adbConfig) : IDeviceConnectionService
{
    public bool IsAdbAvailable() => adbConfig.IsAdbAvailable(null);

    public async Task<ConnectionResult> ConnectAsync(string ipAddress)
    {
        await adbService.ConnectAsync(ipAddress);

        var devices = await adbService.GetConnectedDevicesAsync();
        var match = devices.FirstOrDefault(d => d.IpAddress.StartsWith(ipAddress));

        if (match != null)
        {
            settings.AddOrUpdateDevice(ipAddress, match.DeviceName);
            return new ConnectionResult(ConnectionStatus.Connected, match.DeviceName);
        }

        return new ConnectionResult(ConnectionStatus.AwaitingAuthorization);
    }

    public async Task<bool> WaitForAuthorizationAsync(string ipAddress, CancellationToken ct)
    {
        try
        {
            for (var i = 0; i < 60; i++)
            {
                await Task.Delay(2000, ct);

                if (i > 0 && i % 5 == 0)
                {
                    await adbService.ConnectAsync(ipAddress);
                }

                var devices = await adbService.GetConnectedDevicesAsync();
                var match = devices.FirstOrDefault(d => d.IpAddress.StartsWith(ipAddress));

                if (match != null)
                {
                    settings.AddOrUpdateDevice(ipAddress, match.DeviceName);
                    return true;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // User cancelled
        }

        return false;
    }

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
        return await adbService.GetConnectedDevicesAsync();
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
