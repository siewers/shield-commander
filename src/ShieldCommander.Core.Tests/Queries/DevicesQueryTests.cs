using AdvancedSharpAdbClient.Models;
using ShieldCommander.Core.Models;
using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Tests.Queries;

public class DevicesQueryTests
{
    private readonly DevicesQuery _query = new();

    [Test]
    public async Task Map_OnlineDevice_ReadsSerialModelAndState()
    {
        DeviceData[] data = [new() { Serial = "10.0.0.150:5555", State = DeviceState.Online, Model = "SHIELD_Android_TV" }];

        var devices = _query.Map(data);

        await Assert.That(devices).Count().IsEqualTo(1);
        await Assert.That(devices[0].IpAddress).IsEqualTo("10.0.0.150:5555");
        await Assert.That(devices[0].DeviceName).IsEqualTo("SHIELD_Android_TV");
        await Assert.That(devices[0].State).IsEqualTo(AdbDeviceState.Online);
    }

    [Test]
    public async Task Map_UnauthorizedDeviceWithoutModel_IsIncludedWithoutName()
    {
        DeviceData[] data = [new() { Serial = "10.0.0.150:5555", State = DeviceState.Unauthorized }];

        var devices = _query.Map(data);

        await Assert.That(devices).Count().IsEqualTo(1);
        await Assert.That(devices[0].DeviceName).IsNull();
        await Assert.That(devices[0].State).IsEqualTo(AdbDeviceState.Unauthorized);
    }

    [Test]
    public async Task Map_MixedStates_MapsEach()
    {
        DeviceData[] data =
        [
            new() { Serial = "10.0.0.150:5555", State = DeviceState.Offline },
            new() { Serial = "10.0.0.99:5555", State = DeviceState.Online },
            new() { Serial = "10.0.0.98:5555", State = DeviceState.Recovery },
        ];

        var devices = _query.Map(data);

        await Assert.That(devices.Select(d => d.State)).IsEquivalentTo([AdbDeviceState.Offline, AdbDeviceState.Online, AdbDeviceState.Unknown]);
    }

    [Test]
    public async Task Map_NoDevices_ReturnsEmpty()
    {
        var devices = _query.Map([]);

        await Assert.That(devices).IsEmpty();
    }
}
