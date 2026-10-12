using ShieldCommander.Core.Models;
using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Tests.Queries;

public class DevicesQueryTests
{
    private readonly DevicesQuery _query = new();

    [Test]
    public async Task Parse_OnlineDevice_ReadsSerialModelAndState()
    {
        var devices = _query.Parse("""
            List of devices attached
            10.0.0.150:5555        device product:mdarcy model:SHIELD_Android_TV device:mdarcy transport_id:2
            """);

        await Assert.That(devices).Count().IsEqualTo(1);
        await Assert.That(devices[0].IpAddress).IsEqualTo("10.0.0.150:5555");
        await Assert.That(devices[0].DeviceName).IsEqualTo("SHIELD_Android_TV");
        await Assert.That(devices[0].State).IsEqualTo(AdbDeviceState.Online);
    }

    [Test]
    public async Task Parse_UnauthorizedDevice_IsIncludedWithState()
    {
        var devices = _query.Parse("""
            List of devices attached
            10.0.0.150:5555        unauthorized transport_id:1
            """);

        await Assert.That(devices).Count().IsEqualTo(1);
        await Assert.That(devices[0].State).IsEqualTo(AdbDeviceState.Unauthorized);
    }

    [Test]
    public async Task Parse_OfflineAndTabSeparated_ParsesBoth()
    {
        var devices = _query.Parse("List of devices attached\n10.0.0.150:5555\toffline\n10.0.0.99:5555\tdevice\n");

        await Assert.That(devices).Count().IsEqualTo(2);
        await Assert.That(devices[0].State).IsEqualTo(AdbDeviceState.Offline);
        await Assert.That(devices[1].State).IsEqualTo(AdbDeviceState.Online);
    }

    [Test]
    public async Task Parse_DaemonStartupLines_AreSkipped()
    {
        var devices = _query.Parse("""
            * daemon not running; starting now at tcp:5037
            * daemon started successfully
            List of devices attached
            """);

        await Assert.That(devices).IsEmpty();
    }
}
