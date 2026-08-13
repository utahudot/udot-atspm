using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtspmDocsGenerator;

internal static partial class XmlDocumentationReader
{
    public static string? ReadSummary(MemberDeclarationSyntax member, bool includeInheritDoc = false)
    {
        var documentation = member.GetLeadingTrivia()
            .Select(trivia => trivia.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .LastOrDefault();
        var summary = documentation?.Content
            .OfType<XmlElementSyntax>()
            .FirstOrDefault(element => element.StartTag.Name.LocalName.ValueText == "summary");

        if (summary is not null)
        {
            return Normalize(Flatten(summary.Content));
        }

        if (!includeInheritDoc || documentation is null)
        {
            return null;
        }

        var inheritDoc = documentation.Content
            .OfType<XmlEmptyElementSyntax>()
            .FirstOrDefault(element => element.Name.LocalName.ValueText == "inheritdoc");
        var cref = inheritDoc?.Attributes
            .OfType<XmlCrefAttributeSyntax>()
            .Select(attribute => attribute.Cref.ToString())
            .FirstOrDefault();

        return inheritDoc is null
            ? null
            : string.IsNullOrWhiteSpace(cref) ? "Inherited documentation." : $"See {SimplifyCref(cref)}.";
    }

    private static string Flatten(SyntaxList<XmlNodeSyntax> nodes)
    {
        var builder = new StringBuilder();
        foreach (var node in nodes)
        {
            switch (node)
            {
                case XmlTextSyntax text:
                    foreach (var token in text.TextTokens)
                    {
                        builder.Append(token.ValueText);
                    }
                    break;
                case XmlElementSyntax element:
                    builder.Append(Flatten(element.Content));
                    break;
                case XmlEmptyElementSyntax element:
                    var value = element.Attributes.OfType<XmlCrefAttributeSyntax>()
                        .Select(attribute => SimplifyCref(attribute.Cref.ToString()))
                        .FirstOrDefault()
                        ?? element.Attributes.OfType<XmlNameAttributeSyntax>()
                            .Select(attribute => attribute.Identifier.Identifier.ValueText)
                            .FirstOrDefault();
                    builder.Append(value ?? element.ToString());
                    break;
            }
        }
        return builder.ToString();
    }

    private static string SimplifyCref(string cref)
    {
        var value = cref.Trim();
        var colonIndex = value.IndexOf(':');
        if (colonIndex >= 0)
        {
            value = value[(colonIndex + 1)..];
        }
        return value.Replace("{", "<", StringComparison.Ordinal).Replace("}", ">", StringComparison.Ordinal);
    }

    private static string? Normalize(string value)
    {
        var normalized = WhitespaceRegex().Replace(value, " ").Trim();
        return normalized.Length == 0 ? null : normalized;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
