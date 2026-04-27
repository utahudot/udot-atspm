#region license
// Copyright 2026 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Commands/TransferConfigCommand.cs
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

namespace DatabaseInstaller.Commands
{
    public class TransferConfigCommand : Command, ICommandOption<TransferConfigCommandConfiguration>
    {

        public TransferConfigCommand() : base("transfer-config", "Copy configuration data from the ATSPM Config API into the target database")
        {
            AddOption(ApiBaseUrlOption);
            AddOption(BearerTokenOption);
            AddOption(DeleteOption);
            AddOption(UpdateLocationsOption);
            AddOption(ImportSpeedDevicesOption);
        }

        public Option<string> ApiBaseUrlOption { get; set; } = new("--api-base-url", () => "https://atspm.udot.utah.gov/", "Base URL for the source ATSPM site");
        public Option<string> BearerTokenOption { get; set; } = new("--bearer-token", "Bearer token used to access the source Config API");
        public Option<bool> DeleteOption { get; set; } = new("--delete", "Delete before inserting locations");
        public Option<bool> UpdateLocationsOption { get; set; } = new("--update-locations", "Import location configuration from the source Config API");
        public Option<bool> ImportSpeedDevicesOption { get; set; } = new("--update-speed", "Import speed devices from the source Config API");

        public ModelBinder<TransferConfigCommandConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<TransferConfigCommandConfiguration>();

            binder.BindMemberFromValue(b => b.ApiBaseUrl, ApiBaseUrlOption);
            binder.BindMemberFromValue(b => b.BearerToken, BearerTokenOption);
            binder.BindMemberFromValue(b => b.Delete, DeleteOption);
            binder.BindMemberFromValue(b => b.UpdateLocations, UpdateLocationsOption);
            binder.BindMemberFromValue(b => b.ImportSpeedDevices, ImportSpeedDevicesOption);

            return binder;
        }


        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<TransferConfigCommandConfiguration>().BindCommandLine();
            services.AddHostedService<TransferConfigCommandHostedService>();
        }
    }

    public class TransferConfigCommandConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string BearerToken { get; set; }
        public bool Delete { get; set; }
        public bool UpdateLocations { get; set; }
        public bool UpdateGeneralConfiguration { get; set; }
        public bool ImportSpeedDevices { get; set; }
    }
}
