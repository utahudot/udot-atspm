using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AtspmDocsGenerator;

public sealed partial class ConfigurationSourceAnalyzer
{
    private static readonly CSharpParseOptions ParseOptions =
        new(languageVersion: LanguageVersion.Latest, documentationMode: DocumentationMode.Parse);

    public IReadOnlyDictionary<string, ConfigurationSection> Analyze(
        string sourceRoot,
        IEnumerable<string> sourcePaths)
    {
        var normalizedRoot = Path.GetFullPath(sourceRoot);
        var sections = new Dictionary<string, ConfigurationSection>(StringComparer.Ordinal);
        var types = new List<TypeDeclarationSyntax>();

        foreach (var file in EnumerateSourceFiles(normalizedRoot, sourcePaths))
        {
            var source = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(
                source,
                ParseOptions,
                path: file,
                encoding: Encoding.UTF8);
            var root = tree.GetCompilationUnitRoot();

            types.AddRange(root.DescendantNodes().OfType<TypeDeclarationSyntax>());
        }

        var typesByName = types
            .GroupBy(type => type.Identifier.ValueText, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        foreach (var type in types)
        {
            var attribute = type.AttributeLists
                .SelectMany(list => list.Attributes)
                .FirstOrDefault(IsConfigurationSectionAttribute);

            if (attribute is null)
            {
                continue;
            }

            var section = CreateSection(type, attribute, normalizedRoot, typesByName);
            if (!sections.TryAdd(section.SectionName, section))
            {
                throw new InvalidDataException(
                    $"Configuration section '{section.SectionName}' is declared more than once.");
            }
        }

        return sections;
    }

    private static IEnumerable<string> EnumerateSourceFiles(
        string sourceRoot,
        IEnumerable<string> sourcePaths)
    {
        var files = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var sourcePath in sourcePaths)
        {
            var fullPath = SourcePath.ResolveWithinRoot(
                sourceRoot,
                sourcePath,
                "Configured source path");

            if (!Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException(
                    $"Configured source path does not exist: {fullPath}");
            }

            foreach (var file in Directory.EnumerateFiles(fullPath, "*.cs", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceRoot, file);
                if (relativePath
                    .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Any(part => part is ".git" or "bin" or "obj"))
                {
                    continue;
                }

                files.Add(Path.GetFullPath(file));
            }
        }

        return files;
    }

    private static ConfigurationSection CreateSection(
        TypeDeclarationSyntax type,
        AttributeSyntax attribute,
        string sourceRoot,
        IReadOnlyDictionary<string, TypeDeclarationSyntax> typesByName)
    {
        var arguments = attribute.ArgumentList?.Arguments ?? default;
        var sectionArgument = FindArgument(arguments, "sectionName", 0)
            ?? throw new InvalidDataException(
                $"ConfigurationSection on '{type.Identifier.ValueText}' does not define a section name.");

        var sectionName = ReadSectionName(sectionArgument.Expression);
        var descriptionArgument = FindArgument(arguments, "description", 1);
        var description = descriptionArgument is null
            ? null
            : ReadOptionalString(descriptionArgument.Expression);

        var properties = type.Members
            .OfType<PropertyDeclarationSyntax>()
            .Where(IsDocumentedProperty)
            .Select(property => CreateProperty(
                property,
                typesByName,
                new HashSet<string>(StringComparer.Ordinal) { type.Identifier.ValueText }))
            .ToArray();

        var line = type.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        var relativePath = Path.GetRelativePath(sourceRoot, type.SyntaxTree.FilePath)
            .Replace('\\', '/');

        return new ConfigurationSection(
            sectionName,
            description,
            XmlDocumentationReader.ReadSummary(type),
            relativePath,
            line,
            properties);
    }

    private static AttributeArgumentSyntax? FindArgument(
        SeparatedSyntaxList<AttributeArgumentSyntax> arguments,
        string name,
        int positionalIndex)
    {
        var named = arguments.FirstOrDefault(argument =>
            string.Equals(
                argument.NameColon?.Name.Identifier.ValueText
                    ?? argument.NameEquals?.Name.Identifier.ValueText,
                name,
                StringComparison.OrdinalIgnoreCase));

        if (named is not null)
        {
            return named;
        }

        return arguments
            .Where(argument => argument.NameColon is null && argument.NameEquals is null)
            .ElementAtOrDefault(positionalIndex);
    }

    private static string ReadSectionName(ExpressionSyntax expression)
    {
        if (expression is LiteralExpressionSyntax literal
            && literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return literal.Token.ValueText;
        }

        if (expression is InvocationExpressionSyntax invocation
            && invocation.Expression is IdentifierNameSyntax identifier
            && identifier.Identifier.ValueText == "nameof"
            && invocation.ArgumentList.Arguments.Count == 1)
        {
            return invocation.ArgumentList.Arguments[0].Expression
                .ToString()
                .Split('.')
                .Last();
        }

        throw new InvalidDataException(
            $"Unsupported configuration section expression '{expression}'. Use a string literal or nameof(...).");
    }

    private static string? ReadOptionalString(ExpressionSyntax expression)
    {
        if (expression.IsKind(SyntaxKind.NullLiteralExpression))
        {
            return null;
        }

        if (expression is LiteralExpressionSyntax literal
            && literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return literal.Token.ValueText;
        }

        throw new InvalidDataException(
            $"Unsupported configuration description expression '{expression}'. Use a string literal or null.");
    }

    private static bool IsConfigurationSectionAttribute(AttributeSyntax attribute)
    {
        var name = attribute.Name.ToString().Split('.').Last();
        return name is "ConfigurationSection" or "ConfigurationSectionAttribute";
    }

    private static bool IsDocumentedProperty(PropertyDeclarationSyntax property)
    {
        var isStatic = property.Modifiers.Any(SyntaxKind.StaticKeyword);
        var isPublic = property.Modifiers.Any(SyntaxKind.PublicKeyword);

        return isPublic && !isStatic;
    }

    private static ConfigurationProperty CreateProperty(
        PropertyDeclarationSyntax property,
        IReadOnlyDictionary<string, TypeDeclarationSyntax> typesByName,
        IReadOnlySet<string> visitedTypes)
    {
        var typeName = property.Type
            .WithoutTrivia()
            .NormalizeWhitespace()
            .ToFullString();
        var defaultExpression = property.Initializer?.Value
            .WithoutTrivia()
            .NormalizeWhitespace()
            .ToFullString()
            ?? "Not set";
        var isRequired = property.Modifiers.Any(SyntaxKind.RequiredKeyword)
            || property.AttributeLists
                .SelectMany(list => list.Attributes)
                .Any(attribute =>
                {
                    var name = attribute.Name.ToString().Split('.').Last();
                    return name is "Required" or "RequiredAttribute";
                });

        return new ConfigurationProperty(
            property.Identifier.ValueText,
            typeName,
            defaultExpression,
            isRequired,
            XmlDocumentationReader.ReadSummary(property, includeInheritDoc: true),
            GetEnvironmentVariableSuffixes(property, typesByName, visitedTypes));
    }

    private static IReadOnlyList<string> GetEnvironmentVariableSuffixes(
        PropertyDeclarationSyntax property,
        IReadOnlyDictionary<string, TypeDeclarationSyntax> typesByName,
        IReadOnlySet<string> visitedTypes)
    {
        var propertyName = property.Identifier.ValueText;
        var (typeName, isCollection, isDictionary) = DescribeType(property.Type);
        if (isDictionary)
        {
            return [$"{propertyName}__KEY"];
        }

        var index = isCollection ? "__0" : string.Empty;
        if (!typesByName.TryGetValue(typeName, out var nestedType) || visitedTypes.Contains(typeName))
        {
            return [$"{propertyName}{index}"];
        }

        var nextVisited = new HashSet<string>(visitedTypes, StringComparer.Ordinal) { typeName };
        var childSuffixes = nestedType.Members
            .OfType<PropertyDeclarationSyntax>()
            .Where(IsDocumentedProperty)
            .SelectMany(child => GetEnvironmentVariableSuffixes(child, typesByName, nextVisited))
            .ToArray();
        return childSuffixes.Length == 0
            ? [$"{propertyName}{index}"]
            : childSuffixes.Select(child => $"{propertyName}{index}__{child}").ToArray();
    }

    private static (string TypeName, bool IsCollection, bool IsDictionary) DescribeType(TypeSyntax type)
    {
        type = type is NullableTypeSyntax nullable ? nullable.ElementType : type;
        if (type is ArrayTypeSyntax array)
        {
            return (GetSimpleTypeName(array.ElementType), true, false);
        }

        if (type is GenericNameSyntax generic)
        {
            var name = generic.Identifier.ValueText;
            var isDictionary = name is "Dictionary" or "IDictionary" or "IReadOnlyDictionary";
            var isCollection = name is "IEnumerable" or "ICollection" or "IReadOnlyCollection"
                or "IList" or "IReadOnlyList" or "List" or "HashSet";
            var elementType = isDictionary
                ? generic.TypeArgumentList.Arguments.Last()
                : generic.TypeArgumentList.Arguments.First();
            return (GetSimpleTypeName(elementType), isCollection, isDictionary);
        }

        return (GetSimpleTypeName(type), false, false);
    }

    private static string GetSimpleTypeName(TypeSyntax type)
    {
        var value = type.ToString().TrimEnd('?');
        return value.Split('.').Last();
    }
}
