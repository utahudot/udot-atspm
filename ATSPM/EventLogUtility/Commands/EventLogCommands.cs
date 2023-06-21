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
            this.AddCommand(AggregationCommand);
            this.AddCommand(LogCommand);
            this.AddCommand(ExtractCommand);
            this.AddCommand(SignalInfoCommand);
        }

        public AggregationCommand AggregationCommand { get; set; } = new AggregationCommand();
        public LogConsoleCommand LogCommand { get; set; } = new LogConsoleCommand();
        public ExtractConsoleCommand ExtractCommand { get; set; } = new ExtractConsoleCommand();
        public SignalInfoCommand SignalInfoCommand { get; set; } = new SignalInfoCommand();
    }
}
