using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Utah.Udot.Atspm.Infrastructure.Configuration;
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

class Stuff
{
    static void Stuff(string[] args)
    {
        // 1. Setup Paths (Defaults to current directory if no arg provided)
        string searchPath = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

        // We'll look for the map file in the same folder as the .exe tool
        string toolDir = AppContext.BaseDirectory;
        string mapPath = Path.Combine(toolDir, "container-config-map.json");
        string outputDir = Path.Combine(searchPath, "docs", "containers");

        if (!File.Exists(mapPath))
        {
            Console.WriteLine($"Error: map file not found at {mapPath}");
            return;
        }

        Directory.CreateDirectory(outputDir);

        // 2. Parse all C# files in the target directory using Roslyn
        Console.WriteLine($"Scanning {searchPath}...");
        var configTypes = DiscoverConfigClasses(searchPath);

        // 3. Load the Mapping
        var containerMap = JsonSerializer.Deserialize<Dictionary<string, string[]>>(File.ReadAllText(mapPath))!;

        // 4. Generate Files
        foreach (var container in containerMap)
        {
            WriteContainerMarkdown(container.Key, container.Value, configTypes, outputDir, searchPath);
        }

        Console.WriteLine("Generation complete!");
    }

    static Dictionary<string, ClassDeclarationSyntax> DiscoverConfigClasses(string path)
    {
        var discovered = new Dictionary<string, ClassDeclarationSyntax>();

        // Recursively find all .cs files
        var files = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
            var root = tree.GetCompilationUnitRoot();

            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var cls in classes)
            {
                // Look for [ConfigurationSection("Name")]
                var attr = cls.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .FirstOrDefault(a => a.Name.ToString().Contains("ConfigurationSection"));

                if (attr != null)
                {
                    // Extract name from Attribute Argument: [ConfigurationSection("MySection")]
                    var sectionName = attr.ArgumentList?.Arguments.FirstOrDefault()
                        ?.Expression.ToString().Trim('"') ?? cls.Identifier.Text;

                    discovered[sectionName] = cls;
                }
            }
        }
        return discovered;
    }

    static void WriteContainerMarkdown(string name, string[] sections, Dictionary<string, ClassDeclarationSyntax> configTypes, string outputDir, string searchPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {name} — Configuration Reference\n");
        sb.AppendLine($"This page documents configuration options for **{name}**.\n");

        sb.AppendLine("## Table of Contents\n");
        foreach (var s in sections) sb.AppendLine($"- [{s}](#{s.ToLower()})");
        sb.AppendLine("\n---\n");

        foreach (var sectionName in sections)
        {
            if (configTypes.TryGetValue(sectionName, out var classNode))
            {
                WriteConfigSectionMarkdown(sb, classNode, sectionName, searchPath);
            }
        }

        File.WriteAllText(Path.Combine(outputDir, $"{name}.md"), sb.ToString());
    }

    static void WriteConfigSectionMarkdown(StringBuilder sb, ClassDeclarationSyntax cls, string sectionName, string searchPath)
    {
        // Get Attribute Data
        var attr = cls.AttributeLists.SelectMany(al => al.Attributes)
            .First(a => a.Name.ToString().Contains("ConfigurationSection"));

        // Extract "Description" named argument if it exists: [ConfigurationSection(Description = "Hello")]
        var descArg = attr.ArgumentList?.Arguments
            .FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == "Description")
            ?.Expression.ToString().Trim('"');

        sb.AppendLine($"## {sectionName}\n");
        if (!string.IsNullOrEmpty(descArg)) sb.AppendLine($"> {descArg}\n");

        // Get Relative Path for [View Source]
        var fullPath = cls.SyntaxTree.FilePath;
        var relativePath = Path.GetRelativePath(searchPath, fullPath).Replace("\\", "/");
        sb.AppendLine($"[View Source]({relativePath})\n");

        sb.AppendLine("<details><summary><strong>View Settings</strong></summary>\n");
        WriteSettingsTable(sb, cls, sectionName);
        sb.AppendLine("</details>\n---\n");
    }

    static void WriteSettingsTable(StringBuilder sb, ClassDeclarationSyntax cls, string sectionName)
    {
        sb.AppendLine("| Key | Type | Default | Env Var | Description |");
        sb.AppendLine("|-----|------|---------|---------|-------------|");

        foreach (var prop in cls.Members.OfType<PropertyDeclarationSyntax>())
        {
            var name = prop.Identifier.Text;
            var type = prop.Type.ToString();
            var defaultValue = prop.Initializer?.Value.ToString().Trim('"') ?? "";
            var envVar = $"{sectionName}__{name}";

            // Extract XML Summary
            var trivia = cls.GetLeadingTrivia().ToString(); // Simple version for brevity
            // For production, use a Regex or full Trivia analysis to clean up XML tags

            sb.AppendLine($"| `{name}` | `{type}` | `{defaultValue}` | `{envVar}` | Doc |");
        }
    }
}
