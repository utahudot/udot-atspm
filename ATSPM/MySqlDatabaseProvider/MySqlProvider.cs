using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.MySqlDatabaseProvider
{
    public class MySqlProvider
    {
        public const string ProviderName = "MySql";

        public static string Migration => typeof(MySqlProvider).Assembly.FullName;
    }
}
