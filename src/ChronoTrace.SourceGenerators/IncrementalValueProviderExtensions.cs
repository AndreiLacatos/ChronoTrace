using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ChronoTrace.SourceGenerators;

internal static class IncrementalValueProviderExtensions
{
    internal static IncrementalValueProvider<(T, Logger)> EnrichWithLogger<T>(
        this IncrementalValueProvider<T> valueProvider,
        IncrementalValueProvider<Logger> loggerProvider)
    {
        return valueProvider.Combine(loggerProvider);
    }

    internal static IncrementalValuesProvider<(T, Logger)> EnrichWithLogger<T>(
        this IncrementalValuesProvider<T> valueProvider,
        IncrementalValueProvider<Logger> loggerProvider)
    {
        return valueProvider.Combine(loggerProvider);
    }

    internal static IncrementalValueProvider<Logger> CreateLoggerProvider(this IncrementalValueProvider<AnalyzerConfigOptionsProvider> valueProvider)
    {
        return valueProvider.Select((provider, _) =>
        {
            provider.GlobalOptions.TryGetValue("build_property.SolutionDir", out var solutionDir);
            provider.GlobalOptions.TryGetValue("build_property.ChronoTraceGeneratorsLogLevel", out var loglevel);
            var logger = new Logger(solutionDir ?? string.Empty, loglevel ?? string.Empty);
            logger.Info("Source generation started");
            return logger;
        });
    }
}