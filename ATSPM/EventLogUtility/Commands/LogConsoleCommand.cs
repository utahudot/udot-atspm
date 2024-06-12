#region license
// Copyright 2024 Utah Departement of Transportation
// for EventLogUtility - ATSPM.EventLogUtility.Commands/LogConsoleCommand.cs
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CommandLine.Invocation;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using ATSPM.Application.Configuration;
using System.CommandLine.Binding;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Reflection.Metadata;
using ATSPM.Infrastructure.Services.HostedServices;

namespace ATSPM.EventLogUtility.Commands
{
    public class LogConsoleCommand : Command, ICommandOption<EventLogLoggingConfiguration>
    {
        public LogConsoleCommand() : base("log", "Logs data from Location controllers")
        {
            IncludeOption.AddValidator(r =>
            {
                if (r.GetValueForOption(ExcludeOption)?.Count() > 0)
                    r.ErrorMessage = "Can't use include option when also using exclude option";
            });
            ExcludeOption.AddValidator(r =>
            {
                if (r.GetValueForOption(IncludeOption)?.Count() > 0)
                    r.ErrorMessage = "Can't use exclude option when also using include option";
            });

            AddArgument(PingControllerArg);
            AddArgument(DeleteLocalFileArg);

            AddOption(IncludeOption);
            AddOption(ExcludeOption);
            AddOption(TypeOption);
            AddOption(PathCommandOption);

            //this.SetHandler((d, i, e, p) =>
            //{
            //    Console.WriteLine($"{this.Name} is executing");

            //    foreach (var s in i)
            //    {
            //        Console.WriteLine($"Extracting event logs for Location {s}");
            //    }

            //    foreach (var s in e)
            //    {
            //        Console.WriteLine($"Excluding event logs for Location {s}");
            //    }

            //    Console.WriteLine($"Extraction path {p}");

            //}, DateOption, IncludeOption, ExcludeOption, PathCommandOption);
        }

        public Argument<bool> PingControllerArg { get; set; } = new Argument<bool>("ping", "Ping to verify Location controller is online");

        public Argument<bool> DeleteLocalFileArg { get; set; } = new Argument<bool>("delete local", "Delete local file");

        public LocationIncludeCommandOption IncludeOption { get; set; } = new();

        public LocationExcludeCommandOption ExcludeOption { get; set; } = new();

        public LocationTypeCommandOption TypeOption { get; set; } = new();

        public PathCommandOption PathCommandOption { get; set; } = new();

        public ModelBinder<EventLogLoggingConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<EventLogLoggingConfiguration>();

            binder.BindMemberFromValue(b => b.Included, IncludeOption);
            binder.BindMemberFromValue(b => b.Excluded, ExcludeOption);
            binder.BindMemberFromValue(b => b.ControllerTypes, TypeOption);
            binder.BindMemberFromValue(b => b.Path, PathCommandOption);

            return binder;
        }

        public void BindCommandOptions(IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<EventLogLoggingConfiguration>().BindCommandLine();
            services.AddHostedService<LocationLoggerUtilityHostedService>();
        }
    }
}
