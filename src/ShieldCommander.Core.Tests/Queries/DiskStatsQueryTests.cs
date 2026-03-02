using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Tests.Queries;

public class DiskStatsQueryTests
{
    private readonly DiskStatsQuery _query = new();

    // Captured from NVIDIA Shield at 10.0.0.99
    private const string RealOutput = """
        pgpgin 8223825
        pgpgout 414736
        Latency: 1ms [512B Data Write]
        Recent Disk Write Speed (kB/s) = 21141
        """;

    [Test]
    public async Task Parse_RealOutput_AllFields()
    {
        var result = _query.Parse(RealOutput);

        await Assert.That(result.BytesRead).IsEqualTo(8_223_825L * 1024);
        await Assert.That(result.BytesWritten).IsEqualTo(414_736L * 1024);
        await Assert.That(result.WriteLatencyMs).IsEqualTo(1);
        await Assert.That(result.WriteSpeed).IsEqualTo(21_141L * 1024);
    }

    [Test]
    public async Task Parse_EmptyOutput_ReturnsZeros()
    {
        var result = _query.Parse("");

        await Assert.That(result.BytesRead).IsEqualTo(0);
        await Assert.That(result.BytesWritten).IsEqualTo(0);
        await Assert.That(result.WriteLatencyMs).IsEqualTo(0);
        await Assert.That(result.WriteSpeed).IsEqualTo(0);
    }
}
