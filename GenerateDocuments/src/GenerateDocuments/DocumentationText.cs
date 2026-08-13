using System.Text;

namespace AtspmDocsGenerator;

internal static class DocumentationText
{
    public static string BuildRepositoryUrl(
        string repositoryUrl,
        string operation,
        string repositoryRef,
        string? relativePath = null)
    {
        var builder = new StringBuilder(repositoryUrl.TrimEnd('/'));
        builder.Append('/').Append(operation).Append('/').Append(Uri.EscapeDataString(repositoryRef));

        if (!string.IsNullOrWhiteSpace(relativePath))
        {
            foreach (var segment in relativePath.Replace('\\', '/').Split('/'))
            {
                builder.Append('/').Append(Uri.EscapeDataString(segment));
            }
        }

        return builder.ToString();
    }

    public static string RepositoryLabel(string repositoryUrl)
    {
        if (!Uri.TryCreate(repositoryUrl, UriKind.Absolute, out var uri))
        {
            return repositoryUrl;
        }

        var label = uri.AbsolutePath.Trim('/');
        return label.EndsWith(".git", StringComparison.OrdinalIgnoreCase) ? label[..^4] : label;
    }

    public static string EscapeText(string value) =>
        value.Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);

    public static string EscapeCode(string value) =>
        EscapeText(value).Replace("`", "\\`", StringComparison.Ordinal);

    public static string NormalizeLineEndings(string value) =>
        value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
}
