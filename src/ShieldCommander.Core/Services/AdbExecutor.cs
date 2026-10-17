using ShieldCommander.Core.Models;
using ShieldCommander.Core.Services.Commands;
using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Services;

public sealed class AdbExecutor(IAdbRunner runner) : IAdbExecutor
{
    public Task<AdbResult> ExecuteAsync(IAdbCommand command, CancellationToken ct = default) => command.ExecuteAsync(runner, ct);

    public Task<T> QueryAsync<T>(IAdbQuery<T> query, CancellationToken ct = default) => query.ExecuteAsync(runner, ct);
}
