using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtspmDocsGenerator;

public sealed partial class LogMessageSourceAnalyzer
{
    public const string DefaultSourcePath = "Atspm/Infrastructure/LogMessages";

    public static readonly IReadOnlySet<int> AllowedDuplicateEventIds =
        new HashSet<int> { 201, 202, 203, 1000, 1001, 1002, 1003, 9001 };

    private static readonly CSharpParseOptions ParseOptions =
        new(languageVersion: LanguageVersion.Latest, documentationMode: DocumentationMode.Parse);

    public IReadOnlyList<LogMessageDefinition> Analyze(string sourceRoot, string sourcePath)
    {
        var normalizedRoot = Path.GetFullPath(sourceRoot);
        var fullSourcePath = SourcePath.ResolveWithinRoot(
            normalizedRoot,
            sourcePath,
            "Log message source path");

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

        var unexpectedDuplicates = messages
            .GroupBy(message => message.EventId)
            .Where(group => group.Count() > 1 && !AllowedDuplicateEventIds.Contains(group.Key))
            .Select(group => group.Key)
            .Order()
            .ToArray();
        if (unexpectedDuplicates.Length > 0)
        {
            throw new InvalidDataException(
                $"Duplicate logger event IDs are not allowed: {string.Join(", ", unexpectedDuplicates)}.");
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
            XmlDocumentationReader.ReadSummary(method),
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

}
