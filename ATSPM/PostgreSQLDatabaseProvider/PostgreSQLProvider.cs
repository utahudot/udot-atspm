using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.PostgreSQLDatabaseProvider
{
    public class PostgreSQLProvider
    {
        public const string ProviderName = "PostgreSQL";

        public static string Migration => typeof(PostgreSQLProvider).Assembly.FullName;
    }
}
