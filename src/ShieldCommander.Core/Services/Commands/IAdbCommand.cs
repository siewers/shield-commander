using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Commands;

public interface IAdbCommand
{
    string Name { get; }

    Task<AdbResult> ExecuteAsync(IAdbRunner runner, CancellationToken ct);
}
