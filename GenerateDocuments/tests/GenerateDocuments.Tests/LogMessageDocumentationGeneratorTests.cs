namespace AtspmDocsGenerator.Tests;

public sealed class LogMessageDocumentationGeneratorTests
{
    [Fact]
    public void GenerateWritesNumericallySortedTableAndPreservesDuplicateIds()
    {
        using var directory = new TemporaryDirectory();
        var outputPath = System.IO.Path.Combine(directory.Path, "log-messages.md");
        var messages = new[]
        {
            new LogMessageDefinition(20, "Later", "Warning", "Later summary.", "Logs/Later.cs", 8),
            new LogMessageDefinition(10, "Second duplicate", "Error", "Duplicate summary.", "Logs/Second.cs", 12),
            new LogMessageDefinition(10, "First duplicate", "Information", "First summary.", "Logs/First.cs", 4)
        };
        var options = new CliOptions(
            directory.Path,
            directory.Path,
            "map.json",
            "https://github.com/example/source",
            "abc123",
            new DateTimeOffset(2026, 8, 18, 19, 15, 0, TimeSpan.Zero));

        var result = new LogMessageDocumentationGenerator().Generate(messages, outputPath, options);

        Assert.Equal(new LogMessageGenerationResult(3, 1), result);
        var markdown = File.ReadAllText(outputPath);
        var firstIndex = markdown.IndexOf("First duplicate", StringComparison.Ordinal);
        var secondIndex = markdown.IndexOf("Second duplicate", StringComparison.Ordinal);
        var laterIndex = markdown.IndexOf("Later", StringComparison.Ordinal);
        Assert.True(firstIndex < secondIndex);
        Assert.True(secondIndex < laterIndex);
        Assert.Contains("| `10` | First duplicate | `Information` | First summary.", markdown);
        Assert.Contains("Logs/First.cs#L4", markdown);
        Assert.Contains("id=\"download-log-pdf\"", markdown);
        Assert.Contains("id=\"download-log-excel\"", markdown);
    }
}
