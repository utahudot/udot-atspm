#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages/ScanServiceLogMessages.cs
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
    /// Log messages for the watchdog scan orchestration service.
    /// </summary>
    public partial class ScanServiceLogMessages
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Log messages for the watchdog scan orchestration service.
        /// </summary>
        /// <param name="logger">The logger instance used to emit structured log messages.</param>
        /// <param name="serviceName">Name of the consuming service, added as a contextual label.</param>
        public ScanServiceLogMessages(ILogger logger, string serviceName)
        {
            _logger = logger.WithAddedLabels(new Dictionary<string, string>()
            {
                { "service", serviceName }
            });
        }

        /// <summary>
        /// Logs that the watchdog email is being skipped because the scan date falls on a weekend.
        /// </summary>
        /// <param name="scanDate">The scan date that fell on a weekend.</param>
        [LoggerMessage(EventId = 1200, EventName = "Skipping Weekend Email", Level = LogLevel.Information, Message = "Skipping watchdog email: WeekdayOnly is enabled and scan date {scanDate} falls on a weekend.")]
        public partial void SkippingWeekendEmail(DateTime scanDate);

        /// <summary>
        /// Logs a failed attempt to save watchdog error logs.
        /// </summary>
        /// <param name="attempt">The attempt number that failed.</param>
        /// <param name="ex">The exception thrown while saving.</param>
        [LoggerMessage(EventId = 1201, EventName = "Save Error Logs Attempt Failed", Level = LogLevel.Error, Message = "Attempt {attempt} to save watchdog error logs failed.")]
        public partial void SaveErrorLogsAttemptFailed(int attempt, Exception ex);

        /// <summary>
        /// Logs that a retry of saving watchdog error logs is scheduled.
        /// </summary>
        /// <param name="delayMs">The delay before retrying, in milliseconds.</param>
        [LoggerMessage(EventId = 1202, EventName = "Save Error Logs Retrying", Level = LogLevel.Information, Message = "Retrying watchdog error log save in {delayMs} ms.")]
        public partial void SaveErrorLogsRetrying(int delayMs);

        /// <summary>
        /// Logs that a watchdog scan has started.
        /// </summary>
        /// <param name="scanDate">The scan date driving the scan.</param>
        /// <param name="amEnabled">Whether AM error emailing is enabled.</param>
        /// <param name="pmEnabled">Whether PM error emailing is enabled.</param>
        /// <param name="rampEnabled">Whether ramp error emailing is enabled.</param>
        [LoggerMessage(EventId = 1203, EventName = "Scan Started", Level = LogLevel.Information, Message = "Watchdog scan started for {scanDate}. AM: {amEnabled}, PM: {pmEnabled}, Ramp: {rampEnabled}")]
        public partial void ScanStarted(DateTime scanDate, bool amEnabled, bool pmEnabled, bool rampEnabled);

        /// <summary>
        /// Logs that a watchdog scan was aborted because no email options were enabled.
        /// </summary>
        [LoggerMessage(EventId = 1204, EventName = "No Email Options Enabled", Level = LogLevel.Error, Message = "Watchdog scan aborted: no email options are enabled.")]
        public partial void NoEmailOptionsEnabled();

        /// <summary>
        /// Logs that a scan segment reused already-persisted errors for the scan date.
        /// </summary>
        /// <param name="segment">The scan segment (AM, PM, or Ramp).</param>
        /// <param name="count">The number of existing errors reused.</param>
        [LoggerMessage(EventId = 1205, EventName = "Segment Errors Reused", Level = LogLevel.Information, Message = "{segment} scan reused {count} existing error(s) for the scan date.")]
        public partial void SegmentErrorsReused(string segment, int count);

        /// <summary>
        /// Logs that a scan segment computed and saved new errors.
        /// </summary>
        /// <param name="segment">The scan segment (AM, PM, or Ramp).</param>
        /// <param name="count">The number of errors computed and saved.</param>
        [LoggerMessage(EventId = 1206, EventName = "Segment Errors Computed", Level = LogLevel.Information, Message = "{segment} scan computed and saved {count} new error(s).")]
        public partial void SegmentErrorsComputed(string segment, int count);

        /// <summary>
        /// Logs the segmented result breakdown for a scan segment.
        /// </summary>
        /// <param name="segment">The scan segment (AM, PM, or Ramp).</param>
        /// <param name="newCount">Count of new issues.</param>
        /// <param name="dailyRecurringCount">Count of daily recurring issues.</param>
        /// <param name="recurringCount">Count of recurring issues.</param>
        /// <param name="dayBeforeCount">Count of day-before issues.</param>
        [LoggerMessage(EventId = 1207, EventName = "Segment Summary", Level = LogLevel.Information, Message = "{segment} segmented results - New: {newCount}, DailyRecurring: {dailyRecurringCount}, Recurring: {recurringCount}, DayBefore: {dayBeforeCount}")]
        public partial void SegmentSummary(string segment, int newCount, int dailyRecurringCount, int recurringCount, int dayBeforeCount);

        /// <summary>
        /// Logs that the watchdog scan analysis has completed.
        /// </summary>
        /// <param name="scanDate">The scan date driving the scan.</param>
        /// <param name="totalErrors">Total number of errors gathered across all segments.</param>
        /// <param name="elapsedMs">Elapsed analysis time, in milliseconds.</param>
        [LoggerMessage(EventId = 1208, EventName = "Scan Completed", Level = LogLevel.Information, Message = "Watchdog scan analysis completed for {scanDate}. Total errors: {totalErrors}. Elapsed: {elapsedMs} ms.")]
        public partial void ScanCompleted(DateTime scanDate, int totalErrors, long elapsedMs);
    }
}
