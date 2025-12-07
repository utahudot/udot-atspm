#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Commands/MoveEventLogsSqlServerToPostgresCommand.cs
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
using System.Reflection;

namespace DatabaseInstaller.Commands
{
    public class SearchEventsCommand : Command, ICommandOption<TransferCommandConfiguration>
    {
        public SearchEventsCommand() : base("search", "Apply migrations and optionally seed the admin user")
        {
            AddOption(StartOption);
            AddOption(EndOption);
            AddOption(LocationsOption);
            AddOption(DeviceOption);
            AddOption(EventCodesOption);
            AddOption(OutputCsvPathOption);
        }

        public Option<DateTime> StartOption { get; set; } = new("--start", "Start Date");
        public Option<DateTime> EndOption { get; set; } = new("--end", "Start Date");
        public Option<string> LocationsOption { get; set; } = new("--locations", "Comma seperated list of location identifiers") { IsRequired = false };
        public Option<int?> DeviceOption { get; set; } = new("--device", "Id of Device Type used to import events for just that device type") { IsRequired = false };
        public Option<string> EventCodesOption { get; set; } = new("--eventCodes", "Comma seperated list of event codes") { IsRequired = false };
        public Option<string> OutputCsvPathOption { get; set; } = new("--outputPath", "Path to write csv") { IsRequired = false };

        public ModelBinder<TransferCommandConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<TransferCommandConfiguration>();

            binder.BindMemberFromValue(b => b.Start, StartOption);
            binder.BindMemberFromValue(b => b.End, EndOption);
            binder.BindMemberFromValue(b => b.Locations, LocationsOption);
            binder.BindMemberFromValue(b => b.Device, DeviceOption);
            binder.BindMemberFromValue(b => b.EventCodes, EventCodesOption);
            binder.BindMemberFromValue(b => b.OutputCsvPath, OutputCsvPathOption);

            return binder;
        }

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<TransferCommandConfiguration>().BindCommandLine();
            services.AddHostedService<SearchEventsHostedService>();
        }
    }


}