using System.CommandLine;

namespace DatabaseInstaller.Commands
{
    public class DatabaseInstallerCommands : RootCommand
    {
        public DatabaseInstallerCommands() : base("DatabaseInstaller utility")
        {
            // Initialize and add the UpdateCommand
            var updateCommand = new UpdateCommand();
            AddCommand(updateCommand);
            var copyCommand = new CopyConfigurationCommand();
            AddCommand(copyCommand);
            //var transferCommand = new TransferEvenLogs();
            //AddCommand(transferCommand);
        }
    }
}
