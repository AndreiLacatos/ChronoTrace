using ChronoTrace.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Time.Testing;

namespace ChronoTrace.SourceGenerators.Tests;

internal static class SourceGenerationRunner
{
    internal static GeneratorDriver Run(string source, AnalyzerConfigOptionsProvider? optionsProvider = null)
    {
        // create a syntax tree from the input source code
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // list the assemblies that the source code references, add system base dll 
        // and the dll containing the definition of ProfileAttribute
        IEnumerable<PortableExecutableReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ProfileAttribute).Assembly.Location),
        ];

        // create a Roslyn compilation unit for the syntax tree
        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: [syntaxTree],
            references: references);

        // create an instance of the interceptor source generator
        var generator = new InterceptorGenerator();

        // configure source generator external dependencies
        generator.ConfigureDependencies(new GeneratorDependencies
        {
            TimeProvider = new FakeTimeProvider(new DateTimeOffset(2020, 04, 29, 13, 17, 19, TimeSpan.Zero)),
        });

        // run the generator against the compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        if (optionsProvider is not null)
        {
            driver = driver.WithUpdatedAnalyzerConfigOptions(optionsProvider);
        }
        return driver.RunGenerators(compilation);
    }
}
