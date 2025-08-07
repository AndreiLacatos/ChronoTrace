using System.Collections.Immutable;
using ChronoTrace.SourceGenerators.DataStructures;
using Microsoft.CodeAnalysis;

namespace ChronoTrace.SourceGenerators.IncrementalValueProviderExtensions;

/// <summary>
/// Utility functions for filtering relevant data from a large pool.
/// </summary>
internal static class IncrementalGeneratorFilterExtensions
{
    /// <summary>
    /// Selects only those method invocations that are attributed to a tracked method.
    /// </summary>
    /// <param name="allInvocations">Pool of all method invocations</param>
    /// <param name="attributedMethods">Pool of tracked methods</param>
    /// <returns>List of relevant method invocations</returns>
    internal static IncrementalValuesProvider<MethodInvocation> FilterTrackedMethodInvocations(
        this IncrementalValuesProvider<MethodInvocation> allInvocations,
        IncrementalValueProvider<ImmutableHashSet<ISymbol>> attributedMethods)
    {
        return attributedMethods
            .Combine(allInvocations.Collect())
            .SelectMany((tuple, _) =>
            {
                var (attributedSet, invocationSet) = tuple;
                var relevantInvocations = new List<MethodInvocation>();
                foreach (var invInfo in invocationSet)
                {
                    if (attributedSet.Contains(invInfo.TargetMethod))
                    {
                        relevantInvocations.Add(invInfo);
                    }
                }
                return relevantInvocations;
            });
    }
}
