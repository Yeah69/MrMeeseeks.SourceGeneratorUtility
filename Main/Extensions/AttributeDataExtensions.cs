using Microsoft.CodeAnalysis;

namespace MrMeeseeks.SourceGeneratorUtility.Extensions;

public static class AttributeDataExtensions
{
    public static Location GetLocation(this AttributeData attributeData) =>
        attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None;
}