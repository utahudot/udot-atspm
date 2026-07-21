#region license
// Copyright 2026 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Commands/TransferV4ConfigCommand.cs
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using DatabaseInstaller.Services;

namespace DatabaseInstaller.Commands
{
    public class TransferV4ConfigCommand : Command, ICommandOption<TransferV4ConfigCommandConfiguration>
    {
        public TransferV4ConfigCommand() : base("transferv4-config", "Migrate configuration data from ATSPM v4 database into v5 Locations")
        {
            AddOption(SourceOption);
            AddOption(LocationsOption);
            AddOption(TypeOption);
        }

        /// <summary>
        /// Connection string to the v4 ATSPM database (typically named 'MOE')
        /// </summary>
        public Option<string> SourceOption { get; set; } = new("--source", "Connection string for the source v4 ATSPM MOE database")
        {
            IsRequired = true
        };

        /// <summary>
        /// Comma-separated list of LocationIdentifiers to migrate. If not provided, all Signals are migrated.
        /// </summary>
        public Option<string> LocationsOption { get; set; } = new("--locations", "Comma separated list of signal/location identifiers to migrate")
        {
            IsRequired = false
        };

        /// <summary>
        /// Device type to filter Signals. Maps to v5 DeviceType enum.
        /// </summary>
        public Option<int?> TypeOption { get; set; } = new("--type", "Device type ID to filter signals being imported (optional)")
        {
            IsRequired = false
        };

        public ModelBinder<TransferV4ConfigCommandConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<TransferV4ConfigCommandConfiguration>();

            binder.BindMemberFromValue(b => b.Source, SourceOption);
            binder.BindMemberFromValue(b => b.Locations, LocationsOption);
            binder.BindMemberFromValue(b => b.Type, TypeOption);

            return binder;
        }

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<TransferV4ConfigCommandConfiguration>().BindCommandLine();
            services.AddHostedService<TransferV4ConfigCommandHostedService>();
        }
    }

    public class TransferV4ConfigCommandConfiguration
    {
        /// <summary>
        /// Connection string to v4 ATSPM database
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Comma-separated list of LocationIdentifiers to migrate (null = migrate all)
        /// </summary>
        public string Locations { get; set; }

        /// <summary>
        /// Device type filter (null = no filter)
        /// </summary>
        public int? Type { get; set; }
    }
}
