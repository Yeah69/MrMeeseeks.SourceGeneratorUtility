using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using MrMeeseeks.SourceGeneratorUtility.Extensions;

namespace MrMeeseeks.SourceGeneratorUtility;

public interface INamedTypeCache
{
    IImmutableSet<INamedTypeSymbol> All { get; }

    IImmutableSet<INamedTypeSymbol> ForAssembly(IAssemblySymbol assembly);
}

public class NamedTypeCache : INamedTypeCache
{
    private readonly ICheckInternalsVisible _checkInternalsVisible;
    private readonly Lazy<IImmutableSet<INamedTypeSymbol>> _all;
    private IImmutableDictionary<IAssemblySymbol, IImmutableSet<INamedTypeSymbol>> _assemblyCache =
        ImmutableDictionary<IAssemblySymbol, IImmutableSet<INamedTypeSymbol>>.Empty;

    public NamedTypeCache(
        GeneratorExecutionContext context,
        ICheckInternalsVisible checkInternalsVisible)
    {
        _checkInternalsVisible = checkInternalsVisible;
        _all = new Lazy<IImmutableSet<INamedTypeSymbol>>(
            () => context
                .Compilation
                .SourceModule
                .ReferencedAssemblySymbols
                .Prepend(context.Compilation.Assembly)
                .SelectMany(ForAssembly)
                .ToImmutableHashSet<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default));
    }

    public IImmutableSet<INamedTypeSymbol> All => _all.Value;
    public IImmutableSet<INamedTypeSymbol> ForAssembly(IAssemblySymbol assembly)
    {
        if (_assemblyCache.TryGetValue(assembly, out var set)) return set;

        var freshSet = GetImplementationsFrom(assembly);
        _assemblyCache = _assemblyCache.Add(assembly, freshSet);
        return freshSet;
    }

    private IImmutableSet<INamedTypeSymbol> GetImplementationsFrom(IAssemblySymbol assemblySymbol)
    {
        var internalsAreVisible = _checkInternalsVisible.Check(assemblySymbol);
                
        return GetAllNamespaces(assemblySymbol.GlobalNamespace)
            .SelectMany(ns => ns.GetTypeMembers())
            .SelectMany(t => t.AllNestedTypesAndSelf())
            .Where(nts => nts.IsAccessiblePublicly() || internalsAreVisible && nts.IsAccessibleInternally())
            .ToImmutableHashSet<INamedTypeSymbol>(CustomSymbolEqualityComparer.Default);
    }

    private static IEnumerable<INamespaceSymbol> GetAllNamespaces(INamespaceSymbol root)
    {
        yield return root;
        foreach(var child in root.GetNamespaceMembers())
        foreach(var next in GetAllNamespaces(child))
            yield return next;
    }
}