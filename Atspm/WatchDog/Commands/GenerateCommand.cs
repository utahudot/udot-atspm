#region license
// Copyright 2025 Utah Departement of Transportation
// for WatchDog - Utah.Udot.ATSPM.WatchDog.Commands/GenerateCommand.cs
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

namespace Utah.Udot.ATSPM.WatchDog.Commands
{

    public class GenerateCommand : Command, ICommandOption<WatchdogConfiguration>
    {
        public GenerateCommand() : base("generate", "To generate watchdog")
        {
            AddOption(PmScanDateOption);
            AddOption(AmScanDateOption);
            AddOption(RampMissedDetectorHitsStartScanDateOption);
            AddOption(RampMissedDetectorHitsEndScanDateOption);

            AddOption(AmStartHourOption);
            AddOption(AmEndHourOption);
            AddOption(PmPeakStartHourOption);
            AddOption(PmPeakEndHourOption);
            AddOption(RampDetectorStartHourOption);
            AddOption(RampDetectorEndHourOption);
            AddOption(RampMissedDetectorHitStartHourOption);
            AddOption(RampMissedDetectorHitEndHourOption);
            AddOption(RampMainlineStartHourOption);
            AddOption(RampMainlineEndHourOption);
            AddOption(RampStuckQueueStartHourOption);
            AddOption(RampStuckQueueEndHourOption);

            AddOption(WeekdayOnlyOption);
            AddOption(ConsecutiveCountOption);
            AddOption(MinPhaseTerminationsOption);
            AddOption(PercentThresholdOption);
            AddOption(MinimumRecordsOption);
            AddOption(LowHitThresholdOption);
            AddOption(LowHitRampThresholdOption);
            AddOption(MaximumPedestrianEventsOption);
            AddOption(RampMissedEventsThresholdOption);

            AddOption(EmailAllErrorsOption);
            AddOption(EmailPmErrorsOption);
            AddOption(EmailAmErrorsOption);
            AddOption(EmailRampErrorsOption);
            AddOption(DefaultEmailAddressOption);
            AddOption(SortOption);
        }

        public Option<DateTime> PmScanDateOption { get; set; } = new("--pmScanDate", "Scan Date");
        public Option<DateTime> AmScanDateOption { get; set; } = new("--amScanDate", "Scan Date");
        public Option<DateTime> RampMissedDetectorHitsStartScanDateOption { get; set; } = new("--rampMissedDetectorHitsStartScanDate", "Ramp Missed Detector Hits Start");
        public Option<DateTime> RampMissedDetectorHitsEndScanDateOption { get; set; } = new("--rampMissedDetectorHitsEndScanDate", "Ramp Missed Detector Hits End");

        public Option<int> AmStartHourOption { get; set; } = new("--amStartHour", "Am Start Hour");
        public Option<int> AmEndHourOption { get; set; } = new("--amEndHour", "Am End Hour");
        public Option<int> PmPeakStartHourOption { get; set; } = new("--pmPeakStartHour", "Pm Peak Start Hour");
        public Option<int> PmPeakEndHourOption { get; set; } = new("--pmPeakEndHour", "Pm Peak End Hour");
        public Option<int> RampDetectorStartHourOption { get; set; } = new("--rampDetectorStartHour", "Ramp Detector Start Hour");
        public Option<int> RampDetectorEndHourOption { get; set; } = new("--rampDetectorEndHour", "Ramp Detector End Hour");
        public Option<int> RampMissedDetectorHitStartHourOption { get; set; } = new("--rampMissedDetectorHitStartHour", "Ramp Missed Detector Hit Start Hour");
        public Option<int> RampMissedDetectorHitEndHourOption { get; set; } = new("--rampMissedDetectorHitEndHour", "Ramp Missed Detector Hit End Hour");
        public Option<int> RampMainlineStartHourOption { get; set; } = new("--rampMainlineStartHour", "Ramp Mainline Start Hour");
        public Option<int> RampMainlineEndHourOption { get; set; } = new("--rampMainlineEndHour", "Ramp Mainline End Hour");
        public Option<int> RampStuckQueueStartHourOption { get; set; } = new("--rampStuckQueueStartHour", "Ramp Stuck Queue Start Hour");
        public Option<int> RampStuckQueueEndHourOption { get; set; } = new("--rampStuckQueueEndHour", "Ramp Stuck Queue End Hour");

        public Option<bool> WeekdayOnlyOption { get; set; } = new("--weekdayOnly", "Weekday Only");
        public Option<int> ConsecutiveCountOption { get; set; } = new("--consecutiveCount", "Consecutive Count");
        public Option<int> MinPhaseTerminationsOption { get; set; } = new("--minPhaseTerminations", "Min Phase Terminations");
        public Option<double> PercentThresholdOption { get; set; } = new("--percentThreshold", "Percent Threshold");
        public Option<int> MinimumRecordsOption { get; set; } = new("--minimumRecords", "Minimum Records");
        public Option<int> LowHitThresholdOption { get; set; } = new("--lowHitThreshold", "Low Hit Threshold");
        public Option<int> LowHitRampThresholdOption { get; set; } = new("--lowHitRampThreshold", "Low Hit Ramp Threshold");
        public Option<int> MaximumPedestrianEventsOption { get; set; } = new("--maximumPedestrianEvents", "Maximum Pedestrian Events");
        public Option<int> RampMissedEventsThresholdOption { get; set; } = new("--rampMissedEventsThreshold", "Ramp Missed Events Threshold");

        public Option<bool> EmailAllErrorsOption { get; set; } = new("--emailAllErrors", "Email All Errors");
        public Option<bool> EmailPmErrorsOption { get; set; } = new("--emailPmErrors", "Email All Errors");
        public Option<bool> EmailAmErrorsOption { get; set; } = new("--emailAmErrors", "Email All Errors");
        public Option<bool> EmailRampErrorsOption { get; set; } = new("--emailRampErrors", "Email All Errors");
        public Option<string> DefaultEmailAddressOption { get; set; } = new("--defaultEmailAddress", "Default Email Address");
        public Option<string> SortOption { get; set; } = new("--sort", "Sort column");

        public ModelBinder<WatchdogConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<WatchdogConfiguration>();

            binder.BindMemberFromValue(b => b.PmScanDate, PmScanDateOption);
            binder.BindMemberFromValue(b => b.AmScanDate, AmScanDateOption);
            binder.BindMemberFromValue(b => b.RampMissedDetectorHitsStartScanDate, RampMissedDetectorHitsStartScanDateOption);
            binder.BindMemberFromValue(b => b.RampMissedDetectorHitsEndScanDate, RampMissedDetectorHitsEndScanDateOption);

            binder.BindMemberFromValue(b => b.AmStartHour, AmStartHourOption);
            binder.BindMemberFromValue(b => b.AmEndHour, AmEndHourOption);
            binder.BindMemberFromValue(b => b.PmPeakStartHour, PmPeakStartHourOption);
            binder.BindMemberFromValue(b => b.PmPeakEndHour, PmPeakEndHourOption);
            binder.BindMemberFromValue(b => b.RampDetectorStartHour, RampDetectorStartHourOption);
            binder.BindMemberFromValue(b => b.RampDetectorEndHour, RampDetectorEndHourOption);
            binder.BindMemberFromValue(b => b.RampMissedDetectorHitStartHour, RampMissedDetectorHitStartHourOption);
            binder.BindMemberFromValue(b => b.RampMissedDetectorHitEndHour, RampMissedDetectorHitEndHourOption);
            binder.BindMemberFromValue(b => b.RampMainlineStartHour, RampMainlineStartHourOption);
            binder.BindMemberFromValue(b => b.RampMainlineEndHour, RampMainlineEndHourOption);
            binder.BindMemberFromValue(b => b.RampStuckQueueStartHour, RampStuckQueueStartHourOption);
            binder.BindMemberFromValue(b => b.RampStuckQueueEndHour, RampStuckQueueEndHourOption);

            binder.BindMemberFromValue(b => b.WeekdayOnly, WeekdayOnlyOption);
            binder.BindMemberFromValue(b => b.ConsecutiveCount, ConsecutiveCountOption);
            binder.BindMemberFromValue(b => b.MinPhaseTerminations, MinPhaseTerminationsOption);
            binder.BindMemberFromValue(b => b.PercentThreshold, PercentThresholdOption);
            binder.BindMemberFromValue(b => b.MinimumRecords, MinimumRecordsOption);
            binder.BindMemberFromValue(b => b.LowHitThreshold, LowHitThresholdOption);
            binder.BindMemberFromValue(b => b.LowHitRampThreshold, LowHitRampThresholdOption);
            binder.BindMemberFromValue(b => b.MaximumPedestrianEvents, MaximumPedestrianEventsOption);
            binder.BindMemberFromValue(b => b.RampMissedEventsThreshold, RampMissedEventsThresholdOption);

            binder.BindMemberFromValue(b => b.EmailAllErrors, EmailAllErrorsOption);
            binder.BindMemberFromValue(b => b.EmailPmErrors, EmailPmErrorsOption);
            binder.BindMemberFromValue(b => b.EmailAmErrors, EmailAmErrorsOption);
            binder.BindMemberFromValue(b => b.EmailRampErrors, EmailRampErrorsOption);
            binder.BindMemberFromValue(b => b.DefaultEmailAddress, DefaultEmailAddressOption);
            binder.BindMemberFromValue(b => b.Sort, SortOption);

            return binder;
        }

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.Configure<WatchdogConfiguration>(host.Configuration.GetSection(nameof(WatchdogConfiguration)));

            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<WatchdogConfiguration>().BindCommandLine();
            services.AddHostedService<ScanHostedService>();
        }
    }
}
