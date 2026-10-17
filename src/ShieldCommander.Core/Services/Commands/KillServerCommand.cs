using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Commands;

internal sealed class KillServerCommand : IAdbCommand
{
    public string Name => "KillServer";

    public async Task<AdbResult> ExecuteAsync(IAdbRunner runner)
    {
        return await runner.RunServerAsync(async (client, ct) =>
        {
            await client.KillAdbAsync(ct);
            return string.Empty;
        });
    }
}
