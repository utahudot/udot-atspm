#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Commands/TransferDailyToHourlyEventLogsCommand.cs
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

using DatabaseInstaller.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;

namespace DatabaseInstaller.Commands
{
    public class TransferDailyToHourlyEventLogsCommand : Command, ICommandOption<TransferDailyToHourlyConfiguration>
    {
        public TransferDailyToHourlyEventLogsCommand() : base("transfer-daily-hourly", "Transfer logs daily compression to hourly compression")
        {
            AddOption(SourceOption);
            AddOption(SourceTableOption);
            AddOption(StartOption);
            AddOption(EndOption);
            AddOption(BatchOption);
            AddOption(DeviceOption);
        }

        public Option<string> SourceOption { get; set; } = new("--source", "Connection string for the source SQL Server");
        public Option<string> SourceTableOption { get; set; } = new("--source-table", "String for the source SQL table");
        public Option<DateTime> StartOption { get; set; } = new("--start", "Start date");
        public Option<DateTime> EndOption { get; set; } = new("--end", "End date");
        public Option<string> DataTypeOption { get; set; } = new("--data-type", "Type of data to import") { IsRequired = false };
        public Option<int?> DeviceOption { get; set; } = new("--device", "Id of Device Type used to import events for just that device type") { IsRequired = false };
        public Option<int?> BatchOption { get; set; } = new("--batch", "Size of batches for importing event logs") { IsRequired = false };

        public ModelBinder<TransferDailyToHourlyConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<TransferDailyToHourlyConfiguration>();

            binder.BindMemberFromValue(b => b.Source, SourceOption);
            binder.BindMemberFromValue(b => b.Start, StartOption);
            binder.BindMemberFromValue(b => b.End, EndOption);
            binder.BindMemberFromValue(b => b.Device, DeviceOption);
            binder.BindMemberFromValue(b => b.Batch, BatchOption);
            binder.BindMemberFromValue(b => b.DataType, DataTypeOption);

            return binder;
        }

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddOptions<TransferDailyToHourlyConfiguration>().BindCommandLine();
            services.AddSingleton(GetOptionsBinder());
            services.AddHostedService<TransferDailyToHourlyEventLogsService>();
        }
    }

    public class TransferDailyToHourlyConfiguration
    {
        public string Source { get; set; }
        public string SourceTable { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string DataType { get; set; } = "IndianaEvent";
        public int? Device { get; set; }
        public int? Batch { get; set; }
    }
}
