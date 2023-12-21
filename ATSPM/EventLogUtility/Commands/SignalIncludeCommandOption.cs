using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.EventLogUtility.Commands
{
    public class LocationIncludeCommandOption : Option<IEnumerable<string>>
    {
        public LocationIncludeCommandOption() : base("--include", "List of Location controller numbers to include")
        {
            AllowMultipleArgumentsPerToken = true;
            AddAlias("-i");
        }
    }
}
