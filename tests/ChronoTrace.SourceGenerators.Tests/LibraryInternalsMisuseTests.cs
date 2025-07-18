using Microsoft.CodeAnalysis;
using Shouldly;

namespace ChronoTrace.SourceGenerators.Tests;

public class LibraryInternalsMisuseTests
{
    [Fact]
    public void UsageOfProfilinSettings_ShouldGenerateCompilerWarnings()
    {
        var source = 
            """
            using ChronoTrace.ProfilingInternals.Settings;
            
            var d = ProfilingSettings.Default;
            ProfilingSettingsProvider.UpdateSettings(new ProfilingSettings{ OutputPath = "" });
            """;

        var (_, diagnostics) = SourceGenerationRunner.Run(source, new MockAnalyzerConfigOptionsProvider());

        diagnostics.Length.ShouldBe(3);
        diagnostics.ShouldContain(d =>
            d.Severity == DiagnosticSeverity.Warning &&
            d.GetMessage(null).StartsWith("'ProfilingSettings.Default' is for internal use")
        );
        diagnostics.ShouldContain(d =>
            d.Severity == DiagnosticSeverity.Warning &&
            d.GetMessage(null).StartsWith("'ProfilingSettingsProvider.UpdateSettings' is for internal use")
        );
        diagnostics.ShouldContain(d =>
            d.Severity == DiagnosticSeverity.Warning &&
            d.GetMessage(null).StartsWith("'ProfilingSettings' is for internal use")
        );
    }

    [Fact]
    public void AccessingProfilingContext_ShouldGenerateCompilerWarnings()
    {
        var source = 
            """
            using ChronoTrace.ProfilingInternals;
            
            var c = ProfilingContextAccessor.Current;
            """;

        var (_, diagnostics) = SourceGenerationRunner.Run(source, new MockAnalyzerConfigOptionsProvider());

        diagnostics.Length.ShouldBe(1);
        diagnostics.ShouldContain(d =>
            d.Severity == DiagnosticSeverity.Warning &&
            d.GetMessage(null).StartsWith("'ProfilingContextAccessor.Current' is for internal use")
        );
    }

    [Fact]
    public void ProfilingContextInteractions_ShouldGenerateCompilerWarnings()
    {
        var source = 
            """
            using ChronoTrace.ProfilingInternals;
                        
            var c = ProfilingContextAccessor.Current;
            var id = c.BeginMethodProfiling("");
            c.EndMethodProfiling(id);
            c.CollectTraces();
            """;

        var (_, diagnostics) = SourceGenerationRunner.Run(source, new MockAnalyzerConfigOptionsProvider());

        diagnostics.Length.ShouldBe(4);
        diagnostics.ShouldContain(d =>
            d.Severity == DiagnosticSeverity.Warning &&
            d.GetMessage(null).StartsWith("'ProfilingContextAccessor.Current' is for internal use")
        );
        diagnostics.ShouldContain(d =>
            d.Severity == DiagnosticSeverity.Warning &&
            d.GetMessage(null).StartsWith("'ProfilingContext.BeginMethodProfiling' is for internal use")
        );
        diagnostics.ShouldContain(d =>
            d.Severity == DiagnosticSeverity.Warning &&
            d.GetMessage(null).StartsWith("'ProfilingContext.EndMethodProfiling' is for internal use")
        );
        diagnostics.ShouldContain(d =>
            d.Severity == DiagnosticSeverity.Warning &&
            d.GetMessage(null).StartsWith("'ProfilingContext.CollectTraces' is for internal use")
        );
    }
}
