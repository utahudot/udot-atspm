#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages/ScanHostedServiceLogMessages.cs
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

using Google.Cloud.Diagnostics.Common;
using Microsoft.Extensions.Logging;

namespace Utah.Udot.Atspm.Infrastructure.LogMessages
{
    /// <summary>
    /// Log messages for watchdog scan configuration resolved by the hosted service.
    /// </summary>
    public partial class ScanHostedServiceLogMessages
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Log messages for watchdog scan configuration resolved by the hosted service.
        /// </summary>
        /// <param name="logger">The logger instance used to emit structured log messages.</param>
        /// <param name="serviceName">Name of the consuming service, added as a contextual label.</param>
        public ScanHostedServiceLogMessages(ILogger logger, string serviceName)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string>()
            {
                { "service", serviceName }
            });
        }

        /// <summary>
        /// Logs the effective watchdog scan dates and analysis windows after configuration and command-line overrides are resolved.
        /// </summary>
        [LoggerMessage(EventId = 1260, EventName = "Resolved Watchdog Scan Options", Level = LogLevel.Information, Message =
            "Resolved watchdog scan options. TimeZone: {timeZoneId}. DateTimeKind: {dateTimeKind}. PM date: {pmScanDate}, PM window: {pmStart} to {pmEnd}. AM date: {amScanDate}, AM window: {amStart} to {amEnd}. Ramp start date: {rampStartScanDate}, ramp end date: {rampEndScanDate}, ramp detector window: {rampDetectorStart} to {rampDetectorEnd}, ramp missed-detector window: {rampMissedDetectorStart} to {rampMissedDetectorEnd}, ramp mainline window: {rampMainlineStart} to {rampMainlineEnd}, ramp stuck-queue window: {rampStuckQueueStart} to {rampStuckQueueEnd}.")]
        public partial void ResolvedWatchdogScanOptions(
            string timeZoneId,
            string dateTimeKind,
            DateTime pmScanDate,
            DateTime pmStart,
            DateTime pmEnd,
            DateTime amScanDate,
            DateTime amStart,
            DateTime amEnd,
            DateTime rampStartScanDate,
            DateTime rampEndScanDate,
            DateTime rampDetectorStart,
            DateTime rampDetectorEnd,
            DateTime rampMissedDetectorStart,
            DateTime rampMissedDetectorEnd,
            DateTime rampMainlineStart,
            DateTime rampMainlineEnd,
            DateTime rampStuckQueueStart,
            DateTime rampStuckQueueEnd);
    }
}
