using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Queries;

public sealed class PackageInfoQuery(string packageName) : IAdbQuery<InstalledPackage>
{
    public async Task<InstalledPackage> ExecuteAsync(IAdbRunner runner, CancellationToken ct)
    {
        var cmd = $"dumpsys package {packageName}; echo ---; "
                + $"stat -c %s $(pm path {packageName} | sed 's/package://g') 2>/dev/null";

        var output = await runner.RunShellAsync(cmd, ct);

        return output.Length > 0 ? Parse(output) : new InstalledPackage(packageName);
    }

    public InstalledPackage Parse(string output)
    {
        var sections = output.Split("---", 2);
        var package = PackageParsing.ParseDumpsys(packageName, sections[0]);
        var codeSize = sections.Length > 1 ? PackageParsing.ParseSize(sections[1]) : null;

        return codeSize is not null ? package with { CodeSize = codeSize } : package;
    }
}
