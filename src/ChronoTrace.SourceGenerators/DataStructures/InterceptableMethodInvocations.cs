using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ChronoTrace.SourceGenerators.DataStructures
{
    #pragma warning disable RSEXPERIMENTAL002
    /// <summary>
    /// Represents a target method that is subject to interception, along with all
    /// the specific locations in source code where its invocations are intercepted.
    /// This record is used by the source generator to gather information
    /// needed for generating interceptor methods.
    /// </summary>
    internal sealed class InterceptableMethodInvocations
    {
        internal IMethodSymbol TargetMethod { get; set; } = null!;
        internal IEnumerable<(Location InvocationLocation, InterceptableLocation InterceptableLocation)> Locations { get; set; } = null!;
        internal MethodMetadata Metadata { get; set; } = null!;
    }
    #pragma warning restore RSEXPERIMENTAL002
}
