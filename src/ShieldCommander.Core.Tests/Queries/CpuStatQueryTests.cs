using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Tests.Queries;

public class CpuStatQueryTests
{
    private readonly CpuStatQuery _query = new();

    // Captured from NVIDIA Shield at 10.0.0.99: cat /proc/stat; cat /proc/loadavg; ls /proc/
    // Trimmed to essential lines for test clarity
    private const string RealOutput = """
        cpu  237000 3302 135877 1734135 5016 21801 9017 0 0 0
        cpu0 54254 737 34157 428750 846 8251 5917 0 0 0
        cpu1 59030 832 33080 435013 1519 6494 1015 0 0 0
        cpu2 57102 878 31438 439792 1291 3668 1074 0 0 0
        cpu3 66614 854 37201 430579 1358 3386 1009 0 0 0
        intr 41444213 0 931388 0 0 0 0 0 0 0
        ctxt 60611017
        btime 1772484772
        processes 14911
        procs_running 1
        procs_blocked 0
        softirq 14691864 6 4001100 361512 3437844 0 0 228668 3070196 0 3592538
        2.24 1.99 1.82 4/1699 14909
        1
        10
        103
        1077
        asound
        net
        self
        version
        """;

    [Test]
    public async Task Parse_RealOutput_AggregateCpuValues()
    {
        var result = _query.Parse(RealOutput);

        await Assert.That(result.User).IsEqualTo(237_000);
        await Assert.That(result.Nice).IsEqualTo(3_302);
        await Assert.That(result.System).IsEqualTo(135_877);
        await Assert.That(result.Idle).IsEqualTo(1_734_135);
        await Assert.That(result.IoWait).IsEqualTo(5_016);
        await Assert.That(result.Irq).IsEqualTo(21_801);
        await Assert.That(result.SoftIrq).IsEqualTo(9_017);
        await Assert.That(result.Steal).IsEqualTo(0);
    }

    [Test]
    public async Task Parse_RealOutput_FourCores()
    {
        var result = _query.Parse(RealOutput);

        await Assert.That(result.Cores).Count().IsEqualTo(4);
        await Assert.That(result.Cores[0].Name).IsEqualTo("CPU0");
        await Assert.That(result.Cores[3].Name).IsEqualTo("CPU3");
    }

    [Test]
    public async Task Parse_RealOutput_CoreActiveAndTotal()
    {
        var result = _query.Parse(RealOutput);

        // cpu0: active = 54254+737+34157+846+8251+5917+0 = 104162, total = 104162+428750 = 532912
        await Assert.That(result.Cores[0].Active).IsEqualTo(104_162);
        await Assert.That(result.Cores[0].Total).IsEqualTo(532_912);
    }

    [Test]
    public async Task Parse_RealOutput_ThreadCount()
    {
        var result = _query.Parse(RealOutput);

        // From "4/1699" in loadavg line
        await Assert.That(result.ThreadCount).IsEqualTo(1699);
    }

    [Test]
    public async Task Parse_RealOutput_ProcessCount()
    {
        var result = _query.Parse(RealOutput);

        // All-digit entries after loadavg: 1, 10, 103, 1077
        await Assert.That(result.ProcessCount).IsEqualTo(4);
    }

    [Test]
    public async Task Parse_EmptyOutput_ReturnsZeros()
    {
        var result = _query.Parse("");

        await Assert.That(result.User).IsEqualTo(0);
        await Assert.That(result.Cores).IsEmpty();
        await Assert.That(result.ProcessCount).IsEqualTo(0);
        await Assert.That(result.ThreadCount).IsEqualTo(0);
    }
}
