using System.Text;

namespace AtspmDocsGenerator;

using static DocumentationText;

public sealed class LogMessageDocumentationGenerator
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    public LogMessageGenerationResult Generate(
        IReadOnlyList<LogMessageDefinition> messages,
        string outputPath,
        CliOptions options)
    {
        var duplicateEventIdCount = messages
            .GroupBy(message => message.EventId)
            .Count(group => group.Count() > 1);
        var builder = new StringBuilder();
        builder.AppendLine("# Log message reference");
        builder.AppendLine();
        builder.AppendLine(
            "Logger messages declared in `Utah.Udot.Atspm.Infrastructure.LogMessages`, sorted by event ID.");
        builder.AppendLine();
        builder.AppendLine(
            $"Generated from [{RepositoryLabel(options.RepositoryUrl)} at " +
            $"`{EscapeCode(options.RepositoryRef)}`]({BuildRepositoryUrl(options.RepositoryUrl, "tree", options.RepositoryRef)}).");
        builder.AppendLine();
        builder.AppendLine($"This reference contains **{messages.Count}** log messages.");
        builder.AppendLine();

        if (duplicateEventIdCount > 0)
        {
            builder.AppendLine(
                $"**Note:** {duplicateEventIdCount} event IDs are used by more than one message. " +
                "The source column identifies each declaration.");
            builder.AppendLine();
        }

        builder.AppendLine("| Event ID | Event name | Level | Summary | Source |");
        builder.AppendLine("| ---: | --- | --- | --- | --- |");

        foreach (var message in messages
                     .OrderBy(message => message.EventId)
                     .ThenBy(message => message.EventName, StringComparer.Ordinal)
                     .ThenBy(message => message.SourcePath, StringComparer.Ordinal))
        {
            var sourceUrl = BuildRepositoryUrl(
                options.RepositoryUrl,
                "blob",
                options.RepositoryRef,
                message.SourcePath) + $"#L{message.SourceLine}";
            builder.AppendLine(
                $"| `{message.EventId}` " +
                $"| {EscapeTableText(message.EventName)} " +
                $"| `{EscapeCode(message.Level)}` " +
                $"| {EscapeTableText(message.Summary ?? string.Empty)} " +
                $"| [{EscapeTableText(Path.GetFileName(message.SourcePath))}]({sourceUrl}) |");
        }

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(
            outputPath,
            NormalizeLineEndings(builder.ToString()),
            Utf8WithoutBom);

        return new LogMessageGenerationResult(messages.Count, duplicateEventIdCount);
    }

    private static string EscapeTableText(string value) => EscapeText(value);
}
