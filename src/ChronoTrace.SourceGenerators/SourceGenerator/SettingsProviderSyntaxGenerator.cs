namespace ChronoTrace.SourceGenerators.SourceGenerator;

internal sealed class SettingsProviderSyntaxGenerator
{
    internal string MakeSettingsProvider(string? outputPath)
    {
        return $$"""
               namespace ChronoTrace.ProfilingInternals.Settings;
               
               [global::System.CodeDom.Compiler.GeneratedCodeAttribute("[YourGeneratorName]", "[GeneratorVersion]")]
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