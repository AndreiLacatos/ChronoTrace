using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ChronoTrace.SourceGenerators.DataStructures;

#pragma warning disable RSEXPERIMENTAL002
/// <summary>
/// Represents a single, specific invocation of a method that is a candidate for interception.
/// This record captures the target method, its invocation site, and the data required
/// for an <c>[InterceptsLocation]</c> attribute to target this specific call.
/// Used during code analysis, collection of such items are converted to <see cref="InterceptableMethodInvocations"/>
/// objects before passed to code generation.
/// </summary>
internal sealed record MethodInvocation(
    IMethodSymbol TargetMethod,
    Location Location,
    InterceptableLocation InterceptableLocation,
    MethodMetadata Metadata);

#pragma warning restore RSEXPERIMENTAL002
