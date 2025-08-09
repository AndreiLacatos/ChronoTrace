namespace ChronoTrace.ProfilingInternals.DataExport.Json;

/// <summary>
/// Provides static methods to map a CallGraph object to a TimingReport object.
/// </summary>
internal static class TimingReportMapper
{
    /// <summary>
    /// Maps a CallGraph collection to a TimingReport.
    /// </summary>
    /// <param name="callGraphs">The source CallGraph collection to transform.</param>
    /// <returns>A new TimingReport object with the equivalent hierarchy.</returns>
    internal static TimingReport Map(IEnumerable<CallGraph> callGraphs)
    {
        return new TimingReport
        {
            MethodTimings = callGraphs.Select(c => MapNode(c.Root)).ToList(),
        };
    }

    /// <summary>
    /// Recursively maps a source CallGraph.Node to a destination TimingReport.MethodTiming.
    /// </summary>
    /// <param name="sourceNode">The source node to map.</param>
    /// <returns>The newly created destination node.</returns>
    private static TimingReport.MethodTiming MapNode(CallGraph.Node sourceNode)
    {
        return new TimingReport.MethodTiming
        {
            MethodName = sourceNode.MethodName,
            ExecutionTime = sourceNode.ExecutionTime,
            CallGraph = sourceNode.Nodes.Select(MapNode).ToList(),
        };
    }
}
