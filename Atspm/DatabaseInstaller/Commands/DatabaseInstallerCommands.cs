#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Commands/DatabaseInstallerCommands.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.CommandLine;

namespace DatabaseInstaller.Commands
{
    public class DatabaseInstallerCommands : RootCommand
    {
        public DatabaseInstallerCommands() : base("DatabaseInstaller utility")
        {
            // Initialize and add the UpdateCommand
            AddCommand(UpdateCommand);
            AddCommand(CopyConfigurationCommand);
            AddCommand(MoveEventLogsSqlServerToPostgresCommand);
            AddCommand(TransferEventLogsCommand);
            AddCommand(TransferSpeedEventsCommand);
            AddCommand(TranslateEventLogsCommand);
            AddCommand(TransferDailyToHourlyEventLogsCommand);
            AddCommand(SetupTestCommand);
        }

        public UpdateCommand UpdateCommand { get; set; } = new UpdateCommand();
        public TransferConfigCommand CopyConfigurationCommand { get; set; } = new TransferConfigCommand();
        public MoveEventLogsSqlServerToPostgresCommand MoveEventLogsSqlServerToPostgresCommand { get; set; } = new MoveEventLogsSqlServerToPostgresCommand();
        public TransferEventLogsCommand TransferEventLogsCommand { get; set; } = new TransferEventLogsCommand();
        public TransferSpeedEventsCommand TransferSpeedEventsCommand { get; set; } = new TransferSpeedEventsCommand();
        public TranslateEventLogsCommand TranslateEventLogsCommand { get; set; } = new TranslateEventLogsCommand();
        public TransferDailyToHourlyEventLogsCommand TransferDailyToHourlyEventLogsCommand { get; set; } = new TransferDailyToHourlyEventLogsCommand();
        public SetupTestCommand SetupTestCommand { get; set; } = new SetupTestCommand();

    }
}
