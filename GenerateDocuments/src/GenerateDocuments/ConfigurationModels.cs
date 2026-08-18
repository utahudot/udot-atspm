namespace AtspmDocsGenerator;

public sealed record ConfigurationSection(
    string SectionName,
    string? AttributeDescription,
    string? Summary,
    string SourcePath,
    int SourceLine,
    IReadOnlyList<ConfigurationProperty> Properties);

public sealed record ConfigurationProperty(
    string Name,
    string TypeName,
    string DefaultExpression,
    bool IsRequired,
    string? Summary,
    IReadOnlyList<string>? EnvironmentVariableSuffixes = null,
    IReadOnlyList<string>? Options = null);

public sealed record GenerationResult(int PageCount, int DocumentedSectionCount);
