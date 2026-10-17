using AdvancedSharpAdbClient.Models;
using ShieldCommander.Core.Models;
using ShieldCommander.Core.Services.Commands;

namespace ShieldCommander.Core.Tests.Commands;

public class InstallPackageCommandTests
{
    [Test]
    public async Task ToProgress_Preparing_IsTransferringAtZero()
    {
        var args = new InstallProgressEventArgs(PackageInstallProgressState.Preparing);

        var progress = InstallPackageCommand.ToProgress(args);

        await Assert.That(progress).IsEqualTo(new InstallProgress(InstallPhase.Transferring, 0));
    }

    [Test]
    public async Task ToProgress_Uploading_IsTransferringWithWholePercent()
    {
        var args = new InstallProgressEventArgs(0, 1, 42.7);

        var progress = InstallPackageCommand.ToProgress(args);

        await Assert.That(progress).IsEqualTo(new InstallProgress(InstallPhase.Transferring, 42));
    }

    [Test]
    [Arguments(PackageInstallProgressState.Installing)]
    [Arguments(PackageInstallProgressState.PostInstall)]
    public async Task ToProgress_PackageManagerWorking_IsInstallingWithoutPercent(PackageInstallProgressState state)
    {
        var args = new InstallProgressEventArgs(state);

        var progress = InstallPackageCommand.ToProgress(args);

        await Assert.That(progress).IsEqualTo(new InstallProgress(InstallPhase.Installing));
    }

    [Test]
    public async Task ToProgress_Finished_ReportsNothing()
    {
        var args = new InstallProgressEventArgs(PackageInstallProgressState.Finished);

        var progress = InstallPackageCommand.ToProgress(args);

        await Assert.That(progress).IsNull();
    }
}
