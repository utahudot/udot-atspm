#region license
// Copyright 2026 Utah Departement of Transportation
// for EventLogUtility - Utah.Udot.Atspm.EventLogUtility.Commands/SignalTimingPlanBackfillCommand.cs
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
using Utah.Udot.ATSPM.EventLogUtility.Commands;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;

namespace Utah.Udot.Atspm.EventLogUtility.Commands
{
    /// <summary>
    /// Backfills signal timing plans from stored event logs.
    /// </summary>
    public class SignalTimingPlanBackfillCommand : Command, ICommandOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignalTimingPlanBackfillCommand"/> class.
        /// </summary>
        public SignalTimingPlanBackfillCommand() : base("signal-timing-plans", "Backfills signal timing plans from stored event logs")
        {
            AddAlias("plans");

            ProcessingBatchSizeOption.SetDefaultValue(50000);
            PrallelProcessesOption.SetDefaultValue(5);
            SignalTimingPlanOffsetHoursOption.SetDefaultValue(72);

            Start.AddValidator(r =>
            {
                if (r.GetValueForOption(End) <= r.GetValueForOption(Start))
                    r.ErrorMessage = "Start date must be before end date";
            });
            End.AddValidator(r =>
            {
                if (r.GetValueForOption(Start) >= r.GetValueForOption(End))
                    r.ErrorMessage = "End date must be after start date";
            });

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

            AddOption(Start);
            AddOption(End);
            AddOption(ProcessingBatchSizeOption);
            AddOption(PrallelProcessesOption);
            AddOption(SignalTimingPlanOffsetHoursOption);
            AddOption(IncludeLocationOption);
            AddOption(ExcludeLocationOption);
            AddOption(LocationTypeOption);
            AddOption(AreaOption);
            AddOption(JurisdictionOption);
            AddOption(RegionOption);
        }

        public StartOption Start { get; set; } = new();

        public EndOption End { get; set; } = new();

        public ProcessingBatchSizeOption ProcessingBatchSizeOption { get; set; } = new();

        public PrallelProcessesOption PrallelProcessesOption { get; set; } = new();

        public SignalTimingPlanOffsetHoursOption SignalTimingPlanOffsetHoursOption { get; set; } = new();

        public LocationIncludeCommandOption IncludeLocationOption { get; set; } = new();

        public LocationExcludeCommandOption ExcludeLocationOption { get; set; } = new();

        public LocationTypeCommandOption LocationTypeOption { get; set; } = new();

        public LocationAreaCommandOption AreaOption { get; set; } = new();

        public LocationJurisdictionCommandOption JurisdictionOption { get; set; } = new();

        public LocationRegionCommandOption RegionOption { get; set; } = new();

        /// <inheritdoc/>
        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.Configure<SignalTimingPlanBackfillConfiguration>(host.Configuration.GetSection(nameof(SignalTimingPlanBackfillConfiguration)));
            services.Configure<DeviceEventLoggingConfiguration>(host.Configuration.GetSection(nameof(DeviceEventLoggingConfiguration)));

            var backfillConfiguration = new ModelBinder<SignalTimingPlanBackfillConfiguration>();

            backfillConfiguration.BindMemberFromValue(b => b.Start, Start);
            backfillConfiguration.BindMemberFromValue(b => b.End, End);
            backfillConfiguration.BindMemberFromValue(b => b.ProcessingBatchSize, ProcessingBatchSizeOption);
            backfillConfiguration.BindMemberFromValue(b => b.ParallelProcesses, PrallelProcessesOption);
            backfillConfiguration.BindMemberFromValue(b => b.SignalTimingPlanOffsetHours, SignalTimingPlanOffsetHoursOption);

            var eventAggregationQueryOptions = new ModelBinder<EventAggregationQueryOptions>();

            eventAggregationQueryOptions.BindMemberFromValue(b => b.IncludedLocations, IncludeLocationOption);
            eventAggregationQueryOptions.BindMemberFromValue(b => b.ExcludedLocations, ExcludeLocationOption);
            eventAggregationQueryOptions.BindMemberFromValue(b => b.IncludedLocationTypes, LocationTypeOption);
            eventAggregationQueryOptions.BindMemberFromValue(b => b.IncludedAreas, AreaOption);
            eventAggregationQueryOptions.BindMemberFromValue(b => b.IncludedJurisdictions, JurisdictionOption);
            eventAggregationQueryOptions.BindMemberFromValue(b => b.IncludedRegions, RegionOption);

            var deviceEventLoggingConfiguration = new ModelBinder<DeviceEventLoggingConfiguration>();
            deviceEventLoggingConfiguration.BindMemberFromValue(b => b.SignalTimingPlanOffsetHours, SignalTimingPlanOffsetHoursOption);

            services.AddOptions<SignalTimingPlanBackfillConfiguration>()
                .Configure<BindingContext>((a, b) =>
                {
                    backfillConfiguration.UpdateInstance(a, b);
                    eventAggregationQueryOptions.UpdateInstance(a.EventAggregationQueryOptions, b);
                })
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<DeviceEventLoggingConfiguration>()
                .Configure<BindingContext>((a, b) => deviceEventLoggingConfiguration.UpdateInstance(a, b))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddHostedService<SignalTimingPlanBackfillHostedService>();
        }
    }
}
