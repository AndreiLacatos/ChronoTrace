using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ChronoTrace.SourceGenerators.DataStructures;
#pragma warning disable RSEXPERIMENTAL002
internal sealed record InterceptableMethods(
    IMethodSymbol TargetMethod,
    MethodMetadata Methometadata,
    IEnumerable<IGrouping<string?, InterceptableLocation>> Invocations);

internal sealed record InterceptableClassMethods(
    string ClassName,
    IEnumerable<InterceptableMethods> InterceptableInvocations);
#pragma warning restore RSEXPERIMENTAL002

