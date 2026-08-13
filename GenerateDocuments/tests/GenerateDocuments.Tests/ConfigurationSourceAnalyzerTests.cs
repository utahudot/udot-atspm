namespace AtspmDocsGenerator.Tests;

public sealed class ConfigurationSourceAnalyzerTests
{
    [Fact]
    public void AnalyzeReadsDocumentationTypesAndInitializerExpressions()
    {
        using var directory = new TemporaryDirectory();
        directory.WriteFile(
            "Options.cs",
            """
            using System;
            using System.Collections.Generic;
            using System.ComponentModel.DataAnnotations;

            /// <summary>
            /// Configuration for a sample process.
            /// </summary>
            [ConfigurationSection(nameof(SampleOptions), "Sample options")]
            public class SampleOptions
            {
                /// <summary>
                /// Dates containing <see cref="DateTime"/> values.
                /// </summary>
                public IEnumerable<DateTime?> Dates { get; set; } = [];

                /// <summary>Output path.</summary>
                [Required]
                public string Path { get; set; } = System.IO.Path.GetTempPath();

                public required NestedOptions Nested { get; set; } = new();

                /// <inheritdoc cref="NestedOptions"/>
                public NestedOptions Inherited { get; set; } = new NestedOptions();

                private string Hidden { get; set; } = "hidden";

                string ImplicitlyPrivate { get; set; } = "hidden";

                public static string StaticValue { get; set; } = "static";
            }

            public class NestedOptions;
            """);

        var sections = new ConfigurationSourceAnalyzer().Analyze(directory.Path, ["."]);

        var section = Assert.Single(sections).Value;
        Assert.Equal("SampleOptions", section.SectionName);
        Assert.Equal("Sample options", section.AttributeDescription);
        Assert.Equal("Configuration for a sample process.", section.Summary);
        Assert.Equal("Options.cs", section.SourcePath);
        Assert.Equal(8, section.SourceLine);
        Assert.Equal(4, section.Properties.Count);

        var dates = Assert.Single(section.Properties, property => property.Name == "Dates");
        Assert.Equal("IEnumerable<DateTime?>", dates.TypeName);
        Assert.Equal("[]", dates.DefaultExpression);
        Assert.Equal("Dates containing DateTime values.", dates.Summary);

        var path = Assert.Single(section.Properties, property => property.Name == "Path");
        Assert.True(path.IsRequired);
        Assert.Equal("System.IO.Path.GetTempPath()", path.DefaultExpression);
        Assert.DoesNotContain(System.IO.Path.GetTempPath(), path.DefaultExpression);

        var nested = Assert.Single(section.Properties, property => property.Name == "Nested");
        Assert.True(nested.IsRequired);
        Assert.Equal("new()", nested.DefaultExpression);

        var inherited = Assert.Single(section.Properties, property => property.Name == "Inherited");
        Assert.Equal("See NestedOptions.", inherited.Summary);
    }

    [Fact]
    public void AnalyzeBuildsIndexedAndNestedEnvironmentVariableSuffixes()
    {
        using var directory = new TemporaryDirectory();
        directory.WriteFile(
            "Options.cs",
            """
            [ConfigurationSection("Sample")]
            public class SampleOptions
            {
                public string[] Names { get; set; }
                public NestedOptions Nested { get; set; }
                public NestedOptions[] Items { get; set; }
            }

            public class NestedOptions
            {
                public string Host { get; set; }
                private string Hidden { get; set; }
            }
            """);

        var section = Assert.Single(
            new ConfigurationSourceAnalyzer().Analyze(directory.Path, ["."])).Value;

        Assert.Equal(["Names__0"], section.Properties.Single(property => property.Name == "Names").EnvironmentVariableSuffixes);
        Assert.Equal(["Nested__Host"], section.Properties.Single(property => property.Name == "Nested").EnvironmentVariableSuffixes);
        Assert.Equal(["Items__0__Host"], section.Properties.Single(property => property.Name == "Items").EnvironmentVariableSuffixes);
    }

    [Fact]
    public void AnalyzeSupportsNamedAttributeArgumentsAndRecords()
    {
        using var directory = new TemporaryDirectory();
        directory.WriteFile(
            "NamedOptions.cs",
            """
            [ConfigurationSection(sectionName: "ExplicitSection", description: null)]
            public record NamedOptions
            {
                public bool Enabled { get; init; }
            }
            """);

        var sections = new ConfigurationSourceAnalyzer().Analyze(directory.Path, ["."]);

        var section = Assert.Single(sections).Value;
        Assert.Equal("ExplicitSection", section.SectionName);
        Assert.Null(section.AttributeDescription);
        Assert.Equal("Not set", Assert.Single(section.Properties).DefaultExpression);
    }

    [Fact]
    public void AnalyzeRejectsSourcePathsOutsideTheRepository()
    {
        using var directory = new TemporaryDirectory();

        var exception = Assert.Throws<InvalidDataException>(
            () => new ConfigurationSourceAnalyzer().Analyze(directory.Path, [".."]));

        Assert.Contains("inside the source root", exception.Message);
    }
}
