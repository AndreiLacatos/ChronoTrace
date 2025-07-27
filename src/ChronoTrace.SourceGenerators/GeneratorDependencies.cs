using ChronoTrace.SourceGenerators.Compat;

namespace ChronoTrace.SourceGenerators
{
    internal sealed class GeneratorDependencies
    {
        internal IBuildTimeProvider TimeProvider { get; set; } = new BuildTimeProvider();

        internal static GeneratorDependencies Default => new GeneratorDependencies();
    }
}
