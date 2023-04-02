using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace MrMeeseeks.SourceGeneratorUtility.Extensions;

public static class GenerationExecutionContextExtensions
{
    public static void NormalizeWhitespaceAndAddSource(
        this GeneratorExecutionContext context,
        string hintName,
        StringBuilder codeBuilder)
    {
        var partialCodeSource = CSharpSyntaxTree
            .ParseText(SourceText.From(codeBuilder.ToString(), Encoding.UTF8))
            .GetRoot()
            .NormalizeWhitespace()
            .SyntaxTree
            .GetText();
                    
        context.AddSource(hintName, partialCodeSource);
    }
}