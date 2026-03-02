#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices/WatchDogLogService.cs
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

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.TempExtensions;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices
{
    public partial class WatchDogRampLogService
    {
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly ILogger<WatchDogRampLogService> logger;

        public WatchDogRampLogService(IIndianaEventLogRepository controllerEventLogRepository,
            ILogger<WatchDogRampLogService> logger)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.logger = logger;
        }

        public async Task<List<WatchDogLogEvent>> GetWatchDogIssues(
            WatchdogRampLoggingOptions options,
            List<Location> locations,
            CancellationToken cancellationToken)
        {
            if (locations.IsNullOrEmpty())
            {
                return new List<WatchDogLogEvent>();
            }
            else
            {
                var errors = new ConcurrentBag<WatchDogLogEvent>();

                foreach (var Location in locations)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return errors.ToList();
                    }
                    List<IndianaEvent> locationEvents = new List<IndianaEvent>();

                    var RampMainLineLastRun = controllerEventLogRepository
                        .GetEventsBetweenDates(
                            Location.LocationIdentifier,
                            options.RampMissedDetectorHitStart,
                            options.RampMissedDetectorHitEnd)
                        .ToList();

                    var RampDetector = controllerEventLogRepository
                        .GetEventsBetweenDates(
                            Location.LocationIdentifier,
                            options.RampDetectorStart,
                            options.RampDetectorEnd)
                        .ToList();

                    locationEvents.AddRange(RampMainLineLastRun);
                    locationEvents.AddRange(RampDetector);

                    CheckDetectors(Location, options, locationEvents, errors);
                    CheckRampMissedDetectorHits(Location, options, locationEvents, errors);


                }
                return errors.ToList();
            }
        }

        private async Task CheckDetectors(Location location, WatchdogRampLoggingOptions options, List<IndianaEvent> locationEvents, ConcurrentBag<WatchDogLogEvent> errors)
        {
            var detectorEventCodes = new List<short> { 82, 81 };
            //Check for low Ramp hits
            CheckForLowRampDetectorHits(location, options, locationEvents, errors, detectorEventCodes);
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ///////////////////// WATCH DOG ERRORS /////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        //WatchDogIssueType LowRampDetectorHits - 10 - PM
        public void CheckForLowRampDetectorHits(Location location, WatchdogRampLoggingOptions options, List<IndianaEvent> locationEvents, ConcurrentBag<WatchDogLogEvent> errors, List<short> detectorEventCodes)
        {
            if (location.LocationTypeId != 2)
            {
                return;
            }
            var detectors = location.GetDetectorsForLocation();
            var detectionTypeValidIds = new List<int> { 8, 9, 10, 11 };

            var result = detectors
                .Where(d => d.DetectionTypes.Any(i => detectionTypeValidIds.Contains((int)i.Id)));

            foreach (var detector in detectors)
                try
                {
                    var channel = detector.DetectorChannel;
                    var currentVolume = locationEvents.Where(e => e.EventParam == detector.DetectorChannel &&
                        detectorEventCodes.Contains(e.EventCode) &&
                        e.Timestamp >= options.RampDetectorStart &&
                        e.Timestamp <= options.RampDetectorEnd
                        ).Count();
                    //Compare collected hits to low hit ramp threshold, 
                    if (currentVolume < Convert.ToInt32(options.LowHitRampThreshold))
                    {
                        AddDetectorError(
                            location,
                            options.RampMissedDetectorHitsStartScanDate,
                            detector,
                            WatchDogIssueTypes.LowRampDetectorHits,
                            $"CH: {channel} - Count: {currentVolume.ToString().ToLowerInvariant()}",
                            errors);
                    }

                }
                catch (Exception ex)
                {
                    logger.LogError($"CheckForLowRampDetectorHits {detector.Id} {ex.Message}");
                }
        }

        //WatchDogIssueType RampMissedDetectorHits - 11
        public void CheckRampMissedDetectorHits(Location location, WatchdogRampLoggingOptions options, List<IndianaEvent> locationEvents, ConcurrentBag<WatchDogLogEvent> errors)
        {
            if (location.LocationTypeId != 2)
                return;

            var detectors = location.GetDetectorsForLocation();
            var validDetectionTypeIds = new HashSet<int> { 8, 9, 10, 11 };
            var validEventCodes = new HashSet<short> { 1371, 1372, 1373 };
            var bucketSizeSeconds = 20;
            int wiggleSeconds = 9;

            // 1️⃣ Determine ramp window
            DateTime now = DateTime.Now;
            DateTime rampStart = options.RampMissedDetectorHitsStartScanDate;
            DateTime rampEnd = options.RampMissedDetectorHitsEndScanDate;

            // Ensure max 24-hour window
            if ((rampEnd - rampStart).TotalHours > 24)
                rampEnd = rampStart.AddHours(24);

            // 2️⃣ Define 3-hour periods
            var periodDefinitions = new List<(TimeSpan Start, TimeSpan End, string Label)>
            {
                (TimeSpan.FromHours(0), TimeSpan.FromHours(3), "12am-3am"),
                (TimeSpan.FromHours(3), TimeSpan.FromHours(6), "3am-6am"),
                (TimeSpan.FromHours(6), TimeSpan.FromHours(9), "6am-9am"),
                (TimeSpan.FromHours(9), TimeSpan.FromHours(12), "9am-12pm"),
                (TimeSpan.FromHours(12), TimeSpan.FromHours(15), "12pm-3pm"),
                (TimeSpan.FromHours(15), TimeSpan.FromHours(18), "3pm-6pm"),
                (TimeSpan.FromHours(18), TimeSpan.FromHours(21), "6pm-9pm"),
                (TimeSpan.FromHours(21), TimeSpan.FromHours(24), "9pm-12am")
            };

            foreach (var detector in detectors.Where(d => d.DetectionTypes.Any(dt => validDetectionTypeIds.Contains((int)dt.Id))))
            {
                try
                {
                    var channel = detector.DetectorChannel;

                    // Pull timestamps for this detector in ramp window
                    var timestamps = locationEvents
                        .Where(e => e.EventParam == channel && validEventCodes.Contains(e.EventCode) &&
                                    e.Timestamp >= rampStart && e.Timestamp <= rampEnd)
                        .Select(e => e.Timestamp)
                        .ToList();

                    if (!timestamps.Any())
                    {
                        AddDetectorError(location, options.RampMissedDetectorHitsStartScanDate, detector, WatchDogIssueTypes.RampMissedDetectorHits,
                            $"CH: {channel} - No events received in entire ramp window", errors);
                        continue;
                    }

                    // 3️⃣ Process each 3-hour period, clipped to ramp window
                    var periods = new List<(DateTime Start, DateTime End, string Label)>();
                    foreach (var (startTs, endTs, label) in periodDefinitions)
                    {
                        DateTime periodStart = rampStart.Date + startTs;
                        DateTime periodEnd = rampStart.Date + endTs;

                        // Skip periods outside ramp window
                        if (periodEnd <= rampStart || periodStart >= rampEnd)
                            continue;

                        // Clip periods to ramp window
                        periodStart = periodStart < rampStart ? rampStart : periodStart;
                        periodEnd = periodEnd > rampEnd ? rampEnd : periodEnd;

                        periods.Add((periodStart, periodEnd, label));
                    }

                    foreach (var (periodStart, periodEnd, label) in periods)
                    {
                        var periodTimestamps = timestamps
                            .Where(ts => ts >= periodStart && ts < periodEnd)
                            .ToList();

                        if (!periodTimestamps.Any())
                        {
                            AddDetectorError(location, options.RampMissedDetectorHitsStartScanDate, detector, WatchDogIssueTypes.RampMissedDetectorHits,
                                $"CH: {channel} - No events in period {label}", errors);
                            continue;
                        }

                        int totalBuckets = (int)Math.Ceiling((periodEnd - periodStart).TotalSeconds / bucketSizeSeconds);
                        var bucketsWithHits = new HashSet<int>();

                        foreach (var ts in periodTimestamps)
                        {
                            int bucketIndex = (int)((ts - periodStart).TotalSeconds / bucketSizeSeconds);
                            bucketsWithHits.Add(bucketIndex);

                            // Previous bucket if within wiggle
                            if (bucketIndex > 0 && (ts - periodStart).TotalSeconds % bucketSizeSeconds <= wiggleSeconds)
                                bucketsWithHits.Add(bucketIndex - 1);

                            // Next bucket if within wiggle
                            if (bucketIndex < totalBuckets - 1 && (bucketSizeSeconds - (ts - periodStart).TotalSeconds % bucketSizeSeconds) <= wiggleSeconds)
                                bucketsWithHits.Add(bucketIndex + 1);
                        }

                        int missedBuckets = totalBuckets - bucketsWithHits.Count;
                        if (missedBuckets > options.RampMissedEventsThreshold)
                        {
                            AddDetectorError(location, options.RampMissedDetectorHitsStartScanDate, detector, WatchDogIssueTypes.RampMissedDetectorHits,
                                $"CH: {channel} - Period {label} missed {missedBuckets} intervals", errors);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"CheckRampMissedDetectorHits {detector.Id} {ex.Message}");
                }
            }
        }

        ///////////////////////////////////////////////////////////
        /////////////////////// HELPER ////////////////////////////
        ///////////////////////////////////////////////////////////
        private static void AddDetectorError(Location location, DateTime timestamp, Detector detector, WatchDogIssueTypes issueType, string message, ConcurrentBag<WatchDogLogEvent> errors)
        {
            var error = new WatchDogLogEvent(
                location.Id,
                location.LocationIdentifier,
                timestamp,
                WatchDogComponentTypes.Detector,
                detector?.Id ?? -1,
                issueType,
                message,
                null);

            if (!errors.Contains(error))
                errors.Add(error);
        }

    }
}