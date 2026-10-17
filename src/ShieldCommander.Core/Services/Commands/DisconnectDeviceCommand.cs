using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Commands;

public sealed class DisconnectDeviceCommand(string? address = null) : IAdbCommand
{
    public string Name => "DisconnectDevice";

    public async Task<AdbResult> ExecuteAsync(IAdbRunner runner, CancellationToken ct)
    {
        return await runner.RunServerAsync(async (client, token) =>
        {
            // Without an address the adb server disconnects every network device
            await client.ExecuteServerCommandAsync("host", $"disconnect:{address}", token);
            return string.Empty;
        }, ct);
    }
}
