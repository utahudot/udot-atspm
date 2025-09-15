#region license
// Copyright 2025 Utah Departement of Transportation
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
    public class ScanHostedService(ILogger<ScanHostedService> log, IServiceScopeFactory serviceProvider, IOptions<WatchdogConfiguration> options) : HostedServiceBase(log, serviceProvider)
    {
        private readonly WatchdogConfiguration _options = options.Value;

        /// <inheritdoc/>
        public override async Task Process(IServiceScope scope, Stopwatch stopwatch = null, CancellationToken cancellationToken = default)
        {
            var options = new WatchdogLoggingOptions
            {
                ConsecutiveCount = _options.ConsecutiveCount,
                LowHitThreshold = _options.LowHitThreshold,
                MaximumPedestrianEvents = _options.MaximumPedestrianEvents,
                MinimumRecords = _options.MinimumRecords,
                MinPhaseTerminations = _options.MinPhaseTerminations,
                PercentThreshold = _options.PercentThreshold,
                PreviousDayPMPeakEnd = _options.PreviousDayPMPeakEnd,
                PreviousDayPMPeakStart = _options.PreviousDayPMPeakStart,
                ScanDate = _options.ScanDate,
                ScanDayEndHour = _options.ScanDayEndHour,
                ScanDayStartHour = _options.ScanDayStartHour,
                WeekdayOnly = _options.WeekdayOnly
            };
            var emailOptions = new WatchdogEmailOptions
            {
                PreviousDayPMPeakEnd = _options.PreviousDayPMPeakEnd,
                PreviousDayPMPeakStart = _options.PreviousDayPMPeakStart,
                ScanDate = _options.ScanDate,
                ScanDayEndHour = _options.ScanDayEndHour,
                ScanDayStartHour = _options.ScanDayStartHour,
                WeekdayOnly = _options.WeekdayOnly,
                DefaultEmailAddress = _options.DefaultEmailAddress,
                EmailAllErrors = _options.EmailAllErrors,
                Sort = _options.Sort
            };

            var scanService = scope.ServiceProvider.GetService<ScanService>();

            await scanService.StartScan(options, emailOptions, cancellationToken);
        }
    }
}