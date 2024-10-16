#region license
// Copyright 2024 Utah Department of Transportation
// for DatabaseInstaller.Commands/CopyConfigurationCommand.cs
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

using DatabaseInstaller.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using Utah.Udot.Atspm.Common;

namespace DatabaseInstaller.Commands
{
    public class CopyConfigurationCommand : Command, ICommandOption<CopyConfigCommandConfiguration>
    {

        public CopyConfigurationCommand() : base("copy-config", "Copy configuration data from SQL Server to PostgreSQL")
        {
            AddOption(SourceOption);
            AddOption(TargetOption);
        }

        public Option<string> SourceOption { get; set; } = new("--source", "Connection string for the source SQL Server");
        public Option<string> TargetOption { get; set; } = new("--target", "Connection string for the target PostgreSQL database");

        public ModelBinder<CopyConfigCommandConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<CopyConfigCommandConfiguration>();

            binder.BindMemberFromValue(b => b.Source, SourceOption);
            binder.BindMemberFromValue(b => b.Target, TargetOption);

            return binder;
        }


        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<CopyConfigCommandConfiguration>().BindCommandLine();
            services.AddHostedService<CopyConfigCommandHostedService>();
        }
    }

    public class CopyConfigCommandConfiguration
    {
        public string Source { get; set; }
        public string Target { get; set; }

    }
}
