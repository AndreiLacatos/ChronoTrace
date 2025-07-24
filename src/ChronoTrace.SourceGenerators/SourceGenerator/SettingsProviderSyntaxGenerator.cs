namespace ChronoTrace.SourceGenerators.SourceGenerator;

/// <summary>
/// An internal syntax generator responsible for creating the C# source code
/// for a static class that initializes <see cref="ChronoTrace.ProfilingInternals.Settings.ProfilingSettings"/>
/// via a <see cref="System.Runtime.CompilerServices.ModuleInitializerAttribute"/>.
/// </summary>
internal sealed class SettingsProviderSyntaxGenerator
{
    internal enum TraceOutput
    {
        Stdout,
        Json,
    }

    /// <summary>
    /// Defines trace data export configuration
    /// </summary>
    /// <param name="Output">Whether to output traces to stdout</param>
    /// <param name="OutputPath">Desired file output path</param>
    internal sealed record ExportSettings(TraceOutput Output, string? OutputPath);
    private readonly string _version;

    public SettingsProviderSyntaxGenerator(string version)
    {
        _version = version;
    }

    /// <summary>
    /// Generates the C# source code for the <c>ProfilingSettingsInitializer</c> class.
    /// </summary>
    /// <param name="exportSettings">Defines trace data export configuration.</param>
    /// <returns>A string containing the C# source code for the settings initializer class.</returns>
    internal string MakeSettingsProvider(ExportSettings exportSettings)
    {
        var exportSettingsDeclaration = exportSettings.Output switch
        {
            TraceOutput.Json => $$"""
                                  var dataExportSettings = new global::ChronoTrace.ProfilingInternals.Settings.DataExport.JsonExporterSettings
                                          {
                                              OutputPath = @"{{exportSettings.OutputPath}}",
                                          };
                                  """,
            _ => "var dataExportSettings = new global::ChronoTrace.ProfilingInternals.Settings.DataExport.StdoutExportSettings();",
        };

        return $$"""
               namespace ChronoTrace.ProfilingInternals.Settings;
               
               [global::System.CodeDom.Compiler.GeneratedCodeAttribute("[{{Constants.ChronoTrace}}]", "[{{_version}}]")]
               internal static class ProfilingSettingsInitializer
               {
                   [global::System.Runtime.CompilerServices.ModuleInitializer]
                   internal static void Initialize()
                   {
                       {{exportSettingsDeclaration}}
                       var settings = new global::ChronoTrace.ProfilingInternals.Settings.ProfilingSettings
                       {
                           DataExportSettings = dataExportSettings,
                       };
                       global::ChronoTrace.ProfilingInternals.Settings.ProfilingSettingsProvider.UpdateSettings(settings);
                   }
               }

               """;
    }
}
