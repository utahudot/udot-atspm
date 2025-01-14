#region license
// Copyright 2024 Utah Departement of Transportation
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

namespace DatabaseInstaller.Commands
{
    public class MoveEventLogsSqlServerToPostgresCommand : Command, ICommandOption<TransferCommandConfiguration>
    {
        public MoveEventLogsSqlServerToPostgresCommand() : base("copy-sql", "Apply migrations and optionally seed the admin user")
        {
            AddOption(SourceOption);
            AddOption(StartOption);
            AddOption(EndOption);
        }

        public Option<string> SourceOption { get; set; } = new("--source", "Connection string for the source SQL Server");
        public Option<DateTime> StartOption { get; set; } = new("--start", "Start Date");
        public Option<DateTime> EndOption { get; set; } = new("--end", "Start Date");

        public ModelBinder<TransferCommandConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<TransferCommandConfiguration>();

            binder.BindMemberFromValue(b => b.Source, SourceOption);
            binder.BindMemberFromValue(b => b.Start, StartOption);
            binder.BindMemberFromValue(b => b.End, EndOption);

            return binder;
        }

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<TransferCommandConfiguration>().BindCommandLine();
            services.AddHostedService<MoveEventLogsSqlServerToPostgresHostedService>();
        }
    }


}