using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ChronoTrace.SourceGenerators.IncrementalValueProviderExtensions;

/// <summary>
/// Provides extension methods for integrating a <see cref="Logger"/> into the
/// Roslyn incremental source generation pipeline.
/// </summary>
/// <remarks>
/// <para>
///   IMPORTANT: FOR DEVELOPMENT AND DEBUGGING OF SOURCE GENERATORS ONLY.
/// </para>
/// <para>
///   These extensions are intended to facilitate logging during the development and debugging
///   of a source generator by providing easy access to the <see cref="Logger"/> instance.
///   The <see cref="Logger"/> itself is a <c>DEBUG</c>-only utility, and its logging
///   functionality is stripped in <c>RELEASE</c> builds.
/// </para>
/// </remarks>
internal static class LoggerProviderExtensions
{
    /// <summary>
    /// Combines an <see cref="IncrementalValueProvider{T}"/> with an <see cref="IncrementalValueProvider{Logger}"/>,
    /// making the logger instance available alongside the value.
    /// </summary>
    /// <typeparam name="T">The type of the value in the primary provider.</typeparam>
    /// <param name="valueProvider">The primary incremental value provider.</param>
    /// <param name="loggerProvider">The incremental value provider for the <see cref="Logger"/>.</param>
    /// <returns>
    /// An <see cref="IncrementalValueProvider{T}"/> yielding tuples of (<typeparamref name="T"/>, <see cref="Logger"/>).
    /// </returns>
    internal static IncrementalValueProvider<(T, Logger)> EnrichWithLogger<T>(
        this IncrementalValueProvider<T> valueProvider,
        IncrementalValueProvider<Logger> loggerProvider)
    {
        return valueProvider.Combine(loggerProvider);
    }

    /// <summary>
    /// Combines an <see cref="IncrementalValuesProvider{T}"/> with an <see cref="IncrementalValueProvider{Logger}"/>,
    /// making the logger instance available alongside each value in the collection.
    /// </summary>
    /// <typeparam name="T">The type of the values in the primary provider.</typeparam>
    /// <param name="valueProvider">The primary incremental values provider.</param>
    /// <param name="loggerProvider">The incremental value provider for the <see cref="Logger"/>.</param>
    /// <returns>
    /// An <see cref="IncrementalValuesProvider{T}"/> yielding tuples of (<typeparamref name="T"/>, <see cref="Logger"/>).
    /// </returns>
    internal static IncrementalValuesProvider<(T, Logger)> EnrichWithLogger<T>(
        this IncrementalValuesProvider<T> valueProvider,
        IncrementalValueProvider<Logger> loggerProvider)
    {
        return valueProvider.Combine(loggerProvider);
    }

    /// <summary>
    /// Creates an <see cref="IncrementalValueProvider{Logger}"/> by extracting necessary
    /// configuration from global analyzer config options.
    /// </summary>
    /// <param name="valueProvider">
    /// An <see cref="IncrementalValueProvider{AnalyzerConfigOptionsProvider}"/> that provides access to MSBuild properties.
    /// </param>
    /// <returns>
    /// An <see cref="IncrementalValueProvider{AnalyzerConfigOptionsProvider}"/> that provides a configured <see cref="Logger"/> instance.
    /// </returns>
    internal static IncrementalValueProvider<Logger> CreateLoggerProvider(
        this IncrementalValueProvider<AnalyzerConfigOptionsProvider> valueProvider)
    {
        return valueProvider.Select((provider, _) =>
        {
            provider.GlobalOptions.TryGetValue(Constants.BuildProperties.SolutionDir, out var solutionDir);
            provider.GlobalOptions.TryGetValue(Constants.BuildProperties.LogLevel, out var loglevel);
            var logger = new Logger(solutionDir ?? string.Empty, loglevel ?? string.Empty);
            logger.Info("Source generation started");
            return logger;
        });
    }
}
