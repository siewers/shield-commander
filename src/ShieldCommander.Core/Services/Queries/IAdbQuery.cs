namespace ShieldCommander.Core.Services.Queries;

internal interface IAdbQuery<T>
{
    Task<T> ExecuteAsync(AdbRunner runner);

    T Parse(string output);
}
