namespace ShieldCommander.Core.Services;

public sealed class AdbConfigService : IAdbConfigService
{
    private readonly IAdbPathProvider _pathProvider;
    private readonly IAdbPathResolver _pathResolver;
    private readonly ISettingsService _settings;

    public AdbConfigService(
        IAdbPathProvider pathProvider,
        IAdbPathResolver pathResolver,
        ISettingsService settings)
    {
        _pathProvider = pathProvider;
        _pathResolver = pathResolver;
        _settings = settings;

        // Initialize the path provider with the saved or auto-detected path
        pathProvider.CurrentPath = settings.AdbPath ?? pathResolver.FindAdb();
    }

    public string? CurrentPath => string.IsNullOrWhiteSpace(_settings.AdbPath) ? null : _pathProvider.CurrentPath;

    public string ResolveAdbPath() => _pathResolver.FindAdb();

    public bool IsAdbAvailable(string? path) => _pathResolver.IsAvailable(path ?? _pathProvider.CurrentPath);

    public void SetAdbPath(string? path)
    {
        _settings.AdbPath = path;
        _pathProvider.CurrentPath = path ?? _pathResolver.FindAdb();
    }

    public string GetInstallInstructions()
    {
        if (OperatingSystem.IsMacOS())
        {
            return "brew install android-platform-tools";
        }

        if (OperatingSystem.IsWindows())
        {
            return "winget install Google.PlatformTools";
        }

        if (OperatingSystem.IsLinux())
        {
            return "sudo apt install android-tools-adb";
        }

        return "Unsupported operating system";
    }
}
