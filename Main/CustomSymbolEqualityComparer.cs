using Microsoft.CodeAnalysis;

namespace MrMeeseeks.SourceGeneratorUtility;

public sealed class CustomSymbolEqualityComparer : IEqualityComparer<ISymbol?>
{
    private readonly bool _considerNullability;
    private readonly SymbolEqualityComparer _originalSymbolEqualityComparer;
    
    public static readonly CustomSymbolEqualityComparer Default = new (false);
    public static readonly CustomSymbolEqualityComparer IncludeNullability = new (true);

    private CustomSymbolEqualityComparer(bool considerNullability)
    {
        _considerNullability = considerNullability;
        _originalSymbolEqualityComparer = considerNullability
            ? SymbolEqualityComparer.IncludeNullability
            : SymbolEqualityComparer.Default;
    }

    public bool Equals(ISymbol? x, ISymbol? y)
    {
        return (x, y) switch
        {
            (INamedTypeSymbol xN, INamedTypeSymbol yN) => InnerEqualsNamed(xN, yN),
            (ITypeParameterSymbol xT, ITypeParameterSymbol yT) => InnerEqualsTypeParameter(xT, yT),
            _ => _originalSymbolEqualityComparer.Equals(x, y)
        };

        bool InnerEqualsNamed(INamedTypeSymbol xNamed, INamedTypeSymbol yNamed)
        {
            if (xNamed is IErrorTypeSymbol || yNamed is IErrorTypeSymbol) 
                return _originalSymbolEqualityComparer.Equals(x, y);
            
            return _originalSymbolEqualityComparer.Equals(xNamed.ContainingNamespace, yNamed.ContainingNamespace) 
                   && xNamed.Name == yNamed.Name 
                   && xNamed.TypeArguments.Length == yNamed.TypeArguments.Length 
                   && xNamed.TypeArguments.Zip(yNamed.TypeArguments, Equals).All(b => b)
                   && (!_considerNullability 
                       // either both annotated
                       || xNamed.NullableAnnotation == NullableAnnotation.Annotated && yNamed.NullableAnnotation == NullableAnnotation.Annotated
                       // or both not annotated
                       || xNamed.NullableAnnotation != NullableAnnotation.Annotated && yNamed.NullableAnnotation != NullableAnnotation.Annotated);
        }

        bool InnerEqualsTypeParameter(ITypeParameterSymbol xTypeParameter, ITypeParameterSymbol yTypeParameter) =>
            xTypeParameter.Name == yTypeParameter.Name
            && (!_considerNullability 
                // either both annotated
                || xTypeParameter.NullableAnnotation == NullableAnnotation.Annotated && yTypeParameter.NullableAnnotation == NullableAnnotation.Annotated
                // or both not annotated
                || xTypeParameter.NullableAnnotation != NullableAnnotation.Annotated && yTypeParameter.NullableAnnotation != NullableAnnotation.Annotated)
            && (xTypeParameter.DeclaringMethod is {} xDeclaringMethod && yTypeParameter.DeclaringMethod is {} yDeclaringMethod && Equals(xDeclaringMethod, yDeclaringMethod) 
                || xTypeParameter.DeclaringType is {} xDeclaringType && yTypeParameter.DeclaringType is {} yDeclaringType && Equals(xDeclaringType, yDeclaringType) 
                || xTypeParameter is { DeclaringType: null, DeclaringMethod: null } && yTypeParameter is { DeclaringType: null, DeclaringMethod: null });
    }

    public int GetHashCode(ISymbol? obj) => _originalSymbolEqualityComparer.GetHashCode(obj);
}