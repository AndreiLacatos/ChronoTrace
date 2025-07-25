using System;
using ChronoTrace.SourceGenerators.IncrementalValueProviderExtensions;
using Microsoft.CodeAnalysis;

namespace ChronoTrace.SourceGenerators
{
    internal static class SourceGenerationGuard
    {
        internal static Action<SourceProductionContext, SourceGenerationToggleWrappedValue<TSource>> WhenSourceGenerationEnabled<TSource>(
            Action<SourceProductionContext, TSource> action)
        {
            return (spc, val) =>
            {
                if (val.SourceGenerationToggle is SourceGenerationDisabled)
                {
                    return;
                }

                action(spc, val.Value);
            };
        }
    }
}
