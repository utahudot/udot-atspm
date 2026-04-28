#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices/WatchDogRampLogService.cs
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

            var rampDetectors = detectors
                .Where(d => d.DetectionTypes.Any(i => detectionTypeValidIds.Contains((int)i.Id)));

            foreach (var detector in rampDetectors)
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
                            errors,
                            channel.ToString());
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

            var validDetectionTypeIds = new HashSet<int> { 8, 9, 10, 11 };
            var validEventCodes = new HashSet<short> { 1371, 1372, 1373 };
            var bucketSizeSeconds = 20;
            int wiggleSeconds = 9;
            var detectors = location.GetDetectorsForLocation()
                .Where(d => d.DetectionTypes.Any(dt => validDetectionTypeIds.Contains((int)dt.Id)))
                .ToList();

            if (!detectors.Any())
            {
                return;
            }

            var representativeDetector = detectors.First();
            var rampStart = options.RampMissedDetectorHitStart;
            var rampEnd = options.RampMissedDetectorHitEnd;

            if (rampEnd <= rampStart)
            {
                rampEnd = rampEnd.AddDays(1);
            }

            var timestamps = locationEvents
                .Where(e => validEventCodes.Contains(e.EventCode) &&
                            e.Timestamp >= rampStart &&
                            e.Timestamp < rampEnd)
                .Select(e => e.Timestamp)
                .OrderBy(ts => ts)
                .ToList();

            if (!timestamps.Any())
            {
                AddDetectorError(
                    location,
                    options.RampMissedDetectorHitsStartScanDate,
                    representativeDetector,
                    WatchDogIssueTypes.RampMissedDetectorHits,
                    "No ramp mainline events received in the configured window",
                    errors,
                    representativeDetector.DetectorChannel.ToString());
                return;
            }

            int totalBuckets = (int)Math.Ceiling((rampEnd - rampStart).TotalSeconds / bucketSizeSeconds);
            var bucketsWithHits = new HashSet<int>();

            foreach (var ts in timestamps)
            {
                int bucketIndex = (int)((ts - rampStart).TotalSeconds / bucketSizeSeconds);
                bucketsWithHits.Add(bucketIndex);

                var secondsIntoBucket = (ts - rampStart).TotalSeconds % bucketSizeSeconds;

                if (bucketIndex > 0 && secondsIntoBucket <= wiggleSeconds)
                {
                    bucketsWithHits.Add(bucketIndex - 1);
                }

                if (bucketIndex < totalBuckets - 1 && (bucketSizeSeconds - secondsIntoBucket) <= wiggleSeconds)
                {
                    bucketsWithHits.Add(bucketIndex + 1);
                }
            }

            int missedBuckets = totalBuckets - bucketsWithHits.Count;
            if (missedBuckets > options.RampMissedEventsThreshold)
            {
                AddDetectorError(
                    location,
                    options.RampMissedDetectorHitsStartScanDate,
                    representativeDetector,
                    WatchDogIssueTypes.RampMissedDetectorHits,
                    $"Ramp mainline window missed {missedBuckets} intervals between {rampStart:t} and {rampEnd:t}",
                    errors,
                    representativeDetector.DetectorChannel.ToString());
            }
        }

        ///////////////////////////////////////////////////////////
        /////////////////////// HELPER ////////////////////////////
        ///////////////////////////////////////////////////////////
        private static void AddDetectorError(Location location, DateTime timestamp, Detector detector, WatchDogIssueTypes issueType, string message, ConcurrentBag<WatchDogLogEvent> errors, string key)
        {
            //This should usually have the channel as the key so that it can be used to ignore.
            var error = new WatchDogLogEvent(
                location.Id,
                location.LocationIdentifier,
                timestamp,
                WatchDogComponentTypes.Detector,
                detector?.Id ?? -1,
                issueType,
                message,
                key,
                phase: null);

            if (!errors.Contains(error))
                errors.Add(error);
        }

    }
}
