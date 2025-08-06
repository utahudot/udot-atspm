#region license
// Copyright 2025 Utah Departement of Transportation
// for EventLogUtility - Utah.Udot.Atspm.EventLogUtility.Commands/DecodeEventsCommand.cs
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
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;

namespace Utah.Udot.Atspm.EventLogUtility.Commands
{
    public class DecodeEventsCommand : Command, ICommandOption<DecodeEventsConfiguration>
    {
        public DecodeEventsCommand() : base("decode-events", "Import and decode event logs from files")
        {
            AddGlobalOption(PathCommandOption);
        }

        public PathCommandOption PathCommandOption { get; set; } = new();

        public ModelBinder<DecodeEventsConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<DecodeEventsConfiguration>();

            binder.BindMemberFromValue(b => b.Path, PathCommandOption);

            return binder;
        }

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<DecodeEventsConfiguration>().BindCommandLine();
            services.AddHostedService<DecodeEventLogHostedService>();
        }
    }
}
