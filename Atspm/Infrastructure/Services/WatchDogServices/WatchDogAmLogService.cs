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
    public partial class WatchDogAmLogService
    {
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly AnalysisPhaseCollectionService analysisPhaseCollectionService;
        private readonly ILogger<WatchDogRampLogService> logger;

        public WatchDogAmLogService(IIndianaEventLogRepository controllerEventLogRepository,
            AnalysisPhaseCollectionService analysisPhaseCollectionService,
            ILogger<WatchDogRampLogService> logger)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.analysisPhaseCollectionService = analysisPhaseCollectionService;
            this.logger = logger;
        }

        public async Task<List<WatchDogLogEvent>> GetWatchDogIssues(
            WatchdogAmLoggingOptions options,
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

                    var AmAnalysis = controllerEventLogRepository
                        .GetEventsBetweenDates(
                            Location.LocationIdentifier,
                            options.AmAnalysisStart,
                            options.AmAnalysisEnd)
                        .ToList();

                    locationEvents.AddRange(AmAnalysis);

                    var tasks = new List<Task>();
                    tasks.Add(CheckLocationForPhaseErrors(Location, options, locationEvents, errors));

                    await Task.WhenAll(tasks);
                }

                return errors.ToList();
            }
        }


        private async Task CheckLocationForPhaseErrors(
            Location Location,
            WatchdogAmLoggingOptions options,
            List<IndianaEvent> LocationEvents,
            ConcurrentBag<WatchDogLogEvent> errors)
        {
            var amPlanEvents = LocationEvents.GetPlanEvents(
            options.AmAnalysisStart,
            options.AmAnalysisEnd).ToList();
            //Do we want to use the ped events extension here?
            var amPedEvents = LocationEvents.Where(e =>
                new List<short>
                {
                    21,
                    23
                }.Contains(e.EventCode)
                && e.Timestamp >= options.AmAnalysisStart
                && e.Timestamp <= options.AmAnalysisEnd).ToList();

            var amCycleEvents = LocationEvents.Where(e =>
                new List<short>
                {
                    1,
                    8,
                    11
                }.Contains(e.EventCode)
                && e.Timestamp >= options.AmAnalysisStart
                && e.Timestamp <= options.AmAnalysisEnd).ToList();

            var splitsEventCodes = new List<short>();
            for (short i = 130; i <= 149; i++)
                splitsEventCodes.Add(i);
            var amSplitsEvents = LocationEvents.Where(e =>
                splitsEventCodes.Contains(e.EventCode)
                && e.Timestamp >= options.AmAnalysisStart
                && e.Timestamp <= options.AmAnalysisEnd).ToList();

            var amTerminationEvents = LocationEvents.Where(e =>
            new List<short>
            {
                4,
                5,
                6,
                7
            }.Contains(e.EventCode)
            && e.Timestamp >= options.AmAnalysisStart
            && e.Timestamp <= options.AmAnalysisEnd).ToList();

            LocationEvents = null;


            AnalysisPhaseCollectionData analysisPhaseCollection = null;
            try
            {
                //Since this is used for the am calculations we need to use that :)
                analysisPhaseCollection = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(
                    Location.LocationIdentifier,
                    options.AmAnalysisStart,
                    options.AmAnalysisEnd,
                    amPlanEvents,
                    amCycleEvents,
                    amSplitsEvents,
                    amPedEvents,
                    amTerminationEvents,
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

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ///////////////////// WATCH DOG ERRORS /////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        //WatchDogIssueType StuckPed - 3 - AM
        public async Task CheckForStuckPed(AnalysisPhaseData phase, Approach approach, WatchdogAmLoggingOptions options, ConcurrentBag<WatchDogLogEvent> errors)
        {
            if (phase.PedestrianEvents.Count > options.MaximumPedestrianEvents)
            {
                var message = $"{phase.PedestrianEvents.Count} Pedestrian Activations";

                logger.LogDebug($"Location {approach.Location.LocationIdentifier} {phase.PedestrianEvents.Count} Pedestrian Activations");

                AddApproachError(
                    approach.Location,
                    options.AmScanDate,
                    approach.Id,
                    WatchDogIssueTypes.StuckPed,
                    message,
                    errors,
                    phase.PhaseNumber);
            }
        }

        //WatchDogIssueType ForceOffThreshold - 4 - AM
        public async Task CheckForForceOff(AnalysisPhaseData phase, Approach approach, WatchdogAmLoggingOptions options, ConcurrentBag<WatchDogLogEvent> errors)
        {
            if (phase.PercentForceOffs > options.PercentThreshold && phase.TerminationEvents.Count(t => t.EventCode != 7) > options.MinPhaseTerminations)
            {
                var message = $"Force Offs {Math.Round(phase.PercentForceOffs * 100, 1)}%";

                logger.LogDebug($"Location {approach.Location.LocationIdentifier} Has ForceOff Errors");

                AddApproachError(
                    approach.Location,
                    options.AmScanDate,
                    approach.Id,
                    WatchDogIssueTypes.ForceOffThreshold,
                    message,
                    errors,
                    phase.PhaseNumber);
            }

        }

        //WatchDogIssueType MaxOutThreshold - 5 - AM
        public async Task CheckForMaxOut(AnalysisPhaseData phase, Approach approach, WatchdogAmLoggingOptions options, ConcurrentBag<WatchDogLogEvent> errors)
        {
            if (phase.PercentMaxOuts > options.PercentThreshold && phase.TotalPhaseTerminations > options.MinPhaseTerminations)
            {
                var message = $"Max Outs {Math.Round(phase.PercentMaxOuts * 100, 1)}%";

                logger.LogDebug($"Location {approach.Location.LocationIdentifier} Has MaxOut Errors");

                AddApproachError(
                    approach.Location,
                    options.AmScanDate,
                    approach.Id,
                    WatchDogIssueTypes.MaxOutThreshold,
                    message,
                    errors,
                    phase.PhaseNumber);
            }

        }

        ///////////////////////////////////////////////////////////
        /////////////////////// HELPER ////////////////////////////
        ///////////////////////////////////////////////////////////

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