using AdvancedSharpAdbClient.Models;
using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Commands;

internal sealed class InstallPackageCommand(string apkFilePath) : IAdbCommand
{
    public string Name => "InstallPackage";

    public async Task<AdbResult> ExecuteAsync(IAdbRunner runner)
    {
        return await runner.RunOnDeviceAsync(async (client, device, ct) =>
        {
            // Streams the APK into the package manager, so no temporary copy is left on the device when the install fails
            await using var apk = File.OpenRead(apkFilePath);
            await client.InstallAsync(device, apk, (Action<InstallProgressEventArgs>?)null, ct, "-r");
            return "Success";
        });
    }
}
