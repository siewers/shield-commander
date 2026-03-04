using ShieldCommander.Core.Models;
using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Tests.Queries;

public class UptimeQueryTests
{
    private readonly UptimeQuery _query = new();

    [Test]
    public async Task Parse_RealOutput_HoursAndMinutes()
    {
        var info = Apply(" 23:24:21 up  1:31,  0 users,  load average: 2.26, 1.99, 1.82");
        await Assert.That(info.Uptime).IsNotNull();
        await Assert.That(info.Uptime!.Value).IsEqualTo(new TimeSpan(0, 1, 31, 0));
    }

    [Test]
    public async Task Parse_DaysAndHoursMinutes()
    {
        var info = Apply(" 10:30:00 up 5 days, 3:45, 1 user, load average: 0.1");
        await Assert.That(info.Uptime).IsNotNull();
        await Assert.That(info.Uptime!.Value).IsEqualTo(new TimeSpan(5, 3, 45, 0));
    }

    [Test]
    public async Task Parse_DaysWithMinutes_ExtractsBoth()
    {
        var info = Apply(" 10:30:00 up 2 days, 30 min, 1 user");
        await Assert.That(info.Uptime).IsNotNull();
        await Assert.That(info.Uptime!.Value).IsEqualTo(new TimeSpan(2, 0, 30, 0));
    }

    [Test]
    public async Task Parse_MinutesOnly()
    {
        var info = Apply(" 10:30:00 up 15 min, 1 user");
        await Assert.That(info.Uptime).IsNotNull();
        await Assert.That(info.Uptime!.Value).IsEqualTo(TimeSpan.FromMinutes(15));
    }

    [Test]
    public async Task Parse_NoUpKeyword_ReturnsNull()
    {
        var info = Apply("garbage output");
        await Assert.That(info.Uptime).IsNull();
    }

    private DeviceInfo Apply(string output)
    {
        var info = new DeviceInfo();
        _query.Apply(output, info);
        return info;
    }
}
