#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Commands/UpdateCommand.cs
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
    public class UpdateCommand : Command, ICommandOption<UpdateCommandConfiguration>
    {
        public UpdateCommand() : base("update", "Apply migrations and optionally seed the admin user")
        {
            AddOption(ConfigConnectionOption);
            AddOption(AggregationConnectionOption);
            AddOption(EventLogConnectionOption);
            AddOption(IdentityConnectionOption);
            AddOption(ProviderOption);
            AddOption(AdminEmailOption);
            AddOption(AdminPasswordOption);
            AddOption(AdminRoleOption);
            AddOption(SeedAdminOption);
        }

        public Option<string> ConfigConnectionOption { get; set; } = new("--config-connection", "Connection string for ConfigContext");
        public Option<string> AggregationConnectionOption { get; set; } = new("--aggregation-connection", "Connection string for AggregationContext");
        public Option<string> EventLogConnectionOption { get; set; } = new("--eventlog-connection", "Connection string for EventLogContext");
        public Option<string> IdentityConnectionOption { get; set; } = new("--identity-connection", "Connection string for IdentityContext");
        public Option<string> ProviderOption { get; set; } = new("--provider", "Provider string for Context");

        public Option<string> AdminEmailOption { get; set; } = new("--admin-email", "Admin user's email address");
        public Option<string> AdminPasswordOption { get; set; } = new("--admin-password", "Admin user's password");
        public Option<string> AdminRoleOption { get; set; } = new("--admin-role", "Admin user's role");
        public Option<bool> SeedAdminOption { get; set; } = new("--seed-admin", "Seed the admin user");

        public ModelBinder<UpdateCommandConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<UpdateCommandConfiguration>();

            binder.BindMemberFromValue(b => b.ConfigConnection, ConfigConnectionOption);
            binder.BindMemberFromValue(b => b.AggregationConnection, AggregationConnectionOption);
            binder.BindMemberFromValue(b => b.EventLogConnection, EventLogConnectionOption);
            binder.BindMemberFromValue(b => b.IdentityConnection, IdentityConnectionOption);
            binder.BindMemberFromValue(b => b.Provider, ProviderOption);
            binder.BindMemberFromValue(b => b.AdminEmail, AdminEmailOption);
            binder.BindMemberFromValue(b => b.AdminPassword, AdminPasswordOption);
            binder.BindMemberFromValue(b => b.AdminRole, AdminRoleOption);
            binder.BindMemberFromValue(b => b.SeedAdmin, SeedAdminOption);

            return binder;
        }

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<UpdateCommandConfiguration>().BindCommandLine();
            services.AddHostedService<UpdateCommandHostedService>();
        }
    }

    public class UpdateCommandConfiguration
    {
        public string ConfigConnection { get; set; }
        public string AggregationConnection { get; set; }
        public string EventLogConnection { get; set; }
        public string IdentityConnection { get; set; }
        public string Provider { get; set; }
        public string AdminEmail { get; set; }
        public string AdminPassword { get; set; }
        public string AdminRole { get; set; }
        public bool SeedAdmin { get; set; }
    }
}
