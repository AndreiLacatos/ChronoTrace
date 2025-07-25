using System.Threading.Tasks;
using ChronoTrace.SourceGenerators.DataStructures;
using Microsoft.CodeAnalysis;

namespace ChronoTrace.SourceGenerators
{
    internal static class MethodSymbolExtensions
    {
        /// <summary>
        /// Performs analysis on the method symbol and returns the results as a <see cref="MethodType"/> value.
        /// </summary>
        /// <param name="methodSymbol">Method to perform the analysis on</param>
        /// <param name="compilation">The ambient compilation context</param>
        /// <returns>A <see cref="MethodType"/> value indicating the type of the method</returns>
        internal static MethodType GetMethodType(this IMethodSymbol methodSymbol, Compilation compilation)
        {
            if (!methodSymbol.IsAsync)
            {
                return methodSymbol.ReturnsVoid ? MethodType.SyncVoid : MethodType.SyncNonVoid;
            }

            // get the well-known Task type from the compilation
            var taskSymbol = compilation.GetTypeByMetadataName(typeof(Task).FullName!);
            var valueTaskSymbol = compilation.GetTypeByMetadataName(typeof(ValueTask).FullName!);

            // check if the return type is the non-generic Task
            var returnType = (methodSymbol.ReturnType as INamedTypeSymbol)!;
            if (SymbolEqualityComparer.Default.Equals(returnType, taskSymbol)
                || SymbolEqualityComparer.Default.Equals(returnType, valueTaskSymbol))
            {
                return MethodType.SimpleAsync;
            }

            // check if the return type is the generic Task<> or ValueTask<>
            var genericTaskSymbol = compilation.GetTypeByMetadataName(typeof(Task<>).FullName!);
            var genericValueTaskSymbol = compilation.GetTypeByMetadataName(typeof(ValueTask<>).FullName!);
            if (returnType.IsGenericType 
                && (SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, genericTaskSymbol)
                    || SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, genericValueTaskSymbol)))
            {
                return MethodType.GenericAsync;
            }

            // could not correctly determine return type, fall back to void
            return MethodType.SyncVoid;
        }
    }
}
