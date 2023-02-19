using Microsoft.CodeAnalysis;

namespace MrMeeseeks.SourceGeneratorUtility.Extensions;

// ReSharper disable once InconsistentNaming
public static class INamespaceSymbolExtensions
{
    // Picked from https://github.com/YairHalberstadt/stronginject Thank you!
    public static string FullName(this INamespaceSymbol @namespace) =>
        @namespace.ToDisplayString(new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces));
}