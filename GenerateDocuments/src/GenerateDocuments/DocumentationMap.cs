using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace AtspmDocsGenerator;

public sealed class DocumentationMap
{
    [JsonPropertyName("$schema")]
    public string? Schema { get; init; }

    public int SchemaVersion { get; init; }

    public IReadOnlyList<string> SourcePaths { get; init; } = [];

    public IReadOnlyList<ContainerDefinition> Containers { get; init; } = [];
}

public sealed class ContainerDefinition
{
    public string Name { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public IReadOnlyList<string> Sections { get; init; } = [];
}

public static class DocumentationMapLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public static DocumentationMap Load(string path)
    {
        var map = JsonSerializer.Deserialize<DocumentationMap>(
            File.ReadAllText(path),
            SerializerOptions)
            ?? throw new InvalidDataException("The configuration map is empty.");

        Validate(map);
        return map;
    }

    public static void Validate(DocumentationMap map)
    {
        if (map.SchemaVersion != 1)
        {
            throw new InvalidDataException(
                $"Unsupported configuration map schema version '{map.SchemaVersion}'. Expected version 1.");
        }

        if (map.SourcePaths.Count == 0 || map.SourcePaths.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidDataException("At least one non-empty source path is required.");
        }

        EnsureUnique(map.SourcePaths, "source path");

        if (map.Containers.Count == 0)
        {
            throw new InvalidDataException("At least one container definition is required.");
        }

        EnsureUnique(
            map.Containers.Select(container => container.Name),
            "container name");
        EnsureUnique(
            map.Containers.Select(container => container.Slug),
            "container slug");

        foreach (var container in map.Containers)
        {
            if (string.IsNullOrWhiteSpace(container.Name))
            {
                throw new InvalidDataException("Container names cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(container.Slug)
                || !Regex.IsMatch(container.Slug, "^[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*$", RegexOptions.CultureInvariant))
            {
                throw new InvalidDataException(
                    $"Container slug '{container.Slug}' must contain ASCII letters or numbers separated by single hyphens.");
            }

            if (container.Sections.Count == 0 || container.Sections.Any(string.IsNullOrWhiteSpace))
            {
                throw new InvalidDataException(
                    $"Container '{container.Name}' must define at least one configuration section.");
            }

            EnsureUnique(container.Sections, $"section in container '{container.Name}'");
        }
    }

    private static void EnsureUnique(IEnumerable<string> values, string valueDescription)
    {
        var duplicate = values
            .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
        {
            throw new InvalidDataException(
                $"Duplicate {valueDescription} '{duplicate.Key}' was found.");
        }
    }
}
