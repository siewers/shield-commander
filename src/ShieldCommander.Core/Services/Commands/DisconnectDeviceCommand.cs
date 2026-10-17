using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Commands;

internal sealed class DisconnectDeviceCommand(string? address = null) : IAdbCommand
{
    public string Name => "DisconnectDevice";

    public async Task<AdbResult> ExecuteAsync(IAdbRunner runner)
    {
        return await runner.RunServerAsync(async (client, ct) =>
        {
            // Without an address the adb server disconnects every network device
            await client.ExecuteServerCommandAsync("host", $"disconnect:{address}", ct);
            return string.Empty;
        });
    }
}
