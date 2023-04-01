using Microsoft.CodeAnalysis;

namespace MrMeeseeks.SourceGeneratorUtility.Extensions;

public static class INamedTypeSymbolExtensions
{
    public static IEnumerable<INamedTypeSymbol> AllBaseTypesAndSelf(this INamedTypeSymbol type)
    {
        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct)) 
            yield break;

        yield return type;
        foreach (var baseType in type.AllBaseTypes())
            yield return baseType;
    }
    public static IEnumerable<INamedTypeSymbol> AllBaseTypes(this INamedTypeSymbol type)
    {
        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct)) 
            yield break;
        
        var temp = type.BaseType;
        while (temp is {})
        {
            yield return temp;
            temp = temp.BaseType;
        }
    }
    public static IEnumerable<INamedTypeSymbol> AllDerivedTypesAndSelf(this INamedTypeSymbol type)
    {
        yield return type;
        
        foreach (var derivedType in type.AllDerivedTypes())
            yield return derivedType;
    }
    public static IEnumerable<INamedTypeSymbol> AllDerivedTypes(this INamedTypeSymbol type)
    {
        foreach (var interfaceType in type
                     .AllInterfaces)
            yield return interfaceType;
        if (type.TypeKind is TypeKind.Class or TypeKind.Struct)
        {
            var temp = type.BaseType;
            while (temp is {})
            {
                yield return temp;
                temp = temp.BaseType;
            }
        }
    }
    public static IEnumerable<INamedTypeSymbol> AllNestedTypesAndSelf(this INamedTypeSymbol type)
    {
        yield return type;
        foreach (var nestedType in type.AllNestedTypes())
        {
            yield return nestedType;
        }
    }
    public static IEnumerable<INamedTypeSymbol> AllNestedTypes(this INamedTypeSymbol type)
    {
        foreach (var typeMember in type.GetTypeMembers())
        {
            foreach (var nestedType in typeMember.AllNestedTypesAndSelf())
            {
                yield return nestedType;
            }
        }
    }
    
    public static INamedTypeSymbol UnboundIfGeneric(this INamedTypeSymbol type) =>
        type is { IsGenericType: true, IsUnboundGenericType: false }
            ? type.ConstructUnboundGenericType()
            : type;
    
    public static INamedTypeSymbol OriginalDefinitionIfUnbound(this INamedTypeSymbol type) =>
        type.IsUnboundGenericType
            ? type.OriginalDefinition
            : type;
}