namespace ATSPM.Infrastructure.SqlDatabaseProvider
{
    public class SqlServerProvider
    {
        public const string ProviderName = "SqlServer";

        public static string Migration => typeof(SqlServerProvider).Assembly.FullName;
    }
}