namespace ChronoTrace.ProfilingInternals;

internal class ProfilingContextAccessor
{
    private static readonly AsyncLocal<ProfilingContext?> Context = new AsyncLocal<ProfilingContext?>();

    internal static ProfilingContext Current
    {
        get
        {
            Context.Value ??= new ProfilingContext();
            return Context.Value;
        }
    }
}
