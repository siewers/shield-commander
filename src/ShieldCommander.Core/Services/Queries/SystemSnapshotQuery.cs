using ShieldCommander.Core.Models;

namespace ShieldCommander.Core.Services.Queries;

public sealed class SystemSnapshotQuery : IAdbQuery<SystemSnapshot>
{
    private static readonly AdbBatchQueryCollection<DynamicSections> Queries = DynamicSections.CreateQueries();

    public async Task<SystemSnapshot> ExecuteAsync(IAdbRunner runner, CancellationToken ct)
    {
        var output = await runner.RunShellAsync(Queries.ToCombinedCommand(), ct);
        var sections = new DynamicSections();
        Queries.ApplyAll(output, sections);
        return new SystemSnapshot(sections.Cpu, sections.Memory.Snapshot, sections.Disk, sections.Network, sections.Thermal);
    }
}
