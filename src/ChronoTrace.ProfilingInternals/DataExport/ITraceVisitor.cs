namespace ChronoTrace.ProfilingInternals.DataExport;

/// <summary>
/// Defines a contract for components that process captured trace data.
/// Implementers of this interface can perform specific actions on traces as they are iterated over.
/// </summary>
internal interface ITraceVisitor
{
    /// <summary>
    /// Invoked once before any individual traces are visited.
    /// Allows the visitor to perform any necessary initialization or setup.
    /// </summary>
    void BeginVisit();

    /// <summary>
    /// Invoked for each <see cref="Trace"/> object encountered.
    /// This method is responsible for processing the individual trace data.
    /// </summary>
    /// <param name="trace">The <see cref="Trace"/> object currently being visited.</param>
    void VisitTrace(Trace trace);

    /// <summary>
    /// Invoked once after all traces have been visited.
    /// Allows the visitor to perform any finalization, cleanup, or aggregation of results.
    /// </summary>
    void Complete();
}
