using ChronoTrace.SourceGenerators.DataStructures;
using ChronoTrace.SourceGenerators.SourceGenerator.NameProviders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SymbolDisplayFormat = Microsoft.CodeAnalysis.SymbolDisplayFormat;

namespace ChronoTrace.SourceGenerators.SourceGenerator;

internal class InterceptorSyntaxGenerator
{
    private readonly Logger _logger;
    private readonly GeneratedNamespaceProvider _generatedNamespaceProvider;
    private readonly InterceptorClassNameProvider _interceptorClassNameProvider;
    private readonly InterceptorHandlerNameProvider _interceptorHandlerNameProvider;

    internal InterceptorSyntaxGenerator(Logger logger)
    {
        _logger = logger;
        _generatedNamespaceProvider = new GeneratedNamespaceProvider();
        _interceptorClassNameProvider = new InterceptorClassNameProvider();
        _interceptorHandlerNameProvider = new InterceptorHandlerNameProvider();
    }
    
    internal CompilationUnitSyntax MakeMethodInterceptor(InterceptableMethodInvocations invocations)
    {
        var className = invocations.TargetMethod.ContainingType.Name;
        var methodName = invocations.TargetMethod.Name;
        _logger.Info($"Generating interceptor for {className}.{methodName}");

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
        var classDeclaration = MakeClassDeclaration(invocations);

        // add a static method which handles target method interception
        var interceptorMethod = MakeInterceptorHandler(invocations, methodName);

        // annotate it with InterceptsLocations attribute(s)
        interceptorMethod = AddInterceptorAttributes(invocations, interceptorMethod);

        // add the method to the class
        classDeclaration = classDeclaration.WithMembers(
            SingletonList<MemberDeclarationSyntax>(interceptorMethod));

        // create a file scoped namespace declaration
        var namespaceDeclaration = FileScopedNamespaceDeclaration(
            IdentifierName(_generatedNamespaceProvider.GetNamespace()));

        // add the namespace & the class
        compilationUnit = compilationUnit
            .WithMembers(SingletonList<MemberDeclarationSyntax>(
                namespaceDeclaration.WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration))));

        _logger.Flush();
        return compilationUnit;
    }

    private ClassDeclarationSyntax MakeClassDeclaration(InterceptableMethodInvocations invocations)
    {
        var classDeclaration = ClassDeclaration(_interceptorClassNameProvider.GetClassName(invocations.TargetMethod));

        // make it public & static
        classDeclaration = classDeclaration
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword)));
        return classDeclaration;
    }
    
    private MethodDeclarationSyntax MakeInterceptorHandler(
        InterceptableMethodInvocations invocations,
        string methodName)
    {
        // make its return type void
        // TODO: copy return type of the intercepted method
        var interceptorMethod = MethodDeclaration(
            PredefinedType(
                Token(SyntaxKind.VoidKeyword)),
            Identifier(_interceptorHandlerNameProvider.GetHandlerName(invocations.TargetMethod)));

        // make the method public & static
        interceptorMethod = interceptorMethod.WithModifiers(TokenList(
            Token(SyntaxKind.PublicKeyword),
            Token(SyntaxKind.StaticKeyword)));
        
        // add parameters, the first is always the extended type
        // "this SubjectClass subject"
        // TODO: add remaining parameters, inherited from the target method
        var fullyQualifiedClassName = invocations
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
        return interceptorMethod;
    }

    private MethodDeclarationSyntax AddInterceptorAttributes(
        InterceptableMethodInvocations invocations,
        MethodDeclarationSyntax interceptorMethod)
    {
        // create an InterceptsLocation for each invocation location of the target method
        _logger.Debug($"Has {invocations.Locations.Count()} interceptable invocations");
        var attributes = new List<AttributeListSyntax>();
        foreach (var (invocationLocation, interceptableLocation) in invocations.Locations)
        {
            _logger.Debug($"\tInvoked at {invocationLocation.SourceTree?.FilePath ?? "Unknown file"} " +
                $"{invocationLocation.GetLineSpan().Span}");
            var attribute = MakeInterceptsLocationAttribute(interceptableLocation);
            attributes.Add(
                AttributeList(
                    SingletonSeparatedList(attribute)
                )
            );
        }

        // use the attributes to annotate the interceptor method
        interceptorMethod = interceptorMethod.WithAttributeLists(List(attributes));
        return interceptorMethod;
    }

#pragma warning disable RSEXPERIMENTAL002
    private static AttributeSyntax MakeInterceptsLocationAttribute(InterceptableLocation location)
#pragma warning restore RSEXPERIMENTAL002
    {
        return Attribute(
                IdentifierName("global::System.Runtime.CompilerServices.InterceptsLocationAttribute"))
            .WithArgumentList(
                AttributeArgumentList(
                    SeparatedList<AttributeArgumentSyntax>(
                        new SyntaxNodeOrToken[]
                        {
                            AttributeArgument(
                                    LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        Literal(location.Version)))
                                .WithNameColon(
                                    NameColon(
                                        IdentifierName(
                                            "version"))),
                            Token(SyntaxKind.CommaToken),
                            AttributeArgument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(location.Data)))
                                .WithNameColon(
                                    NameColon(
                                        IdentifierName(
                                            "data"))),
                        })));
    }
}