using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtspmDocsGenerator;

public sealed partial class ConfigurationSourceAnalyzer
{
    private static readonly CSharpParseOptions ParseOptions =
        new(languageVersion: LanguageVersion.Latest, documentationMode: DocumentationMode.Parse);

    public IReadOnlyDictionary<string, ConfigurationSection> Analyze(
        string sourceRoot,
        IEnumerable<string> sourcePaths)
    {
        var normalizedRoot = Path.GetFullPath(sourceRoot);
        var sections = new Dictionary<string, ConfigurationSection>(StringComparer.Ordinal);

        foreach (var file in EnumerateSourceFiles(normalizedRoot, sourcePaths))
        {
            var source = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(
                source,
                ParseOptions,
                path: file,
                encoding: Encoding.UTF8);
            var root = tree.GetCompilationUnitRoot();

            foreach (var type in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                var attribute = type.AttributeLists
                    .SelectMany(list => list.Attributes)
                    .FirstOrDefault(IsConfigurationSectionAttribute);

                if (attribute is null)
                {
                    continue;
                }

                var section = CreateSection(type, attribute, normalizedRoot);
                if (!sections.TryAdd(section.SectionName, section))
                {
                    throw new InvalidDataException(
                        $"Configuration section '{section.SectionName}' is declared more than once.");
                }
            }
        }

        return sections;
    }

    private static IEnumerable<string> EnumerateSourceFiles(
        string sourceRoot,
        IEnumerable<string> sourcePaths)
    {
        var files = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var sourcePath in sourcePaths)
        {
            var fullPath = Path.GetFullPath(Path.Combine(sourceRoot, sourcePath));
            EnsureWithinSourceRoot(sourceRoot, fullPath);

            if (!Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException(
                    $"Configured source path does not exist: {fullPath}");
            }

            foreach (var file in Directory.EnumerateFiles(fullPath, "*.cs", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceRoot, file);
                if (relativePath
                    .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Any(part => part is ".git" or "bin" or "obj"))
                {
                    continue;
                }

                files.Add(Path.GetFullPath(file));
            }
        }

        return files;
    }

    private static void EnsureWithinSourceRoot(string sourceRoot, string path)
    {
        var rootWithSeparator = sourceRoot.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!path.Equals(sourceRoot, StringComparison.OrdinalIgnoreCase)
            && !path.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                $"Configured source path must remain inside the source root: {path}");
        }
    }

    private static ConfigurationSection CreateSection(
        TypeDeclarationSyntax type,
        AttributeSyntax attribute,
        string sourceRoot)
    {
        var arguments = attribute.ArgumentList?.Arguments ?? default;
        var sectionArgument = FindArgument(arguments, "sectionName", 0)
            ?? throw new InvalidDataException(
                $"ConfigurationSection on '{type.Identifier.ValueText}' does not define a section name.");

        var sectionName = ReadSectionName(sectionArgument.Expression);
        var descriptionArgument = FindArgument(arguments, "description", 1);
        var description = descriptionArgument is null
            ? null
            : ReadOptionalString(descriptionArgument.Expression);

        var properties = type.Members
            .OfType<PropertyDeclarationSyntax>()
            .Where(IsDocumentedProperty)
            .Select(CreateProperty)
            .ToArray();

        var line = type.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        var relativePath = Path.GetRelativePath(sourceRoot, type.SyntaxTree.FilePath)
            .Replace('\\', '/');

        return new ConfigurationSection(
            sectionName,
            description,
            ReadDocumentation(type),
            relativePath,
            line,
            properties);
    }

    private static AttributeArgumentSyntax? FindArgument(
        SeparatedSyntaxList<AttributeArgumentSyntax> arguments,
        string name,
        int positionalIndex)
    {
        var named = arguments.FirstOrDefault(argument =>
            string.Equals(
                argument.NameColon?.Name.Identifier.ValueText
                    ?? argument.NameEquals?.Name.Identifier.ValueText,
                name,
                StringComparison.OrdinalIgnoreCase));

        if (named is not null)
        {
            return named;
        }

        return arguments
            .Where(argument => argument.NameColon is null && argument.NameEquals is null)
            .ElementAtOrDefault(positionalIndex);
    }

    private static string ReadSectionName(ExpressionSyntax expression)
    {
        if (expression is LiteralExpressionSyntax literal
            && literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return literal.Token.ValueText;
        }

        if (expression is InvocationExpressionSyntax invocation
            && invocation.Expression is IdentifierNameSyntax identifier
            && identifier.Identifier.ValueText == "nameof"
            && invocation.ArgumentList.Arguments.Count == 1)
        {
            return invocation.ArgumentList.Arguments[0].Expression
                .ToString()
                .Split('.')
                .Last();
        }

        throw new InvalidDataException(
            $"Unsupported configuration section expression '{expression}'. Use a string literal or nameof(...).");
    }

    private static string? ReadOptionalString(ExpressionSyntax expression)
    {
        if (expression.IsKind(SyntaxKind.NullLiteralExpression))
        {
            return null;
        }

        if (expression is LiteralExpressionSyntax literal
            && literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return literal.Token.ValueText;
        }

        throw new InvalidDataException(
            $"Unsupported configuration description expression '{expression}'. Use a string literal or null.");
    }

    private static bool IsConfigurationSectionAttribute(AttributeSyntax attribute)
    {
        var name = attribute.Name.ToString().Split('.').Last();
        return name is "ConfigurationSection" or "ConfigurationSectionAttribute";
    }

    private static bool IsDocumentedProperty(PropertyDeclarationSyntax property)
    {
        var isStatic = property.Modifiers.Any(SyntaxKind.StaticKeyword);
        var isPublic = property.Modifiers.Count == 0
            || property.Modifiers.Any(SyntaxKind.PublicKeyword);

        return isPublic && !isStatic;
    }

    private static ConfigurationProperty CreateProperty(PropertyDeclarationSyntax property)
    {
        var typeName = property.Type
            .WithoutTrivia()
            .NormalizeWhitespace()
            .ToFullString();
        var defaultExpression = property.Initializer?.Value
            .WithoutTrivia()
            .NormalizeWhitespace()
            .ToFullString()
            ?? "Not set";
        var isRequired = property.Modifiers.Any(SyntaxKind.RequiredKeyword)
            || property.AttributeLists
                .SelectMany(list => list.Attributes)
                .Any(attribute =>
                {
                    var name = attribute.Name.ToString().Split('.').Last();
                    return name is "Required" or "RequiredAttribute";
                });

        return new ConfigurationProperty(
            property.Identifier.ValueText,
            typeName,
            defaultExpression,
            isRequired,
            ReadDocumentation(property));
    }

    private static string? ReadDocumentation(MemberDeclarationSyntax member)
    {
        var documentation = member.GetLeadingTrivia()
            .Select(trivia => trivia.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .LastOrDefault();

        if (documentation is null)
        {
            return null;
        }

        var summary = documentation.Content
            .OfType<XmlElementSyntax>()
            .FirstOrDefault(element =>
                element.StartTag.Name.LocalName.ValueText == "summary");

        if (summary is not null)
        {
            return NormalizeDocumentation(FlattenXml(summary.Content));
        }

        var inheritDoc = documentation.Content
            .OfType<XmlEmptyElementSyntax>()
            .FirstOrDefault(element => element.Name.LocalName.ValueText == "inheritdoc");

        if (inheritDoc is null)
        {
            return null;
        }

        var cref = inheritDoc.Attributes
            .OfType<XmlCrefAttributeSyntax>()
            .Select(attribute => attribute.Cref.ToString())
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(cref)
            ? "Inherited documentation."
            : $"See {SimplifyCref(cref)}.";
    }

    private static string FlattenXml(SyntaxList<XmlNodeSyntax> nodes)
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
                    builder.Append(FlattenXml(element.Content));
                    break;
                case XmlEmptyElementSyntax element
                    when element.Name.LocalName.ValueText is "see" or "seealso":
                    var cref = element.Attributes
                        .OfType<XmlCrefAttributeSyntax>()
                        .Select(attribute => attribute.Cref.ToString())
                        .FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(cref))
                    {
                        builder.Append(SimplifyCref(cref));
                    }

                    break;
                case XmlEmptyElementSyntax element:
                    builder.Append(element.ToString());
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

        return value.Replace("{", "<", StringComparison.Ordinal)
            .Replace("}", ">", StringComparison.Ordinal);
    }

    private static string? NormalizeDocumentation(string value)
    {
        var normalized = WhitespaceRegex().Replace(value, " ").Trim();
        return normalized.Length == 0 ? null : normalized;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
