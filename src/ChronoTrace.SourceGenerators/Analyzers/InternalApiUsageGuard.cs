using System.Collections.Immutable;
using ChronoTrace.ProfilingInternals.Protection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ChronoTrace.SourceGenerators.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InternalApiUsageGuard : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        "CT0001",
        "Internal API misuse",
        "'{0}' is for internal use by ChronoTrace and is not part of the public API",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description:
        "Some ChronoTrace internals must be publicly exposed due to technical reasons, but are not meant for  consumption and should not be used directly!");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeInvocation,
            SyntaxKind.InvocationExpression,
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxKind.ObjectCreationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var currentNode = context.Node;
        if (currentNode is MemberAccessExpressionSyntax &&
            currentNode.Parent is InvocationExpressionSyntax)
        {
            // in this case, a method call was mistakenly identified as simple property access
            return;
        }

        // if the call site is in a generated file, analysis is not performed
        if (IsGeneratedCode(context.Node.SyntaxTree))
        {
            return;
        }

        // get the symbol being called
        var symbolInfo = context.SemanticModel.GetSymbolInfo(context.Node);
        var calledSymbol = symbolInfo.Symbol;

        // handle method groups (e.g., in delegates) or other scenarios where the direct symbol isn't available
        if (calledSymbol == null && symbolInfo.CandidateSymbols.Length > 0)
        {
            calledSymbol = symbolInfo.CandidateSymbols.First();
        }

        if (calledSymbol == null)
        {
            return;
        }
        
        // find the original definition
        calledSymbol = calledSymbol.OriginalDefinition;

        // check for the [LibraryUsage] attribute.
        if (HasInternalUsageOnlyAttribute(calledSymbol))
        {
            var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation(), GetDisplayNameForSymbol(calledSymbol));
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool HasInternalUsageOnlyAttribute(ISymbol? symbol)
    {
        if (symbol is null)
        {
            return false;
        }

        // check the symbol itself for the attribute
        if (symbol.GetAttributes().Any(ad => ad.AttributeClass?.Name == nameof(LibraryUsageAttribute)))
        {
            return true;
        }

        // check the containing type as well
        return symbol.ContainingType != null && symbol
            .ContainingType.GetAttributes().Any(ad => ad.AttributeClass?.Name == nameof(LibraryUsageAttribute));
    }

    private static bool IsGeneratedCode(SyntaxTree tree)
    {
        if (!string.IsNullOrWhiteSpace(tree.FilePath))
        {
            var fileName = Path.GetFileName(tree.FilePath);
            if (fileName.EndsWith(".g.cs"))
            {
                return true;
            }
        }
        
        return false;
    }

    private static string GetDisplayNameForSymbol(ISymbol symbol)
    {
        switch (symbol)
        {
            case IMethodSymbol { MethodKind: MethodKind.Constructor } method:
                return method.ContainingType.Name;
            case IMethodSymbol { AssociatedSymbol: IPropertySymbol property }:
                symbol = property;
                break;
        }

        if (symbol.ContainingType != null && symbol is not INamedTypeSymbol)
        {
            return $"{symbol.ContainingType.Name}.{symbol.Name}";
        }

        return symbol.Name;
    }
}
