using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Commands;

internal sealed class ConnectDeviceCommand(string ipAddress, int port) : IAdbCommand
{
    // The adb server waits for the OS TCP timeout (about 75 seconds on macOS) when the host does not answer
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    public string Name => "ConnectDevice";

    public async Task<AdbResult> ExecuteAsync(IAdbRunner runner)
    {
        using var cts = new CancellationTokenSource(Timeout);

        try
        {
            return await runner.RunServerAsync((client, ct) => client.ConnectAsync(ipAddress, port, ct), cts.Token);
        }
        catch (OperationCanceledException)
        {
            return new AdbResult(false, "", $"Timed out connecting to {ipAddress}:{port}");
        }
    }
}
