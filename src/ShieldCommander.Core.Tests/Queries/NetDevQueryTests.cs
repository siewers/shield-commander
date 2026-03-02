using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Tests.Queries;

public class NetDevQueryTests
{
    private readonly NetDevQuery _query = new();

    // Captured from NVIDIA Shield at 10.0.0.99: cat /proc/net/dev
    private const string RealOutput = """
        Inter-|   Receive                                                |  Transmit
         face |bytes    packets errs drop fifo frame compressed multicast|bytes    packets errs drop fifo colls carrier compressed
        ip_vti0:       0       0    0    0    0     0          0         0        0       0    0    0    0     0       0          0
          sit0:       0       0    0    0    0     0          0         0        0       0    0    0    0     0       0          0
         wlan0:       0       0    0    0    0     0          0         0      388       4    0    0    0     0       0          0
            lo:   55885     490    0    0    0     0          0         0    55885     490    0    0    0     0       0          0
          eth0: 3988265217 2787138    0    0    0     0          0     25682 34217039  388641    0    0    0     0       0          0
        ip6tnl0:       0       0    0    0    0     0          0         0        0       0    0    0    0     0       0          0
        ip6_vti0:       0       0    0    0    0     0          0         0        0       0    0    0    0     0       0          0
        """;

    [Test]
    public async Task Parse_RealOutput_AggregatesAllInterfacesExceptLoopback()
    {
        var result = _query.Parse(RealOutput);

        // ip_vti0(0) + sit0(0) + wlan0(0) + eth0(3988265217) + ip6tnl0(0) + ip6_vti0(0) — lo excluded
        await Assert.That(result.BytesIn).IsEqualTo(3_988_265_217L);
        await Assert.That(result.PacketsIn).IsEqualTo(2_787_138L);
        // wlan0(388) + eth0(34217039)
        await Assert.That(result.BytesOut).IsEqualTo(34_217_427L);
        // wlan0(4) + eth0(388641)
        await Assert.That(result.PacketsOut).IsEqualTo(388_645L);
    }

    [Test]
    public async Task Parse_EmptyOutput_ReturnsZeros()
    {
        var result = _query.Parse("");

        await Assert.That(result.BytesIn).IsEqualTo(0);
        await Assert.That(result.BytesOut).IsEqualTo(0);
    }
}
