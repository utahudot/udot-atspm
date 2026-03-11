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
    public class TimeSpaceDiagramReportService : ReportServiceBase<TimeSpaceDiagramOptions, IEnumerable<TimeSpaceDiagramPhaseResult>>
    {
        private sealed class ProcessedRouteLocation
        {
            public required RouteLocation RouteLocation { get; set; }
            public List<IndianaEvent> ControllerEventLogs { get; set; } = [];
            public PhaseDetail? PrimaryPhaseDetail { get; set; }
            public PhaseDetail? OpposingPhaseDetail { get; set; }
            public int ProgrammedCycleLength { get; set; }
            public TmcForPhaseDto PrimaryTmcEvents { get; set; } = new();
            public TmcForPhaseDto OpposingTmcEvents { get; set; } = new();
            public string? ErrorMessage { get; set; }
        }

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
        public override async Task<IEnumerable<TimeSpaceDiagramPhaseResult>> ExecuteAsync(TimeSpaceDiagramOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var routeLocations = GetLocationsFromRouteId(parameter.RouteId);
            var routeName = GetRouteNameFromId(parameter.RouteId);
            var routeLabel = GetRouteLabel(parameter.RouteId, routeName);
            if (routeLocations.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No route locations are configured for {routeLabel}. Add at least one route location before running this report.");
            }
            var srmTracks = parameter.IncludeSrmSearch
                ? timeSpaceDiagramSrmCsvService.GetTracks(
                    parameter.Start,
                    parameter.End,
                    routeLocations,
                    parameter.SrmCsvContentBase64)
                : new List<SrmEntityTrack>();

            var eventCodes = new List<short>() { 82, 81 };
            var tasks = new List<Task<TimeSpaceDiagramPhaseResult>>();
            routeLocations.Sort((r1, r2) => r1.Order - r2.Order);
            var (processedRouteLocations, programmedSplits) =
                ProcessRouteLocations(routeLocations, parameter, routeLabel);

            for (int i = 0; i < routeLocations.Count; i++)
            {
                var nextLocationDistance = i == routeLocations.Count - 1
                    ? 0
                    : routeLocations[i].NextLocationDistance?.Distance ?? 0;
                var previousLocationDistance = i == 0
                    ? 0
                    : routeLocations[i].PreviousLocationDistance?.Distance ?? 0;
                var processedRouteLocation = processedRouteLocations[i];

                if (processedRouteLocation.ErrorMessage != null ||
                    processedRouteLocation.PrimaryPhaseDetail == null)
                {
                    tasks.Add(Task.FromResult(CreateErrorPhaseResult(
                        parameter,
                        processedRouteLocation.RouteLocation,
                        processedRouteLocation.PrimaryPhaseDetail,
                        phaseType: "Primary",
                        order: i,
                        distanceToNextLocation: nextLocationDistance,
                        distanceToPreviousLocation: previousLocationDistance,
                        errorMessage: processedRouteLocation.ErrorMessage ??
                                      $"Primary phase is not configured for location {processedRouteLocation.RouteLocation.LocationIdentifier} (expected phase {processedRouteLocation.RouteLocation.PrimaryPhase}, direction {processedRouteLocation.RouteLocation.PrimaryDirection})")));
                }
                else
                {
                    tasks.Add(GetChartDataForPhase(
                        parameter,
                        processedRouteLocation.ControllerEventLogs,
                        processedRouteLocation.PrimaryPhaseDetail,
                        processedRouteLocation.ProgrammedCycleLength,
                        processedRouteLocation.PrimaryTmcEvents,
                        routeLocations[i],
                        srmTracks,
                        programmedSplits,
                        eventCodes,
                        nextLocationDistance,
                        previousLocationDistance,
                        i == 0,
                        i == routeLocations.Count - 1,
                        "Primary",
                        i));
                }
            }

            for (int i = routeLocations.Count - 1, j = 0; i >= 0; i--, j++)
            {
                var nextLocationDistance = i == 0
                    ? 0
                    : routeLocations[i].PreviousLocationDistance?.Distance ?? 0;
                var previousLocationDistance = i == routeLocations.Count - 1
                    ? 0
                    : routeLocations[i].NextLocationDistance?.Distance ?? 0;
                var processedRouteLocation = processedRouteLocations[i];

                if (processedRouteLocation.ErrorMessage != null ||
                    processedRouteLocation.OpposingPhaseDetail == null)
                {
                    tasks.Add(Task.FromResult(CreateErrorPhaseResult(
                        parameter,
                        processedRouteLocation.RouteLocation,
                        processedRouteLocation.OpposingPhaseDetail,
                        phaseType: "Opposing",
                        order: j,
                        distanceToNextLocation: nextLocationDistance,
                        distanceToPreviousLocation: previousLocationDistance,
                        errorMessage: processedRouteLocation.ErrorMessage ??
                                      $"Opposing phase is not configured for location {processedRouteLocation.RouteLocation.LocationIdentifier} (expected phase {processedRouteLocation.RouteLocation.OpposingPhase}, direction {processedRouteLocation.RouteLocation.OpposingDirection})")));
                }
                else
                {
                    tasks.Add(GetChartDataForPhase(
                        parameter,
                        processedRouteLocation.ControllerEventLogs,
                        processedRouteLocation.OpposingPhaseDetail,
                        processedRouteLocation.ProgrammedCycleLength,
                        processedRouteLocation.OpposingTmcEvents,
                        routeLocations[i],
                        srmTracks,
                        programmedSplits,
                        eventCodes,
                        nextLocationDistance,
                        previousLocationDistance,
                        isFirstElement: i == routeLocations.Count - 1,
                        isLastElement: i == 0,
                        "Opposing",
                        j));
                }
            }

            var results = await Task.WhenAll(tasks);
            return results;
        }

        private string GetRouteNameFromId(int routeId)
        {
            var routeName = routeRepository.GetList().Where(r => r.Id == routeId)?.FirstOrDefault()?.Name;
            return routeName != null ? routeName : "";
        }

        private static string GetRouteLabel(int routeId, string routeName)
        {
            return string.IsNullOrWhiteSpace(routeName)
                ? $"route id {routeId}"
                : $"route '{routeName}' (id {routeId})";
        }

        private (
            List<ProcessedRouteLocation> processedRouteLocations,
            List<IndianaEvent> programmedSplits)
        ProcessRouteLocations(
            IEnumerable<RouteLocation> routeLocations,
            TimeSpaceDiagramOptions parameter,
            string routeLabel)
        {
            var processedRouteLocations = new List<ProcessedRouteLocation>();
            var programmedSplitsForTimePeriod = new List<IndianaEvent>();

            foreach (var routeLocation in routeLocations)
            {
                var processedRouteLocation = new ProcessedRouteLocation
                {
                    RouteLocation = routeLocation
                };

                try
                {
                    if (routeLocation.NextLocationDistance == null &&
                        routeLocation.PreviousLocationDistance == null)
                    {
                        processedRouteLocation.ErrorMessage =
                            $"Distance is not configured for {routeLabel} at location {routeLocation.LocationIdentifier}. Configure next and/or previous location distance.";
                        processedRouteLocations.Add(processedRouteLocation);
                        continue;
                    }

                    var location = LocationRepository.GetLatestVersionOfLocation(
                        routeLocation.LocationIdentifier,
                        parameter.Start);

                    if (location == null)
                    {
                        processedRouteLocation.ErrorMessage =
                            $"Unable to load location details for {routeLocation.LocationIdentifier} in {routeLabel}.";
                        processedRouteLocations.Add(processedRouteLocation);
                        continue;
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
                        processedRouteLocation.ErrorMessage =
                            $"Missing phase configuration at {location.LocationDescription()} ({location.LocationIdentifier}) in {routeLabel}. Expected primary phase {routeLocation.PrimaryPhase} ({routeLocation.PrimaryDirection}) and opposing phase {routeLocation.OpposingPhase} ({routeLocation.OpposingDirection}).";
                        processedRouteLocations.Add(processedRouteLocation);
                        continue;
                    }

                    if (parameter.SpeedLimit == null &&
                        (primaryPhaseDetail.Approach.Mph == null ||
                         opposingPhaseDetail.Approach.Mph == null))
                    {
                        var missingSpeedForPrimary = primaryPhaseDetail.Approach.Mph == null;
                        var missingSpeedForOpposing = opposingPhaseDetail.Approach.Mph == null;
                        var missingSpeedParts = new List<string>();
                        if (missingSpeedForPrimary)
                        {
                            missingSpeedParts.Add($"primary phase {routeLocation.PrimaryPhase} ({routeLocation.PrimaryDirection})");
                        }

                        if (missingSpeedForOpposing)
                        {
                            missingSpeedParts.Add($"opposing phase {routeLocation.OpposingPhase} ({routeLocation.OpposingDirection})");
                        }

                        processedRouteLocation.ErrorMessage =
                            $"Speed is not configured at {location.LocationDescription()} ({location.LocationIdentifier}) in {routeLabel} for {string.Join(" and ", missingSpeedParts)} and no report speed override was provided.";
                        processedRouteLocations.Add(processedRouteLocation);
                        continue;
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

                    processedRouteLocation.ControllerEventLogs = controllerEventLogs;
                    processedRouteLocation.PrimaryPhaseDetail = primaryPhaseDetail;
                    processedRouteLocation.OpposingPhaseDetail = opposingPhaseDetail;
                    processedRouteLocation.ProgrammedCycleLength = currentProgrammedCycleLength;
                    processedRouteLocation.PrimaryTmcEvents =
                        GetTMCDataForPhase(location, primaryPhaseDetail, controllerEventLogs, parameter);
                    processedRouteLocation.OpposingTmcEvents =
                        GetTMCDataForPhase(location, opposingPhaseDetail, controllerEventLogs, parameter);
                    processedRouteLocations.Add(processedRouteLocation);
                }
                catch (Exception ex)
                {
                    processedRouteLocation.ErrorMessage =
                        $"Unexpected error while processing {routeLabel} at location {routeLocation.LocationIdentifier}: {ex.Message}";
                    processedRouteLocations.Add(processedRouteLocation);
                }
            }

            return (processedRouteLocations, programmedSplitsForTimePeriod);
        }

        private List<IndianaEvent> GetEventOverallapingTime(
            DateTime start,
            IReadOnlyList<IndianaEvent> programmedCycleForPlan,
            string eventType)
        {
            if (programmedCycleForPlan == null || programmedCycleForPlan.Count == 0)
                return new List<IndianaEvent>();

            // First try: exact timestamp match
            var exactMatches = programmedCycleForPlan
                .Where(e => e.Timestamp == start)
                .ToList();

            if (exactMatches.Count > 0)
                return exactMatches;

            // Fallback: latest event per EventCode before the given start
            var fallbackMatches = programmedCycleForPlan
                .Where(e => e.Timestamp < start)
                .GroupBy(e => e.EventCode)
                .Select(g => g.OrderByDescending(e => e.Timestamp).First())
                .ToList();

            return fallbackMatches; // will be empty if nothing found
        }

        private async Task<TimeSpaceDiagramPhaseResult> GetChartDataForPhase(
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
                    $"No controller event logs found for location {currentPhase.Approach.Location.LocationIdentifier}, phase {currentPhase.Approach.ProtectedPhaseNumber} ({phaseType}), in time range {parameter.Start:u} to {parameter.End:u}.",
                    order);
            }

            try
            {
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

                return TimeSpaceDiagramPhaseResult.Success(viewModel);
            }
            catch (Exception ex)
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
                    $"Error building time-space data for location {currentPhase.Approach.Location.LocationIdentifier}, phase {currentPhase.Approach.ProtectedPhaseNumber} ({phaseType}), in time range {parameter.Start:u} to {parameter.End:u}: {ex.Message}",
                    order);
            }
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

        private TimeSpaceDiagramPhaseResult CreateEmptyPhaseResult(
            TimeSpaceDiagramOptions parameter,
            PhaseDetail phase,
            TmcForPhaseDto tmcEventsForPhase,
            RouteLocation routeLocation,
            List<SrmEntityTrack> srmTracks,
            double distanceToNextLocation,
            double distanceToPreviousLocation,
            string phaseType,
            string? error,
            int order)
        {
            return TimeSpaceDiagramPhaseResult.Failure(error ?? "Unknown error");
        }

        private TimeSpaceDiagramPhaseResult CreateErrorPhaseResult(
            TimeSpaceDiagramOptions parameter,
            RouteLocation routeLocation,
            PhaseDetail? phase,
            string phaseType,
            int order,
            double distanceToNextLocation,
            double distanceToPreviousLocation,
            string errorMessage)
        {
            return TimeSpaceDiagramPhaseResult.Failure(errorMessage);
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
