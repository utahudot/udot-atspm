namespace AtspmDocsGenerator;

public sealed record CliOptions(
    string SourceRoot,
    string OutputRoot,
    string MapPath,
    string RepositoryUrl,
    string RepositoryRef,
    DateTimeOffset GeneratedAt)
{
    private static readonly string[] RequiredOptionNames =
    [
        "--source-root",
        "--output-root",
        "--map",
        "--repository-url",
        "--repository-ref"
    ];

    public const string HelpText =
        """
        ATSPM configuration documentation generator

        Usage:
          dotnet run --project src/GenerateDocuments -- \
            --source-root <path> \
            --output-root <path> \
            --map <path> \
            --repository-url <url> \
            --repository-ref <git-ref-or-sha>

        Options:
          --source-root     Root of the source repository to scan.
          --output-root     Dedicated directory for generated Markdown pages.
          --map             Path to the container configuration map.
          --repository-url  Public source repository URL used in generated links.
          --repository-ref  Git branch, tag, or commit SHA used in generated links.
          --help            Show this help.
        """;

    public static CliParseResult Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return CliParseResult.Failure("No options were provided.");
        }

        if (args.Length == 1 && args[0] is "--help" or "-h")
        {
            return CliParseResult.Help();
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);

        for (var index = 0; index < args.Length; index += 2)
        {
            var name = args[index];
            if (!RequiredOptionNames.Contains(name, StringComparer.Ordinal))
            {
                return CliParseResult.Failure($"Unknown option '{name}'.");
            }

            if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                return CliParseResult.Failure($"Option '{name}' requires a value.");
            }

            if (!values.TryAdd(name, args[index + 1]))
            {
                return CliParseResult.Failure($"Option '{name}' was provided more than once.");
            }
        }

        var missing = RequiredOptionNames.Where(name => !values.ContainsKey(name)).ToArray();
        if (missing.Length > 0)
        {
            return CliParseResult.Failure($"Missing required options: {string.Join(", ", missing)}.");
        }

        var sourceRoot = Path.GetFullPath(values["--source-root"]);
        if (!Directory.Exists(sourceRoot))
        {
            return CliParseResult.Failure($"Source root does not exist: {sourceRoot}");
        }

        var mapPath = Path.GetFullPath(values["--map"]);
        if (!File.Exists(mapPath))
        {
            return CliParseResult.Failure($"Configuration map does not exist: {mapPath}");
        }

        if (!Uri.TryCreate(values["--repository-url"], UriKind.Absolute, out var repositoryUri)
            || repositoryUri.Scheme is not ("http" or "https"))
        {
            return CliParseResult.Failure("--repository-url must be an absolute HTTP or HTTPS URL.");
        }

        if (string.IsNullOrWhiteSpace(values["--repository-ref"]))
        {
            return CliParseResult.Failure("--repository-ref cannot be empty.");
        }

        var repositoryUrl = repositoryUri
            .GetLeftPart(UriPartial.Path)
            .TrimEnd('/');

        if (repositoryUrl.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            repositoryUrl = repositoryUrl[..^4];
        }

        return CliParseResult.Success(new CliOptions(
            sourceRoot,
            Path.GetFullPath(values["--output-root"]),
            mapPath,
            repositoryUrl,
            values["--repository-ref"],
            DateTimeOffset.UtcNow));
    }
}

public sealed record CliParseResult(CliOptions? Options, string? Error, bool ShowHelp)
{
    public static CliParseResult Success(CliOptions options) => new(options, null, false);

    public static CliParseResult Failure(string error) => new(null, error, false);

    public static CliParseResult Help() => new(null, null, true);
}
