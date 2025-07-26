using ChronoTrace.ProfilingInternals.Compat;
using Trace = ChronoTrace.ProfilingInternals.DataExport.Trace;

namespace ChronoTrace.ProfilingInternals
{
    /// <summary>
    /// An internal utility class responsible for converting <see cref="ProfiledMethodInvocation"/>
    /// objects into <see cref="Trace"/> objects.
    /// </summary>
    internal static class TraceAdapter
    {
        internal static Trace Adapt(ProfiledMethodInvocation invocation)
        {
            return new Trace
            {
                MethodName = invocation.MethodName,
                ExecutionTime = StopwatchExtensions.GetElapsedTime(invocation.InvocationTick, invocation.ReturnTick.Value),
            };
        }
    }
}
