using System.Text.Json;

namespace AtspmDocsGenerator;

public static class Program
{
    public static int Main(string[] args)
    {
        var parseResult = CliOptions.Parse(args);

        if (parseResult.ShowHelp)
        {
            Console.WriteLine(CliOptions.HelpText);
            return 0;
        }

        if (parseResult.Options is null)
        {
            Console.Error.WriteLine(parseResult.Error);
            Console.Error.WriteLine();
            Console.Error.WriteLine(CliOptions.HelpText);
            return 2;
        }

        try
        {
            var options = parseResult.Options;
            var map = DocumentationMapLoader.Load(options.MapPath);
            var sections = new ConfigurationSourceAnalyzer().Analyze(
                options.SourceRoot,
                map.SourcePaths);
            var logMessages = new LogMessageSourceAnalyzer().Analyze(
                options.SourceRoot,
                LogMessageSourceAnalyzer.DefaultSourcePath);
            GenerationResult? result = null;
            LogMessageGenerationResult? logResult = null;

            OutputDirectoryTransaction.Run(options.OutputRoot, stagingRoot =>
            {
                var stagingOptions = options with { OutputRoot = stagingRoot };
                result = new MarkdownDocumentationGenerator().Generate(
                    map,
                    sections,
                    stagingOptions);
                logResult = new LogMessageDocumentationGenerator().Generate(
                    logMessages,
                    Path.Combine(stagingRoot, "log-messages.md"),
                    stagingOptions);
            });

            Console.WriteLine(
                $"Generated {result!.PageCount} container pages from " +
                $"{result.DocumentedSectionCount} configuration sections.");
            Console.WriteLine(
                $"Generated a log message reference containing {logResult!.MessageCount} messages " +
                $"({logResult.DuplicateEventIdCount} duplicated event IDs).");

            var mappedSections = map.Containers
                .SelectMany(container => container.Sections)
                .Select(MappedSectionName.GetBaseName)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var section in sections.Keys.Except(mappedSections).Order(StringComparer.Ordinal))
            {
                Console.WriteLine($"Warning: configuration section '{section}' is not mapped to a container.");
            }

            return 0;
        }
        catch (Exception exception) when (
            exception is ArgumentException
            or DirectoryNotFoundException
            or FileNotFoundException
            or InvalidDataException
            or IOException
            or JsonException)
        {
            Console.Error.WriteLine($"Generation failed: {exception.Message}");
            return 1;
        }
    }
}
