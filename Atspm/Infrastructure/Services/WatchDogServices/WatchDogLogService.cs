﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for WatchDog - WatchDog.Services/WatchDogLogService.cs
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
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.TempExtensions;

namespace Utah.Udot.ATSPM.Infrastructure.Services.WatchDogServices
{
    public partial class WatchDogLogService
    {
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly AnalysisPhaseCollectionService analysisPhaseCollectionService;
        private readonly PhaseService phaseService;
        private readonly ILogger<WatchDogLogService> logger;

        public WatchDogLogService(IIndianaEventLogRepository controllerEventLogRepository,
            AnalysisPhaseCollectionService analysisPhaseCollectionService,
            PhaseService phaseService,
            ILogger<WatchDogLogService> logger)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.analysisPhaseCollectionService = analysisPhaseCollectionService;
            this.phaseService = phaseService;
            this.logger = logger;
        }

        public async Task<List<WatchDogLogEvent>> GetWatchDogIssues(
            LoggingOptions options,
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

                foreach (var Location in locations)//.Where(s => s.LocationIdentifier == "7115"))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return errors.ToList();
                    }
                    var locationEvents = controllerEventLogRepository.GetEventsBetweenDates(
                        Location.LocationIdentifier,
                        options.AnalysisStart,
                        options.AnalysisEnd).ToList();
                    var recordsError = await CheckLocationRecordCount(options.ScanDate, Location, options, locationEvents);
                    if (recordsError != null)
                    {
                        errors.Add(recordsError);
                        continue;
                    }
                    var tasks = new List<Task>();
                    tasks.Add(CheckLocationForPhaseErrors(Location, options, locationEvents, errors));
                    tasks.Add(CheckDetectors(Location, options, locationEvents, errors));
                    //CheckApplicationEvents(locations, options);
                    await Task.WhenAll(tasks);
                }
                return errors.ToList();
            }
        }

        private async Task CheckDetectors(Location location, LoggingOptions options, List<IndianaEvent> locationEvents, ConcurrentBag<WatchDogLogEvent> errors)
        {
            var detectorEventCodes = new List<short> { 82, 81 };
            CheckForUnconfiguredDetectors(location, options, locationEvents, errors, detectorEventCodes);
            CheckForLowDetectorHits(location, options, locationEvents, errors, detectorEventCodes);
        }

        private void CheckForLowDetectorHits(Location location, LoggingOptions options, List<IndianaEvent> locationEvents, ConcurrentBag<WatchDogLogEvent> errors, List<short> detectorEventCodes)
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
                        if (options.WeekdayOnly && options.ScanDate.DayOfWeek == DayOfWeek.Monday)
                        {
                            start = options.ScanDate.AddDays(-3).Date.AddHours(options.PreviousDayPMPeakStart);
                            end = options.ScanDate.AddDays(-3).Date.AddHours(options.PreviousDayPMPeakEnd);
                        }
                        else
                        {
                            start = options.ScanDate.AddDays(-1).Date.AddHours(options.PreviousDayPMPeakStart);
                            end = options.ScanDate.AddDays(-1).Date.AddHours(options.PreviousDayPMPeakEnd);
                        }
                        var currentVolume = locationEvents.Where(e => e.EventParam == detector.DetectorChannel && detectorEventCodes.Contains(e.EventCode)).Count();
                        //Compare collected hits to low hit threshold, 
                        if (currentVolume < Convert.ToInt32(options.LowHitThreshold))
                        {
                            var error = new WatchDogLogEvent(
                                location.Id,
                                location.LocationIdentifier,
                                options.ScanDate,
                                WatchDogComponentTypes.Detector,
                                detector.Id,
                                WatchDogIssueTypes.LowDetectorHits,
                                $"CH: {channel} - Count: {currentVolume.ToString().ToLowerInvariant()}",
                                null);
                            if (!errors.Contains(error))
                                errors.Add(error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"CheckForLowDetectorHits {detector.Id} {ex.Message}");
                }
        }

        private static void CheckForUnconfiguredDetectors(Location Location, LoggingOptions options, List<IndianaEvent> LocationEvents, ConcurrentBag<WatchDogLogEvent> errors, List<short> detectorEventCodes)
        {
            var detectorChannelsFromEvents = LocationEvents.Where(e => detectorEventCodes.Contains(e.EventCode)).Select(e => e.EventParam).Distinct().ToList();
            var detectorChannelsFromDetectors = Location.GetDetectorsForLocation().Select(d => d.DetectorChannel).Distinct().ToList();
            foreach (var channel in detectorChannelsFromEvents)
            {
                if (!detectorChannelsFromDetectors.Contains(channel))
                {
                    var error = new WatchDogLogEvent(
                                Location.Id,
                                Location.LocationIdentifier,
                                options.ScanDate,
                                WatchDogComponentTypes.Detector,
                                -1,
                                WatchDogIssueTypes.UnconfiguredDetector,
                                $"Unconfigured detector channel-{channel}",
                                null);
                    if (!errors.Contains(error))
                        errors.Add(error);
                }
            }
        }

        private async Task CheckLocationForPhaseErrors(
            Location Location,
            LoggingOptions options,
            List<IndianaEvent> LocationEvents,
            ConcurrentBag<WatchDogLogEvent> errors)
        {
            var planEvents = LocationEvents.GetPlanEvents(
            options.AnalysisStart,
            options.AnalysisEnd).ToList();
            //Do we want to use the ped events extension here?
            var pedEvents = LocationEvents.Where(e =>
                new List<short>
                {
                    21,
                    23
                }.Contains(e.EventCode)
                && e.Timestamp >= options.AnalysisStart
                && e.Timestamp <= options.AnalysisEnd).ToList();
            var cycleEvents = LocationEvents.Where(e =>
                new List<short>
                {
                    1,
                    8,
                    11
                }.Contains(e.EventCode)
                && e.Timestamp >= options.AnalysisStart
                && e.Timestamp <= options.AnalysisEnd).ToList();
            var splitsEventCodes = new List<short>();
            for (short i = 130; i <= 149; i++)
                splitsEventCodes.Add(i);
            var splitsEvents = LocationEvents.Where(e =>
                splitsEventCodes.Contains(e.EventCode)
                && e.Timestamp >= options.AnalysisStart
                && e.Timestamp <= options.AnalysisEnd).ToList();
            var terminationEvents = LocationEvents.Where(e =>
            new List<short>
            {
                4,
                5,
                6,
                7
            }.Contains(e.EventCode)
            && e.Timestamp >= options.AnalysisStart
            && e.Timestamp <= options.AnalysisEnd).ToList();
            LocationEvents = null;

            CheckForUnconfiguredApproaches(Location, options, errors, cycleEvents);

            AnalysisPhaseCollectionData analysisPhaseCollection = null;
            try
            {
                analysisPhaseCollection = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(
                    Location.LocationIdentifier,
                    options.AnalysisStart,
                    options.AnalysisEnd,
                    planEvents,
                    cycleEvents,
                    splitsEvents,
                    pedEvents,
                    terminationEvents,
                    Location,
                    options.ConsecutiveCount);

            }
            catch (Exception e)
            {
                logger.LogError($"Unable to get analysis phase for Location {Location.LocationIdentifier}");
            }

            if (analysisPhaseCollection != null)
            {
                foreach (var phase in analysisPhaseCollection.AnalysisPhases)
                //Parallel.ForEach(APcollection.Cycles, options,phase =>
                {
                    var taskList = new List<Task>();
                    var approach = Location.Approaches.Where(a => a.ProtectedPhaseNumber == phase.PhaseNumber).FirstOrDefault();
                    if (approach != null)
                    {
                        try
                        {
                            taskList.Add(CheckForMaxOut(phase, approach, options, errors));
                        }
                        catch (Exception e)
                        {
                            logger.LogError($"{phase.locationIdentifier} {phase.PhaseNumber} - Max Out Error {e.Message}");
                        }

                        try
                        {

                            taskList.Add(CheckForForceOff(phase, approach, options, errors));
                        }
                        catch (Exception e)
                        {
                            logger.LogError($"{phase.locationIdentifier} {phase.PhaseNumber} - Force Off Error {e.Message}");
                        }

                        try
                        {
                            taskList.Add(CheckForStuckPed(phase, approach, options, errors));
                        }
                        catch (Exception e)
                        {
                            logger.LogError($"{phase.locationIdentifier} {phase.PhaseNumber} - Stuck Ped Error {e.Message}");
                        }
                    }
                    await Task.WhenAll(taskList);
                }
            }
        }

        private void CheckForUnconfiguredApproaches(Location Location, LoggingOptions options, ConcurrentBag<WatchDogLogEvent> errors, List<IndianaEvent> cycleEvents)
        {
            var phasesInUse = cycleEvents.Where(d => d.EventCode == 1).Select(d => d.EventParam).Distinct();
            foreach (var phaseNumber in phasesInUse)
            {
                var phase = phaseService.GetPhases(Location).Find(p => p.PhaseNumber == phaseNumber);
                if (phase == null)
                {
                    var error = new WatchDogLogEvent
                    (
                        Location.Id,
                        Location.LocationIdentifier,
                        options.ScanDate,
                        WatchDogComponentTypes.Approach,
                        -1,
                        WatchDogIssueTypes.UnconfiguredApproach,
                        $"No corresponding approach configured",
                        phaseNumber
                    );
                    if (!errors.Contains(error))
                    {
                        logger.LogDebug($"Location {Location.LocationIdentifier} {phaseNumber} Not Configured");
                        errors.Add(error);
                    }
                }
            }
        }

        private async Task CheckForStuckPed(AnalysisPhaseData phase, Approach approach, LoggingOptions options, ConcurrentBag<WatchDogLogEvent> errors)
        {
            if (phase.PedestrianEvents.Count > options.MaximumPedestrianEvents)
            {
                var error = new WatchDogLogEvent
                (
                    approach.Location.Id,
                    approach.Location.LocationIdentifier,
                    options.ScanDate,
                    WatchDogComponentTypes.Approach,
                    approach.Id,
                    WatchDogIssueTypes.StuckPed,
                    phase.PedestrianEvents.Count + " Pedestrian Activations",
                    phase.PhaseNumber
                );
                if (!errors.Contains(error))
                {
                    logger.LogDebug($"Location {approach.Location.LocationIdentifier} {phase.PedestrianEvents.Count} Pedestrian Activations");
                    errors.Add(error);
                }
            }
        }

        private async Task CheckForForceOff(AnalysisPhaseData phase, Approach approach, LoggingOptions options, ConcurrentBag<WatchDogLogEvent> errors)
        {
            if (phase.PercentForceOffs > options.PercentThreshold &&
                phase.TerminationEvents.Where(t => t.EventCode != 7).Count() > options.MinPhaseTerminations)
            {
                var error = new WatchDogLogEvent
                (
                    approach.Location.Id,
                    approach.Location.LocationIdentifier,
                    options.ScanDate,
                    WatchDogComponentTypes.Approach,
                    approach.Id,
                    WatchDogIssueTypes.ForceOffThreshold,
                    "Force Offs " + Math.Round(phase.PercentForceOffs * 100, 1) + "%",
                    phase.PhaseNumber
                );
                if (!errors.Contains(error))
                {
                    logger.LogDebug($"Location {approach.Location.LocationIdentifier} Has ForceOff Errors");
                    errors.Add(error);
                }
            }
        }

        private async Task CheckForMaxOut(AnalysisPhaseData phase, Approach approach, LoggingOptions options, ConcurrentBag<WatchDogLogEvent> errors)
        {
            if (phase.PercentMaxOuts > options.PercentThreshold &&
                phase.TotalPhaseTerminations > options.MinPhaseTerminations)
            {
                var error = new WatchDogLogEvent
                (
                    approach.Location.Id,
                    approach.Location.LocationIdentifier,
                    options.ScanDate,
                    WatchDogComponentTypes.Approach,
                    approach.Id,
                    WatchDogIssueTypes.MaxOutThreshold,
                    "Max Outs " + Math.Round(phase.PercentMaxOuts * 100, 1) + "%",
                    phase.PhaseNumber
                );
                if (errors.Count == 0 || !errors.Contains(error))
                {
                    logger.LogDebug($"Location {approach.Location.LocationIdentifier} Has MaxOut Errors");
                    errors.Add(error);
                }
            }
        }

        private async Task<WatchDogLogEvent> CheckLocationRecordCount(DateTime dateToCheck, Location Location, LoggingOptions options, List<IndianaEvent> LocationEvents)
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
                    options.ScanDate,
                    WatchDogComponentTypes.Location,
                    Location.Id,
                    WatchDogIssueTypes.RecordCount,
                    "Missing Records - IP: " + string.Join(",", Location.Devices.Select(d => d.Ipaddress.ToString()).ToList()),
                    null
                );
            }

        }
    }
}