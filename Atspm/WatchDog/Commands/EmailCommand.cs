#region license
// Copyright 2025 Utah Departement of Transportation
// for WatchDog - Utah.Udot.ATSPM.WatchDog.Commands/EmailCommand.cs
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
using Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices;
using Utah.Udot.NetStandardToolkit.Configuration;

namespace Utah.Udot.ATSPM.WatchDog.Commands
{
    public class EmailCommand : Command, ICommandOption<EmailConfiguration>
    {
        public EmailCommand() : base("email", "Email service configuration")
        {
            AddOption(HostOption);
            AddOption(PortOption);
            AddOption(EnableSslOption);
            AddOption(UserNameOption);
            AddOption(PasswordOption);
        }

        public Option<string> HostOption { get; set; } = new("--host", "Host");
        public Option<int> PortOption { get; set; } = new("--port", "Port");
        public Option<bool> EnableSslOption { get; set; } = new("--EnableSsl", "Enable Ssl");
        public Option<string> UserNameOption { get; set; } = new("--userName", "UserName");
        public Option<string> PasswordOption { get; set; } = new("--password", "Password");


        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.Configure<EmailConfiguration>(host.Configuration.GetSection("EmailConfiguration"));
            services.AddScoped<WatchdogEmailService>();

            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<EmailConfiguration>().BindCommandLine();
        }

        public ModelBinder<EmailConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<EmailConfiguration>();

            binder.BindMemberFromValue(b => b.Host, HostOption);
            binder.BindMemberFromValue(b => b.Port, PortOption);
            binder.BindMemberFromValue(b => b.EnableSsl, EnableSslOption);
            binder.BindMemberFromValue(b => b.UserName, UserNameOption);
            binder.BindMemberFromValue(b => b.Password, PasswordOption);

            return binder;
        }
    }
}
