using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Tests.Queries;

public class MemInfoQueryTests
{
    private readonly MemInfoQuery _query = new();

    // Captured from NVIDIA Shield at 10.0.0.99: cat /proc/meminfo
    private const string RealOutput = """
        MemTotal:        3016708 kB
        MemFree:          101480 kB
        MemAvailable:     645176 kB
        Buffers:           12456 kB
        Cached:           629556 kB
        SwapCached:         9852 kB
        Active:          1153280 kB
        Inactive:         594644 kB
        Active(anon):     867968 kB
        Inactive(anon):   311848 kB
        Active(file):     285312 kB
        Inactive(file):   282796 kB
        Unevictable:           0 kB
        Mlocked:               0 kB
        SwapTotal:        524284 kB
        SwapFree:         245060 kB
        Dirty:                 0 kB
        Writeback:             0 kB
        AnonPages:       1100804 kB
        Mapped:           314940 kB
        Shmem:             73900 kB
        Slab:             128664 kB
        SReclaimable:      43944 kB
        SUnreclaim:        84720 kB
        KernelStack:       27200 kB
        PageTables:        55660 kB
        NFS_Unstable:          0 kB
        Bounce:                0 kB
        WritebackTmp:          0 kB
        CommitLimit:     2032636 kB
        Committed_AS:   52887988 kB
        VmallocTotal:   263061440 kB
        VmallocUsed:           0 kB
        VmallocChunk:          0 kB
        AnonHugePages:         0 kB
        ShmemHugePages:        0 kB
        ShmemPmdMapped:        0 kB
        NvMapMemFree:       1020 kB
        NvMapMemUsed:     740388 kB
        CmaTotal:         475136 kB
        CmaFree:           44080 kB
        HugePages_Total:       0
        HugePages_Free:        0
        HugePages_Rsvd:        0
        HugePages_Surp:        0
        Hugepagesize:       2048 kB
        """;

    [Test]
    public async Task Parse_RealOutput_ParsesAllFields()
    {
        var result = _query.Parse(RealOutput);

        await Assert.That(result.Total).IsEqualTo(3_016_708L * 1024);
        await Assert.That(result.Snapshot.Total).IsEqualTo(3_016_708L * 1024);
        await Assert.That(result.Snapshot.Free).IsEqualTo(101_480L * 1024);
        await Assert.That(result.Snapshot.Available).IsEqualTo(645_176L * 1024);
        await Assert.That(result.Snapshot.Buffers).IsEqualTo(12_456L * 1024);
        await Assert.That(result.Snapshot.Cached).IsEqualTo(629_556L * 1024);
        await Assert.That(result.Snapshot.SwapTotal).IsEqualTo(524_284L * 1024);
        await Assert.That(result.Snapshot.SwapFree).IsEqualTo(245_060L * 1024);
    }

    [Test]
    public async Task Parse_EmptyOutput_ReturnsZeros()
    {
        var result = _query.Parse("");

        await Assert.That(result.Total).IsEqualTo(0);
        await Assert.That(result.Snapshot.Free).IsEqualTo(0);
    }
}
