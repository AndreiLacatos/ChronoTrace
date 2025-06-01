namespace ChronoTrace.ProfilingInternals.DataExport;

/// <summary>
/// Encapsulate a single trace collected during runtime.
/// </summary>
internal readonly struct Trace
{
    /// <summary>
    /// Name of the traced method.
    /// </summary>
    internal required string MethodName { get; init; }

    /// <summary>
    /// Measured execution time.
    /// </summary>
    internal required TimeSpan ExecutionTime { get; init; }
}
