using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Tests.Queries;

public class ThermalQueryTests
{
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

    private readonly ThermalQuery _query = new();

    [Test]
    public async Task Parse_RealOutput_ExtractsCurrentTemperatures()
    {
        var result = _query.Parse(RealOutput);

        await Assert.That(result.Zones).Count().IsEqualTo(5);
        await AssertZone(0, "CPU0", 56.000004f);
        await AssertZone(1, "CPU1", 56.000004f);
        await AssertZone(2, "CPU2", 56.000004f);
        await AssertZone(3, "CPU3", 56.000004f);
        await AssertZone(4, "GPU", 55.500004f);

        return;

        // Value passes through float.TryParse, so compare at float precision
        async Task AssertZone(int index, string name, float value)
        {
            await Assert.That(result.Zones[index].Name).IsEqualTo(name);
            await Assert.That(result.Zones[index].Value).IsEqualTo(value);
        }
    }

    [Test]
    public async Task Parse_EmptyOutput_ReturnsEmptyZones()
    {
        var result = _query.Parse("");

        await Assert.That(result.Zones).IsEmpty();
    }
}
