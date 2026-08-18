using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace AtspmDocsGenerator;

using static DocumentationText;

public sealed partial class MarkdownDocumentationGenerator
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions ExampleJsonOptions = new()
    {
        WriteIndented = true
    };

    public GenerationResult Generate(
        DocumentationMap map,
        IReadOnlyDictionary<string, ConfigurationSection> availableSections,
        CliOptions options)
    {
        DocumentationMapLoader.Validate(map);
        var documentedSections = new HashSet<string>(StringComparer.Ordinal);
        var pages = map.Containers
            .Select(container =>
            {
                var sections = container.Sections
                    .Select(sectionName => ResolveMappedSection(container.Name, sectionName, availableSections))
                    .ToArray();
                documentedSections.UnionWith(sections.Select(section => section.SectionName));
                return new
                {
                    FileName = $"{container.Slug}.md",
                    Contents = BuildContainerPage(container, sections, options)
                };
            })
            .Append(new
            {
                FileName = "index.md",
                Contents = BuildIndexPage(map.Containers, options)
            })
            .ToArray();

        var expectedFiles = map.Containers
            .Select(container => $"{container.Slug}.md")
            .Append("index.md")
            .Append("log-messages.md")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Directory.CreateDirectory(options.OutputRoot);
        foreach (var existingFile in Directory.EnumerateFiles(options.OutputRoot, "*.md"))
        {
            if (!expectedFiles.Contains(Path.GetFileName(existingFile)))
            {
                File.Delete(existingFile);
            }
        }

        foreach (var page in pages)
        {
            WriteFile(
                Path.Combine(options.OutputRoot, page.FileName),
                page.Contents);
        }

        return new GenerationResult(map.Containers.Count, documentedSections.Count);
    }

    private static string BuildIndexPage(
        IEnumerable<ContainerDefinition> containers,
        CliOptions options)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Configuration reference");
        builder.AppendLine();
        builder.AppendLine(
            "Configuration sections are grouped by the ATSPM service or utility that consumes them.");
        builder.AppendLine();
        AppendGenerationTimestamp(builder, options);
        builder.AppendLine("## Containers");
        builder.AppendLine();

        foreach (var container in containers)
        {
            builder.AppendLine($"- [{EscapeText(container.Name)}]({container.Slug}.md)");
        }

        builder.AppendLine();
        builder.AppendLine("## References");
        builder.AppendLine();
        builder.AppendLine("- [Log messages](log-messages.md)");

        return NormalizeLineEndings(builder.ToString());
    }

    private static string BuildContainerPage(
        ContainerDefinition container,
        IReadOnlyList<ConfigurationSection> sections,
        CliOptions options)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"# {EscapeText(container.Name)} configuration");
        builder.AppendLine();
        builder.AppendLine(
            $"Configuration options available to the **{EscapeText(container.Name)}** container.");
        builder.AppendLine();
        AppendGenerationTimestamp(builder, options);
        builder.AppendLine("## Contents");
        builder.AppendLine();

        foreach (var section in sections)
        {
            builder.AppendLine(
                $"- [{EscapeText(section.SectionName)}](#{ToAnchor(section.SectionName)})");
        }

        builder.AppendLine();

        foreach (var section in sections)
        {
            AppendSection(builder, section, options);
        }

        AppendExampleConfiguration(builder, sections);

        return NormalizeLineEndings(builder.ToString());
    }

    private static void AppendExampleConfiguration(
        StringBuilder builder,
        IEnumerable<ConfigurationSection> sections)
    {
        var configuration = new JsonObject();

        foreach (var section in sections)
        {
            var path = section.SectionName.Split(':', StringSplitOptions.RemoveEmptyEntries);
            var current = configuration;

            foreach (var segment in path)
            {
                if (current[segment] is not JsonObject child)
                {
                    child = new JsonObject();
                    current[segment] = child;
                }

                current = child;
            }

            foreach (var property in section.Properties)
            {
                current[property.Name] = CreateExampleValue(property);
            }
        }

        builder.AppendLine("## Example JSON configuration");
        builder.AppendLine();
        builder.AppendLine(
            "This example includes every documented setting. Replace placeholder secrets, URLs, paths, and connection details before use.");
        builder.AppendLine();
        builder.AppendLine("```json");
        builder.AppendLine(configuration.ToJsonString(ExampleJsonOptions));
        builder.AppendLine("```");
    }

    private static JsonNode? CreateExampleValue(ConfigurationProperty property)
    {
        var name = property.Name;
        var typeName = property.TypeName.TrimEnd('?');
        var defaultExpression = property.DefaultExpression;

        if (name.Contains("Password", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Secret", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Key", StringComparison.OrdinalIgnoreCase))
        {
            return JsonValue.Create("replace-with-a-secret");
        }

        if (property.TypeName.EndsWith("?", StringComparison.Ordinal)
            && defaultExpression == "Not set")
        {
            return null;
        }

        if (IsCollectionType(typeName))
        {
            return new JsonArray();
        }

        if (typeName.Contains("Dictionary<", StringComparison.Ordinal)
            || typeName.Contains("IDictionary<", StringComparison.Ordinal))
        {
            return new JsonObject();
        }

        if (typeName == "string")
        {
            return JsonValue.Create(CreateExampleString(name, defaultExpression));
        }

        if (typeName == "bool")
        {
            return JsonValue.Create(bool.TryParse(defaultExpression, out var value) && value);
        }

        if (typeName is "byte" or "short" or "int" or "long")
        {
            var numeric = defaultExpression.TrimEnd('L', 'l');
            return JsonValue.Create(
                long.TryParse(numeric, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                    ? value
                    : 0);
        }

        if (typeName is "float" or "double" or "decimal")
        {
            var numeric = defaultExpression.TrimEnd('F', 'f', 'D', 'd', 'M', 'm');
            return JsonValue.Create(
                decimal.TryParse(numeric, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                    ? value
                    : 0);
        }

        if (typeName is "DateTime" or "DateTimeOffset")
        {
            return JsonValue.Create("2026-01-01T00:00:00Z");
        }

        if (typeName == "TimeSpan")
        {
            return JsonValue.Create("00:05:00");
        }

        if (typeName == "DirectoryInfo")
        {
            return JsonValue.Create("./data");
        }

        if (typeName == "RepositoryConfiguration")
        {
            return new JsonObject
            {
                ["Provider"] = "PostgreSql",
                ["ConnectionString"] = "Host=localhost;Port=5432;Database=atspm;Username=atspm;Password=replace-with-a-secret"
            };
        }

        if (defaultExpression.Contains('.', StringComparison.Ordinal)
            && !defaultExpression.Contains('(', StringComparison.Ordinal))
        {
            return JsonValue.Create(defaultExpression.Split('.').Last());
        }

        return new JsonObject();
    }

    private static bool IsCollectionType(string typeName) =>
        typeName.EndsWith("[]", StringComparison.Ordinal)
        || typeName.StartsWith("IEnumerable<", StringComparison.Ordinal)
        || typeName.StartsWith("IReadOnlyCollection<", StringComparison.Ordinal)
        || typeName.StartsWith("IReadOnlyList<", StringComparison.Ordinal)
        || typeName.StartsWith("ICollection<", StringComparison.Ordinal)
        || typeName.StartsWith("IList<", StringComparison.Ordinal)
        || typeName.StartsWith("List<", StringComparison.Ordinal)
        || typeName.StartsWith("HashSet<", StringComparison.Ordinal);

    private static string CreateExampleString(string name, string defaultExpression)
    {
        if (defaultExpression.StartsWith('"') && defaultExpression.EndsWith('"'))
        {
            try
            {
                return JsonSerializer.Deserialize<string>(defaultExpression) ?? string.Empty;
            }
            catch (JsonException)
            {
                // Fall through to a descriptive placeholder.
            }
        }

        return name switch
        {
            "Host" => "localhost",
            "Database" => "atspm",
            "User" or "UserName" => "atspm",
            "Issuer" => "https://identity.example.com",
            "Audience" => "atspm",
            "Authority" => "https://identity-provider.example.com",
            "ClientId" => "atspm",
            "CallbackPath" => "/signin-oidc",
            "Path" or "BasePath" => "./data",
            "DefaultEmailAddress" => "atspm@example.com",
            "Website" => "https://atspm.example.com",
            "TimeZoneId" => "America/Denver",
            "FileFormat" => "csv",
            "DateTimeFormat" => "yyyy-MM-dd HH:mm:ss",
            _ when name.EndsWith("Url", StringComparison.OrdinalIgnoreCase) => "https://example.com",
            _ => "replace-me"
        };
    }

    private static void AppendGenerationTimestamp(StringBuilder builder, CliOptions options)
    {
        builder.AppendLine(GenerationTimestamp(options.GeneratedAt));
        builder.AppendLine();
    }

    private static void AppendSection(
        StringBuilder builder,
        ConfigurationSection section,
        CliOptions options)
    {
        builder.AppendLine($"## {EscapeText(section.SectionName)}");
        builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(section.AttributeDescription))
        {
            builder.AppendLine($"> {EscapeText(section.AttributeDescription)}");
            builder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(section.Summary)
            && !string.Equals(
                section.AttributeDescription,
                section.Summary,
                StringComparison.Ordinal))
        {
            builder.AppendLine(EscapeText(section.Summary));
            builder.AppendLine();
        }

        var sourceUrl = BuildRepositoryUrl(
            options.RepositoryUrl,
            "blob",
            options.RepositoryRef,
            section.SourcePath) + $"#L{section.SourceLine}";
        builder.AppendLine($"[View source]({sourceUrl})");
        builder.AppendLine();
        builder.AppendLine("| Setting | Type | Default | Required | Options | Environment variable | Description |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");

        foreach (var property in section.Properties)
        {
            var environmentVariables = property.EnvironmentVariableSuffixes is { Count: > 0 }
                ? property.EnvironmentVariableSuffixes
                : [property.Name];
            var environmentVariable = string.Join(
                "<br>",
                environmentVariables.Select(suffix =>
                    $"`{EscapeCode(MappedSectionName.ToEnvironmentVariablePrefix(section.SectionName))}__{EscapeCode(suffix)}`"));
            var enumOptions = property.Options is { Count: > 0 }
                ? string.Join("<br>", property.Options.Select(option => $"`{EscapeCode(option)}`"))
                : string.Empty;
            builder.AppendLine(
                $"| `{EscapeCode(property.Name)}` " +
                $"| `{EscapeCode(property.TypeName)}` " +
                $"| `{EscapeCode(property.DefaultExpression)}` " +
                $"| {(property.IsRequired ? "Yes" : "No")} " +
                $"| {enumOptions} " +
                $"| {environmentVariable} " +
                $"| {EscapeTableText(property.Summary ?? string.Empty)} |");
        }

        if (section.Properties.Count == 0)
        {
            builder.AppendLine("| _No public configuration properties_ |  |  |  |  |  |  |");
        }

        builder.AppendLine();
    }

    private static ConfigurationSection ResolveMappedSection(
        string containerName,
        string mappedSectionName,
        IReadOnlyDictionary<string, ConfigurationSection> availableSections)
    {
        if (availableSections.TryGetValue(mappedSectionName, out var section))
        {
            return section;
        }

        var baseSectionName = MappedSectionName.GetBaseName(mappedSectionName);
        if (!string.Equals(baseSectionName, mappedSectionName, StringComparison.Ordinal)
            && availableSections.TryGetValue(baseSectionName, out section))
        {
            return section with { SectionName = mappedSectionName };
        }

        throw new InvalidDataException(
            $"Container '{containerName}' references unknown configuration section '{mappedSectionName}'.");
    }

    private static string EscapeTableText(string value) => EscapeText(value);

    private static string ToAnchor(string value)
    {
        var anchor = NonAnchorCharacterRegex()
            .Replace(value.Replace(":", "", StringComparison.Ordinal).ToLowerInvariant(), "-")
            .Trim('-');
        return RepeatedHyphenRegex().Replace(anchor, "-");
    }

    private static void WriteFile(string path, string contents) =>
        File.WriteAllText(path, contents, Utf8WithoutBom);

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAnchorCharacterRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex RepeatedHyphenRegex();
}
