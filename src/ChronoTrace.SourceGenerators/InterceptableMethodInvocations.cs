using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ChronoTrace.SourceGenerators;

#pragma warning disable RSEXPERIMENTAL002
internal sealed record InterceptableMethodInvocations(
    IMethodSymbol TargetMethod,
    IEnumerable<(Location InvocationLocation, InterceptableLocation InterceptableLocation)> Locations);
#pragma warning restore RSEXPERIMENTAL002
