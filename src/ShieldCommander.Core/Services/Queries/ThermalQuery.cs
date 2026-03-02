using System.Globalization;
using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Queries;

internal sealed class ThermalQuery : IAdbBatchQuery<DynamicSections>
{
    public string Name => nameof(DynamicSections.Thermal);

    public string CommandText => "dumpsys thermalservice";

    public ThermalSnapshot Parse(ReadOnlySpan<char> output)
    {
        var zones = new List<ThermalZone>();
        var inTemps = false;

        foreach (var line in output.EnumerateLines())
        {
            var trimmed = line.Trim();

            if (!inTemps)
            {
                if (trimmed.StartsWith("Current temperatures from HAL"))
                {
                    inTemps = true;
                }

                continue;
            }

            if (trimmed.IndexOf("mValue=") < 0)
            {
                break;
            }

            trimmed.ExtractDumpsysFields(out var tName, out var tValue);
            if (float.TryParse(tValue, CultureInfo.InvariantCulture, out var temp))
            {
                zones.Add(new ThermalZone(tName.ToString(), temp));
            }
        }

        return new ThermalSnapshot(zones);
    }

    public void Apply(ReadOnlySpan<char> output, DynamicSections target)
        => target.Thermal = Parse(output);
}
