using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ChronoTrace.SourceGenerators;

[Generator]
public class InterceptorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var loggerProvider = context.AnalyzerConfigOptionsProvider.CreateLoggerProvider();

        var trackedMethodInvocations = GroupInvocations(
            SelectTrackedMethodInvocations(
                SelectMethodInvocations(context.SyntaxProvider),
                SelectAttributedMethods(context.SyntaxProvider)))
            .EnrichWithLogger(loggerProvider);

        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource(
                "ChronoTrace.CompilerUtilities.g.cs",
                """
                namespace System.Runtime.CompilerServices;

                [global::System.Diagnostics.Conditional("DEBUG")]
                [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
                #pragma warning disable CS9113 // ignore warning thrown for unread parameters
                public sealed class InterceptsLocationAttribute(int version, string data) : global::System.Attribute;
                #pragma warning restore CS9113
                """
            );
        });

        context.RegisterSourceOutput(trackedMethodInvocations, GenerateInterceptors);
    }

    private static IncrementalValueProvider<ImmutableHashSet<ISymbol>> SelectAttributedMethods(SyntaxValueProvider syntaxProvider)
    {
        return syntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: Constants.ProfileAttribute,
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, _) =>
                {
                    if (ctx.TargetSymbol is IMethodSymbol methodSymbol)
                    {
                        return methodSymbol;
                    }

                    return null;
                })
            .Where(static m => m is not null)
            .Select(static (m, _) => m!)
            .Select(ISymbol (m, _) => m.OriginalDefinition)
            .Collect()
            .Select((symbols, _) => symbols.ToImmutableHashSet(SymbolEqualityComparer.Default));
    }

    private static IncrementalValuesProvider<MethodInvocation> SelectMethodInvocations(SyntaxValueProvider syntaxProvider)
    {
        return syntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is InvocationExpressionSyntax,
                transform: static (ctx, ct) =>
                {
                    var invocationSyntax = (InvocationExpressionSyntax)ctx.Node;
                    var symbolInfo = ctx.SemanticModel.GetSymbolInfo(invocationSyntax, ct);

                    IMethodSymbol? targetMethodSymbol = null;
                    if (symbolInfo.Symbol is IMethodSymbol directSymbol)
                    {
                        targetMethodSymbol = directSymbol.OriginalDefinition;
                    }

                    if (targetMethodSymbol == null)
                    {
                        return null;
                    }

#pragma warning disable RSEXPERIMENTAL002 // / Experimental interceptable location API
                    if (ctx.SemanticModel.GetInterceptableLocation(invocationSyntax, ct) is { } location)
                    {
                        return new MethodInvocation(targetMethodSymbol, invocationSyntax.GetLocation(), location);
                    }
#pragma warning restore RSEXPERIMENTAL002

                    return null;
                })
            .Where(static x => x is not null)
            .Select((x, _) => x!);
    }

    private static IncrementalValuesProvider<MethodInvocation> SelectTrackedMethodInvocations(
        IncrementalValuesProvider<MethodInvocation> allInvocations,
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

    private static IncrementalValuesProvider<InterceptableMethodInvocations> GroupInvocations(
        IncrementalValuesProvider<MethodInvocation> allTrackedInvocations)
    {
        return allTrackedInvocations
            .Collect()
            .SelectMany((invocationsArray, _) => invocationsArray
                .GroupBy(inv => inv.TargetMethod, SymbolEqualityComparer.Default)
                .Select(group => new InterceptableMethodInvocations(
                    (IMethodSymbol)group.Key!,
                    group.Select(item => (item.Location, item.InterceptableLocation))))
            );
    }

    private static void GenerateInterceptors(
        SourceProductionContext context,
        (InterceptableMethodInvocations, Logger) enrichedInvocation)
    {
        var (interceptableInvocation, logger) = enrichedInvocation;
        var className = interceptableInvocation.TargetMethod.ContainingType.Name;
        var methodName = interceptableInvocation.TargetMethod.Name;
        logger.Info($"Generating interceptor for {className}.{methodName}");

        // add necessary using statements
        // add using System.Runtime.CompilerServices; required by InterceptsLocation attribute
        var compilationUnit = CompilationUnit()
            .WithUsings(
                SingletonList(
                    UsingDirective(
                        QualifiedName(
                            QualifiedName(
                                IdentifierName("System"),
                                IdentifierName("Runtime")),
                            IdentifierName("CompilerServices")))));

        // create class declaration
        var classDeclaration = ClassDeclaration($"{className}ProfilingInterceptorExtensions");

        // make it public & static
        classDeclaration = classDeclaration
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword)));

        // add a static method which handles target method interception
        // make its return tpe void
        // TODO: copy return type of the intercepted method
        var interceptorMethod = MethodDeclaration(
            PredefinedType(
                Token(SyntaxKind.VoidKeyword)),
            Identifier($"Intercept{methodName}"));

        // make the method public & static
        interceptorMethod = interceptorMethod.WithModifiers(TokenList(
                Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.StaticKeyword)));
        
        // add parameters, the first is always the extended type
        // "this SubjectClass subject"
        // TODO: add remaining parameters, inherited from the target method
        var fullyQualifiedClassName = interceptableInvocation
            .TargetMethod
            .ContainingType
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        interceptorMethod = interceptorMethod.WithParameterList(
            ParameterList(
                SeparatedList<ParameterSyntax>(
                    new SyntaxNodeOrToken[]
                    {
                        Parameter(Identifier("subject"))
                            .WithModifiers(
                                TokenList(
                                    Token(SyntaxKind.ThisKeyword)))
                            .WithType(
                                IdentifierName(fullyQualifiedClassName)),
                        /*Token(SyntaxKind.CommaToken),
                        Parameter(
                                Identifier("parameter"))
                            .WithType(
                                PredefinedType(
                                    Token(SyntaxKind.StringKeyword)))*/
                    })));
        
        // add the method body, which does the following:
        // 1. Console.WriteLine("Intercepted!");
        // 2. invokes the target method
        var consoleLogStatement = ExpressionStatement(
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Console"),
                        IdentifierName("WriteLine")))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal("Intercepted!")))))));

        // TODO: forward any parameters
        var proxyCall = ExpressionStatement(
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("subject"),
                        IdentifierName(methodName))));

        interceptorMethod = interceptorMethod.WithBody(Block(
            consoleLogStatement,
            proxyCall));
        
        // create an InterceptsLocation for each invocation location of the target method
        var attributes = new List<AttributeListSyntax>();
        foreach (var location in interceptableInvocation.Locations)
        {
            var attribute = Attribute(
                    IdentifierName("InterceptsLocation"))
                .WithArgumentList(
                    AttributeArgumentList(
                        SeparatedList<AttributeArgumentSyntax>(
                            new SyntaxNodeOrToken[]
                            {
                                AttributeArgument(
                                        LiteralExpression(
                                            SyntaxKind.NumericLiteralExpression,
                                            Literal(location.InterceptableLocation.Version)))
                                    .WithNameColon(
                                        NameColon(
                                            IdentifierName(
                                                "version"))),
                                Token(SyntaxKind.CommaToken),
                                AttributeArgument(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal(location.InterceptableLocation.Data)))
                                    .WithNameColon(
                                        NameColon(
                                            IdentifierName(
                                                "data"))),
                            })));
            attributes.Add(
                AttributeList(
                    SingletonSeparatedList(attribute)
                )
            );
        }

        // use the attributes to annotate the interceptor method
        interceptorMethod = interceptorMethod.WithAttributeLists(List(attributes));

        // add the method to the class
        classDeclaration = classDeclaration.WithMembers(
            SingletonList<MemberDeclarationSyntax>(interceptorMethod));

        // create a file scoped namespace declaration
        var namespaceDeclaration = FileScopedNamespaceDeclaration(IdentifierName("ProfilingInterceptors"));

        // add the namespace & the class
        compilationUnit = compilationUnit
            .WithMembers(SingletonList<MemberDeclarationSyntax>(
                namespaceDeclaration.WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration))));

        var generatedSources = compilationUnit
            .NormalizeWhitespace()
            .GetText(Encoding.UTF8);

        logger.Flush();
        context.AddSource($"{className}_{methodName}_ProfilingInterceptors.g.cs", generatedSources);
    }
}
