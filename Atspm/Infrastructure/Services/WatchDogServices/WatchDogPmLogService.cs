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
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.Watchdog;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.TempExtensions;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices
{
    public partial class WatchDogPmLogService
    {
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly PhaseService phaseService;
        private readonly ILogger<WatchDogRampLogService> logger;

        public WatchDogPmLogService(IIndianaEventLogRepository controllerEventLogRepository,
            PhaseService phaseService,
            ILogger<WatchDogRampLogService> logger)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.phaseService = phaseService;
            this.logger = logger;
        }

        public async Task<List<WatchDogLogEvent>> GetWatchDogIssues(
            WatchdogPmLoggingOptions options,
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

                    var PmAnalysis = controllerEventLogRepository
                        .GetEventsBetweenDates(
                            Location.LocationIdentifier,
                            options.PmAnalysisStart,
                            options.PmAnalysisEnd)
                        .ToList();
                    var RampMainline = controllerEventLogRepository
                        .GetEventsBetweenDates(
                            Location.LocationIdentifier,
                            options.PmScanDate.Date + new TimeSpan(options.RampMainlineStartHour, 0, 0),
                            options.PmScanDate.Date + new TimeSpan(options.RampMainlineEndHour, 0, 0))
                        .ToList();
                    var RampStuckQueue = controllerEventLogRepository
                        .GetEventsBetweenDates(
                            Location.LocationIdentifier,
                            options.PmScanDate.Date + new TimeSpan(options.RampStuckQueueStartHour, 0, 0),
                            options.PmScanDate.Date + new TimeSpan(options.RampStuckQueueEndHour, 0, 0))
                        .ToList();

                    locationEvents.AddRange(PmAnalysis);
                    locationEvents.AddRange(RampMainline);
                    locationEvents.AddRange(RampStuckQueue);

                    var recordsError = await CheckLocationRecordCount(options.PmScanDate, Location, options, locationEvents);
                    if (recordsError != null)
                    {
                        errors.Add(recordsError);
                        continue;
                    }
                    var tasks = new List<Task>();
                    tasks.Add(CheckLocationForPhaseErrors(Location, options, locationEvents, errors));
                    tasks.Add(CheckDetectors(Location, options, locationEvents, errors));

                    await Task.WhenAll(tasks);
                }

                return errors.ToList();
            }
        }

        private async Task CheckDetectors(Location location, WatchdogPmLoggingOptions options, List<IndianaEvent> locationEvents, ConcurrentBag<WatchDogLogEvent> errors)
        {
            var detectorEventCodes = new List<short> { 82, 81 };
            CheckForUnconfiguredDetectors(location, options, locationEvents, errors, detectorEventCodes);
            CheckForLowDetectorHits(location, options, locationEvents, errors, detectorEventCodes);
            CheckRampMeteringDetectors(location, options, locationEvents, errors);
        }

        private void CheckRampMeteringDetectors(Location location, WatchdogPmLoggingOptions options, List<IndianaEvent> locationEvents, ConcurrentBag<WatchDogLogEvent> errors)
        {
            int rampMeterMeasureId = 37;
            if (location.LocationTypeId == ((short)LocationTypes.RM))
            {
                CheckMainlineDetections(location, options, locationEvents, errors);
                CheckStuckQueueDetections(location, options, locationEvents, errors);
            }
        }

        private void CheckDetections(
            Location location,
            WatchdogPmLoggingOptions options,
            List<IndianaEvent> locationEvents,
            ConcurrentBag<WatchDogLogEvent> errors,
            IEnumerable<short> eventCodes,
            WatchDogIssueTypes issueType,
            WatchDogComponentTypes componentType,
            string issueDescription,
            int startHour,
            int endHour,
            bool checkMissing)
        {
            try
            {
                var (start, end) = CalculateStartAndEndTime(options, startHour, endHour);

                var currentVolume = locationEvents.Where(e => e.Timestamp >= start && e.Timestamp <= end && eventCodes.Contains(e.EventCode));
                var rampDetectors = location.GetDetectorsForLocation()
                    .Where(d => d.DetectionTypes.Any(dt => dt.Id == DetectionTypes.IQ || dt.Id == DetectionTypes.EQ));

                bool hasIQ = rampDetectors.Any(d => d.DetectionTypes.Any(dt => dt.Id == DetectionTypes.IQ));
                bool hasEQ = rampDetectors.Any(d => d.DetectionTypes.Any(dt => dt.Id == DetectionTypes.EQ));

                var additionalInfo = GetAdditionalInfo(hasIQ, hasEQ);

                if ((checkMissing && !currentVolume.Any()) || (!checkMissing && currentVolume.Any()))
                {
                    var error = new WatchDogLogEvent(
                        location.Id,
                        location.LocationIdentifier,
                        options.PmScanDate,
                        componentType,
                        location.Id,
                        issueType,
                        $"{issueDescription} {currentVolume.Count().ToString().ToLowerInvariant()}{additionalInfo}",
                        null);

                    if (!errors.Contains(error))
                        errors.Add(error);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{issueType} {location.Id} {ex.Message}");
            }
        }

        private (DateTime start, DateTime end) CalculateStartAndEndTime(WatchdogPmLoggingOptions options, int startHour, int endHour)
        {
            var scanDate = options.PmScanDate;
            var start = scanDate.Date.AddHours(startHour);
            var end = scanDate.Date.AddHours(endHour);

            return (start, end);
        }

        private string GetAdditionalInfo(bool hasIQ, bool hasEQ)
        {
            var additionalInfo = "";
            if (hasIQ)
                additionalInfo += " Intermediate Queue";
            if (hasEQ)
                additionalInfo += (string.IsNullOrEmpty(additionalInfo) ? "" : ",") + " Excessive Queue";

            return additionalInfo;
        }

        private async Task CheckLocationForPhaseErrors(
            Location Location,
            WatchdogPmLoggingOptions options,
            List<IndianaEvent> LocationEvents,
            ConcurrentBag<WatchDogLogEvent> errors)
        {
            var pmCycleEvents = LocationEvents.Where(e =>
                new List<short>
                {
                    1,
                    8,
                    11
                }.Contains(e.EventCode)
                && e.Timestamp >= options.PmAnalysisStart
                && e.Timestamp <= options.PmAnalysisEnd).ToList();

            CheckForUnconfiguredApproaches(Location, options, errors, pmCycleEvents);
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ///////////////////// WATCH DOG ERRORS /////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        //WatchDogIssueType RecordCount - 1 - ALL Times
        public async Task<WatchDogLogEvent> CheckLocationRecordCount(DateTime dateToCheck, Location Location, WatchdogPmLoggingOptions options, List<IndianaEvent> LocationEvents)
        {
            if (LocationEvents.Count > options.MinimumRecords)
            {
                logger.LogDebug($"Location {Location.LocationIdentifier} has {LocationEvents.Count} records");
                return null;
            }
            else
            {
                logger.LogDebug($"Location {Location.LocationIdentifier} Does Not Have Sufficient records");
                return new WatchDogLogEvent(
                    Location.Id,
                    Location.LocationIdentifier,
                    options.PmScanDate,
                    WatchDogComponentTypes.Location,
                    Location.Id,
                    WatchDogIssueTypes.RecordCount,
                    "Missing Records - IP: " + string.Join(",", Location.Devices.Select(d => d.Ipaddress.ToString()).ToList()),
                    null
                );
            }
        }

        //WatchDogIssueType LowDetectorHits - 2 - PM
        public void CheckForLowDetectorHits(Location location, WatchdogPmLoggingOptions options, List<IndianaEvent> locationEvents, ConcurrentBag<WatchDogLogEvent> errors, List<short> detectorEventCodes)
        {
            var detectors = location.GetDetectorsForLocationThatSupportMetric(6);
            //Parallel.ForEach(detectors, options, detector =>
            foreach (var detector in detectors)
                try
                {
                    if (detector.DetectionTypes != null && detector.DetectionTypes.Any(d => d.Id == DetectionTypes.AC))
                    {
                        var channel = detector.DetectorChannel;
                        var direction = detector.Approach.DirectionType.Description;
                        var start = new DateTime();
                        var end = new DateTime();

                        var currentVolume = locationEvents.Where(e => e.EventParam == detector.DetectorChannel &&
                            detectorEventCodes.Contains(e.EventCode) && e.Timestamp >= options.PmAnalysisStart
                            && e.Timestamp <= options.PmAnalysisEnd).Count();
                        //Compare collected hits to low hit threshold, 
                        if (currentVolume < Convert.ToInt32(options.LowHitThreshold))
                        {
                            var message = $"CH: {channel} - Count: {currentVolume.ToString().ToLowerInvariant()}";

                            AddDetectorError(
                                location,
                                options.PmScanDate,
                                detector,
                                WatchDogIssueTypes.LowDetectorHits,
                                message,
                                errors);
                        }

                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"CheckForLowDetectorHits {detector.Id} {ex.Message}");
                }
        }

        //WatchDogIssueType UnconfiguredApproach - 6 - PM
        public void CheckForUnconfiguredApproaches(Location Location, WatchdogPmLoggingOptions options, ConcurrentBag<WatchDogLogEvent> errors, List<IndianaEvent> cycleEvents)
        {
            var phasesInUse = cycleEvents.Where(d => d.EventCode == 1 && d.Timestamp >= options.PmAnalysisStart
                && d.Timestamp <= options.PmAnalysisEnd).Select(d => d.EventParam).Distinct();
            foreach (var phaseNumber in phasesInUse)
            {
                var phase = phaseService.GetPhases(Location).Find(p => p.PhaseNumber == phaseNumber);
                if (phase == null)
                {
                    logger.LogDebug($"Location {Location.LocationIdentifier} {phaseNumber} Not Configured");

                    AddApproachError(
                        Location,
                        options.PmScanDate,
                        -1,
                        WatchDogIssueTypes.UnconfiguredApproach,
                        "No corresponding approach configured",
                        errors,
                        phaseNumber);
                }
            }
        }

        //WatchDogIssueType UnconfiguredDetector - 7 - PM
        public static void CheckForUnconfiguredDetectors(Location Location, WatchdogPmLoggingOptions options, List<IndianaEvent> LocationEvents, ConcurrentBag<WatchDogLogEvent> errors, List<short> detectorEventCodes)
        {
            var detectorChannelsFromEvents = LocationEvents.Where(e => detectorEventCodes.Contains(e.EventCode)
            && e.Timestamp >= options.PmAnalysisStart && e.Timestamp <= options.PmAnalysisEnd)
                .Select(e => e.EventParam).Distinct().ToList();
            var detectorChannelsFromDetectors = Location.GetDetectorsForLocation().Select(d => d.DetectorChannel).Distinct().ToList();
            foreach (var channel in detectorChannelsFromEvents)
            {
                if (!detectorChannelsFromDetectors.Contains(channel))
                {
                    AddDetectorError(
                        Location,
                        options.PmScanDate,
                        detector: null,
                        WatchDogIssueTypes.UnconfiguredDetector,
                        $"Unconfigured detector channel-{channel}",
                        errors);
                }
            }
        }

        //WatchDogIssueType MissingMainlineData - 8 - PM
        public void CheckMainlineDetections(Location location, WatchdogPmLoggingOptions options, List<IndianaEvent> locationEvents, ConcurrentBag<WatchDogLogEvent> errors)
        {
            var mainlineEventCodes = new List<short> { 1371, 1372, 1373 };
            CheckDetections(
                location,
                options,
                locationEvents,
                errors,
                mainlineEventCodes,
                WatchDogIssueTypes.MissingMainlineData,
                WatchDogComponentTypes.Location,
                "Missing Mainline Data",
                options.RampMainlineStartHour,
                options.RampMainlineEndHour,
                checkMissing: true);
        }

        //WatchDogIssueType StuckQueueDetection - 9 - PM
        public void CheckStuckQueueDetections(Location location, WatchdogPmLoggingOptions options, List<IndianaEvent> locationEvents, ConcurrentBag<WatchDogLogEvent> errors)
        {
            var eventCodes = Enumerable.Range(1171, 31).Where(i => i % 2 != 0).Select(i => (short)i).ToList();
            CheckDetections(
                location,
                options,
                locationEvents,
                errors,
                eventCodes,
                WatchDogIssueTypes.StuckQueueDetection,
                WatchDogComponentTypes.Detector,
                "Stuck Queue Detections",
                options.RampStuckQueueStartHour,
                options.RampStuckQueueEndHour,
                checkMissing: false);
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

        private static void AddApproachError(Location location, DateTime timestamp, int approachId, WatchDogIssueTypes issueType, string message, ConcurrentBag<WatchDogLogEvent> errors, int? phaseNumber = null)
        {
            var error = new WatchDogLogEvent(
                location.Id,
                location.LocationIdentifier,
                timestamp,
                WatchDogComponentTypes.Approach,
                approachId,
                issueType,
                message,
                phaseNumber);

            if (!errors.Contains(error))
                errors.Add(error);
        }
    }
}