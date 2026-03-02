namespace ShieldCommander.Core.Services;

internal sealed class ShellBatchRunner(AdbRunner runner)
{
    public async Task<TTarget> ExecuteAsync<TTarget>(AdbBatchQueryCollection<TTarget> commands)
        where TTarget : new()
    {
        var combinedCommand = commands.ToCombinedCommand();
        var output = await runner.RunShellAsync(combinedCommand);
        var result = new TTarget();
        commands.ApplyAll(output, result);
        return result;
    }
}
