namespace ChronoTrace.SourceGenerators.SourceGenerator;

/// <summary>
/// An internal syntax generator responsible for creating the C# source code
/// for a static class that initializes <see cref="ChronoTrace.ProfilingInternals.Settings.ProfilingSettings"/>
/// via a <see cref="System.Runtime.CompilerServices.ModuleInitializerAttribute"/>.
/// </summary>
internal sealed class SettingsProviderSyntaxGenerator
{
    /// <summary>
    /// Generates the C# source code for the <c>ProfilingSettingsInitializer</c> class.
    /// </summary>
    /// <param name="outputPath">
    /// The output path to be embedded into the generated code.
    /// </param>
    /// <returns>A string containing the C# source code for the settings initializer class.</returns>
    internal string MakeSettingsProvider(string? outputPath)
    {
        return $$"""
               namespace ChronoTrace.ProfilingInternals.Settings;
               
               [global::System.CodeDom.Compiler.GeneratedCodeAttribute("[{{Constants.ChronoTrace}}]", "[0.0.1-prealpha]")]
               internal static class ProfilingSettingsInitializer
               {
                   [global::System.Runtime.CompilerServices.ModuleInitializer]
                   internal static void Initialize()
                   {
                       var settings = new global::ChronoTrace.ProfilingInternals.Settings.ProfilingSettings
                       {
                           OutputPath = @"{{outputPath}}",
                       };
                       global::ChronoTrace.ProfilingInternals.Settings.ProfilingSettingsProvider.UpdateSettings(settings);
                   }
               }

               """;
    }
}
