#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices/ScanHostedService.cs
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Infrastructure.LogMessages;
using Utah.Udot.Atspm.Infrastructure.Services.HostedServices;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices
{
    /// <summary>
    /// Hosted background service that executes the signal performance scan process as part of the Watchdog system.
    /// Inherits from <see cref="HostedServiceBase"/> to provide scoped execution and logging.
    /// 
    /// This service configures scan and email options from <see cref="WatchdogConfiguration"/> and invokes
    /// the <see cref="ScanService"/> to perform the scan operation, typically used for monitoring and reporting
    /// on traffic signal performance and anomalies.
    /// </summary>
    public class ScanHostedService(ILogger<ScanHostedService> log, IServiceScopeFactory serviceProvider, IOptions<WatchdogConfiguration> options, TimeProvider timeProvider) : HostedServiceBase(log, serviceProvider)
    {
        private readonly WatchdogConfiguration _options = options.Value;
        private readonly ScanHostedServiceLogMessages logMessages = new(log, nameof(ScanHostedService));

        /// <inheritdoc/>
        public override async Task Process(IServiceScope scope, Stopwatch stopwatch = null, CancellationToken cancellationToken = default)
        {
            var amScanDate = _options.GetAmScanDate(timeProvider);
            var pmScanDate = _options.GetPmScanDate(timeProvider);
            var rampMissedDetectorHitsStartScanDate = _options.GetRampMissedDetectorHitsStartScanDate(timeProvider);
            var rampMissedDetectorHitsEndScanDate = _options.GetRampMissedDetectorHitsEndScanDate(timeProvider);

            logMessages.ResolvedWatchdogScanOptions(
                _options.TimeZoneId,
                pmScanDate,
                pmScanDate.Date + new TimeSpan(_options.PmPeakStartHour, 0, 0),
                pmScanDate.Date + new TimeSpan(_options.PmPeakEndHour, 0, 0),
                amScanDate,
                amScanDate.Date + new TimeSpan(_options.AmStartHour, 0, 0),
                amScanDate.Date + new TimeSpan(_options.AmEndHour, 0, 0),
                rampMissedDetectorHitsStartScanDate,
                rampMissedDetectorHitsEndScanDate,
                rampMissedDetectorHitsStartScanDate.Date + new TimeSpan(_options.RampDetectorStartHour, 0, 0),
                rampMissedDetectorHitsEndScanDate.Date + new TimeSpan(_options.RampDetectorEndHour, 0, 0),
                rampMissedDetectorHitsStartScanDate.Date + new TimeSpan(_options.RampMissedDetectorHitStartHour, 0, 0),
                rampMissedDetectorHitsEndScanDate.Date + new TimeSpan(_options.RampMissedDetectorHitEndHour, 0, 0),
                pmScanDate.Date + new TimeSpan(_options.RampMainlineStartHour, 0, 0),
                pmScanDate.Date + new TimeSpan(_options.RampMainlineEndHour, 0, 0),
                pmScanDate.Date + new TimeSpan(_options.RampStuckQueueStartHour, 0, 0),
                pmScanDate.Date + new TimeSpan(_options.RampStuckQueueEndHour, 0, 0));

            var options = new WatchdogLoggingOptions
            {
                AmScanDate = amScanDate,
                PmScanDate = pmScanDate,
                RampMissedDetectorHitsStartScanDate = rampMissedDetectorHitsStartScanDate,
                RampMissedDetectorHitsEndScanDate = rampMissedDetectorHitsEndScanDate,
                AmStartHour = _options.AmStartHour,
                AmEndHour = _options.AmEndHour,
                PmPeakStartHour = _options.PmPeakStartHour,
                PmPeakEndHour = _options.PmPeakEndHour,
                RampDetectorStartHour = _options.RampDetectorStartHour,
                RampDetectorEndHour = _options.RampDetectorEndHour,
                RampMissedDetectorHitStartHour = _options.RampMissedDetectorHitStartHour,
                RampMissedDetectorHitEndHour = _options.RampMissedDetectorHitEndHour,
                RampMainlineStartHour = _options.RampMainlineStartHour,
                RampMainlineEndHour = _options.RampMainlineEndHour,
                RampStuckQueueStartHour = _options.RampStuckQueueStartHour,
                RampStuckQueueEndHour = _options.RampStuckQueueEndHour,
                WeekdayOnly = _options.WeekdayOnly,
                ConsecutiveCount = _options.ConsecutiveCount,
                MinPhaseTerminations = _options.MinPhaseTerminations,
                PercentThreshold = _options.PercentThreshold,
                MinimumRecords = _options.MinimumRecords,
                LowHitThreshold = _options.LowHitThreshold,
                LowHitRampThreshold = _options.LowHitRampThreshold,
                MaximumPedestrianEvents = _options.MaximumPedestrianEvents,
                RampMissedEventsThreshold = _options.RampMissedEventsThreshold,
            };
            var emailOptions = new WatchdogEmailOptions
            {
                //EmailScanDate = _options.PmScanDate,
                AmScanDate = amScanDate,
                PmScanDate = pmScanDate,
                RampMissedDetectorHitsStartScanDate = rampMissedDetectorHitsStartScanDate,
                AmStartHour = _options.AmStartHour,
                AmEndHour = _options.AmEndHour,
                PmPeakStartHour = _options.PmPeakStartHour,
                PmPeakEndHour = _options.PmPeakEndHour,
                RampDetectorStartHour = _options.RampDetectorStartHour,
                RampDetectorEndHour = _options.RampDetectorEndHour,
                RampMissedDetectorHitStartHour = _options.RampMissedDetectorHitStartHour,
                RampMissedDetectorHitEndHour = _options.RampMissedDetectorHitEndHour,
                RampMainlineStartHour = _options.RampMainlineStartHour,
                RampMainlineEndHour = _options.RampMainlineEndHour,
                RampStuckQueueStartHour = _options.RampStuckQueueStartHour,
                RampStuckQueueEndHour = _options.RampStuckQueueEndHour,

                WeekdayOnly = _options.WeekdayOnly,
                DefaultEmailAddress = _options.DefaultEmailAddress,
                EmailAllErrors = _options.EmailAllErrors,
                EmailAmErrors = _options.EmailAmErrors,
                EmailPmErrors = _options.EmailPmErrors,
                EmailRampErrors = _options.EmailRampErrors,
                Sort = _options.Sort
            };

            var scanService = scope.ServiceProvider.GetService<ScanService>();

            await scanService.StartScan(options, emailOptions, cancellationToken);
        }
    }
}
