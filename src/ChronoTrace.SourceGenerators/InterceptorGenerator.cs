using System.Collections.Immutable;
using ChronoTrace.SourceGenerators.CodeInspection;
using ChronoTrace.SourceGenerators.DataStructures;
using ChronoTrace.SourceGenerators.IncrementalValueProviderExtensions;
using ChronoTrace.SourceGenerators.SourceGenerator;
using Microsoft.CodeAnalysis;
using static ChronoTrace.SourceGenerators.SourceGenerationGuard;
using static ChronoTrace.SourceGenerators.CodeInspection.CallGraphInspector;

namespace ChronoTrace.SourceGenerators;

[Generator]
public class InterceptorGenerator : IIncrementalGenerator
{
    private GeneratorDependencies? _dependencies;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        ConfigureDependencies(GeneratorDependencies.Default);
        var outputPathProvider = context.AnalyzerConfigOptionsProvider.CreateTraceOutputPathProvider();
        var versionProvider = context.AnalyzerConfigOptionsProvider.CreateVersionProvider();
        var sourceGenerationToggleProvider = context.AnalyzerConfigOptionsProvider.CreateSourceGenerationToggleProvider();

        var attributedMethods = context.SyntaxProvider.FilterAttributedMethods();
        var adjacentMethodInvocations = attributedMethods
            .Combine(context.CompilationProvider)
            .Select((data, cancellationToken) => DiscoverAmbientMethods(data.Left, data.Right, cancellationToken));

        var trackedMethodInvocations = context.SyntaxProvider
            .FilterMethodInvocations()
            .FilterTrackedMethodInvocations(attributedMethods)
            .Collect()
            .Combine(adjacentMethodInvocations).Select((data, _) => data.Left.AddRange(data.Right))
            .SelectMany((x, _) => x)
            .GroupInvocationsByMethod()
            .GroupInvocationsByClass()
            .Combine(versionProvider);

        var generator = new TraceGenerator(_dependencies!);
        context.RegisterSourceOutput(
            versionProvider.WithSourceGenerationToggle(sourceGenerationToggleProvider),
            WhenSourceGenerationEnabled<string>(generator.GenerateInterceptsLocationAttribute));
        context.RegisterSourceOutput(
            outputPathProvider.Combine(versionProvider).WithSourceGenerationToggle(sourceGenerationToggleProvider),
            WhenSourceGenerationEnabled<(string? Left, string Right)>(generator.GenerateSettingsProvider));
        context.RegisterSourceOutput(
            trackedMethodInvocations.WithSourceGenerationToggle(sourceGenerationToggleProvider),
            WhenSourceGenerationEnabled<(ImmutableArray<InterceptableMethodInvocations> Left, string Right)>(
                generator.GenerateInterceptors));
    }

    internal void ConfigureDependencies(GeneratorDependencies dependencies)
    {
        _dependencies ??= dependencies;
    }
}
