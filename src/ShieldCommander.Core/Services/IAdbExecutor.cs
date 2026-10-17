using ShieldCommander.Core.Models;
using ShieldCommander.Core.Services.Commands;
using ShieldCommander.Core.Services.Queries;

namespace ShieldCommander.Core.Services;

public interface IAdbExecutor
{
    Task<AdbResult> ExecuteAsync(IAdbCommand command, CancellationToken ct = default);

    Task<T> QueryAsync<T>(IAdbQuery<T> query, CancellationToken ct = default);
}
