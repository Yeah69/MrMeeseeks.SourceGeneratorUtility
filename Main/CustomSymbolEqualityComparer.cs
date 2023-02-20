using Microsoft.CodeAnalysis;

namespace MrMeeseeks.SourceGeneratorUtility;

public sealed class CustomSymbolEqualityComparer : IEqualityComparer<ISymbol?>
{
    private readonly bool _considerNullability;
    private readonly SymbolEqualityComparer _originalSymbolEqualityComparer;
    
    public static readonly CustomSymbolEqualityComparer Default = new (false);
    public static readonly CustomSymbolEqualityComparer IncludeNullability = new (true);

    internal CustomSymbolEqualityComparer(bool considerNullability)
    {
        _considerNullability = considerNullability;
        _originalSymbolEqualityComparer = considerNullability
            ? SymbolEqualityComparer.IncludeNullability
            : SymbolEqualityComparer.Default;
    }

    public bool Equals(ISymbol? x, ISymbol? y)
    {
        if (x is not INamedTypeSymbol xNamed || y is not INamedTypeSymbol yNamed
            || x is IErrorTypeSymbol || y is IErrorTypeSymbol) 
            return _originalSymbolEqualityComparer.Equals(x, y);

        return _originalSymbolEqualityComparer.Equals(xNamed.ContainingNamespace, yNamed.ContainingNamespace) 
               && xNamed.Name == yNamed.Name 
               && xNamed.TypeArguments.Length == yNamed.TypeArguments.Length 
               && xNamed.TypeArguments.Zip(yNamed.TypeArguments, Equals).All(b => b)
               && (!_considerNullability || xNamed.NullableAnnotation == yNamed.NullableAnnotation);
    }

    public int GetHashCode(ISymbol? obj) => _originalSymbolEqualityComparer.GetHashCode(obj);
}