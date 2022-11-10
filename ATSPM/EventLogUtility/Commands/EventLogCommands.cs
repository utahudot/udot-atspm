using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.EventLogUtility.Commands
{
    public class EventLogCommands : RootCommand
    {
        public EventLogCommands()
        {
            var logCmd = new LogConsoleCommand();
            var extractCmd = new ExtractConsoleCommand();

            this.AddCommand(logCmd);
            this.AddCommand(extractCmd);
        }
    }
}
