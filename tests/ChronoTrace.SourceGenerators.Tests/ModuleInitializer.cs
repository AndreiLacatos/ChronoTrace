using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ChronoTrace.SourceGenerators.Tests;

public static partial class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.AddScrubber(sb =>
        {
            const string replacement = "$1TESTING$2)";
            var updated = GeneratedLocationHash().Replace(sb.ToString(), replacement);
            sb.Clear();
            sb.Append(updated);
        });
        VerifySourceGenerators.Initialize();
    }

    [GeneratedRegex("""(InterceptsLocationAttribute\(version: 1, data: ")[^"]+(")\)""")]
    private static partial Regex GeneratedLocationHash();
}