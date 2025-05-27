using System.Diagnostics;
using ChronoTrace.ProfilingInternals.DataExport;

namespace ChronoTrace.ProfilingInternals;

public sealed class ProfilingContext
{
    private readonly SemaphoreSlim _semaphore;
    private readonly List<ProfiledMethodInvocation> _methodCalls;
    private readonly ITraceVisitor _visitor;
    private ushort _invocationCounter;
    private ushort _pendingCalls;

    internal ProfilingContext(ITraceVisitor visitor)
    {
        _visitor = visitor;
        _invocationCounter = 0;
        _pendingCalls = 0;
        _methodCalls = new List<ProfiledMethodInvocation>(capacity: 100);
        _semaphore = new SemaphoreSlim(1, 1);
    }

    public ushort BeginMethodProfiling(string methodName)
    {
        _semaphore.Wait();
        ++_pendingCalls;
        var invocationId = ++_invocationCounter;
        var pendingCall = new ProfiledMethodInvocation
        {
            Id = invocationId,
            MethodName = methodName,
        };

        _methodCalls.Add(pendingCall);
        _semaphore.Release();

        return invocationId;
    }

    public void EndMethodProfiling(ushort invocationId)
    {
        var currentTicks = Stopwatch.GetTimestamp();
        _semaphore.Wait();
        if (invocationId == 0 || invocationId > _invocationCounter)
        {
            // invalid invocationId
            _semaphore.Release();
            return;
        }

        --_pendingCalls;
        _methodCalls[invocationId - 1].ReturnTick = currentTicks;
        _semaphore.Release();
    }

    public void CollectTraces()
    {
        _semaphore.Wait();
        var hasPendingCalls = _pendingCalls > 0;
        _semaphore.Release();
        if (hasPendingCalls)
        {
            // current profiling scope is not finished yet, there are profiled methods in execution
            return;
        }

        _visitor.BeginVisit();
        for (var i = 0; i < _invocationCounter; i++)
        {
            _visitor.VisitTrace(TraceAdapter.Adapt(_methodCalls[i]));
        }
        _visitor.Complete();
        
        // reset state
        _invocationCounter = 0;
        _methodCalls.Clear();
    }
}
