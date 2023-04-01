using Microsoft.CodeAnalysis;

namespace MrMeeseeks.SourceGeneratorUtility;

public interface ICheckInternalsVisible
{
    bool Check(IAssemblySymbol assembly);
    bool Check(ISymbol symbol);
}

public class CheckInternalsVisible : ICheckInternalsVisible
{
    private readonly Compilation _compilation;
    private readonly INamedTypeSymbol? _internalsVisibleToAttribute;

    internal CheckInternalsVisible(
        GeneratorExecutionContext generatorExecutionContext)
    {
        _compilation = generatorExecutionContext.Compilation;
        _internalsVisibleToAttribute = generatorExecutionContext
            .Compilation
            .GetTypeByMetadataName("System.Runtime.CompilerServices.InternalsVisibleToAttribute");
    }
    
    public bool Check(IAssemblySymbol assembly) =>
        CustomSymbolEqualityComparer.Default.Equals(_compilation.Assembly, assembly) 
        || assembly
            .GetAttributes()
            .Any(ad =>
                _internalsVisibleToAttribute is not null
                && CustomSymbolEqualityComparer.Default.Equals(
                    ad.AttributeClass, 
                    _internalsVisibleToAttribute)
                && ad.ConstructorArguments.Length == 1
                && ad.ConstructorArguments[0].Value is string assemblyName
                && Equals(assemblyName, _compilation.AssemblyName));

    public bool Check(ISymbol symbol) => 
        Check(symbol.ContainingAssembly);
}