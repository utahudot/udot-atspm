using System.Text;

namespace AtspmDocsGenerator.Tests;

public sealed class MarkdownDocumentationGeneratorTests
{
    [Fact]
    public void GenerateMatchesTheContainerPageSnapshot()
    {
        using var directory = new TemporaryDirectory();
        var output = System.IO.Path.Combine(directory.Path, "output");
        Directory.CreateDirectory(output);
        File.WriteAllText(System.IO.Path.Combine(output, "stale.md"), "stale");

        var map = new DocumentationMap
        {
            SchemaVersion = 1,
            SourcePaths = ["src"],
            Containers =
            [
                new ContainerDefinition
                {
                    Name = "Event Log Utility",
                    Slug = "event-log-utility",
                    Sections = ["SampleOptions"]
                }
            ]
        };
        var sections = new Dictionary<string, ConfigurationSection>(StringComparer.Ordinal)
        {
            ["SampleOptions"] = new(
                "SampleOptions",
                "Sample configuration.",
                "Options used by the sample utility.",
                "Atspm/Infrastructure/Configuration/SampleOptions.cs",
                10,
                [
                    new(
                        "Dates",
                        "IEnumerable<DateTime?>",
                        "[]",
                        false,
                        "Dates to include."),
                    new(
                        "Path",
                        "string",
                        "System.IO.Path.GetTempPath()",
                        true,
                        "Output path | directory.",
                        null,
                        ["Local", "Remote"])
                ])
        };
        var options = new CliOptions(
            directory.Path,
            output,
            System.IO.Path.Combine(directory.Path, "map.json"),
            "https://github.com/utahudot/udot-atspm",
            "abc123",
            new DateTimeOffset(2026, 8, 18, 19, 15, 0, TimeSpan.Zero));

        var result = new MarkdownDocumentationGenerator().Generate(map, sections, options);

        Assert.Equal(new GenerationResult(1, 1), result);
        Assert.False(File.Exists(System.IO.Path.Combine(output, "stale.md")));

        var actualPath = System.IO.Path.Combine(output, "event-log-utility.md");
        var expectedPath = System.IO.Path.Combine(
            AppContext.BaseDirectory,
            "Snapshots",
            "event-log-utility.md");
        var actual = File.ReadAllText(actualPath).Replace("\r\n", "\n");
        var expected = File.ReadAllText(expectedPath).Replace("\r\n", "\n");

        Assert.Equal(expected, actual);
        Assert.DoesNotContain(System.IO.Path.GetTempPath(), actual);

        var bytes = File.ReadAllBytes(actualPath);
        Assert.False(
            bytes.Length >= 3
            && bytes[0] == 0xEF
            && bytes[1] == 0xBB
            && bytes[2] == 0xBF,
            "Generated Markdown must be UTF-8 without a byte-order mark.");
    }

    [Fact]
    public void GenerateRejectsUnknownMappedSectionsWithoutChangingExistingOutput()
    {
        using var directory = new TemporaryDirectory();
        var map = new DocumentationMap
        {
            SchemaVersion = 1,
            SourcePaths = ["src"],
            Containers =
            [
                new ContainerDefinition
                {
                    Name = "Example",
                    Slug = "example",
                    Sections = ["MissingOptions"]
                }
            ]
        };
        var output = System.IO.Path.Combine(directory.Path, "output");
        Directory.CreateDirectory(output);
        File.WriteAllText(System.IO.Path.Combine(output, "existing.md"), "existing");
        var options = new CliOptions(
            directory.Path,
            output,
            "map.json",
            "https://github.com/example/source",
            "main",
            DateTimeOffset.UtcNow);

        var exception = Assert.Throws<InvalidDataException>(
            () => new MarkdownDocumentationGenerator().Generate(
                map,
                new Dictionary<string, ConfigurationSection>(),
                options));

        Assert.Contains("MissingOptions", exception.Message);
        Assert.Equal("existing", File.ReadAllText(System.IO.Path.Combine(output, "existing.md")));
    }

    [Fact]
    public void GenerateSupportsMappedSectionPaths()
    {
        using var directory = new TemporaryDirectory();
        var output = System.IO.Path.Combine(directory.Path, "output");
        var map = new DocumentationMap
        {
            SchemaVersion = 1,
            SourcePaths = ["src"],
            Containers =
            [
                new ContainerDefinition
                {
                    Name = "Example",
                    Slug = "example",
                    Sections = ["SampleOptions:Nested"]
                }
            ]
        };
        var sections = new Dictionary<string, ConfigurationSection>(StringComparer.Ordinal)
        {
            ["SampleOptions"] = new(
                "SampleOptions",
                null,
                null,
                "Atspm/Infrastructure/Configuration/SampleOptions.cs",
                10,
                [
                    new(
                        "Host",
                        "string",
                        "Not set",
                        true,
                        null)
                ])
        };
        var options = new CliOptions(
            directory.Path,
            output,
            "map.json",
            "https://github.com/example/source",
            "main",
            DateTimeOffset.UtcNow);

        var result = new MarkdownDocumentationGenerator().Generate(map, sections, options);

        Assert.Equal(new GenerationResult(1, 1), result);

        var actual = File.ReadAllText(System.IO.Path.Combine(output, "example.md"));
        Assert.Contains("- [SampleOptions:Nested](#sampleoptionsnested)", actual);
        Assert.Contains("## SampleOptions:Nested", actual);
        Assert.Contains("`SampleOptions__Nested__Host`", actual);
        Assert.Contains("\"SampleOptions\": {", actual);
        Assert.Contains("\"Nested\": {", actual);
        Assert.Contains("\"Host\": \"localhost\"", actual);
    }
}
