using System.CommandLine;

namespace Utah.Udot.ATSPM.WatchDog.Commands
{
    public class WatchdogCommand : RootCommand
    {
        public WatchdogCommand() : base("Watchdog utility")
        {
            // Initialize and add the UpdateCommand
            var watchdogCommand = new GenerateCommand();
            AddCommand(watchdogCommand);
        }
    }
}
