using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Commands;

public sealed class UninstallPackageCommand(string packageName) : IAdbCommand
{
    public string Name => "UninstallPackage";

    public async Task<AdbResult> ExecuteAsync(IAdbRunner runner, CancellationToken ct)
    {
        // Keep stderr so a "Failure [REASON]" printed there still reaches the caller
        var output = (await runner.RunShellAsync($"pm uninstall {packageName} 2>&1", ct)).Trim();
        var success = output == "Success";
        return new AdbResult(success, output, success ? "" : output);
    }
}
