using System.Collections.Immutable;
using ChronoTrace.ProfilingInternals.Settings;
using ChronoTrace.SourceGenerators.DataStructures;
using ChronoTrace.SourceGenerators.SourceGenerator.NameProviders;
using Microsoft.CodeAnalysis;

namespace ChronoTrace.SourceGenerators.SourceGenerator;

/// <summary>
/// Provides methods for generating source code for the <c>ChronoTrace</c> library.
/// </summary>
internal sealed class TraceGenerator
{
    private readonly GeneratorDependencies _dependencies;

    public TraceGenerator(GeneratorDependencies dependencies)
    {
        _dependencies = dependencies;
    }

    /// <summary>
    /// Generates the <c>[InterceptsLocationAttribute]</c>
    /// </summary>
    /// <param name="ctx">Current <c>IncrementalGeneratorPostInitializationContext</c></param>
    /// <param name="version">Library version</param>
    internal void GenerateInterceptsLocationAttribute(
        SourceProductionContext ctx,
        string version)
    {
        ctx.AddSource(
            "ChronoTrace.CompilerUtilities.g.cs",
            new SourceGeneratorUtilities(_dependencies.TimeProvider, version).MakeInterceptsLocationAttribute()
        );
    }

    /// <summary>
    /// Generates the profiling interceptors for methods subject to interception. Receives the list of method
    /// invocations grouped by their parent class
    /// </summary>
    /// <param name="context">Current <c>SourceProductionContext</c></param>
    /// <param name="props">Tuple of the list of method invocations (grouped by their class) and the library version</param>
    internal void GenerateInterceptors(
        SourceProductionContext context,
        (InterceptableClassMethods Left, string Right) props)
    {
        var (interceptableInvocation, version) = props;
        var generatedSources = new InterceptorSyntaxGenerator()
            .MakeMethodInterceptors(interceptableInvocation);
        context.AddSource(
            new GeneratedSourceFileNameProvider().GetHintName(interceptableInvocation.InterceptableInvocations.First().TargetMethod),
            new SourceGeneratorUtilities(_dependencies.TimeProvider, version).FormatCompilationUnitSyntax(generatedSources));
    }

    /// <summary>
    /// Generates the source files for configuring library settings.
    /// </summary>
    /// <param name="context">Current <c>SourceProductionContext</c></param>
    /// <param name="props">Tuple consisting of desired setting value for the trace output path and the library version</param>
    internal void GenerateSettingsProvider(
        SourceProductionContext context,
        (string? Left, string Right) props)
    {
        var (outputPath, version) = props;
        var settings = new SettingsProviderSyntaxGenerator.ExportSettings(
            outputPath?.Trim().Equals("stdout", StringComparison.InvariantCultureIgnoreCase) ?? false 
                ? SettingsProviderSyntaxGenerator.TraceOutput.Stdout
                : SettingsProviderSyntaxGenerator.TraceOutput.Json,
            outputPath);
        var generatedSources = new SettingsProviderSyntaxGenerator(version)
            .MakeSettingsProvider(settings);
        context.AddSource(
            $"{nameof(ProfilingSettingsProvider)}.g.cs",
            new SourceGeneratorUtilities(_dependencies.TimeProvider, version).FormatSourceCode(generatedSources));
    }
}