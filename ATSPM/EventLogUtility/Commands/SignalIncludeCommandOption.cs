using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.EventLogUtility.Commands
{
    public class SignalIncludeCommandOption : Option<IEnumerable<string>>
    {
        public SignalIncludeCommandOption() : base("--include", "List of signal controller numbers to include")
        {
            AllowMultipleArgumentsPerToken = true;
            AddAlias("-i");
        }
    }
}
