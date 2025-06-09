namespace ChronoTrace.SourceGenerators;

internal sealed class GeneratorDependencies
{
    internal required TimeProvider TimeProvider { get; init; }
    
    internal static GeneratorDependencies Default => new GeneratorDependencies
    {
        TimeProvider = TimeProvider.System,
    };
}
