using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ChronoTrace.SourceGenerators;

#pragma warning disable RSEXPERIMENTAL002
internal sealed record MethodInvocation(IMethodSymbol TargetMethod, Location Location, InterceptableLocation InterceptableLocation);
#pragma warning restore RSEXPERIMENTAL002
