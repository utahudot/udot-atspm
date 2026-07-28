namespace AtspmDocsGenerator.Tests;

public sealed class CliOptionsTests
{
    [Fact]
    public void ParseNormalizesPathsAndRepositoryUrl()
    {
        using var directory = new TemporaryDirectory();
        var mapPath = directory.WriteFile("map.json", "{}");

        var result = CliOptions.Parse(
        [
            "--source-root", directory.Path,
            "--output-root", System.IO.Path.Combine(directory.Path, "output"),
            "--map", mapPath,
            "--repository-url", "https://github.com/example/source.git/",
            "--repository-ref", "abc123"
        ]);

        Assert.Null(result.Error);
        Assert.NotNull(result.Options);
        Assert.Equal("https://github.com/example/source", result.Options.RepositoryUrl);
        Assert.Equal("abc123", result.Options.RepositoryRef);
        Assert.True(System.IO.Path.IsPathFullyQualified(result.Options.OutputRoot));
    }

    [Fact]
    public void ParseReportsMissingRequiredOptions()
    {
        var result = CliOptions.Parse(["--repository-ref", "main"]);

        Assert.Null(result.Options);
        Assert.Contains("--source-root", result.Error);
    }
}
