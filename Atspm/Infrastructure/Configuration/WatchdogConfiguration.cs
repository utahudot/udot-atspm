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

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration values used by Watchdog scans, report windows, thresholds, and email behavior.
    /// </summary>
    [ConfigurationSection(nameof(WatchdogConfiguration), "Configuration for Watchdog scan behavior")]
    public class WatchdogConfiguration
    {
        public const string DefaultTimeZoneId = "America/Denver";

        /// <summary>
        /// Date whose evening period is checked for detector and record-count issues.
        /// </summary>
        public DateTime PmScanDate { get; set; }

        /// <summary>
        /// Date whose early-morning period is checked for phase-termination and pedestrian issues.
        /// </summary>
        public DateTime AmScanDate { get; set; }

        /// <summary>
        /// First date in the ramp-detector missed-event scan range.
        /// </summary>
        public DateTime RampMissedDetectorHitsStartScanDate { get; set; }

        /// <summary>
        /// Last date in the ramp-detector missed-event scan range.
        /// </summary>
        public DateTime RampMissedDetectorHitsEndScanDate { get; set; }

        /// <summary>
        /// Time-zone identifier used to derive scan dates.
        /// </summary>
        public string TimeZoneId { get; set; } = DefaultTimeZoneId;

        /// <summary>
        /// Inclusive start hour for the morning phase-termination scan.
        /// </summary>
        public int AmStartHour { get; set; } = 1;

        /// <summary>
        /// Exclusive end hour for the morning phase-termination scan.
        /// </summary>
        public int AmEndHour { get; set; } = 5;

        /// <summary>
        /// Inclusive start hour for the previous-day PM detector scan.
        /// </summary>
        public int PmPeakStartHour { get; set; } = 18;

        /// <summary
        /// >Exclusive end hour for the PM detector scan.
        /// </summary>
        public int PmPeakEndHour { get; set; } = 17;

        /// <summary>
        /// Inclusive start hour for ramp-detector volume checks.
        /// </summary>
        public int RampDetectorStartHour { get; set; } = 7;

        /// <summary>
        /// Exclusive end hour for ramp-detector volume checks.
        /// </summary>
        public int RampDetectorEndHour { get; set; } = 8;

        /// <summary>
        /// Inclusive start hour for the ramp missed-event analysis window.
        /// </summary>
        public int RampMissedDetectorHitStartHour { get; set; } = 15;

        /// <summary>
        /// Exclusive end hour for the ramp missed-event analysis window.
        /// </summary>
        public int RampMissedDetectorHitEndHour { get; set; } = 7;

        /// <summary>
        /// Inclusive start hour for ramp-mainline detector checks.
        /// </summary>
        public int RampMainlineStartHour { get; set; } = 15;

        /// <summary>
        /// Exclusive end hour for ramp-mainline detector checks.
        /// </summary>
        public int RampMainlineEndHour { get; set; } = 19;

        /// <summary>
        /// Inclusive start hour for ramp stuck-queue checks.
        /// </summary>
        public int RampStuckQueueStartHour { get; set; } = 1;

        /// <summary>
        /// Exclusive end hour for ramp stuck-queue checks.
        /// </summary>
        public int RampStuckQueueEndHour { get; set; } = 4;

        /// <summary>
        /// Whether scheduled scans are skipped on Saturdays and Sundays.
        /// </summary>
        public bool WeekdayOnly { get; set; } = true;

        /// <summary>
        /// Number of consecutive occurrences required to report a stuck-pedestrian issue.
        /// </summary>
        public int ConsecutiveCount { get; set; } = 3;

        /// <summary>
        /// Minimum non-gap or total phase terminations required before percentage thresholds are evaluated.
        /// </summary>
        public int MinPhaseTerminations { get; set; } = 50;

        /// <summary>
        /// Fraction of phase terminations that must be force-offs or max-outs to report an issue.
        /// </summary>
        public double PercentThreshold { get; set; } = .9;

        /// <summary>
        /// Minimum event records a location must have to pass the PM record-count check.
        /// </summary>
        public int MinimumRecords { get; set; } = 500;

        /// <summary>
        /// Detector-volume count below which a standard detector is reported as low-hit.
        /// </summary>
        public int LowHitThreshold { get; set; } = 50;

        /// <summary>
        /// Detector-volume count below which a ramp detector is reported as low-hit.
        /// </summary>
        public int LowHitRampThreshold { get; set; } = 10;

        /// <summary>
        /// Pedestrian-event count above which a phase is reported for excessive activations.
        /// </summary>
        public int MaximumPedestrianEvents { get; set; } = 200;

        /// <summary>
        /// Number of missed ramp event buckets above which an issue is reported.
        /// </summary>
        public int RampMissedEventsThreshold { get; set; } = 3;

        /// <summary>
        /// Whether emails include recurring errors in addition to newly detected errors.
        /// </summary>
        public bool EmailAllErrors { get; set; }

        /// <summary>
        /// Whether PM detector and record-count issues are analyzed and emailed.
        /// </summary>
        public bool EmailPmErrors { get; set; } = true;

        /// <summary>
        /// Whether AM phase-termination and pedestrian issues are analyzed and emailed.
        /// </summary>
        public bool EmailAmErrors { get; set; } = true;

        /// <summary>
        /// Whether ramp detector issues are analyzed and emailed.
        /// </summary>
        public bool EmailRampErrors { get; set; } = true;

        /// <summary>
        /// Fallback recipient address used for watchdog email messages.
        /// </summary>
        public string DefaultEmailAddress { get; set; }

        /// <summary>
        /// Sort expression applied when ordering watchdog errors in reports.
        /// </summary>
        public string Sort { get; set; }

        public DateTime GetPmScanDate(TimeProvider timeProvider) =>
            GetConfiguredOrDefaultDate(PmScanDate, -1, timeProvider);

        public DateTime GetAmScanDate(TimeProvider timeProvider) =>
            GetConfiguredOrDefaultDate(AmScanDate, 0, timeProvider);

        public DateTime GetRampMissedDetectorHitsStartScanDate(TimeProvider timeProvider) =>
            GetConfiguredOrDefaultDate(RampMissedDetectorHitsStartScanDate, -1, timeProvider);

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
