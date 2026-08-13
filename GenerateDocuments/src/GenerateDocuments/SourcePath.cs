namespace AtspmDocsGenerator;

internal static class SourcePath
{
    public static string ResolveWithinRoot(string sourceRoot, string relativePath, string description)
    {
        var fullPath = Path.GetFullPath(Path.Combine(sourceRoot, relativePath));
        var rootWithSeparator = sourceRoot.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!fullPath.Equals(sourceRoot, StringComparison.OrdinalIgnoreCase)
            && !fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                $"{description} must remain inside the source root: {fullPath}");
        }

        return fullPath;
    }
}
