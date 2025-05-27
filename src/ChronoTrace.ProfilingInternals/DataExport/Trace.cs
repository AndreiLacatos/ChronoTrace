namespace ChronoTrace.ProfilingInternals.DataExport;

internal readonly struct Trace
{
    internal required string MethodName { get; init; }
    internal required TimeSpan ExecutionTime { get; init; }
}
