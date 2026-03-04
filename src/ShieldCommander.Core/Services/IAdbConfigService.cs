namespace ShieldCommander.Core.Services;

public interface IAdbConfigService
{
    string? CurrentPath { get; }
    string ResolveAdbPath();
    bool IsAdbAvailable(string? path);
    void SetAdbPath(string? path);
    string GetInstallInstructions();
}
