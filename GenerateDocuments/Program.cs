using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Utah.Udot.Atspm.Infrastructure.Extensions;



var repoRoot = GetRepoRoot();
var outputDir = Path.Combine(repoRoot, "docs", "containers");
Directory.CreateDirectory(outputDir);

var assembly = typeof(ConfigurationSectionAttribute).Assembly;
var xml = new XmlDocReader(assembly);

var mapPath = Path.Combine(AppContext.BaseDirectory, "container-config-map.json");
var containerMap = JsonSerializer.Deserialize<Dictionary<string, string[]>>(File.ReadAllText(mapPath))!;


var configTypes = assembly
    .GetTypes()
    .Where(t => t.GetCustomAttribute<ConfigurationSectionAttribute>() != null)
    .ToDictionary(
        t => t.GetCustomAttribute<ConfigurationSectionAttribute>()!.SectionName,
        t => t
    );

foreach (var container in containerMap)
{
    WriteContainerMarkdown(container.Key, container.Value, configTypes, xml, outputDir);
}




static string GetRepoRoot()
{
    // AppContext.BaseDirectory = .../GenerateDocuments/bin/Debug/net8.0/
    // Go up 4 levels to reach repo root
    return Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.Parent!.FullName;
}





static void WriteContainerMarkdown(
    string containerName,
    string[] sections,
    Dictionary<string, Type> configTypes,
    XmlDocReader xml,
    string outputDir)
{
    var sb = new StringBuilder();

    sb.AppendLine($"# {containerName} — Configuration Reference\n");
    sb.AppendLine($"This page documents all configuration options available to the **{containerName}** container.\n");

    // Table of contents
    sb.AppendLine("## Table of Contents\n");
    foreach (var section in sections)
        sb.AppendLine($"- [{section}](#{section.ToLower()})");
    sb.AppendLine("\n---\n");

    foreach (var sectionName in sections)
    {
        if (!configTypes.TryGetValue(sectionName, out var type))
            continue;

        WriteConfigSectionMarkdown(sb, type, xml);
    }

    var filePath = Path.Combine(outputDir, $"{containerName}.md");
    File.WriteAllText(filePath, sb.ToString());
}

static void WriteConfigSectionMarkdown(
    StringBuilder sb,
    Type type,
    XmlDocReader xml)
{
    var attr = type.GetCustomAttribute<ConfigurationSectionAttribute>()!;
    var typeComment = xml.GetTypeComment(type);

    sb.AppendLine($"## {attr.SectionName}\n");

    //add description
    if (!string.IsNullOrWhiteSpace(attr.Description))
        sb.AppendLine($"> {attr.Description}");

    if (!string.IsNullOrWhiteSpace(typeComment))
        sb.AppendLine($"> {typeComment}");

    sb.AppendLine();

    //add link to source code
    var sourcePath = $"src/MyApp.Configuration/{type.Name}.cs";
    sb.AppendLine($"[View Source]({sourcePath})\n");

    //collapseable settings table
    sb.AppendLine("<details>");
    sb.AppendLine($"<summary><strong>View Settings</strong></summary>\n");

    WriteSettingsTable(sb, type, xml);

    sb.AppendLine("</details>\n");
    sb.AppendLine("---\n");
}


static void WriteSettingsTable(
    StringBuilder sb,
    Type type,
    XmlDocReader xml)
{
    sb.AppendLine("| Key | Type | Default | Required | Env Var | Description |");
    sb.AppendLine("|-----|------|---------|----------|---------|-------------|");

    var instance = Activator.CreateInstance(type);

    foreach (var prop in type.GetProperties())
    {
        var defaultValue = prop.GetValue(instance)?.ToString() ?? "";
        var description = xml.GetPropertyComment(prop) ?? "";

        var required = prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() != null
            ? "Yes"
            : "No";

        var envVar = GenerateEnvVarName(type, prop);

        sb.AppendLine(
            $"| `{prop.Name}` | `{prop.PropertyType.Name}` | `{defaultValue}` | {required} | `{envVar}` | {description} |"
        );
    }

    sb.AppendLine();
}

static string GenerateEnvVarName(Type type, PropertyInfo prop)
{
    var section = type.GetCustomAttribute<ConfigurationSectionAttribute>()!.SectionName;
    return $"{section}__{prop.Name}";
}














[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ConfigurationSectionAttribute : Attribute
{
    public string SectionName { get; }
    public string? Description { get; }

    public ConfigurationSectionAttribute(string sectionName, string? description = null)
    {
        SectionName = sectionName;
        Description = description;
    }
}

[ConfigurationSection("Redis", "Redis cache configuration")]
public class RedisOptions
{
    /// <summary>
    /// The hostname of the Redis server.
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// The port Redis listens on.
    /// </summary>
    public int Port { get; set; } = 6379;

    /// <summary>
    /// Optional password for Redis authentication.
    /// </summary>
    public string? Password { get; set; }
}
