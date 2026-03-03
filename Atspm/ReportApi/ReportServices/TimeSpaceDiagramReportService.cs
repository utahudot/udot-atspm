#region license
// Copyright 2025 Utah Departement of Transportation
// for ReportApi - Utah.Udot.Atspm.ReportApi.ReportServices/TimeSpaceDiagramReportService.cs
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

using Microsoft.EntityFrameworkCore;
using Utah.Udot.Atspm.Business.PriorityDetails;
using Utah.Udot.Atspm.Business.TimeSpaceDiagram;
using Utah.Udot.Atspm.Business.TurningMovementCounts;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.ReportApi.ReportServices
{
    /// <summary>
    /// Time space diagram report service
    /// </summary>
    public class TimeSpaceDiagramReportService : ReportServiceBase<TimeSpaceDiagramOptions, IEnumerable<TimeSpaceDiagramResultForPhase>>
    {
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository LocationRepository;
        private readonly TimeSpaceDiagramForPhaseService timeSpaceDiagramReportService;
        private readonly PhaseService phaseService;
        private readonly LocationPhaseService LocationPhaseService;
        private readonly IRouteLocationsRepository routeLocationsRepository;
        private readonly IRouteRepository routeRepository;
        private readonly PriorityDetailsReportService priorityDetailsReportService;
        private readonly TimeSpaceDiagramSrmCsvService timeSpaceDiagramSrmCsvService;

        public TimeSpaceDiagramReportService(IIndianaEventLogRepository controllerEventLogRepository,
            ILocationRepository locationRepository,
            TimeSpaceDiagramForPhaseService timeSpaceDiagramReportService,
            PhaseService phaseService,
            IRouteLocationsRepository routeLocationsRepository,
            IRouteRepository routeRepository,
            LocationPhaseService locationPhaseService,
            PriorityDetailsReportService priorityDetailsReportService,
            TimeSpaceDiagramSrmCsvService timeSpaceDiagramSrmCsvService)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            LocationRepository = locationRepository;
            this.timeSpaceDiagramReportService = timeSpaceDiagramReportService;
            this.phaseService = phaseService;
            this.routeLocationsRepository = routeLocationsRepository;
            this.routeRepository = routeRepository;
            this.LocationPhaseService = locationPhaseService;
            this.priorityDetailsReportService = priorityDetailsReportService;
            this.timeSpaceDiagramSrmCsvService = timeSpaceDiagramSrmCsvService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<TimeSpaceDiagramResultForPhase>> ExecuteAsync(TimeSpaceDiagramOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var routeLocations = GetLocationsFromRouteId(parameter.RouteId);
            var routeName = GetRouteNameFromId(parameter.RouteId);
            if (routeLocations.Count == 0)
            {
                throw new Exception($"No locations present for route");
            }
            var srmTracks = parameter.IncludeSrmSearch
                ? timeSpaceDiagramSrmCsvService.GetTracks(
                    parameter.Start,
                    parameter.End,
                    routeLocations,
                    parameter.SrmCsvContentBase64)
                : new List<SrmEntityTrack>();

            var eventCodes = new List<short>() { 82, 81 };
            var tasks = new List<Task<TimeSpaceDiagramResultForPhase>>();
            routeLocations.Sort((r1, r2) => r1.Order - r2.Order);

            //Throw exception when no distance is found
            foreach (var routeLocation in routeLocations)
            {
                if (routeLocation.NextLocationDistance == null && routeLocation.PreviousLocationDistance == null)
                {
                    throw new Exception($"Distance not configured for route: {routeName}");
                }
            }

            var (controllerEventLogsList, primaryPhaseDetails, opposingPhaseDetails, programmedCycleLength, primaryTmcEvents, opposingTmcEvents, programmedSplits) = ProcessRouteLocations(routeLocations, parameter);

            for (int i = 0; i < routeLocations.Count; i++)
            {
                var nextLocationDistance = i == routeLocations.Count - 1 ? 0 : routeLocations[i].NextLocationDistance.Distance;
                var previousLocationDistance = i == 0 ? 0 : routeLocations[i].PreviousLocationDistance.Distance;

                tasks.Add(GetChartDataForPhase(parameter,
                    controllerEventLogsList[i],
                    primaryPhaseDetails[i],
                    programmedCycleLength[i],
                    primaryTmcEvents[i],
                    routeLocations[i],
                    srmTracks,
                    programmedSplits,
                    eventCodes,
                    nextLocationDistance,
                    previousLocationDistance,
                    isFirstElement: i == 0,
                    isLastElement: i == routeLocations.Count - 1,
                    "Primary",
                    i
                ));
            }

            for (int i = routeLocations.Count - 1, j = 0; i >= 0; i--, j++)
            {
                var nextLocationDistance = i == 0 ? 0 : routeLocations[i].PreviousLocationDistance.Distance;
                var previousLocationDistance = i == routeLocations.Count - 1 ? 0 : routeLocations[i].NextLocationDistance.Distance;

                tasks.Add(GetChartDataForPhase(parameter,
                    controllerEventLogsList[i],
                    opposingPhaseDetails[i],
                    programmedCycleLength[i],
                    opposingTmcEvents[i],
                    routeLocations[i],
                    srmTracks,
                    programmedSplits,
                    eventCodes,
                    nextLocationDistance,
                    previousLocationDistance,
                    isFirstElement: i == routeLocations.Count - 1,
                    isLastElement: i == 0,
                    "Opposing",
                    j
                ));
            }

            var results = await Task.WhenAll(tasks);
            return results;
        }

        private string GetRouteNameFromId(int routeId)
        {
            var routeName = routeRepository.GetList().Where(r => r.Id == routeId)?.FirstOrDefault()?.Name;
            return routeName != null ? routeName : "";
        }

        private (
            List<List<IndianaEvent>> controllerEventLogsList,
            List<PhaseDetail> primaryPhaseDetails,
            List<PhaseDetail> opposingPhaseDetails,
            List<int> programmedCycleLength,
            List<TmcForPhaseDto> primaryTmcEvents,
            List<TmcForPhaseDto> opposingTmcEvents,
            List<IndianaEvent> programmedSplits)
        ProcessRouteLocations(IEnumerable<RouteLocation> routeLocations, TimeSpaceDiagramOptions parameter)
        {
            var controllerEventLogsList = new List<List<IndianaEvent>>();
            var primaryPhaseDetails = new List<PhaseDetail>();
            var opposingPhaseDetails = new List<PhaseDetail>();
            var programmedCycleLength = new List<int>();
            var primaryTmcEvents = new List<TmcForPhaseDto>();
            var opposingTmcEvents = new List<TmcForPhaseDto>();
            var programmedSplitsForTimePeriod = new List<IndianaEvent>();

            foreach (var routeLocation in routeLocations)
            {
                var location = LocationRepository.GetLatestVersionOfLocation(
                    routeLocation.LocationIdentifier,
                    parameter.Start);

                if (location == null)
                {
                    throw new Exception("Issue fetching location from route");
                }

                var phases = phaseService.GetPhases(location);

                var primaryPhaseDetail = phases.Find(p =>
                    p.Approach.ProtectedPhaseNumber == routeLocation.PrimaryPhase &&
                    p.Approach.DirectionType == routeLocation.PrimaryDirection);

                var opposingPhaseDetail = phases.Find(p =>
                    p.Approach.ProtectedPhaseNumber == routeLocation.OpposingPhase &&
                    p.Approach.DirectionType == routeLocation.OpposingDirection);

                if (primaryPhaseDetail == null || opposingPhaseDetail == null)
                {
                    throw new Exception("Error grabbing phase details");
                }

                if (parameter.SpeedLimit == null &&
                    (primaryPhaseDetail.Approach.Mph == null ||
                     opposingPhaseDetail.Approach.Mph == null))
                {
                    throw new Exception("Speed not configured in route for all phases");
                }

                var controllerEventLogs =
                    controllerEventLogRepository
                        .GetEventsBetweenDates(
                            location.LocationIdentifier,
                            parameter.Start.AddHours(-12),
                            parameter.End.AddHours(12))
                        .ToList();

                int currentProgrammedCycleLength = 0;

                if (controllerEventLogs.Any())
                {
                    var programmedCycleEvents = controllerEventLogs.GetEventsByEventCodes(
                        parameter.Start.AddHours(-12),
                        parameter.End.AddHours(12),
                        new List<short> { 132 });

                    currentProgrammedCycleLength =
                        GetEventOverlappingTime(parameter.Start, programmedCycleEvents, "CycleLength")
                            .FirstOrDefault()?.EventParam ?? 0;

                    if (!programmedSplitsForTimePeriod.Any())
                    {
                        var splitEvents = controllerEventLogs.GetEventsByEventCodes(
                            parameter.Start.AddHours(-12),
                            parameter.End.AddHours(12),
                            new List<short> { 134, 135, 136, 137, 138, 139, 140 });

                        programmedSplitsForTimePeriod.AddRange(
                            GetEventOverallapingTime(parameter.Start, splitEvents, "Program Splits"));
                    }
                }

                primaryTmcEvents.Add(
                    GetTMCDataForPhase(location, primaryPhaseDetail, controllerEventLogs, parameter));

                opposingTmcEvents.Add(
                    GetTMCDataForPhase(location, opposingPhaseDetail, controllerEventLogs, parameter));

                controllerEventLogsList.Add(controllerEventLogs);
                primaryPhaseDetails.Add(primaryPhaseDetail);
                opposingPhaseDetails.Add(opposingPhaseDetail);
                programmedCycleLength.Add(currentProgrammedCycleLength);
            }

            return (
                controllerEventLogsList,
                primaryPhaseDetails,
                opposingPhaseDetails,
                programmedCycleLength,
                primaryTmcEvents,
                opposingTmcEvents,
                programmedSplitsForTimePeriod);
        }

        private List<IndianaEvent> GetEventOverallapingTime(DateTime start, IReadOnlyList<IndianaEvent> programmedCycleForPlan, string eventType)
        {
            var planEvent = programmedCycleForPlan.Where(e => e.Timestamp == start).ToList();


            if (!planEvent.Any())
                planEvent = programmedCycleForPlan.Where(e => e.Timestamp < start)
                    ?.GroupBy(log => log.EventCode)
                    ?.Select(group => group.OrderByDescending(e => e.Timestamp).FirstOrDefault())
                    ?.ToList();

            if (!planEvent.Any())
                throw new NullReferenceException($"Error grabbing {eventType}");

            return planEvent.ToList();
        }

        private async Task<TimeSpaceDiagramResultForPhase> GetChartDataForPhase(
            TimeSpaceDiagramOptions parameter,
            List<IndianaEvent> currentControllerEventLogs,
            PhaseDetail currentPhase,
            int programmedCycleLength,
            TmcForPhaseDto tmcEventsForPhase,
            RouteLocation routeLocation,
            List<SrmEntityTrack> srmTracks,
            List<IndianaEvent> programmedSplits,
            List<short> eventCodes,
            double distanceToNextLocation,
            double distanceToPreviousLocation,
            bool isFirstElement,
            bool isLastElement,
            string phaseType,
            int order)
        {
            if (currentControllerEventLogs == null || !currentControllerEventLogs.Any())
            {
                return CreateEmptyPhaseResult(
                    parameter,
                    currentPhase,
                    tmcEventsForPhase,
                    routeLocation,
                    srmTracks,
                    distanceToNextLocation,
                    distanceToPreviousLocation,
                    phaseType,
                    order);
            }

            var planEvents = currentControllerEventLogs
                .GetPlanEvents(parameter.Start.AddHours(-12), parameter.End.AddHours(12))
                .ToList();

            var locationPhase = await LocationPhaseService.GetLocationPhaseData(
                currentPhase,
                parameter.Start,
                parameter.End,
                0,
                null,
                currentControllerEventLogs,
                planEvents,
                false);

            PriorityDetailsOptions priorityDetailsOptions = new PriorityDetailsOptions
            {
                Start = parameter.Start,
                End = parameter.End,
            };
            PriorityDetailsResult priorityDetails = await priorityDetailsReportService.GetChartDataForPhase(
                priorityDetailsOptions,
                currentControllerEventLogs,
                currentPhase,
                currentPhase.IsPermissivePhase);

            var viewModel = timeSpaceDiagramReportService.GetChartDataForPhase(parameter,
                currentPhase,
                currentControllerEventLogs,
                programmedCycleLength,
                programmedSplits,
                distanceToNextLocation,
                distanceToPreviousLocation,
                isFirstElement,
                isLastElement,
                priorityDetails);

            PopulateCommonPhaseFields(viewModel, currentPhase, phaseType, order, tmcEventsForPhase);

            if (locationPhase != null)
            {
                viewModel.PercentArrivalOnGreen = locationPhase.PercentArrivalOnGreen;
            }
            PopulateSrmTracks(
                viewModel,
                routeLocation,
                srmTracks,
                isFirstElement,
                isLastElement);

            return viewModel;
        }

        private void PopulateCommonPhaseFields(
            TimeSpaceDiagramResultForPhase viewModel,
            PhaseDetail phase,
            string phaseType,
            int order,
            TmcForPhaseDto tmcEventsForPhase)
        {
            viewModel.LocationDescription = phase.Approach.Location.LocationDescription();
            viewModel.ApproachDescription = phase.Approach.Description;
            viewModel.PhaseType = phaseType;
            viewModel.Order = order;
            viewModel.TmcForPhase = tmcEventsForPhase;
        }

        private TimeSpaceDiagramResultForPhase CreateEmptyPhaseResult(
            TimeSpaceDiagramOptions parameter,
            PhaseDetail phase,
            TmcForPhaseDto tmcEventsForPhase,
            RouteLocation routeLocation,
            List<SrmEntityTrack> srmTracks,
            double distanceToNextLocation,
            double distanceToPreviousLocation,
            string phaseType,
            int order)
        {
            var result = new TimeSpaceDiagramResultForPhase(
                phase.Approach.Id,
                phase.Approach.Location.LocationIdentifier,
                parameter.Start,
                parameter.End,
                phase.Approach.ProtectedPhaseNumber,
                phase.Approach.ProtectedPhaseNumber.ToString(),
                distanceToNextLocation,
                distanceToPreviousLocation,
                parameter.SpeedLimit ?? phase.Approach.Mph ?? 0,
                0,
                new(),
                new(),
                new(),
                new(),
                new(),
                new(),
                true,
                0,
                0,
                0,
                0,
                [],
                new())
            {
                LocationDescription = phase.Approach.Location.LocationDescription(),
                ApproachDescription = phase.Approach.Description,
                PhaseType = phaseType,
                Order = order,
                PercentArrivalOnGreen = 0,
                TmcForPhase = tmcEventsForPhase,
            };
            PopulateSrmTracks(
                result,
                routeLocation,
                srmTracks,
                isFirstElement: false,
                isLastElement: false);
            return result;
        }

        private static List<SrmEntityTrack> FilterSrmTracksForLocation(
            List<SrmEntityTrack> tracks,
            string locationIdentifier)
        {
            if (tracks == null || tracks.Count == 0 || string.IsNullOrWhiteSpace(locationIdentifier))
            {
                return new List<SrmEntityTrack>();
            }

            return tracks
                .Where(t => string.Equals(
                    t.StartingIntersection,
                    locationIdentifier,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private static void PopulateSrmTracks(
            TimeSpaceDiagramResultForPhase viewModel,
            RouteLocation routeLocation,
            List<SrmEntityTrack> srmTracks,
            bool isFirstElement,
            bool isLastElement)
        {
            if (viewModel == null || routeLocation == null)
            {
                return;
            }

            if (
                (string.Equals(viewModel.PhaseType, "Primary", StringComparison.OrdinalIgnoreCase) &&
                 isLastElement) ||
                (string.Equals(viewModel.PhaseType, "Opposing", StringComparison.OrdinalIgnoreCase) &&
                 isFirstElement)
            )
            {
                viewModel.SrmEntityTracks = new List<SrmEntityTrack>();
                return;
            }

            var allForLocation = FilterSrmTracksForLocation(
                srmTracks,
                routeLocation.LocationIdentifier);

            var targetDirection =
                string.Equals(viewModel.PhaseType, "Opposing", StringComparison.OrdinalIgnoreCase)
                    ? routeLocation.OpposingDirectionId
                    : routeLocation.PrimaryDirectionId;

            viewModel.SrmEntityTracks = allForLocation
                .Where(t => IsDirectionMatch(t.HeadingDirection, targetDirection))
                .ToList();
        }

        private static bool IsDirectionMatch(DirectionTypes heading, DirectionTypes target)
        {
            if (heading == DirectionTypes.NA || target == DirectionTypes.NA)
            {
                return false;
            }

            if (heading == target)
            {
                return true;
            }

            var headingDegrees = DirectionToDegrees(heading);
            var targetDegrees = DirectionToDegrees(target);
            if (!headingDegrees.HasValue || !targetDegrees.HasValue)
            {
                return false;
            }

            var diff = Math.Abs(headingDegrees.Value - targetDegrees.Value);
            var circularDiff = Math.Min(diff, 360 - diff);
            return circularDiff <= 45.0;
        }

        private static double? DirectionToDegrees(DirectionTypes direction)
        {
            return direction switch
            {
                DirectionTypes.NB => 0,
                DirectionTypes.NE => 45,
                DirectionTypes.EB => 90,
                DirectionTypes.SE => 135,
                DirectionTypes.SB => 180,
                DirectionTypes.SW => 225,
                DirectionTypes.WB => 270,
                DirectionTypes.NW => 315,
                _ => null
            };
        }

        private TmcForPhaseDto GetTMCDataForPhase(Location location, PhaseDetail currentPhase, List<IndianaEvent> currentControllerEventLogs, TimeSpaceDiagramOptions parameter)
        {
            var tasks = new List<Task<TurningMovementCountsLanesResult>>();
            var directionType = currentPhase.Approach.DirectionTypeId;
            var mergingApproaches = GetMergingApproaches(directionType);
            var plans = new List<Plan>();

            if (mergingApproaches == null || currentControllerEventLogs.IsNullOrEmpty())
                return new TmcForPhaseDto();

            var rightTurnDirection = mergingApproaches.RightTurnFrom;
            var leftTurnDirection = mergingApproaches.LeftTurnFrom;

            //Based on right turns it could be either Through Right or Right turns in the location
            var rightTurnMovementTypes = new List<MovementTypes> { MovementTypes.TR, MovementTypes.R };
            //Based on left turns it could be either Through Left or Left turns in the location
            var leftTurnMovementTypes = new List<MovementTypes> { MovementTypes.TL, MovementTypes.L };

            var rightTurnApproachesWithDetectors = location.Approaches
                .Where(a => a.DirectionTypeId == rightTurnDirection)
                .Select(a => new
                {
                    Approach = a,
                    Detectors = a.Detectors
                        .Where(d => rightTurnMovementTypes.Contains(d.MovementType) && d.DetectionTypes.Any(d => d.Id == DetectionTypes.LLC) &&
                                    d.LaneType == LaneTypes.V)
                        .ToList()
                })
                .FirstOrDefault(x => x.Detectors.Any());

            var leftTurnApproachesWithDetectors = location.Approaches
                .Where(a => a.DirectionTypeId == leftTurnDirection)
                .Select(a => new
                {
                    Approach = a,
                    Detectors = a.Detectors
                        .Where(d => leftTurnMovementTypes.Contains(d.MovementType) && d.DetectionTypes.Any(d => d.Id == DetectionTypes.LLC) &&
                                    d.LaneType == LaneTypes.V)
                        .ToList()
                })
                .FirstOrDefault(x => x.Detectors.Any());

            var rightTurnTmc = GetTMCEvents(parameter, currentControllerEventLogs, rightTurnApproachesWithDetectors?.Detectors ?? [], LaneTypes.V, rightTurnDirection, true);
            var leftTurnTmc = GetTMCEvents(parameter, currentControllerEventLogs, leftTurnApproachesWithDetectors?.Detectors ?? [], LaneTypes.V, leftTurnDirection, false);

            return new TmcForPhaseDto() { LeftTurnEvents = leftTurnTmc, RightTurnEvents = rightTurnTmc };
        }

        public List<TmcEventDto> GetTMCEvents(
            TimeSpaceDiagramOptions parameter,
            List<IndianaEvent> currentControllerEventLogs,
            List<Detector> detectorsByDirection,
            LaneTypes laneType,
            DirectionTypes directionType,
            bool IsRightTurn)
        {
            var detectorEvents = new List<IndianaEvent>();
            var detectionEvents = new List<TmcEventDto>();
            foreach (var detector in detectorsByDirection)
            {
                detectorEvents.AddRange(currentControllerEventLogs.GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(
                    parameter.Start,
                    parameter.End,
                    new List<short>() { 82 },
                    detector.DetectorChannel,
                    detector.GetOffset(),
                    detector.LatencyCorrection).ToList());
            }

            foreach (var e in detectorEvents)
            {
                detectionEvents.Add(new TmcEventDto(e.Timestamp, e.EventCode) { IsRightTurnEvent = IsRightTurn, IsLeftTurnEvent = !IsRightTurn, DirectionType = directionType });
            }

            return detectionEvents;
        }

        //HACK: this needs to be moved into the repository
        private List<RouteLocation> GetLocationsFromRouteId(int routeId)
        {
            var routeLocations = routeLocationsRepository.GetList()
                .Include(x => x.NextLocationDistance)
                .Include(x => x.PreviousLocationDistance)
                .Where(l => l.RouteId == routeId).ToList();
            return routeLocations ?? new List<RouteLocation>();
        }

        private List<IndianaEvent> GetEventOverlappingTime(DateTime start, IReadOnlyList<IndianaEvent> programmedCycleForPlan, string eventType)
        {
            var planEvent = programmedCycleForPlan.Where(e => e.Timestamp == start).ToList();
            if (planEvent.Count == 0)
            {
                var planEventInTimeSpan = programmedCycleForPlan.Where(e => e.Timestamp < start)
                    ?.GroupBy(log => log.EventCode)
                    ?.Select(group => group.OrderByDescending(e => e.Timestamp).FirstOrDefault())
                    .ToList();

                if (planEventInTimeSpan != null && planEventInTimeSpan.Count != 0)
                    planEvent = planEventInTimeSpan;
            }

            return planEvent.ToList();
        }

        public MergingApproaches? GetMergingApproaches(DirectionTypes mainDirection)
        {
            return mainDirection switch
            {
                DirectionTypes.NB => new MergingApproaches
                {
                    RightTurnFrom = DirectionTypes.EB,
                    LeftTurnFrom = DirectionTypes.WB
                },

                DirectionTypes.SB => new MergingApproaches
                {
                    RightTurnFrom = DirectionTypes.WB,
                    LeftTurnFrom = DirectionTypes.EB
                },

                DirectionTypes.EB => new MergingApproaches
                {
                    RightTurnFrom = DirectionTypes.SB,
                    LeftTurnFrom = DirectionTypes.NB
                },

                DirectionTypes.WB => new MergingApproaches
                {
                    RightTurnFrom = DirectionTypes.NB,
                    LeftTurnFrom = DirectionTypes.SB
                },
                _ => null,
            };
        }
    }
}
