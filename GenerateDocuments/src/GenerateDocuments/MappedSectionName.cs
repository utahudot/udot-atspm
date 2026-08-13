namespace AtspmDocsGenerator;

internal static class MappedSectionName
{
    public static string GetBaseName(string value)
    {
        var separatorIndex = value.IndexOf(':', StringComparison.Ordinal);
        return separatorIndex < 0 ? value : value[..separatorIndex];
    }

    public static string ToEnvironmentVariablePrefix(string value) =>
        value.Replace(":", "__", StringComparison.Ordinal);
}
