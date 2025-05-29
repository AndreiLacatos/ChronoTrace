using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ChronoTrace.SourceGenerators.IncrementalValueProviderExtensions;

internal static class TraceOutputPathProviderExtensions
{
    internal static IncrementalValueProvider<string?> CreateTraceOutputPathProvider(
        this IncrementalValueProvider<AnalyzerConfigOptionsProvider> valueProvider)
    {
        return valueProvider.Select((provider, _) =>
        {
            provider.GlobalOptions.TryGetValue(Constants.BuildProperties.TimingOutputPath, out var outputPath);
            return outputPath;
        });
    }
}
