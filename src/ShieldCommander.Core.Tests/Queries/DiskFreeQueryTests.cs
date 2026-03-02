using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Tests.Queries;

public class DiskFreeQueryTests
{
    private readonly DiskFreeQuery _query = new();

    // Captured from NVIDIA Shield at 10.0.0.99: df -h /data
    private const string RealOutput = """
        Filesystem            Size  Used Avail Use% Mounted on
        /dev/block/mmcblk0p32  12G  3.0G  8.4G  27% /data/user/0
        """;

    [Test]
    public async Task Parse_RealOutput_ParsesTotal()
    {
        var result = _query.Parse(RealOutput);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Total).IsEqualTo(12L * 1024 * 1024 * 1024);
    }

    [Test]
    public async Task Parse_EmptyOutput_ReturnsNull()
    {
        var result = _query.Parse("");
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Parse_HeaderOnly_ReturnsNull()
    {
        var result = _query.Parse("Filesystem  Size  Used Avail Use% Mounted on");
        await Assert.That(result).IsNull();
    }
}
