namespace ShieldCommander.Core.Services.Queries;

internal interface IAdbBatchQuery<in TTarget>
{
    string Name { get; }

    string CommandText { get; }

    void Apply(ReadOnlySpan<char> output, TTarget target);
}
