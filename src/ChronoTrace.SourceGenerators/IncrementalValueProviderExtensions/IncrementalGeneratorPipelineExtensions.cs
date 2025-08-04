using System.Collections.Immutable;
using ChronoTrace.SourceGenerators.DataStructures;
using Microsoft.CodeAnalysis;

namespace ChronoTrace.SourceGenerators.IncrementalValueProviderExtensions;

/// <summary>
/// Utilities for organizing simple flat lists of <see cref="MethodInvocation"/> into groups by different criteria.
/// </summary>
internal static class IncrementalGeneratorPipelineExtensions
{
    /// <summary>
    /// Receives the list of invocations of tracked methods. Groups invocations based on the method type
    /// </summary>
    /// <param name="allTrackedInvocations">All identified invocations subject to interception</param>
    /// <returns>List of invocations grouped by method</returns>
    internal static IncrementalValuesProvider<InterceptableMethodInvocations> GroupInvocationsByMethod(
        this IncrementalValuesProvider<MethodInvocation> allTrackedInvocations)
    {
        return allTrackedInvocations
            .Collect()
            .SelectMany((invocationsArray, _) => invocationsArray
                .GroupBy(inv => inv.TargetMethod, SymbolEqualityComparer.Default)
                .Select(group => new InterceptableMethodInvocations(
                    (IMethodSymbol)group.Key!,
                    group.Select(item => (item.Location, item.InterceptableLocation)),
                    group.First().Metadata))
            );
    }

    /// <summary>
    /// Receives the list of methods subject to interception (and their list of invocations). Makes groups (lists) of
    /// methods based on the parent class of the method.
    /// </summary>
    /// <param name="trackedInvocations">List of all method invocations</param>
    /// <returns>Returns a list of lists, where each nested list contains methods that are declared in the same class</returns>
    internal static IncrementalValuesProvider<ImmutableArray<InterceptableMethodInvocations>> GroupInvocationsByClass(
        this IncrementalValuesProvider<InterceptableMethodInvocations> trackedInvocations)
    {
        return trackedInvocations
            .Collect()
            .SelectMany((invocationsArray, _) =>
            {
                if (invocationsArray.IsEmpty)
                {
                    return Enumerable.Empty<IEnumerable<InterceptableMethodInvocations>>();
                }

                return invocationsArray.GroupBy(
                    invocation => invocation.TargetMethod.OriginalDefinition.ContainingType,
                    SymbolEqualityComparer.Default
                );
            })
            .Select(static (m, _) => m.ToImmutableArray())
            .Where(static m => !m.IsEmpty);
    }
}
