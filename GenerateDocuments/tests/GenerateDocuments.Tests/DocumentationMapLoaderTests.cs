namespace AtspmDocsGenerator.Tests;

public sealed class DocumentationMapLoaderTests
{
    [Fact]
    public void ValidateRejectsDuplicateSlugs()
    {
        var map = new DocumentationMap
        {
            SchemaVersion = 1,
            SourcePaths = ["src"],
            Containers =
            [
                new ContainerDefinition
                {
                    Name = "First",
                    Slug = "shared",
                    Sections = ["FirstOptions"]
                },
                new ContainerDefinition
                {
                    Name = "Second",
                    Slug = "shared",
                    Sections = ["SecondOptions"]
                }
            ]
        };

        var exception = Assert.Throws<InvalidDataException>(
            () => DocumentationMapLoader.Validate(map));

        Assert.Contains("Duplicate container slug", exception.Message);
    }

    [Fact]
    public void ValidateRejectsUnsafeSlugs()
    {
        var map = new DocumentationMap
        {
            SchemaVersion = 1,
            SourcePaths = ["src"],
            Containers =
            [
                new ContainerDefinition
                {
                    Name = "Example",
                    Slug = "../example",
                    Sections = ["ExampleOptions"]
                }
            ]
        };

        var exception = Assert.Throws<InvalidDataException>(
            () => DocumentationMapLoader.Validate(map));

        Assert.Contains("ASCII letters", exception.Message);
    }
}
