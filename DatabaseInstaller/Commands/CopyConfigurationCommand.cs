#region license
// Copyright 2024 Utah Department of Transportation
// for DatabaseInstaller.Commands/CopyConfiguration.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0.
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
using Utah.Udot.Atspm.Common;

namespace DatabaseInstaller.Commands
{
    public class CopyConfiguration : Command, ICommandOption<CopyConfigurationConfiguration>
    {
        public CopyConfiguration() : base("copy-config", "Copy configuration data from SQL Server to PostgreSQL")
        {
            AddOption(SourceConnectionOption);
            AddOption(TargetConnectionOption);
        }

        public Option<string> SourceConnectionOption { get; set; } = new("--source-connection", "Connection string for the source SQL Server");
        public Option<string> TargetConnectionOption { get; set; } = new("--target-connection", "Connection string for the target PostgreSQL database");

        public ModelBinder<CopyConfigurationConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<CopyConfigurationConfiguration>();

            binder.BindMemberFromValue(b => b.SourceConnection, SourceConnectionOption);
            binder.BindMemberFromValue(b => b.TargetConnection, TargetConnectionOption);

            return binder;
        }

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<CopyConfigurationConfiguration>().BindCommandLine();
        }
    }

    public class CopyConfigurationConfiguration
    {
        public string SourceConnection { get; set; }
        public string TargetConnection { get; set; }

    }
}
