using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Commands;

internal sealed class UninstallPackageCommand(string packageName) : IAdbCommand
{
    public string Name => "UninstallPackage";

    public async Task<AdbResult> ExecuteAsync(IAdbRunner runner)
    {
        // Keep stderr so a "Failure [REASON]" printed there still reaches the caller
        var output = (await runner.RunShellAsync($"pm uninstall {packageName} 2>&1")).Trim();
        var success = output == "Success";
        return new AdbResult(success, output, success ? "" : output);
    }
}
