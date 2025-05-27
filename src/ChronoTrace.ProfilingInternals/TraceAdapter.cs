using System.Diagnostics;
using Trace = ChronoTrace.ProfilingInternals.DataExport.Trace;

namespace ChronoTrace.ProfilingInternals;

internal static class TraceAdapter
{
    internal static Trace Adapt(ProfiledMethodInvocation invocation)
    {
        return new Trace
        {
            MethodName = invocation.MethodName,
            ExecutionTime = Stopwatch.GetElapsedTime(invocation.InvocationTick, invocation.ReturnTick!.Value),
        };
    }
}
