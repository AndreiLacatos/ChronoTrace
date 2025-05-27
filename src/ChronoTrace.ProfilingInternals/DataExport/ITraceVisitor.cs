namespace ChronoTrace.ProfilingInternals.DataExport;

internal interface ITraceVisitor
{
    void BeginVisit();
    void VisitTrace(Trace trace);
    void Complete();
}
