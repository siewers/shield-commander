using AdvancedSharpAdbClient;
using AdvancedSharpAdbClient.Models;
using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services;

public interface IAdbRunner
{
    event Action? SessionLost;

    void OpenSession(string serial);

    void CloseSession();

    Task<string> RunShellAsync(string command, CancellationToken ct = default);

    Task<AdbResult> RunServerAsync(Func<IAdbClient, CancellationToken, Task<string>> action, CancellationToken ct = default);

    Task<AdbResult> RunOnDeviceAsync(Func<IAdbClient, DeviceData, CancellationToken, Task<string>> action, CancellationToken ct = default);

    Task<T> QueryServerAsync<T>(Func<IAdbClient, CancellationToken, Task<T>> query, T fallback, CancellationToken ct = default);
}
