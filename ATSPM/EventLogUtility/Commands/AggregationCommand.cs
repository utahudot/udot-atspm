#region license
// Copyright 2024 Utah Departement of Transportation
// for EventLogUtility - ATSPM.EventLogUtility.Commands/AggregationCommand.cs
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
using ATSPM.Application.Configuration;
using ATSPM.Infrastructure.Services.HostedServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.EventLogUtility.Commands
{
    public class AggregationCommand : Command, ICommandOption<EventLogAggregateConfiguration>
    {
        public AggregationCommand() : base("aggregate", "Run data aggregation")
        {
            AggregationTypeArgument.FromAmong(
                "approach-cycle",
                "approach-pcd-cycle",
                "approach-speed",
                "approach-splitfail",
                "detector-event-count",
                "left-turn-gap",
                "Location-event-data",
                "Location-phase-delay",
                "Location-phase-termination",
                "Location-plan",
                "Location-preempt-priority",
                "split-monitor",
                "yellow-red-activation");

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

            AddArgument(AggregationTypeArgument);
            AddArgument(BinSizeArgument);
            AddOption(DateOption);
            AddOption(IncludeOption);
            AddOption(ExcludeOption);
        }

        public Argument<string> AggregationTypeArgument { get; set; } = new Argument<string>("type", "Aggregation type to run");

        //TODO: add a parse param to handle zero time to too large or a time
        public Argument<int> BinSizeArgument { get; set; } = new Argument<int>("Size", () => 15, "Size in minutes to aggregate");

        public DateCommandOption DateOption { get; set; } = new();

        public LocationIncludeCommandOption IncludeOption { get; set; } = new();

        public LocationExcludeCommandOption ExcludeOption { get; set; } = new();

        public ModelBinder<EventLogAggregateConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<EventLogAggregateConfiguration>();

            binder.BindMemberFromValue(b => b.AggregationType, AggregationTypeArgument);
            binder.BindMemberFromValue(b => b.BinSize, BinSizeArgument);
            binder.BindMemberFromValue(b => b.Dates, DateOption);
            binder.BindMemberFromValue(b => b.Included, IncludeOption);
            binder.BindMemberFromValue(b => b.Excluded, ExcludeOption);

            return binder;
        }

        public void BindCommandOptions(IServiceCollection services)
        {
            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<EventLogAggregateConfiguration>().BindCommandLine();
            services.AddHostedService<TestAggregationHostedService>();
        }
    }
}
