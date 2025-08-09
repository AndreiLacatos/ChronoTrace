namespace ChronoTrace.ProfilingInternals.DataExport;

/// <summary>
/// Represents the complete, hierarchical call graph constructed from a collection of traces.
/// This class serves as an intermediate representation before final serialization.
/// </summary>
internal sealed class CallGraph
{
    /// <summary>
    /// Represents a single node within the call graph, corresponding to one
    /// instrumented method call and its direct descendants.
    /// </summary>
    internal sealed class Node
    {
        /// <summary>
        /// Gets the name of the traced method.
        /// </summary>
        internal required string MethodName { get; init; }

        /// <summary>
        /// Gets the measured execution time of the method call.
        /// </summary>
        internal required TimeSpan ExecutionTime { get; init; }

        /// <summary>
        /// Gets the collection of child nodes that were directly invoked by this method.
        /// </summary>
        internal required IEnumerable<Node> Nodes { get; init; }
    }

    /// <summary>
    /// Gets the root node of the call graph, representing the entry point of the
    /// traced execution path.
    /// </summary>
    internal required Node Root { get; init; }
}
