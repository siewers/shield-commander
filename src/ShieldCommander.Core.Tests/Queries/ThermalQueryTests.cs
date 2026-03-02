using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Tests.Queries;

public class ThermalQueryTests
{
    private readonly ThermalQuery _query = new();

    // Captured from NVIDIA Shield at 10.0.0.99: dumpsys thermalservice
    private const string RealOutput = """
        IsStatusOverride: false
        ThermalEventListeners:
        	callbacks: 1
        	killed: false
        	broadcasts count: -1
        ThermalStatusListeners:
        	callbacks: 3
        	killed: false
        	broadcasts count: -1
        Thermal Status: 0
        Cached temperatures:
        	Temperature{mValue=51.000004, mType=1, mName=GPU, mStatus=0}
        	Temperature{mValue=51.500004, mType=0, mName=CPU0, mStatus=0}
        	Temperature{mValue=51.500004, mType=0, mName=CPU1, mStatus=0}
        	Temperature{mValue=51.500004, mType=0, mName=CPU2, mStatus=0}
        	Temperature{mValue=51.500004, mType=0, mName=CPU3, mStatus=0}
        HAL Ready: true
        HAL connection:
        	ThermalHAL 1.0 connected: yes
        Current temperatures from HAL:
        	Temperature{mValue=56.000004, mType=0, mName=CPU0, mStatus=0}
        	Temperature{mValue=56.000004, mType=0, mName=CPU1, mStatus=0}
        	Temperature{mValue=56.000004, mType=0, mName=CPU2, mStatus=0}
        	Temperature{mValue=56.000004, mType=0, mName=CPU3, mStatus=0}
        	Temperature{mValue=55.500004, mType=1, mName=GPU, mStatus=0}
        Current cooling devices from HAL:
        	CoolingDevice{mValue=0, mType=0, mName=FAN}
        """;

    [Test]
    public async Task Parse_RealOutput_ExtractsCurrentTemperatures()
    {
        var result = _query.Parse(RealOutput);

        await Assert.That(result.Zones).Count().IsEqualTo(5);
        await Assert.That(result.Zones[0].Name).IsEqualTo("CPU0");
        await Assert.That((float)result.Zones[0].Value).IsEqualTo(56.000004f);
        await Assert.That(result.Zones[4].Name).IsEqualTo("GPU");
        await Assert.That((float)result.Zones[4].Value).IsEqualTo(55.500004f);
    }

    [Test]
    public async Task Parse_RealOutput_IgnoresCachedTemperatures()
    {
        var result = _query.Parse(RealOutput);

        // Should only contain "Current temperatures from HAL" section, not "Cached temperatures"
        await Assert.That(result.Summary).Contains("CPU0: 56.0°C");
    }

    [Test]
    public async Task Parse_RealOutput_BuildsSummary()
    {
        var result = _query.Parse(RealOutput);

        await Assert.That(result.Summary)
            .IsEqualTo("CPU0: 56.0°C, CPU1: 56.0°C, CPU2: 56.0°C, CPU3: 56.0°C, GPU: 55.5°C");
    }

    [Test]
    public async Task Parse_EmptyOutput_ReturnsNullSummary()
    {
        var result = _query.Parse("");

        await Assert.That(result.Summary).IsNull();
        await Assert.That(result.Zones).IsEmpty();
    }
}
