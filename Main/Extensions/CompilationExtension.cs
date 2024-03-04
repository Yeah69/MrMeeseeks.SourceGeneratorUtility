using Microsoft.CodeAnalysis;

namespace MrMeeseeks.SourceGeneratorUtility.Extensions;

public static class CompilationExtension
{
    public static INamedTypeSymbol GetTypeByMetadataNameOrThrow(this Compilation compilation, string metadataName) =>
        compilation.GetTypeByMetadataName(metadataName)
        ?? throw new ArgumentException($"Type not found by metadata name \"{metadataName}\".", nameof(metadataName));
}