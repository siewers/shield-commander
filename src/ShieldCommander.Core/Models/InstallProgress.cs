namespace ShieldCommander.Core.Models;

// Percent is only known while transferring; the package manager does not report install progress
public sealed record InstallProgress(InstallPhase Phase, int? Percent = null);
