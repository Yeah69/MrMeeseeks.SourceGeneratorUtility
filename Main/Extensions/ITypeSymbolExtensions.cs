using Microsoft.CodeAnalysis;

namespace MrMeeseeks.SourceGeneratorUtility.Extensions;

// ReSharper disable once InconsistentNaming
public static class ITypeSymbolExtensions
{
    /// <summary>
    /// Picked from https://github.com/YairHalberstadt/stronginject Thank you!
    /// </summary>
    public static bool IsOrReferencesErrorType(this ITypeSymbol type)
    {
        if (!type.ContainingType?.IsOrReferencesErrorType() ?? false)
            return false;
        return type switch
        {
            IErrorTypeSymbol => true,
            IArrayTypeSymbol array => array.ElementType.IsOrReferencesErrorType(),
            IPointerTypeSymbol pointer => pointer.PointedAtType.IsOrReferencesErrorType(),
            INamedTypeSymbol named => !named.IsUnboundGenericType && named.TypeArguments.Any(IsOrReferencesErrorType),
            _ => false,
        };
    }

    /// <summary>
    /// Picked from https://github.com/YairHalberstadt/stronginject Thank you!
    /// </summary>
    public static bool IsAccessibleInternally(this ITypeSymbol type)
    {
        if (type is ITypeParameterSymbol)
            return true;
        if (!type.ContainingType?.IsAccessibleInternally() ?? false)
            return false;
        return type switch
        {
            IArrayTypeSymbol array => array.ElementType.IsAccessibleInternally(),
            IPointerTypeSymbol pointer => pointer.PointedAtType.IsAccessibleInternally(),
            INamedTypeSymbol named => named.DeclaredAccessibility is Accessibility.Public or Accessibility.ProtectedOrInternal or Accessibility.Internal
                                      && named.TypeArguments.All(IsAccessibleInternally),
            _ => false,
        };
    }

    /// <summary>
    /// Picked from https://github.com/YairHalberstadt/stronginject Thank you!
    /// </summary>
    public static bool IsAccessiblePublicly(this ITypeSymbol type)
    {
        if (type is ITypeParameterSymbol)
            return true;
        if (!type.ContainingType?.IsAccessiblePublicly() ?? false)
            return false;
        return type switch
        {
            IArrayTypeSymbol array => array.ElementType.IsAccessiblePublicly(),
            IPointerTypeSymbol pointer => pointer.PointedAtType.IsAccessiblePublicly(),
            INamedTypeSymbol named => named.DeclaredAccessibility is Accessibility.Public
                                      && named.TypeArguments.All(IsAccessiblePublicly),
            _ => false,
        };
    }
    
    public static ITypeSymbol OriginalDefinitionIfUnbound(this ITypeSymbol type) =>
        type is INamedTypeSymbol namedTypeSymbol
            ? namedTypeSymbol.OriginalDefinitionIfUnbound()
            : type;
    
    public static ITypeSymbol UnboundIfGeneric(this ITypeSymbol type) =>
        type is INamedTypeSymbol namedTypeSymbol
            ? namedTypeSymbol.UnboundIfGeneric()
            : type;
    
    /// <summary>
    /// Picked from https://github.com/YairHalberstadt/stronginject Thank you!
    /// </summary>
    public static string FullName(
        this ITypeSymbol type,
        SymbolDisplayMiscellaneousOptions miscellaneousOptions = SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier) =>
        type.ToDisplayString(new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut,
            memberOptions: SymbolDisplayMemberOptions.IncludeRef,
            miscellaneousOptions: miscellaneousOptions));
}