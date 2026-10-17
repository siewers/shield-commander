using ShieldCommander.Core.Models;
using ShieldCommander.Core.Services.Commands;
using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Services;

public sealed class AdbService : IAdbService
{
    private static readonly AdbBatchQueryCollection<DynamicSections> DynamicCommands = DynamicSections.CreateCommands();

    private readonly AdbRunner _runner;

    public event Action? SessionLost;

    public AdbService(IAdbRunner runner)
    {
        _runner = (AdbRunner)runner;
        _runner.SessionLost += () => SessionLost?.Invoke();
    }

    public void OpenSession(string serial) => _runner.OpenSession(serial);

    public void CloseSession() => _runner.CloseSession();

    // Device operations
    public Task<AdbResult> ConnectAsync(string ipAddress, int port, CancellationToken ct = default) =>
        _runner.ExecuteAsync(new ConnectDeviceCommand(ipAddress, port), ct);

    public Task<AdbResult> DisconnectAsync(string ipAddress, CancellationToken ct = default) =>
        _runner.ExecuteAsync(new DisconnectDeviceCommand(ipAddress), ct);

    public Task<AdbResult> DisconnectAllAsync(CancellationToken ct = default) =>
        _runner.ExecuteAsync(new DisconnectDeviceCommand(), ct);

    public Task<AdbResult> KillServerAsync(CancellationToken ct = default) =>
        _runner.ExecuteAsync(new KillServerCommand(), ct);

    public Task<List<ShieldDevice>> GetDevicesAsync(CancellationToken ct = default) =>
        _runner.ExecuteAsync(new DevicesQuery(), ct);

    // Package operations
    public Task<List<InstalledPackage>> GetInstalledPackagesAsync(CancellationToken ct = default) =>
        _runner.ExecuteAsync(new InstalledPackagesQuery(), ct);

    public Task<InstalledPackage> GetPackageInfoAsync(string packageName, CancellationToken ct = default) =>
        _runner.ExecuteAsync(new PackageInfoQuery(packageName), ct);

    public Task<AdbResult> InstallApkAsync(string apkFilePath, CancellationToken ct = default) =>
        _runner.ExecuteAsync(new InstallPackageCommand(apkFilePath), ct);

    public Task<AdbResult> UninstallPackageAsync(string packageName, CancellationToken ct = default) =>
        _runner.ExecuteAsync(new UninstallPackageCommand(packageName), ct);

    // Process operations
    public Task<ProcessDetails> GetProcessDetailsAsync(int pid, string name, CancellationToken ct = default) =>
        _runner.ExecuteAsync(new ProcessStatusQuery(pid, name), ct);

    public Task<AdbResult> TerminateProcessAsync(int pid, string packageName, CancellationToken ct = default) =>
        _runner.ExecuteAsync(new TerminateProcessCommand(pid, packageName), ct);

    public Task<ProcessSnapshot> GetProcessSnapshotAsync(CancellationToken ct = default) =>
        _runner.ExecuteAsync(new ProcessSnapshotQuery(), ct);

    // Device info
    public Task<DeviceInfo> GetDeviceInfoAsync(CancellationToken ct = default) =>
        _runner.ExecuteAsync(new DeviceInfoQuery(), ct);

    public async Task<SystemSnapshot> GetSystemSnapshotAsync(CancellationToken ct = default)
    {
        var output = await _runner.RunShellAsync(DynamicCommands.ToCombinedCommand(), ct);
        var s = new DynamicSections();
        DynamicCommands.ApplyAll(output, s);
        return new SystemSnapshot(s.Cpu, s.Memory.Snapshot, s.Disk, s.Network, s.Thermal);
    }
}
