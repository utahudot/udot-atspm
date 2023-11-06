using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.EventLogUtility.Commands
{
    public class SignalExcludeCommandOption : Option<IEnumerable<string>>
    {
        public SignalExcludeCommandOption() : base("--exclude", "List of signal controller numbers to exclude")
        {
            AllowMultipleArgumentsPerToken = true;
            AddAlias("-e");
        }
    }
}
