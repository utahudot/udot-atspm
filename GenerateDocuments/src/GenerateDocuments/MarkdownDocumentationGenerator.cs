using System.Text;
using System.Text.RegularExpressions;

namespace AtspmDocsGenerator;

public sealed partial class MarkdownDocumentationGenerator
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    public GenerationResult Generate(
        DocumentationMap map,
        IReadOnlyDictionary<string, ConfigurationSection> availableSections,
        CliOptions options)
    {
        DocumentationMapLoader.Validate(map);
        Directory.CreateDirectory(options.OutputRoot);

        var expectedFiles = map.Containers
            .Select(container => $"{container.Slug}.md")
            .Append("index.md")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var existingFile in Directory.EnumerateFiles(options.OutputRoot, "*.md"))
        {
            if (!expectedFiles.Contains(Path.GetFileName(existingFile)))
            {
                File.Delete(existingFile);
            }
        }

        var documentedSections = new HashSet<string>(StringComparer.Ordinal);

        foreach (var container in map.Containers)
        {
            var sections = container.Sections
                .Select(sectionName => ResolveMappedSection(container.Name, sectionName, availableSections))
                .ToArray();

            foreach (var section in sections)
            {
                documentedSections.Add(section.SectionName);
            }

            WriteFile(
                Path.Combine(options.OutputRoot, $"{container.Slug}.md"),
                BuildContainerPage(container, sections, options));
        }

        WriteFile(
            Path.Combine(options.OutputRoot, "index.md"),
            BuildIndexPage(map.Containers, options));

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
        AppendGeneratedFrom(builder, options);
        builder.AppendLine("## Containers");
        builder.AppendLine();

        foreach (var container in containers)
        {
            builder.AppendLine($"- [{EscapeText(container.Name)}]({container.Slug}.md)");
        }

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
        AppendGeneratedFrom(builder, options);
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

        return NormalizeLineEndings(builder.ToString());
    }

    private static void AppendGeneratedFrom(StringBuilder builder, CliOptions options)
    {
        var treeUrl = BuildRepositoryUrl(
            options.RepositoryUrl,
            "tree",
            options.RepositoryRef);
        builder.AppendLine(
            $"Generated from [{RepositoryLabel(options.RepositoryUrl)} at " +
            $"`{EscapeCode(options.RepositoryRef)}`]({treeUrl}).");
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
        builder.AppendLine("| Setting | Type | Default | Required | Environment variable | Description |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- |");

        foreach (var property in section.Properties)
        {
            var environmentVariable = $"{ToEnvironmentVariablePrefix(section.SectionName)}__{property.Name}";
            builder.AppendLine(
                $"| `{EscapeCode(property.Name)}` " +
                $"| `{EscapeCode(property.TypeName)}` " +
                $"| `{EscapeCode(property.DefaultExpression)}` " +
                $"| {(property.IsRequired ? "Yes" : "No")} " +
                $"| `{EscapeCode(environmentVariable)}` " +
                $"| {EscapeTableText(property.Summary ?? string.Empty)} |");
        }

        if (section.Properties.Count == 0)
        {
            builder.AppendLine("| _No public configuration properties_ |  |  |  |  |  |");
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

        var baseSectionName = GetBaseSectionName(mappedSectionName);
        if (!string.Equals(baseSectionName, mappedSectionName, StringComparison.Ordinal)
            && availableSections.TryGetValue(baseSectionName, out section))
        {
            return section with { SectionName = mappedSectionName };
        }

        throw new InvalidDataException(
            $"Container '{containerName}' references unknown configuration section '{mappedSectionName}'.");
    }

    private static string GetBaseSectionName(string mappedSectionName)
    {
        var separatorIndex = mappedSectionName.IndexOf(':', StringComparison.Ordinal);
        return separatorIndex < 0
            ? mappedSectionName
            : mappedSectionName[..separatorIndex];
    }

    private static string ToEnvironmentVariablePrefix(string sectionName) =>
        sectionName.Replace(":", "__", StringComparison.Ordinal);

    private static string BuildRepositoryUrl(
        string repositoryUrl,
        string operation,
        string repositoryRef,
        string? relativePath = null)
    {
        var builder = new StringBuilder(repositoryUrl.TrimEnd('/'));
        builder.Append('/');
        builder.Append(operation);
        builder.Append('/');
        builder.Append(Uri.EscapeDataString(repositoryRef));

        if (!string.IsNullOrWhiteSpace(relativePath))
        {
            foreach (var segment in relativePath.Replace('\\', '/').Split('/'))
            {
                builder.Append('/');
                builder.Append(Uri.EscapeDataString(segment));
            }
        }

        return builder.ToString();
    }

    private static string RepositoryLabel(string repositoryUrl)
    {
        if (!Uri.TryCreate(repositoryUrl, UriKind.Absolute, out var uri))
        {
            return repositoryUrl;
        }

        var label = uri.AbsolutePath.Trim('/');
        return label.EndsWith(".git", StringComparison.OrdinalIgnoreCase)
            ? label[..^4]
            : label;
    }

    private static string EscapeText(string value) =>
        value.Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);

    private static string EscapeTableText(string value) => EscapeText(value);

    private static string EscapeCode(string value) =>
        EscapeText(value).Replace("`", "\\`", StringComparison.Ordinal);

    private static string ToAnchor(string value)
    {
        var anchor = NonAnchorCharacterRegex()
            .Replace(value.Replace(":", "", StringComparison.Ordinal).ToLowerInvariant(), "-")
            .Trim('-');
        return RepeatedHyphenRegex().Replace(anchor, "-");
    }

    private static string NormalizeLineEndings(string value) =>
        value.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');

    private static void WriteFile(string path, string contents) =>
        File.WriteAllText(path, contents, Utf8WithoutBom);

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAnchorCharacterRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex RepeatedHyphenRegex();
}
