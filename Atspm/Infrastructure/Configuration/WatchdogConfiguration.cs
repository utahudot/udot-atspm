#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/WatchdogConfiguration.cs
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

using Utah.Udot.Atspm.Infrastructure.Attributes;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration options for the Watchdog anomaly detection and email alert service.
    /// </summary>
    [SystemConfigurable]
    public class WatchdogConfiguration
    {
        /// <summary>
        /// Default IANA time zone identifier used when calculating automatic scan dates.
        /// </summary>
        public const string DefaultTimeZoneId = "America/Denver";

        /// <summary>
        /// Target date used for the PM peak Watchdog scan. If not specified, defaults to the previous day in the configured time zone.
        /// </summary>
        public DateTime PmScanDate { get; set; }

        /// <summary>
        /// Target date used for the AM Watchdog scan. If not specified, defaults to the current day in the configured time zone.
        /// </summary>
        public DateTime AmScanDate { get; set; }

        /// <summary>
        /// Start date used when scanning for ramp missed detector hits. If not specified, defaults to the previous day.
        /// </summary>
        public DateTime RampMissedDetectorHitsStartScanDate { get; set; }

        /// <summary>
        /// End date used when scanning for ramp missed detector hits. If not specified, defaults to the current day.
        /// </summary>
        public DateTime RampMissedDetectorHitsEndScanDate { get; set; }

        /// <summary>
        /// Time zone identifier used for automatic scan date calculation.
        /// </summary>
        public string TimeZoneId { get; set; } = DefaultTimeZoneId;

        /// <summary>
        /// Start hour (0-23) of the AM analysis window.
        /// </summary>
        public int AmStartHour { get; set; } = 1;

        /// <summary>
        /// End hour (0-23) of the AM analysis window.
        /// </summary>
        public int AmEndHour { get; set; } = 5;

        /// <summary>
        /// Start hour (0-23) of the PM peak analysis window.
        /// </summary>
        public int PmPeakStartHour { get; set; } = 17;

        /// <summary>
        /// End hour (0-23) of the PM peak analysis window.
        /// </summary>
        public int PmPeakEndHour { get; set; } = 18;

        /// <summary>
        /// Start hour (0-23) of the ramp detector analysis window.
        /// </summary>
        public int RampDetectorStartHour { get; set; } = 7;

        /// <summary>
        /// End hour (0-23) of the ramp detector analysis window.
        /// </summary>
        public int RampDetectorEndHour { get; set; } = 8;

        /// <summary>
        /// Start hour (0-23) of the ramp missed detector hit analysis window.
        /// </summary>
        public int RampMissedDetectorHitStartHour { get; set; } = 15;

        /// <summary>
        /// End hour (0-23) of the ramp missed detector hit analysis window.
        /// </summary>
        public int RampMissedDetectorHitEndHour { get; set; } = 7;

        /// <summary>
        /// Start hour (0-23) of the ramp mainline analysis window.
        /// </summary>
        public int RampMainlineStartHour { get; set; } = 15;

        /// <summary>
        /// End hour (0-23) of the ramp mainline analysis window.
        /// </summary>
        public int RampMainlineEndHour { get; set; } = 19;

        /// <summary>
        /// Start hour (0-23) of the ramp stuck queue analysis window.
        /// </summary>
        public int RampStuckQueueStartHour { get; set; } = 1;

        /// <summary>
        /// End hour (0-23) of the ramp stuck queue analysis window.
        /// </summary>
        public int RampStuckQueueEndHour { get; set; } = 4;

        /// <summary>
        /// Indicates whether Watchdog scans should only evaluate weekdays.
        /// </summary>
        public bool WeekdayOnly { get; set; } = true;

        /// <summary>
        /// Number of consecutive occurrences required to trigger a phase termination anomaly.
        /// </summary>
        public int ConsecutiveCount { get; set; } = 3;

        /// <summary>
        /// Minimum number of phase terminations required before evaluating a phase for anomalies.
        /// </summary>
        public int MinPhaseTerminations { get; set; } = 50;

        /// <summary>
        /// Percentage threshold (0.0 to 1.0) of force-offs or max-outs required to flag an issue.
        /// </summary>
        public double PercentThreshold { get; set; } = .9;

        /// <summary>
        /// Minimum number of controller event log records required within the scan window before flagging missing records.
        /// </summary>
        public int MinimumRecords { get; set; } = 500;

        /// <summary>
        /// Minimum number of detector actuations expected during the scan window before flagging a low detector hit count.
        /// </summary>
        public int LowHitThreshold { get; set; } = 50;

        /// <summary>
        /// Minimum number of ramp detector actuations expected during the scan window before flagging a low ramp hit count.
        /// </summary>
        public int LowHitRampThreshold { get; set; } = 10;

        /// <summary>
        /// Maximum number of pedestrian events allowed during the scan window before flagging stuck pedestrian detection.
        /// </summary>
        public int MaximumPedestrianEvents { get; set; } = 200;

        /// <summary>
        /// Threshold count of missed ramp detector events before flagging an issue.
        /// </summary>
        public int RampMissedEventsThreshold { get; set; } = 3;

        /// <summary>
        /// Indicates whether all detected errors should be included in email notifications even if previously reported.
        /// </summary>
        public bool EmailAllErrors { get; set; }

        /// <summary>
        /// Indicates whether PM scan errors should trigger email notifications.
        /// </summary>
        public bool EmailPmErrors { get; set; } = true;

        /// <summary>
        /// Indicates whether AM scan errors should trigger email notifications.
        /// </summary>
        public bool EmailAmErrors { get; set; } = true;

        /// <summary>
        /// Indicates whether ramp scan errors should trigger email notifications.
        /// </summary>
        public bool EmailRampErrors { get; set; } = true;

        /// <summary>
        /// Fallback email address used for Watchdog notifications.
        /// </summary>
        public string DefaultEmailAddress { get; set; }

        /// <summary>
        /// Default sort column for Watchdog error reports.
        /// </summary>
        public string Sort { get; set; }

        /// <summary>
        /// Resolves the effective PM scan date using the configured date or the previous day in the configured time zone.
        /// </summary>
        /// <param name="timeProvider">Time provider used to determine current UTC time.</param>
        /// <returns>Effective PM scan date.</returns>
        public DateTime GetPmScanDate(TimeProvider timeProvider) =>
            GetConfiguredOrDefaultDate(PmScanDate, -1, timeProvider);

        /// <summary>
        /// Resolves the effective AM scan date using the configured date or the current day in the configured time zone.
        /// </summary>
        /// <param name="timeProvider">Time provider used to determine current UTC time.</param>
        /// <returns>Effective AM scan date.</returns>
        public DateTime GetAmScanDate(TimeProvider timeProvider) =>
            GetConfiguredOrDefaultDate(AmScanDate, 0, timeProvider);

        /// <summary>
        /// Resolves the effective start scan date for ramp missed detector hits.
        /// </summary>
        /// <param name="timeProvider">Time provider used to determine current UTC time.</param>
        /// <returns>Effective start scan date.</returns>
        public DateTime GetRampMissedDetectorHitsStartScanDate(TimeProvider timeProvider) =>
            GetConfiguredOrDefaultDate(RampMissedDetectorHitsStartScanDate, -1, timeProvider);

        /// <summary>
        /// Resolves the effective end scan date for ramp missed detector hits.
        /// </summary>
        /// <param name="timeProvider">Time provider used to determine current UTC time.</param>
        /// <returns>Effective end scan date.</returns>
        public DateTime GetRampMissedDetectorHitsEndScanDate(TimeProvider timeProvider) =>
            GetConfiguredOrDefaultDate(RampMissedDetectorHitsEndScanDate, 0, timeProvider);

        private DateTime GetConfiguredOrDefaultDate(DateTime configuredDate, int dayOffset, TimeProvider timeProvider)
        {
            if (configuredDate != default)
            {
                return AsDatabaseDate(configuredDate);
            }

            var timeZone = GetTimeZoneInfo(TimeZoneId);
            var localToday = TimeZoneInfo.ConvertTimeFromUtc(timeProvider.GetUtcNow().UtcDateTime, timeZone).Date;

            return AsDatabaseDate(localToday.AddDays(dayOffset));
        }

        private static DateTime AsDatabaseDate(DateTime date) =>
            DateTime.SpecifyKind(date.Date, DateTimeKind.Unspecified);

        private static TimeZoneInfo GetTimeZoneInfo(string timeZoneId)
        {
            var configuredTimeZoneId = string.IsNullOrWhiteSpace(timeZoneId)
                ? DefaultTimeZoneId
                : timeZoneId;

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(configuredTimeZoneId);
            }
            catch (TimeZoneNotFoundException) when (configuredTimeZoneId == DefaultTimeZoneId)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
            }
        }
    }
}
