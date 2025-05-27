using ChronoTrace.ProfilingInternals;
using ChronoTrace.SourceGenerators.DataStructures;
using ChronoTrace.SourceGenerators.SourceGenerator.NameProviders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ChronoTrace.SourceGenerators.SourceGenerator;

internal class InterceptorSyntaxGenerator
{
    private const string ProfiledSubjectVariableName = "subject";
    private const string InvocationIdVariableName = "methodInvocationId";
    private const string ProfilingContextVariableName = "profilingContext";
    
    private readonly Logger _logger;
    private readonly GeneratedNamespaceProvider _generatedNamespaceProvider;
    private readonly InterceptorClassNameProvider _interceptorClassNameProvider;
    private readonly InterceptorHandlerNameProvider _interceptorHandlerNameProvider;
    private readonly GeneratedVariableNameConverter _variableNameConverter;

    internal InterceptorSyntaxGenerator(Logger logger)
    {
        _logger = logger;
        _generatedNamespaceProvider = new GeneratedNamespaceProvider();
        _interceptorClassNameProvider = new InterceptorClassNameProvider();
        _interceptorHandlerNameProvider = new InterceptorHandlerNameProvider();
        _variableNameConverter = new GeneratedVariableNameConverter();
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
                                IdentifierName(nameof(System)),
                                IdentifierName(nameof(System.Runtime))),
                            IdentifierName(nameof(System.Runtime.CompilerServices))))));

        // create class declaration
        var classDeclaration = MakeClassDeclaration(invocations);

        // add a static method which handles target method interception
        var interceptorMethod = MakeInterceptorHandler(invocations);

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
    
    private MethodDeclarationSyntax MakeInterceptorHandler(InterceptableMethodInvocations invocations)
    {
        // make its return type the same as the intercepted method
        var returnTypeSymbol = invocations.TargetMethod.ReturnType;
        TypeSyntax returnTypeSyntax;

        var symbolDisplayFormat = new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
        );

        if (invocations.TargetMethod.ReturnsVoid)
        {
            returnTypeSyntax = PredefinedType(Token(SyntaxKind.VoidKeyword));
        }
        else
        {
            var returnTypeName = returnTypeSymbol.ToDisplayString(symbolDisplayFormat);

            returnTypeSyntax = ParseTypeName(returnTypeName);
        }

        var interceptorMethod = MethodDeclaration(
            returnTypeSyntax,
            Identifier(_interceptorHandlerNameProvider.GetHandlerName(invocations.TargetMethod)));

        // make the method public & static
        interceptorMethod = interceptorMethod.WithModifiers(TokenList(
            Token(SyntaxKind.PublicKeyword),
            Token(SyntaxKind.StaticKeyword)));
        
        // add parameters, the first is always the extended type
        // "this SubjectClass subject", unless the intercepted method is static
        var parameters = new List<ParameterSyntax>();
        var syntaxNodeOrTokenList = new List<SyntaxNodeOrToken>();

        if (!invocations.TargetMethod.IsStatic)
        {
            // get the fully qualified name of the containing type, i.e. parent class, of the method being intercepted.
            var instanceTypeName = invocations.TargetMethod.ContainingType.ToDisplayString(symbolDisplayFormat);
            parameters.Add(
                Parameter(Identifier(_variableNameConverter.ToGeneratedVariableName(ProfiledSubjectVariableName)))
                    .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                    .WithType(ParseTypeName(instanceTypeName))
            );
        }

        // add remaining parameters inherited from the target method
        foreach (var originalParameter in invocations.TargetMethod.Parameters)
        {
            // convert original parameter's type symbol to TypeSyntax
            var paramTypeName = originalParameter.Type.ToDisplayString(symbolDisplayFormat);
            var paramTypeSyntax = ParseTypeName(paramTypeName);

            var parameterSyntax = Parameter(Identifier(originalParameter.Name))
                .WithType(paramTypeSyntax);

            // handle parameter modifiers (ref, out, in, params)
            var modifiers = new List<SyntaxToken>();
            switch (originalParameter.RefKind)
            {
                case RefKind.Ref:
                    modifiers.Add(Token(SyntaxKind.RefKeyword));
                    break;
                case RefKind.Out:
                    modifiers.Add(Token(SyntaxKind.OutKeyword));
                    break;
                case RefKind.In:
                    modifiers.Add(Token(SyntaxKind.InKeyword));
                    break;
            }

            if (originalParameter.IsParams)
            {
                modifiers.Add(Token(SyntaxKind.ParamsKeyword));
            }

            if (modifiers.Any())
            {
                parameterSyntax = parameterSyntax.WithModifiers(TokenList(modifiers));
            }

            parameters.Add(parameterSyntax);
        }

        // build a SeparatedList of parameters to pass to WithParameterList()
        for (var i = 0; i < parameters.Count; i++)
        {
            syntaxNodeOrTokenList.Add(parameters[i]);
            if (i < parameters.Count - 1)
            {
                syntaxNodeOrTokenList.Add(Token(SyntaxKind.CommaToken));
            }
        }
        
        interceptorMethod = interceptorMethod.WithParameterList(
            ParameterList(
                SeparatedList<ParameterSyntax>(syntaxNodeOrTokenList)
            )
        );

        // add the method body, which does the following:
        // 1. accesses the current profiling context
        var contextVariableDefinition = LocalDeclarationStatement(
            VariableDeclaration(
                    IdentifierName(
                        Identifier(
                            TriviaList(),
                            SyntaxKind.VarKeyword,
                            "var",
                            "var",
                            TriviaList())))
                .WithVariables(
                    SingletonSeparatedList(
                        VariableDeclarator(
                                Identifier(_variableNameConverter.ToGeneratedVariableName(ProfilingContextVariableName)))
                            .WithInitializer(
                                EqualsValueClause(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                AliasQualifiedName(
                                                    IdentifierName(
                                                        Token(SyntaxKind.GlobalKeyword)),
                                                    IdentifierName(nameof(ChronoTrace))),
                                                IdentifierName(nameof(ProfilingInternals))),
                                            IdentifierName(nameof(ProfilingContextAccessor))),
                                        IdentifierName(nameof(ProfilingContextAccessor.Current))))))));

        // 2. begins profiling the subject method
        var beginMethodProfilingCall = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(_variableNameConverter.ToGeneratedVariableName(ProfilingContextVariableName)),
                    IdentifierName(nameof(ProfilingContext.BeginMethodProfiling))))
            .WithArgumentList(
                ArgumentList(
                    SingletonSeparatedList(
                        Argument(
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(invocations.TargetMethod.Name))))));

        // 3. captures the returned invocation ID
        var methodProfilingStart = LocalDeclarationStatement(
            VariableDeclaration(
                    IdentifierName(
                        Identifier(
                            TriviaList(),
                            SyntaxKind.VarKeyword,
                            "var",
                            "var",
                            TriviaList())))
                .WithVariables(
                    SingletonSeparatedList(
                        VariableDeclarator(
                                Identifier(_variableNameConverter.ToGeneratedVariableName(InvocationIdVariableName)))
                            .WithInitializer(
                                EqualsValueClause(beginMethodProfilingCall)))));

        // 4. wraps the proxied method call in a try-finally block

        // make the proxy call, iterate through the parameters of the
        // intercepted method and pass them to it
        // create a list to hold the ArgumentSyntax nodes
        var arguments = new List<ArgumentSyntax>();
        foreach (var originalParamSymbol in invocations.TargetMethod.Parameters)
        {
            // create an argument using the parameter's name.
            var argument = Argument(IdentifierName(originalParamSymbol.Name));

            // handle parameter modifiers for the argument
            switch (originalParamSymbol.RefKind)
            {
                case RefKind.Ref:
                    argument = argument.WithRefKindKeyword(Token(SyntaxKind.RefKeyword));
                    break;
                case RefKind.Out:
                    argument = argument.WithRefKindKeyword(Token(SyntaxKind.OutKeyword));
                    break;
                case RefKind.In:
                    argument = argument.WithRefKindKeyword(Token(SyntaxKind.InKeyword));
                    break;
            }

            arguments.Add(argument);
        }

        // construct the InvocationExpression with the arguments
        var invocation = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(_variableNameConverter.ToGeneratedVariableName(ProfiledSubjectVariableName)),
                    IdentifierName(invocations.TargetMethod.Name)
                ))
            .WithArgumentList(
                ArgumentList(SeparatedList(arguments))
            );
        StatementSyntax proxyCall = invocations.TargetMethod.ReturnsVoid
            ? ExpressionStatement(invocation)
            : ReturnStatement(invocation);
        var tryProxyCallExecution = TryStatement()
            .WithBlock(Block(proxyCall));
        
        // 5. in the finally branch end the profiling of the subject method & collects traces
        var endMethodProfilingCall = ExpressionStatement(
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(_variableNameConverter.ToGeneratedVariableName(ProfilingContextVariableName)),
                        IdentifierName(nameof(ProfilingContext.EndMethodProfiling))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                IdentifierName(_variableNameConverter.ToGeneratedVariableName(InvocationIdVariableName)))))));
        var collectTracesCall = ExpressionStatement(
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(_variableNameConverter.ToGeneratedVariableName(ProfilingContextVariableName)),
                    IdentifierName(nameof(ProfilingContext.CollectTraces)))));
        tryProxyCallExecution = tryProxyCallExecution.WithFinally(
            FinallyClause(Block(
                endMethodProfilingCall,
                collectTracesCall)));

        interceptorMethod = interceptorMethod.WithBody(Block(
            contextVariableDefinition,
            methodProfilingStart,
            tryProxyCallExecution));
        return interceptorMethod;
    }

    private MethodDeclarationSyntax AddInterceptorAttributes(
        InterceptableMethodInvocations invocations,
        MethodDeclarationSyntax interceptorMethod)
    {
        // create an InterceptsLocation for each invocation location of the target method
        _logger.Debug($"Has {invocations.Locations.Count()} interceptable invocation(s)");
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