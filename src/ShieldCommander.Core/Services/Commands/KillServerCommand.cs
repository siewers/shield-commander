using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Commands;

internal sealed class KillServerCommand : IAdbCommand
{
    public string Name => "KillServer";

    public async Task<AdbResult> ExecuteAsync(IAdbRunner runner, CancellationToken ct)
    {
        return await runner.RunServerAsync(async (client, token) =>
        {
            await client.KillAdbAsync(token);
            return string.Empty;
        }, ct);
    }
}
