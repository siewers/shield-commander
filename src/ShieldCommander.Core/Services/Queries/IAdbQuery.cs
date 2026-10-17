namespace ShieldCommander.Core.Services.Queries;

public interface IAdbQuery<T>
{
    Task<T> ExecuteAsync(IAdbRunner runner, CancellationToken ct);
}
