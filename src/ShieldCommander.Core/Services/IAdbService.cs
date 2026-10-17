using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services;

public interface IAdbService
{
    event Action? SessionLost;

    void OpenSession(string serial);

    void CloseSession();

    Task<AdbResult> ConnectAsync(string ipAddress, int port, CancellationToken ct = default);

    Task<AdbResult> DisconnectAsync(string ipAddress, CancellationToken ct = default);

    Task<AdbResult> DisconnectAllAsync(CancellationToken ct = default);

    Task<AdbResult> KillServerAsync(CancellationToken ct = default);

    Task<List<ShieldDevice>> GetDevicesAsync(CancellationToken ct = default);

    Task<List<InstalledPackage>> GetInstalledPackagesAsync(CancellationToken ct = default);

    Task<InstalledPackage> GetPackageInfoAsync(string packageName, CancellationToken ct = default);

    Task<AdbResult> InstallApkAsync(string apkFilePath, CancellationToken ct = default);

    Task<AdbResult> UninstallPackageAsync(string packageName, CancellationToken ct = default);

    Task<ProcessDetails> GetProcessDetailsAsync(int pid, string name, CancellationToken ct = default);

    Task<AdbResult> TerminateProcessAsync(int pid, string packageName, CancellationToken ct = default);

    Task<ProcessSnapshot> GetProcessSnapshotAsync(CancellationToken ct = default);

    Task<DeviceInfo> GetDeviceInfoAsync(CancellationToken ct = default);

    Task<SystemSnapshot> GetSystemSnapshotAsync(CancellationToken ct = default);
}
