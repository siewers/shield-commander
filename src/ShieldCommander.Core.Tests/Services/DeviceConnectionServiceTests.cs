using Imposter.Abstractions;
using ShieldCommander.Core.Models;
using ShieldCommander.Core.Services;
using TUnit.Core.Enums;

[assembly: GenerateImposter(typeof(IAdbService))]
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
        var adb = IAdbService.Imposter();
        var noRoute = new AdbResult(true, $"failed to connect to '{IpAddress}:5555': No route to host");
        var connected = new AdbResult(true, $"connected to {IpAddress}:5555");
        adb.ConnectAsync(Arg<string>.Any(), Arg<int>.Any())
           .Returns(Task.FromResult(noRoute))
           .Then()
           .Returns(Task.FromResult(connected));

        // Two lookups per attempt: before connecting (stale transport check) and after
        adb.GetDevicesAsync()
           .Returns(Task.FromResult(NoDevices))
           .Then()
           .Returns(Task.FromResult(NoDevices))
           .Then()
           .Returns(Task.FromResult(NoDevices))
           .Then()
           .Returns(Task.FromResult(OnlineDevice));

        var killed = new AdbResult(true, "");
        adb.KillServerAsync().Returns(Task.FromResult(killed));

        var service = CreateService(adb);
        var result = await service.ConnectAsync(IpAddress);

        await Assert.That(result.Status).IsEqualTo(ConnectionStatus.Connected);
        adb.KillServerAsync().Called(Count.Once());
        adb.ConnectAsync(Arg<string>.Any(), Arg<int>.Any()).Called(Count.Exactly(2));
    }

    [Test]
    [ExcludeOn(OS.MacOs)]
    public async Task Connect_NoRouteToHost_OutsideMacOs_DoesNotRestartServer()
    {
        var adb = IAdbService.Imposter();
        var noRoute = new AdbResult(true, $"failed to connect to '{IpAddress}:5555': No route to host");
        adb.ConnectAsync(Arg<string>.Any(), Arg<int>.Any()).Returns(Task.FromResult(noRoute));
        adb.GetDevicesAsync().Returns(Task.FromResult(NoDevices));

        var service = CreateService(adb);
        var result = await service.ConnectAsync(IpAddress);

        await Assert.That(result.Status).IsEqualTo(ConnectionStatus.Failed);
        adb.KillServerAsync().Called(Count.Never());
    }

    [Test]
    public async Task Connect_OtherFailure_DoesNotRestartServer()
    {
        var adb = IAdbService.Imposter();
        var refused = new AdbResult(true, $"failed to connect to '{IpAddress}:5555': Connection refused");
        adb.ConnectAsync(Arg<string>.Any(), Arg<int>.Any()).Returns(Task.FromResult(refused));
        adb.GetDevicesAsync().Returns(Task.FromResult(NoDevices));

        var service = CreateService(adb);
        var result = await service.ConnectAsync(IpAddress);

        await Assert.That(result.Status).IsEqualTo(ConnectionStatus.Failed);
        await Assert.That(result.Error).IsEqualTo(refused.Output);
        adb.KillServerAsync().Called(Count.Never());
    }

    [Test]
    public async Task Connect_Online_OpensSessionForDeviceSerial()
    {
        var adb = IAdbService.Imposter();
        var connected = new AdbResult(true, $"connected to {IpAddress}:5555");
        adb.ConnectAsync(Arg<string>.Any(), Arg<int>.Any()).Returns(Task.FromResult(connected));
        adb.GetDevicesAsync().Returns(Task.FromResult(OnlineDevice));

        var service = CreateService(adb);
        var result = await service.ConnectAsync(IpAddress);

        await Assert.That(result.Status).IsEqualTo(ConnectionStatus.Connected);
        adb.OpenSession($"{IpAddress}:5555").Called(Count.Once());
    }

    [Test]
    public async Task Connect_Failure_DoesNotOpenSession()
    {
        var adb = IAdbService.Imposter();
        var refused = new AdbResult(false, $"failed to connect to '{IpAddress}:5555': Connection refused");
        adb.ConnectAsync(Arg<string>.Any(), Arg<int>.Any()).Returns(Task.FromResult(refused));
        adb.GetDevicesAsync().Returns(Task.FromResult(NoDevices));

        var service = CreateService(adb);
        await service.ConnectAsync(IpAddress);

        adb.OpenSession(Arg<string>.Any()).Called(Count.Never());
    }

    private static DeviceConnectionService CreateService(IAdbServiceImposter adb)
    {
        var settings = ISettingsService.Imposter();
        var discovery = IDeviceDiscoveryService.Imposter();
        var adbConfig = IAdbConfigService.Imposter();
        return new DeviceConnectionService(adb.Instance(), settings.Instance(), discovery.Instance(), adbConfig.Instance());
    }
}
