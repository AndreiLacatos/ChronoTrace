using System.Diagnostics;

namespace ChronoTrace.ProfilingInternals;

internal sealed class ProfiledMethodInvocation
{
    internal required ushort Id { get; init; }
    internal required string MethodName { get; init; }
    internal long InvocationTick  { get; private init; }
    internal long? ReturnTick { get; set; }

    internal ProfiledMethodInvocation()
    {
        InvocationTick = Stopwatch.GetTimestamp();
    }
}
