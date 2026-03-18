#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Commands/ImportLocationsCommand.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
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
    public class ImportLocationsCommand : Command, ICommandOption<ImportLocationsCommandConfiguration>
    {
        public ImportLocationsCommand() : base("import-locations", "Import locations from a CSV file")
        {
            AddOption(FileOption);
            AddOption(ConfigConnectionOption);
            AddOption(ProviderOption);
            AddOption(DeleteOption);
            AddOption(LocationTypeOption);
        }

        public Option<string> FileOption { get; set; } = new("--file", "Path to the CSV file containing locations") { IsRequired = true };
        public Option<string> ConfigConnectionOption { get; set; } = new("--config-connection", "Connection string for ConfigContext (optional - uses appsettings.json if not provided)");
        public Option<string> ProviderOption { get; set; } = new("--provider", "Provider string (optional - uses appsettings.json if not provided)");
        public Option<bool> DeleteOption { get; set; } = new("--delete", "Delete existing locations before importing");
        public Option<string> LocationTypeOption { get; set; } = new("--location-type", "Location type name to use (default: Intersection)");

        public ModelBinder<ImportLocationsCommandConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<ImportLocationsCommandConfiguration>();

            binder.BindMemberFromValue(b => b.File, FileOption);
            binder.BindMemberFromValue(b => b.ConfigConnection, ConfigConnectionOption);
            binder.BindMemberFromValue(b => b.Provider, ProviderOption);
            binder.BindMemberFromValue(b => b.Delete, DeleteOption);
            binder.BindMemberFromValue(b => b.LocationType, LocationTypeOption);

            return binder;
        }

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<ImportLocationsCommandConfiguration>().BindCommandLine();
            services.AddHostedService<ImportLocationsHostedService>();
        }
    }

    public class ImportLocationsCommandConfiguration
    {
        public string File { get; set; }
        public string ConfigConnection { get; set; }
        public string Provider { get; set; }
        public bool Delete { get; set; }
        public string LocationType { get; set; } = "Intersection";
    }
}