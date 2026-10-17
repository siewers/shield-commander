using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Queries;

public sealed class SystemSnapshotQuery : IAdbQuery<SystemSnapshot>
{
    private static readonly AdbBatchQueryCollection<DynamicSections> Commands = DynamicSections.CreateCommands();

    public async Task<SystemSnapshot> ExecuteAsync(IAdbRunner runner, CancellationToken ct)
    {
        var output = await runner.RunShellAsync(Commands.ToCombinedCommand(), ct);
        var sections = new DynamicSections();
        Commands.ApplyAll(output, sections);
        return new SystemSnapshot(sections.Cpu, sections.Memory.Snapshot, sections.Disk, sections.Network, sections.Thermal);
    }
}
