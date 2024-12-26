#region license
// Copyright 2024 Utah Department of Transportation
// for DatabaseInstaller.Commands/UpdateCommand.cs
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
    public class WatchdogCommand : Command, ICommandOption<WatchdogConfiguration>
    {
        public WatchdogCommand() : base("generate", "To generate watchdog")
        {
            AddOption(ScanDateOption);
            AddOption(PreviousDayPMPeakStartOption);
            AddOption(PreviousDayPMPeakEndOption);
            AddOption(WeekdayOnlyOption);
            AddOption(ScanDayStartHourOption);
            AddOption(ScanDayEndHourOption);
            AddOption(ConsecutiveCountOption);
            AddOption(LowHitThresholdOption);
            AddOption(MaximumPedestrianEventsOption);
            AddOption(MinimumRecordsOption);
            AddOption(MinPhaseTerminationsOption);
            AddOption(PercentThresholdOption);
            AddOption(EmailAllErrorsOption);
            AddOption(DefaultEmailAddressOption);
        }

        public Option<DateTime> ScanDateOption { get; set; } = new("--scanDate", "Scan Date");
        public Option<int> PreviousDayPMPeakStartOption { get; set; } = new("--previousDayPmPeakStart", "Previous Day Pm Peak Start");
        public Option<int> PreviousDayPMPeakEndOption { get; set; } = new("--previousDayPmPeakEnd", "Previous Day Pm Peak End");
        public Option<bool> WeekdayOnlyOption { get; set; } = new("--weekdayOnly", "Weekday Only");
        public Option<int> ScanDayStartHourOption { get; set; } = new("--scanDayStartHour", "Scan Day Start Hour");
        public Option<int> ScanDayEndHourOption { get; set; } = new("--scanDayEndHour", "Scan Day End Hour");
        public Option<int> RampMainlineStartHour { get; set; } = new("--rampMainlineStartHour", "Ramp Mainline Start Hour");
        public Option<int> RampMainlineEndHour { get; set; } = new("--rampMainlineEndHour", "Ramp Mainline End Hour");
        public Option<int> RampStuckQueueStartHour { get; set; } = new("--rampStuckQueueStartHour", "Ramp Stuck Queue Start Hour");
        public Option<int> RampStuckQueueEndHour { get; set; } = new("--rampStuckQueueEndHour", "Ramp Stuck Queue End Hour");


        public Option<int> ConsecutiveCountOption { get; set; } = new("--consecutiveCount", "Consecutive Count");
        public Option<int> LowHitThresholdOption { get; set; } = new("--lowHitThreshold", "Low Hit Threshold");
        public Option<int> MaximumPedestrianEventsOption { get; set; } = new("--maximumPedestrianEvents", "Maximum Pedestrian Events");
        public Option<int> MinimumRecordsOption { get; set; } = new("--minimumRecords", "Minimum Records");
        public Option<int> MinPhaseTerminationsOption { get; set; } = new("--minPhaseTerminations", "Min Phase Terminations");
        public Option<double> PercentThresholdOption { get; set; } = new("--percentThreshold", "Percent Threshold");

        public Option<bool> EmailAllErrorsOption { get; set; } = new("--emailAllErrors", "Email All Errors");
        public Option<string> DefaultEmailAddressOption { get; set; } = new("--defaultEmailAddress", "Default Email Address");


        public Option<DateTime> AnalysisStartOption { get; set; }

        public Option<DateTime> AnalysisEndOption { get; set; }

        public ModelBinder<WatchdogConfiguration> GetOptionsBinder()
        {
            var binder = new ModelBinder<WatchdogConfiguration>();

            binder.BindMemberFromValue(b => b.ScanDate, ScanDateOption);
            binder.BindMemberFromValue(b => b.PreviousDayPMPeakStart, PreviousDayPMPeakStartOption);
            binder.BindMemberFromValue(b => b.PreviousDayPMPeakEnd, PreviousDayPMPeakEndOption);
            binder.BindMemberFromValue(b => b.WeekdayOnly, WeekdayOnlyOption);
            binder.BindMemberFromValue(b => b.ScanDayStartHour, ScanDayStartHourOption);
            binder.BindMemberFromValue(b => b.ScanDayEndHour, ScanDayEndHourOption);
            binder.BindMemberFromValue(b => b.ConsecutiveCount, ConsecutiveCountOption);
            binder.BindMemberFromValue(b => b.LowHitThreshold, LowHitThresholdOption);
            binder.BindMemberFromValue(b => b.MaximumPedestrianEvents, MaximumPedestrianEventsOption);
            binder.BindMemberFromValue(b => b.MinimumRecords, MinimumRecordsOption);
            binder.BindMemberFromValue(b => b.MinPhaseTerminations, MinPhaseTerminationsOption);
            binder.BindMemberFromValue(b => b.PercentThreshold, PercentThresholdOption);
            binder.BindMemberFromValue(b => b.EmailAllErrors, EmailAllErrorsOption);
            binder.BindMemberFromValue(b => b.DefaultEmailAddress, DefaultEmailAddressOption);

            return binder;
        }

        public void BindCommandOptions(HostBuilderContext host, IServiceCollection services)
        {
            services.AddOptions<WatchdogConfiguration>().Bind(host.Configuration.GetSection("WatchdogConfiguration"));
            services.Configure<EmailConfiguration>(host.Configuration.GetSection("EmailConfiguration"));
            services.AddScoped<WatchdogEmailService>();

            services.AddSingleton(GetOptionsBinder());
            services.AddOptions<WatchdogConfiguration>().BindCommandLine();

        }
    }
}
