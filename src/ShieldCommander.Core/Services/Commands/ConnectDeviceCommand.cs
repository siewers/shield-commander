using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Commands;

internal sealed class ConnectDeviceCommand(string ipAddress, int port = 5555) : IAdbCommand
{
    // adb waits for the OS TCP timeout (about 75 seconds on macOS) when the host does not answer
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    public string Name => "ConnectDevice";

    public async Task<AdbResult> ExecuteAsync(IAdbRunner runner)
    {
        return await runner.RunAdbAsync($"connect {ipAddress}:{port}", Timeout);
    }
}
