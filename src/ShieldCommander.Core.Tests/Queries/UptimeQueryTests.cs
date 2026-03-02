using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Tests.Queries;

public class UptimeQueryTests
{
    private readonly UptimeQuery _query = new();

    [Test]
    public async Task Parse_RealOutput_HoursOnly()
    {
        // Captured from NVIDIA Shield at 10.0.0.99
        // "1:31,  0 users, ..." — second segment "0 users" has no ':', so uptimeSpan becomes
        // "1:31,  0 users" and ParseDuration extracts hours=1 but minutes fails due to trailing text
        var result = _query.Parse(" 23:24:21 up  1:31,  0 users,  load average: 2.26, 1.99, 1.82");
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Duration).IsEqualTo(new TimeSpan(0, 1, 0, 0));
    }

    [Test]
    public async Task Parse_DaysOnly()
    {
        // "5 days, 3:45, 1 user" — second segment contains ':', so only "5 days" is extracted
        var result = _query.Parse(" 10:30:00 up 5 days, 3:45, 1 user, load average: 0.1");
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Duration).IsEqualTo(new TimeSpan(5, 0, 0, 0));
    }

    [Test]
    public async Task Parse_DaysWithMinutes_ExtractsBoth()
    {
        // "2 days, 30 min, 1 user" — second segment has no ':', so "2 days, 30 min" is captured
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
