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
using System.CommandLine.Binding;
using System.CommandLine.NamingConventionBinder;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;

namespace Utah.Udot.Atspm.EventLogUtility.Commands
{
    public class AggregationCommand : Command, ICommandOption
    {
        public AggregationCommand() : base("aggregate", "Run event aggregation")
        {
            var values = typeof(AggregationModelBase)
                .ListDerivedTypes()
                .Select(s => s.ToSnakeCase())
                .ToArray();

            AggregationTypeArgument.FromAmong(values);

            DateOption.SetDefaultValue(new List<DateTime>() { DateTime.Now.Date.AddDays(-1) });

            AddArgument(AggregationTypeArgument);
            AddOption(DateOption);
            AddOption(PrallelProcessesOption);

            IncludeLocationOption.AddValidator(r =>
            {
                if (r.GetValueForOption(ExcludeLocationOption)?.Count() > 0)
                    r.ErrorMessage = "Can't use include option when also using exclude option";
            });
            ExcludeLocationOption.AddValidator(r =>
            {
                if (r.GetValueForOption(IncludeLocationOption)?.Count() > 0)
                    r.ErrorMessage = "Can't use exclude option when also using include option";
            });

            AddOption(IncludeLocationOption);
            AddOption(ExcludeLocationOption);
            AddOption(LocationTypeOption);
            AddOption(AreaOption);
            AddOption(JurisdictionOption);
            AddOption(RegionOption);
        }

        public Argument<string> AggregationTypeArgument { get; set; } = new Argument<string>("type", () => "all", "Aggregation type to run");

        public DateCommandOption DateOption { get; set; } = new();

        public PrallelProcessesOption PrallelProcessesOption { get; set; } = new();

        public LocationIncludeCommandOption IncludeLocationOption { get; set; } = new();

        public LocationExcludeCommandOption ExcludeLocationOption { get; set; } = new();

        public LocationTypeCommandOption LocationTypeOption { get; set; } = new();

        public LocationAreaCommandOption AreaOption { get; set; } = new();

        public LocationJurisdictionCommandOption JurisdictionOption { get; set; } = new();

        public LocationRegionCommandOption RegionOption { get; set; } = new();

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.Configure<EventLogAggregateConfiguration>(host.Configuration.GetSection(nameof(EventLogAggregateConfiguration)));

            var eventLogAggregateConfiguration = new ModelBinder<EventLogAggregateConfiguration>();

            eventLogAggregateConfiguration.BindMemberFromValue(b => b.AggregationType, AggregationTypeArgument);
            eventLogAggregateConfiguration.BindMemberFromValue(b => b.Dates, DateOption);
            eventLogAggregateConfiguration.BindMemberFromValue(b => b.ParallelProcesses, PrallelProcessesOption);

            var eventAggregationQueryOptions = new ModelBinder<EventAggregationQueryOptions>();

            eventAggregationQueryOptions.BindMemberFromValue(b => b.IncludedLocations, IncludeLocationOption);
            eventAggregationQueryOptions.BindMemberFromValue(b => b.ExcludedLocations, ExcludeLocationOption);
            eventAggregationQueryOptions.BindMemberFromValue(b => b.IncludedLocationTypes, LocationTypeOption);
            eventAggregationQueryOptions.BindMemberFromValue(b => b.IncludedAreas, AreaOption);
            eventAggregationQueryOptions.BindMemberFromValue(b => b.IncludedJurisdictions, JurisdictionOption);
            eventAggregationQueryOptions.BindMemberFromValue(b => b.IncludedRegions, RegionOption);

            services.AddOptions<EventLogAggregateConfiguration>()
                .Configure<BindingContext>((a, b) =>
                {
                    eventLogAggregateConfiguration.UpdateInstance(a, b);
                    eventAggregationQueryOptions.UpdateInstance(a.EventAggregationQueryOptions, b);
                });

            services.AddHostedService<EventAggregationHostedService>();
        }
    }
}
