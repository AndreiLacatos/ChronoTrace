namespace ChronoTrace.SourceGenerators.SourceGenerator
{
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
        internal sealed class ExportSettings
        {
            internal TraceOutput Output { get; set; }
            internal string? OutputPath { get; set; }
        }

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
            string exportSettingsDeclaration;

            if (exportSettings.Output == TraceOutput.Json)
            {
                exportSettingsDeclaration =
                    "var dataExportSettings = new global::ChronoTrace.ProfilingInternals.Settings.DataExport.JsonExporterSettings\n" +
                    "        {\n" +
                    $"            OutputPath = @\"{exportSettings.OutputPath}\",\n" +
                    "        };";
            }
            else
            {
                exportSettingsDeclaration =
                    "var dataExportSettings = new global::ChronoTrace.ProfilingInternals.Settings.DataExport.StdoutExportSettings();";
            }

            return
                $@"namespace ChronoTrace.ProfilingInternals.Settings;

[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""[{Constants.ChronoTrace}]"", ""[{_version}]"")]
internal static class ProfilingSettingsInitializer
{{
    [global::System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Initialize()
    {{
        {exportSettingsDeclaration}
        var settings = new global::ChronoTrace.ProfilingInternals.Settings.ProfilingSettings
        {{
            DataExportSettings = dataExportSettings,
        }};
        global::ChronoTrace.ProfilingInternals.Settings.ProfilingSettingsProvider.UpdateSettings(settings);
    }}
}}";
        }
    }
}
