using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services;

public interface IDeviceConnectionService
{
    // ADB status
    bool IsAdbAvailable();

    // Connection
    Task<ConnectionResult> ConnectAsync(string ipAddress, string? deviceName = null);
    Task<ConnectionResult> WaitForAuthorizationAsync(string ipAddress, CancellationToken ct, string? deviceName = null);
    Task DisconnectAsync(string ipAddress);
    Task DisconnectAllAsync();
    Task<List<ShieldDevice>> GetConnectedDevicesAsync();

    // Session
    event Action? SessionLost;
    void CloseSession();

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
