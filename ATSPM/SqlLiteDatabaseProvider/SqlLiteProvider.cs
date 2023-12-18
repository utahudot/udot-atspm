using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.SqlLiteDatabaseProvider
{
    public class SqlLiteProvider
    {
        public const string ProviderName = "SqlLite";

        public static string Migration => typeof(SqlLiteProvider).Assembly.FullName;
    }
}
