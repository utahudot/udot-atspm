#region license
// Copyright 2025 Utah Departement of Transportation
// for EventLogUtility - Utah.Udot.Atspm.EventLogUtility.Commands/AggregationCommand.cs
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
using System.Text.RegularExpressions;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;

namespace Utah.Udot.Atspm.EventLogUtility.Commands
{
    //public class LogCommandModelBinder : ModelBinder<DeviceEventLoggingConfiguration>
    //{
    //    public LogCommandModelBinder(LogConsoleCommand cmd)
    //    {
    //        BindMemberFromValue(b => b.Path, cmd.PathCommandOption);
    //        BindMemberFromValue(b => b.BatchSize, cmd.BatchSizeOption);
    //        BindMemberFromValue(b => b.ParallelProcesses, cmd.PrallelProcessesOption);
    //    }
    //}

    //public class LogCommandQueryBinder : ModelBinder<DeviceEventLoggingQueryOptions>
    //{
    //    public LogCommandQueryBinder(LogConsoleCommand cmd)
    //    {
    //        BindMemberFromValue(b => b.IncludedLocations, cmd.IncludeOption);
    //        BindMemberFromValue(b => b.ExcludedLocations, cmd.ExcludeOption);
    //        BindMemberFromValue(b => b.IncludedAreas, cmd.AreaOption);
    //        BindMemberFromValue(b => b.IncludedJurisdictions, cmd.JurisdictionOption);
    //        BindMemberFromValue(b => b.IncludedRegions, cmd.RegionOption);
    //        BindMemberFromValue(b => b.IncludedLocationTypes, cmd.LocationTypeOption);
    //        BindMemberFromValue(b => b.DeviceType, cmd.DeviceTypeOption);
    //        BindMemberFromValue(b => b.TransportProtocol, cmd.TransportProtocolOption);
    //        BindMemberFromValue(b => b.TransportProtocol, cmd.TransportProtocolOption);
    //    }
    //}


    public class AggregationCommand : Command, ICommandOption<EventLogAggregateConfiguration>
    {
        public AggregationCommand() : base("aggregate-events", "Run event aggregation")
        {
            var values = typeof(AggregationModelBase).Assembly.GetTypes()
                .Where(w => w.IsSubclassOf(typeof(AggregationModelBase)))
                .Select(s => Regex.Replace(s.Name, @"(?<=[a-z])([A-Z])", @"_$1").ToLower())
                .Prepend("all")
                .ToArray();

            AggregationTypeArgument.FromAmong(values);

            DateOption.SetDefaultValue(DateTime.Now.Date.AddDays(-1));

            //IncludeOption.AddValidator(r =>
            //{
            //    if (r.GetValueForOption(ExcludeOption)?.Count() > 0)
            //        r.ErrorMessage = "Can't use include option when also using exclude option";
            //});
            //ExcludeOption.AddValidator(r =>
            //{
            //    if (r.GetValueForOption(IncludeOption)?.Count() > 0)
            //        r.ErrorMessage = "Can't use exclude option when also using include option";
            //});

            AddArgument(AggregationTypeArgument);
            AddOption(DateOption);
        }

        public Argument<string> AggregationTypeArgument { get; set; } = new Argument<string>("type", () => "all", "Aggregation type to run");

        public DateCommandOption DateOption { get; set; } = new();

        public ModelBinder<EventLogAggregateConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<EventLogAggregateConfiguration>();

            binder.BindMemberFromValue(b => b.AggregationType, AggregationTypeArgument);
            binder.BindMemberFromValue(b => b.Dates, DateOption);

            return binder;
        }

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<EventLogAggregateConfiguration>().BindCommandLine();
            services.AddHostedService<EventAggregationHostedService>();
        }
    }
}
