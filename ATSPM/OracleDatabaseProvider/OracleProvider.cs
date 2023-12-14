using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.OracleDatabaseProvider
{
    public class OracleProvider
    {
        public const string ProviderName = "Oracle";

        public static string Migration => typeof(OracleProvider).Assembly.FullName;
    }
}
