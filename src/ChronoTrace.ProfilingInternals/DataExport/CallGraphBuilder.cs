namespace ChronoTrace.ProfilingInternals.DataExport;

/// <summary>
/// Encapsulates the logic for transforming a flat list of <see cref="Trace"/> objects
/// into a collection of hierarchical <see cref="CallGraph"/> structures.
/// </summary>
internal sealed class CallGraphBuilder
{
    /// <summary>
    /// Assembles one or more hierarchical call graphs from a flat list of traces.
    /// The method assumes the input list is sorted chronologically by invocation time.
    /// The construction algorithm works in reverse, building the call trees from the
    /// leaf nodes up to the roots.
    /// </summary>
    /// <param name="traces">A list of <see cref="Trace"/> objects, expected to be in
    /// the order they were invoked.</param>
    /// <returns>
    /// An enumeration of <see cref="CallGraph"/> objects, where each object represents a
    /// distinct call tree initiated by a root-level method (a trace with no caller).
    /// </returns>
    internal IEnumerable<CallGraph> AssembleCallGraph(List<Trace> traces)
    {
        // STEP 1: Initialization
        // This map holds completed child nodes waiting to be adopted by their parent.
        // Key: The parent's (caller's) name.
        // Value: A list of children nodes.
        var childMap = new Dictionary<string, List<CallGraph.Node>>();

        // This list will hold the final, top-level root nodes of the call graphs.
        var rootNodes = new List<CallGraph.Node>();

        // STEP 2: Iterate Backwards
        // We process the list from the last trace to the first. This "leaf-to-root"
        // approach ensures that by the time we process a method, all methods it called
        // (its children) have already been processed.
        for (var i = traces.Count - 1; i >= 0; i--)
        {
            var currentTrace = traces[i];

            // STEP 3: Find Children
            // Look for any children that have been processed and are waiting for this parent.
            // We use TryGetValue for efficiency.
            childMap.TryGetValue(currentTrace.MethodName, out var children);
            children ??= [];

            // STEP 4: Create the Node
            // Create the new hierarchical node for the current trace.
            var newNode = new CallGraph.Node
            {
                MethodName = currentTrace.MethodName,
                ExecutionTime = currentTrace.ExecutionTime,
                Nodes = children,
            };
            
            // Because children are added to their parent in reverse processing order,
            // we reverse the list here to restore the original invocation order.
            children.Reverse();

            // Crucially, we "consume" the children by removing them from the map.
            // This prevents them from being incorrectly adopted by another call to the
            // same method higher up the call stack (essential for handling recursion).
            childMap.Remove(currentTrace.MethodName);

            // STEP 5: Place the Node for Its Parent
            if (currentTrace.Caller == null)
            {
                // This is a root node (no caller), so add it to our list of roots.
                rootNodes.Add(newNode);
            }
            else
            {
                // This is a child node. Place it in the map for its parent to find.
                var caller = currentTrace.Caller;
                if (!childMap.TryGetValue(caller, out var value))
                {
                    value = [];
                    childMap[caller] = value;
                }

                value.Add(newNode);
            }
        }

        // To maintain the original order of root-level calls, we reverse the list
        // of roots at the very end.
        rootNodes.Reverse();

        // STEP 6: Final Assembly
        // Create the final report object with the collection of fully-formed trees.
        return rootNodes.Select(root => new CallGraph
        {
            Root = root,
        });
    }
}
