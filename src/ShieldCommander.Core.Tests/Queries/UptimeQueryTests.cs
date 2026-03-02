using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Tests.Queries;

public class UptimeQueryTests
{
    private readonly UptimeQuery _query = new();

    [Test]
    public async Task Parse_RealOutput_HoursAndMinutes()
    {
        // Captured from NVIDIA Shield at 10.0.0.99
        // "1:31, 0 users, ..." — uptime is everything before ", 0 users"
        var result = _query.Parse(" 23:24:21 up  1:31,  0 users,  load average: 2.26, 1.99, 1.82");
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Duration).IsEqualTo(new TimeSpan(0, 1, 31, 0));
    }

    [Test]
    public async Task Parse_DaysAndHoursMinutes()
    {
        // "5 days, 3:45, 1 user" — uptime is "5 days, 3:45"
        var result = _query.Parse(" 10:30:00 up 5 days, 3:45, 1 user, load average: 0.1");
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Duration).IsEqualTo(new TimeSpan(5, 3, 45, 0));
    }

    [Test]
    public async Task Parse_DaysWithMinutes_ExtractsBoth()
    {
        // "2 days, 30 min, 1 user" — uptime is "2 days, 30 min"
        var result = _query.Parse(" 10:30:00 up 2 days, 30 min, 1 user");
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Duration).IsEqualTo(new TimeSpan(2, 0, 30, 0));
    }

    [Test]
    public async Task Parse_MinutesOnly()
    {
        var result = _query.Parse(" 10:30:00 up 15 min, 1 user");
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Duration).IsEqualTo(TimeSpan.FromMinutes(15));
    }

    [Test]
    public async Task Parse_NoUpKeyword_ReturnsNull()
    {
        var result = _query.Parse("garbage output");
        await Assert.That(result).IsNull();
    }
}
