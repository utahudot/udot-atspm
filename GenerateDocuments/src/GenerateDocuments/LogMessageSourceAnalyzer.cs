using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtspmDocsGenerator;

public sealed partial class LogMessageSourceAnalyzer
{
    public const string DefaultSourcePath = "Atspm/Infrastructure/LogMessages";

    private static readonly CSharpParseOptions ParseOptions =
        new(languageVersion: LanguageVersion.Latest, documentationMode: DocumentationMode.Parse);

    public IReadOnlyList<LogMessageDefinition> Analyze(string sourceRoot, string sourcePath)
    {
        var normalizedRoot = Path.GetFullPath(sourceRoot);
        var fullSourcePath = Path.GetFullPath(Path.Combine(normalizedRoot, sourcePath));
        EnsureWithinSourceRoot(normalizedRoot, fullSourcePath);

        if (!Directory.Exists(fullSourcePath))
        {
            throw new DirectoryNotFoundException(
                $"Log message source path does not exist: {fullSourcePath}");
        }

        var messages = new List<LogMessageDefinition>();

        foreach (var file in Directory.EnumerateFiles(fullSourcePath, "*.cs", SearchOption.AllDirectories)
                     .Order(StringComparer.Ordinal))
        {
            var tree = CSharpSyntaxTree.ParseText(
                File.ReadAllText(file),
                ParseOptions,
                path: file,
                encoding: Encoding.UTF8);
            var root = tree.GetCompilationUnitRoot();

            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                var attribute = method.AttributeLists
                    .SelectMany(list => list.Attributes)
                    .FirstOrDefault(IsLoggerMessageAttribute);

                if (attribute is not null)
                {
                    messages.Add(CreateDefinition(method, attribute, normalizedRoot));
                }
            }
        }

        return messages
            .OrderBy(message => message.EventId)
            .ThenBy(message => message.EventName, StringComparer.Ordinal)
            .ThenBy(message => message.SourcePath, StringComparer.Ordinal)
            .ToArray();
    }

    private static LogMessageDefinition CreateDefinition(
        MethodDeclarationSyntax method,
        AttributeSyntax attribute,
        string sourceRoot)
    {
        var arguments = attribute.ArgumentList?.Arguments ?? default;
        var eventIdExpression = FindNamedArgument(arguments, "EventId")?.Expression
            ?? throw InvalidAttribute(method, "EventId");
        var eventNameExpression = FindNamedArgument(arguments, "EventName")?.Expression
            ?? throw InvalidAttribute(method, "EventName");
        var levelExpression = FindNamedArgument(arguments, "Level")?.Expression
            ?? throw InvalidAttribute(method, "Level");

        if (eventIdExpression is not LiteralExpressionSyntax eventIdLiteral
            || !int.TryParse(eventIdLiteral.Token.ValueText, out var eventId))
        {
            throw new InvalidDataException(
                $"LoggerMessage on '{method.Identifier.ValueText}' must use an integer EventId literal.");
        }

        if (eventNameExpression is not LiteralExpressionSyntax eventNameLiteral
            || !eventNameLiteral.IsKind(SyntaxKind.StringLiteralExpression))
        {
            throw new InvalidDataException(
                $"LoggerMessage on '{method.Identifier.ValueText}' must use a string EventName literal.");
        }

        var level = levelExpression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            _ => throw new InvalidDataException(
                $"LoggerMessage on '{method.Identifier.ValueText}' has unsupported Level expression '{levelExpression}'.")
        };
        var line = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        var relativePath = Path.GetRelativePath(sourceRoot, method.SyntaxTree.FilePath)
            .Replace('\\', '/');

        return new LogMessageDefinition(
            eventId,
            eventNameLiteral.Token.ValueText,
            level,
            ReadDocumentation(method),
            relativePath,
            line);
    }

    private static AttributeArgumentSyntax? FindNamedArgument(
        SeparatedSyntaxList<AttributeArgumentSyntax> arguments,
        string name) =>
        arguments.FirstOrDefault(argument =>
            string.Equals(
                argument.NameEquals?.Name.Identifier.ValueText
                    ?? argument.NameColon?.Name.Identifier.ValueText,
                name,
                StringComparison.Ordinal));

    private static bool IsLoggerMessageAttribute(AttributeSyntax attribute)
    {
        var name = attribute.Name.ToString().Split('.').Last();
        return name is "LoggerMessage" or "LoggerMessageAttribute";
    }

    private static InvalidDataException InvalidAttribute(
        MethodDeclarationSyntax method,
        string missingArgument) =>
        new($"LoggerMessage on '{method.Identifier.ValueText}' does not define {missingArgument}.");

    private static string? ReadDocumentation(MethodDeclarationSyntax method)
    {
        var documentation = method.GetLeadingTrivia()
            .Select(trivia => trivia.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .LastOrDefault();
        var summary = documentation?.Content
            .OfType<XmlElementSyntax>()
            .FirstOrDefault(element => element.StartTag.Name.LocalName.ValueText == "summary");

        if (summary is null)
        {
            return null;
        }

        var normalized = WhitespaceRegex().Replace(FlattenXml(summary.Content), " ").Trim();
        return normalized.Length == 0 ? null : normalized;
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
                case XmlEmptyElementSyntax element:
                    var value = element.Attributes
                        .OfType<XmlCrefAttributeSyntax>()
                        .Select(attribute => attribute.Cref.ToString())
                        .FirstOrDefault()
                        ?? element.Attributes
                            .OfType<XmlNameAttributeSyntax>()
                            .Select(attribute => attribute.Identifier.Identifier.ValueText)
                            .FirstOrDefault();
                    builder.Append(value ?? element.ToString());
                    break;
            }
        }

        return builder.ToString();
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
                $"Log message source path must remain inside the source root: {path}");
        }
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
