using System.Collections.Immutable;
using ChronoTrace.Attributes;
using ChronoTrace.ProfilingInternals;
using ChronoTrace.SourceGenerators.Analyzers;
using ChronoTrace.SourceGenerators.Compat;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute;

namespace ChronoTrace.SourceGenerators.Tests;

internal static class SourceGenerationRunner
{
    internal static (GeneratorDriver runResult, ImmutableArray<Diagnostic> diagnostics) Run(
        string source,
        AnalyzerConfigOptionsProvider? optionsProvider = null)
    {
        // create a syntax tree from the input source code
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // list the assemblies that the source code references, add system base dll 
        // and the dll containing the definition of ProfileAttribute
        var references = Basic.Reference.Assemblies.NetStandard21.References.All.Union( 
        [
            MetadataReference.CreateFromFile(typeof(ProfileAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ProfilingContext).Assembly.Location),
        ]);

        // create a Roslyn compilation unit for the syntax tree
        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [syntaxTree],
            references: references);

        // create an instance of the interceptor source generator
        var generator = new InterceptorGenerator();

        // configure source generator external dependencies
        var timeProvider = Substitute.For<IBuildTimeProvider>();
        timeProvider.GetUtcNow().Returns(new DateTimeOffset(2020, 04, 29, 13, 17, 19, TimeSpan.Zero));
        generator.ConfigureDependencies(new GeneratorDependencies
        {
            TimeProvider = timeProvider,
        });

        // run the generator against the compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        if (optionsProvider is not null)
        {
            driver = driver.WithUpdatedAnalyzerConfigOptions(optionsProvider);
        }
        
        driver = driver.RunGenerators(compilation);
        var runResult = driver.GetRunResult();
        var compilationAfterGenerator = compilation.AddSyntaxTrees(runResult.GeneratedTrees);
        var analyzer = new InternalApiUsageGuard();

        var diagnostics = compilationAfterGenerator
            .WithAnalyzers([analyzer])
            .GetAnalyzerDiagnosticsAsync()
            .Result;

        return (driver, diagnostics);
    }
}
