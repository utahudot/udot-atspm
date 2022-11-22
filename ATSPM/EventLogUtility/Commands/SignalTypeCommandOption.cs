using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.EventLogUtility.Commands
{
    public class SignalTypeCommandOption : Option<IEnumerable<int>>
    {
        public SignalTypeCommandOption() : base("--controllertype", "List of signal controller types to include")
        {
            AllowMultipleArgumentsPerToken = true;
            AddAlias("-t");
        }
    }
}
