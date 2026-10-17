using AdvancedSharpAdbClient.Models;
using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Commands;

public sealed class InstallPackageCommand(string apkFilePath, IProgress<InstallProgress>? progress = null) : IAdbCommand
{
    public string Name => "InstallPackage";

    public async Task<AdbResult> ExecuteAsync(IAdbRunner runner, CancellationToken ct)
    {
        return await runner.RunOnDeviceAsync(async (client, device, token) =>
        {
            InstallProgress? last = null;

            void OnProgress(InstallProgressEventArgs args)
            {
                // The library reports every chunk it writes, so only pass on changes the user can see
                if (ToProgress(args) is { } current && current != last)
                {
                    last = current;
                    progress?.Report(current);
                }
            }

            // Streams the APK into the package manager, so no temporary copy is left on the device when the install fails
            await using var apk = File.OpenRead(apkFilePath);
            await client.InstallAsync(device, apk, progress is null ? null : OnProgress, token, "-r");
            return "Success";
        }, ct);
    }

    internal static InstallProgress? ToProgress(InstallProgressEventArgs args)
    {
        return args.State switch
        {
            PackageInstallProgressState.Preparing => new InstallProgress(InstallPhase.Transferring, 0),
            PackageInstallProgressState.Uploading
                or PackageInstallProgressState.CreateSession
                or PackageInstallProgressState.WriteSession => new InstallProgress(InstallPhase.Transferring, (int)args.UploadProgress),
            PackageInstallProgressState.Installing
                or PackageInstallProgressState.PostInstall => new InstallProgress(InstallPhase.Installing),

            // Completion is reported by the command result
            _ => null,
        };
    }
}
