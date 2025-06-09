#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Commands/SetupTestCommand.cs
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
    public class SetupTestCommand : Command, ICommandOption<TestingConfiguration>
    {

        public SetupTestCommand() : base("setup-test", "Add data for testing")
        {
            AddOption(LocationCountOption);
            AddOption(DeviceConfigurationIdOption);
            AddOption(ProtocolOption);
        }

        public Option<int> LocationCountOption { get; set; } = new("--location-count", "Number of locations to create");
        public Option<int> DeviceConfigurationIdOption { get; set; } = new("--device-configuration-id", "type of configuration to create");
        public Option<string> ProtocolOption { get; set; } = new("--protocol", "Only create location that match this protocol (ftp, ftps, http)");

        public ModelBinder<TestingConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<TestingConfiguration>();

            binder.BindMemberFromValue(b => b.LocationCount, LocationCountOption);
            binder.BindMemberFromValue(b => b.DeviceConfigurationId, DeviceConfigurationIdOption);
            binder.BindMemberFromValue(b => b.Protocol, ProtocolOption);

            return binder;
        }


        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<TestingConfiguration>().BindCommandLine();
            services.AddHostedService<SetupTestCommandHostedService>();
        }
    }

    public class TestingConfiguration
    {
        public int LocationCount { get; set; }
        public int DeviceConfigurationId { get; set; }
        public string Protocol { get; set; }
    }
}
