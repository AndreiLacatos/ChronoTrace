namespace ChronoTrace.ProfilingInternals;

public sealed class ProfilingContextAccessor
{
    private static readonly AsyncLocal<ProfilingContext?> Context = new AsyncLocal<ProfilingContext?>();

    public static ProfilingContext Current
    {
        get
        {
            Context.Value ??= new ProfilingContext();
            return Context.Value;
        }
    }
}
