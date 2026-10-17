using Imposter.Abstractions;
using ShieldCommander.Core.Models;
using ShieldCommander.Core.Services;
using ShieldCommander.Core.Services.Commands;
using ShieldCommander.Core.Services.Queries;
using TUnit.Core.Enums;

[assembly: GenerateImposter(typeof(IAdbExecutor))]
[assembly: GenerateImposter(typeof(ISettingsService))]
[assembly: GenerateImposter(typeof(IDeviceDiscoveryService))]
[assembly: GenerateImposter(typeof(IAdbConfigService))]

namespace ShieldCommander.Core.Tests.Services;

public class DeviceConnectionServiceTests
{
    private const string IpAddress = "10.0.0.150";

    private static readonly List<ShieldDevice> NoDevices = [];

    private static readonly List<ShieldDevice> OnlineDevice =
    [
        new($"{IpAddress}:5555", "SHIELD_Android_TV", AdbDeviceState.Online),
    ];

    [Test]
    [RunOn(OS.MacOs)]
    public async Task Connect_NoRouteToHost_RestartsServerAndRetries()
    {
        var adb = IAdbExecutor.Imposter();
        var noRoute = new AdbResult(true, $"failed to connect to '{IpAddress}:5555': No route to host");
        var connected = new AdbResult(true, $"connected to {IpAddress}:5555");
        adb.ExecuteAsync(Arg<IAdbCommand>.Is(c => c is ConnectDeviceCommand), Arg<CancellationToken>.Any())
           .Returns(Task.FromResult(noRoute))
           .Then()
           .Returns(Task.FromResult(connected));

        // Two lookups per attempt: before connecting (stale transport check) and after
        adb.QueryAsync<List<ShieldDevice>>(Arg<IAdbQuery<List<ShieldDevice>>>.Any(), Arg<CancellationToken>.Any())
           .Returns(Task.FromResult(NoDevices))
           .Then()
           .Returns(Task.FromResult(NoDevices))
           .Then()
           .Returns(Task.FromResult(NoDevices))
           .Then()
           .Returns(Task.FromResult(OnlineDevice));

        var killed = new AdbResult(true, "");
        adb.ExecuteAsync(Arg<IAdbCommand>.Is(c => c is KillServerCommand), Arg<CancellationToken>.Any())
           .Returns(Task.FromResult(killed));

        var service = CreateService(adb);
        var result = await service.ConnectAsync(IpAddress);

        await Assert.That(result.Status).IsEqualTo(ConnectionStatus.Connected);
        adb.ExecuteAsync(Arg<IAdbCommand>.Is(c => c is KillServerCommand), Arg<CancellationToken>.Any()).Called(Count.Once());
        adb.ExecuteAsync(Arg<IAdbCommand>.Is(c => c is ConnectDeviceCommand), Arg<CancellationToken>.Any()).Called(Count.Exactly(2));
    }

    [Test]
    [ExcludeOn(OS.MacOs)]
    public async Task Connect_NoRouteToHost_OutsideMacOs_DoesNotRestartServer()
    {
        var adb = IAdbExecutor.Imposter();
        var noRoute = new AdbResult(true, $"failed to connect to '{IpAddress}:5555': No route to host");
        adb.ExecuteAsync(Arg<IAdbCommand>.Is(c => c is ConnectDeviceCommand), Arg<CancellationToken>.Any())
           .Returns(Task.FromResult(noRoute));
        adb.QueryAsync<List<ShieldDevice>>(Arg<IAdbQuery<List<ShieldDevice>>>.Any(), Arg<CancellationToken>.Any())
           .Returns(Task.FromResult(NoDevices));

        var service = CreateService(adb);
        var result = await service.ConnectAsync(IpAddress);

        await Assert.That(result.Status).IsEqualTo(ConnectionStatus.Failed);
        adb.ExecuteAsync(Arg<IAdbCommand>.Is(c => c is KillServerCommand), Arg<CancellationToken>.Any()).Called(Count.Never());
    }

    [Test]
    public async Task Connect_OtherFailure_DoesNotRestartServer()
    {
        var adb = IAdbExecutor.Imposter();
        var refused = new AdbResult(true, $"failed to connect to '{IpAddress}:5555': Connection refused");
        adb.ExecuteAsync(Arg<IAdbCommand>.Is(c => c is ConnectDeviceCommand), Arg<CancellationToken>.Any())
           .Returns(Task.FromResult(refused));
        adb.QueryAsync<List<ShieldDevice>>(Arg<IAdbQuery<List<ShieldDevice>>>.Any(), Arg<CancellationToken>.Any())
           .Returns(Task.FromResult(NoDevices));

        var service = CreateService(adb);
        var result = await service.ConnectAsync(IpAddress);

        await Assert.That(result.Status).IsEqualTo(ConnectionStatus.Failed);
        await Assert.That(result.Error).IsEqualTo(refused.Output);
        adb.ExecuteAsync(Arg<IAdbCommand>.Is(c => c is KillServerCommand), Arg<CancellationToken>.Any()).Called(Count.Never());
    }

    [Test]
    public async Task Connect_Online_OpensSessionForDeviceSerial()
    {
        var adb = IAdbExecutor.Imposter();
        var connected = new AdbResult(true, $"connected to {IpAddress}:5555");
        adb.ExecuteAsync(Arg<IAdbCommand>.Is(c => c is ConnectDeviceCommand), Arg<CancellationToken>.Any())
           .Returns(Task.FromResult(connected));
        adb.QueryAsync<List<ShieldDevice>>(Arg<IAdbQuery<List<ShieldDevice>>>.Any(), Arg<CancellationToken>.Any())
           .Returns(Task.FromResult(OnlineDevice));

        var runner = IAdbRunner.Imposter();
        var service = CreateService(adb, runner);
        var result = await service.ConnectAsync(IpAddress);

        await Assert.That(result.Status).IsEqualTo(ConnectionStatus.Connected);
        runner.OpenSession($"{IpAddress}:5555").Called(Count.Once());
    }

    [Test]
    public async Task Connect_Failure_DoesNotOpenSession()
    {
        var adb = IAdbExecutor.Imposter();
        var refused = new AdbResult(false, $"failed to connect to '{IpAddress}:5555': Connection refused");
        adb.ExecuteAsync(Arg<IAdbCommand>.Is(c => c is ConnectDeviceCommand), Arg<CancellationToken>.Any())
           .Returns(Task.FromResult(refused));
        adb.QueryAsync<List<ShieldDevice>>(Arg<IAdbQuery<List<ShieldDevice>>>.Any(), Arg<CancellationToken>.Any())
           .Returns(Task.FromResult(NoDevices));

        var runner = IAdbRunner.Imposter();
        var service = CreateService(adb, runner);
        await service.ConnectAsync(IpAddress);

        runner.OpenSession(Arg<string>.Any()).Called(Count.Never());
    }

    private static DeviceConnectionService CreateService(IAdbExecutorImposter adb, IAdbRunnerImposter? runner = null)
    {
        runner ??= IAdbRunner.Imposter();
        var settings = ISettingsService.Imposter();
        var discovery = IDeviceDiscoveryService.Imposter();
        var adbConfig = IAdbConfigService.Imposter();
        return new DeviceConnectionService(adb.Instance(), runner.Instance(), settings.Instance(), discovery.Instance(), adbConfig.Instance());
    }
}
