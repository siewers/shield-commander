using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services;

public interface IDeviceConnectionService
{
    // ADB status
    bool IsAdbAvailable();

    // Connection
    Task<ConnectionResult> ConnectAsync(string ipAddress);
    Task<bool> WaitForAuthorizationAsync(string ipAddress, CancellationToken ct);
    Task DisconnectAsync(string ipAddress);
    Task DisconnectAllAsync();
    Task<List<ShieldDevice>> GetConnectedDevicesAsync();

    // Saved devices
    IReadOnlyList<SavedDevice> GetSavedDevices();
    void AddOrUpdateDevice(string ipAddress, string? deviceName);
    void RemoveDevice(string ipAddress);
    void SetAutoConnect(string ipAddress, bool autoConnect);
    SavedDevice? GetAutoConnectDevice();

    // Suggestions
    List<DeviceSuggestion> GetSavedSuggestions();
    Task<List<DeviceSuggestion>> ScanForSuggestionsAsync(IReadOnlySet<string> existingIps, CancellationToken ct = default);
}
