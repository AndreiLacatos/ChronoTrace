using ChronoTrace.ProfilingInternals.Compat;

namespace ChronoTrace.SourceGenerators
{
    internal sealed class GeneratorDependencies
    {
        internal ITimeProvider TimeProvider { get; set; } = new TimeProvider();

        internal static GeneratorDependencies Default => new GeneratorDependencies();
    }
}
