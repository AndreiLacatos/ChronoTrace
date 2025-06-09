using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ChronoTrace.SourceGenerators.Tests;


public class MockAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
    private class MockAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly IReadOnlyDictionary<string, string> _properties;

        public MockAnalyzerConfigOptions(IReadOnlyDictionary<string, string> properties)
        {
            _properties = properties;
        }

        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
        {
            return _properties.TryGetValue(key, out value);
        }
    }
    
    public MockAnalyzerConfigOptionsProvider(IDictionary<string, string> properties)
    {
        GlobalOptions = new MockAnalyzerConfigOptions(properties.AsReadOnly());
    }

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => GlobalOptions;

    public override AnalyzerConfigOptions GlobalOptions { get; }
}
