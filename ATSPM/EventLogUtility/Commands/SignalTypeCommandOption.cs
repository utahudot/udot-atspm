using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.EventLogUtility.Commands
{
    public class LocationTypeCommandOption : Option<IEnumerable<int>>
    {
        public LocationTypeCommandOption() : base("--controllertype", "List of Location controller types to include")
        {
            AllowMultipleArgumentsPerToken = true;
            AddAlias("-t");
        }
    }
}
