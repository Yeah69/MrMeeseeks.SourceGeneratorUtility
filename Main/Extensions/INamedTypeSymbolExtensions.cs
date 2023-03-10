using Microsoft.CodeAnalysis;

namespace MrMeeseeks.SourceGeneratorUtility.Extensions;

public static class INamedTypeSymbolExtensions
{
    public static IEnumerable<INamedTypeSymbol> AllBaseTypesAndSelf(this INamedTypeSymbol type)
    {
        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct)) 
            yield break;
        
        var temp = type;
        while (temp is {})
        {
            yield return temp;
            temp = temp.BaseType;
        }
    }
    public static IEnumerable<INamedTypeSymbol> AllDerivedTypesAndSelf(this INamedTypeSymbol type)
    {
        var baseTypesAndSelf = new List<INamedTypeSymbol>();
        if (type.TypeKind is TypeKind.Class or TypeKind.Struct)
        {
            var temp = type;
            while (temp is {})
            {
                baseTypesAndSelf.Add(temp);
                temp = temp.BaseType;
            }
        }
        else if (type.TypeKind is TypeKind.Interface)
            baseTypesAndSelf.Add(type);
        
        return type
            .AllInterfaces
            .Concat(baseTypesAndSelf);
    }
    public static IEnumerable<INamedTypeSymbol> AllNestedTypesAndSelf(this INamedTypeSymbol type)
    {
        yield return type;
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