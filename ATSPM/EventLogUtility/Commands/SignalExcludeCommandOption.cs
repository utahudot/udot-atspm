using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.EventLogUtility.Commands
{
    public class LocationExcludeCommandOption : Option<IEnumerable<string>>
    {
        public LocationExcludeCommandOption() : base("--exclude", "List of Location controller numbers to exclude")
        {
            AllowMultipleArgumentsPerToken = true;
            AddAlias("-e");
        }
    }
}
