﻿using ATSPM.Application.Business.TimeSpaceDiagram;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Data.Models;
using Microsoft.IdentityModel.Tokens;
using ATSPM.Application.Business;
using ATSPM.Application.Business.Common;
using ATSPM.Application.TempExtensions;

namespace ATSPM.ReportApi.ReportServices
{
    public class TimeSpaceDiagramAverageReportService : ReportServiceBase<TimeSpaceDiagramAverageOptions, IEnumerable<TimeSpaceDiagramAverageResult>>
    {
        private readonly IIndianaEventLogRepository _controllerEventLogRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly PhaseService _phaseService;
        private readonly IRouteLocationsRepository _routeLocationsRepository;
        private readonly PlanService _planService;
        private readonly TimeSpaceAverageService _timeSpaceAverageService;

        public TimeSpaceDiagramAverageReportService(IIndianaEventLogRepository controllerEventLogRepository,
            ILocationRepository locationRepository,
            PhaseService phaseService,
            IRouteLocationsRepository routeLocationsRepository,
            PlanService planService,
            TimeSpaceAverageService timeSpaceAverageService)
        {
            _controllerEventLogRepository = controllerEventLogRepository;
            _locationRepository = locationRepository;
            _phaseService = phaseService;
            _routeLocationsRepository = routeLocationsRepository;
            _planService = planService;
            _timeSpaceAverageService = timeSpaceAverageService;
        }

        public override async Task<IEnumerable<TimeSpaceDiagramAverageResult>> ExecuteAsync(TimeSpaceDiagramAverageOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var routeLocations = GetLocationsFromRouteId(parameter.RouteId);
            if (routeLocations.Count == 0)
            {
                throw new Exception($"No locations present for route");
            }

            var eventCodes = new List<DataLoggerEnum>() { DataLoggerEnum.DetectorOff, DataLoggerEnum.DetectorOn };
            var tasks = new List<Task<TimeSpaceDiagramAverageResult>>();
            routeLocations.Sort((r1, r2) => r1.Order - r2.Order);

            //Throw exception when no distance is found
            foreach (var routeLocation in routeLocations)
            {
                if (routeLocation.NextLocationDistance == null && routeLocation.PreviousLocationDistance == null)
                {
                    throw new Exception($"Distance not configured for route: {parameter.RouteId}");
                }
            }

            var averageParamsBase = ProcessRouteLocations(routeLocations, parameter);

            var results = await Task.WhenAll(GetChartData(parameter, routeLocations, averageParamsBase, eventCodes));
            return results;
        }

        private async Task<TimeSpaceDiagramAverageResult> GetChartDataForPhase(
            TimeSpaceDiagramAverageOptions parameter,
            RouteLocation currRouteLocation,
            List<IndianaEvent> currentControllerEventLogs,
            PhaseDetail primaryPhase,
            List<IndianaEvent> programSplits,
            int offset,
            int cycleLength,
            List<DataLoggerEnum> eventCodes,
            double distanceToNextLocation,
            bool isLastElement,
            string phaseType)
        {
            eventCodes.AddRange(TimeSpaceService.GetCycleCodes(primaryPhase.UseOverlap));
            var approachEvents = GetApproachEvents(currentControllerEventLogs, eventCodes, parameter);
            var locationIdentifier = primaryPhase.Approach.Location.LocationIdentifier;
            var sequenceForLocation = parameter.Sequence.Find(item => item.LocationIdentifier == locationIdentifier).Sequence ?? new int[4][];
            var coordPhasesForLocation = parameter.CoordinatedPhases.Find(item => item.LocationIdentifier == locationIdentifier).CoordinatedPhases ?? new int[2];
            bool isCoordPhasesMatchRoutePhases = IsCoordPhasesMatchRoutePhases(coordPhasesForLocation, currRouteLocation.PrimaryPhase, currRouteLocation.OpposingPhase);
            var viewModel = _timeSpaceAverageService.GetChartData(
                parameter,
                primaryPhase,
                approachEvents,
                sequenceForLocation,
                coordPhasesForLocation,
                programSplits,
                offset,
                cycleLength,
                distanceToNextLocation,
                isLastElement,
                isCoordPhasesMatchRoutePhases
                );
            viewModel.LocationDescription = primaryPhase.Approach.Location.LocationDescription();
            viewModel.ApproachDescription = primaryPhase.Approach.Description;
            viewModel.PhaseType = phaseType;
            return viewModel;
        }

        private bool IsCoordPhasesMatchRoutePhases(int[] coordPhasesForLocation, int primaryPhase, int opposingPhase)
        {
            return coordPhasesForLocation.Contains(primaryPhase) && coordPhasesForLocation.Contains(opposingPhase);
        }

        private List<Task<TimeSpaceDiagramAverageResult>> GetChartData(TimeSpaceDiagramAverageOptions parameter,
            List<RouteLocation> routeLocations,
            TimeSpaceAverageBase averageParamsBase,
            List<DataLoggerEnum> eventCodes)
        {
            var results = new List<Task<TimeSpaceDiagramAverageResult>>();
            for (var i = 0; i < routeLocations.Count; i++)
            {
                var nextLocationDistance = i == routeLocations.Count - 1 ? 0 : routeLocations[i].NextLocationDistance.Distance;
                results.Add(GetChartDataForPhase(
                    parameter,
                    routeLocations[i],
                    averageParamsBase.ControllerEventLogsList[i],
                    averageParamsBase.PrimaryPhaseDetails[i],
                    averageParamsBase.ProgramSplits,
                    averageParamsBase.Offset,
                    averageParamsBase.ProgrammedCycleLength,
                    eventCodes,
                    nextLocationDistance,
                    isLastElement: i == routeLocations.Count - 1,
                    "Primary"
                    ));
            }

            for (int i = routeLocations.Count - 1; i >= 0; i--)
            {
                var nextLocationDistance = i == 0 ? 0 : routeLocations[i].PreviousLocationDistance.Distance;
                results.Add(GetChartDataForPhase(
                    parameter,
                    routeLocations[i],
                    averageParamsBase.ControllerEventLogsList[i],
                    averageParamsBase.OpposingPhaseDetails[i],
                    averageParamsBase.ProgramSplits,
                    averageParamsBase.Offset,
                    averageParamsBase.ProgrammedCycleLength,
                    eventCodes,
                    nextLocationDistance,
                    isLastElement: i == 0,
                    "Opposing"
                ));
            }

            return results;
        }

        private TimeSpaceAverageBase ProcessRouteLocations(IEnumerable<RouteLocation> routeLocations,
            TimeSpaceDiagramAverageOptions parameter)
        {
            var controllerEventLogsList = new List<List<IndianaEvent>>();
            var primaryPhaseDetails = new List<PhaseDetail>();
            var opposingPhaseDetails = new List<PhaseDetail>();
            int programmedCycleLength = 0;
            int offset = 0;
            var programmedSplitsForTimePeriod = new List<IndianaEvent>();
            var daysToProcess = GetDaysToProcess(parameter.StartDate, parameter.EndDate, parameter.DaysOfWeek);

            foreach (var routeLocation in routeLocations)
            {
                var planEventsForPeriod = new List<Plan>();
                var location = _locationRepository.GetLatestVersionOfLocation(routeLocation.LocationIdentifier);

                if (location == null)
                {
                    throw new NullReferenceException("Issue fetching location from route");
                }

                var controllerEventLogs = new List<IndianaEvent>();

                foreach (DateOnly date in daysToProcess)
                {
                    var start = date.ToDateTime(parameter.StartTime);
                    var end = date.ToDateTime(parameter.EndTime);
                    var logs = _controllerEventLogRepository.GetEventsBetweenDates(location.LocationIdentifier, start.AddHours(-12), end.AddHours(12)).ToList();
                    var planEvents = logs.GetPlanEvents(start.AddHours(-12), end.AddHours(12));
                    var plan = _planService.GetBasicPlans(start, end, routeLocation.LocationIdentifier, planEvents).FirstOrDefault();

                    if (programmedCycleLength == 0)
                    {
                        var programmedCycleForPlan = logs.GetEventsByEventCodes(start.AddHours(-12), end.AddHours(12), new List<DataLoggerEnum>() { DataLoggerEnum.CycleLengthChange });
                        programmedCycleLength = GetEventOverallapingTime(start, programmedCycleForPlan, "CycleLength").FirstOrDefault().EventParam;
                    }

                    if (offset == 0)
                    {
                        var offsets = logs.GetEventsByEventCodes(start.AddHours(-12), end.AddHours(12), new List<DataLoggerEnum>() { DataLoggerEnum.OffsetLengthChange });
                        offset = GetEventOverallapingTime(start, offsets, "Offset").FirstOrDefault().EventParam;
                    }

                    if (!programmedSplitsForTimePeriod.Any())
                    {
                        var programmedSplits = logs.GetEventsByEventCodes(
                            start.AddHours(-12),
                            end.AddHours(12),
                            new List<DataLoggerEnum>() {
                                DataLoggerEnum.Split1Change,
                                DataLoggerEnum.Split2Change,
                                DataLoggerEnum.Split3Change,
                                DataLoggerEnum.Split3Change,
                                DataLoggerEnum.Split4Change,
                                DataLoggerEnum.Split5Change,
                                DataLoggerEnum.Split6Change,
                                DataLoggerEnum.Split7Change });
                        programmedSplitsForTimePeriod = GetEventOverallapingTime(start, programmedSplits, "Program Splits");
                    }

                    planEventsForPeriod.Add(plan);
                    controllerEventLogs.AddRange(logs);
                }

                if (controllerEventLogs.IsNullOrEmpty())
                {
                    throw new NullReferenceException("No Controller Event Logs found for Location");
                }

                if (!IsPlanSameForTimePeriod(planEventsForPeriod))
                {
                    throw new NullReferenceException("Select different time period since plans dont match for each day");
                }

                var primaryPhaseDetail = _phaseService.GetPhases(location).Find(p => p.PhaseNumber == routeLocation.PrimaryPhase);
                var opposingPhaseDetail = _phaseService.GetPhases(location).Find(p => p.PhaseNumber == routeLocation.OpposingPhase);
                //var phaseToSearch = routeLocation.PrimaryPhase;
                //var phaseDetail = _phaseService.GetPhases(location).Find(p => p.PhaseNumber == phaseToSearch);

                if (primaryPhaseDetail == null || opposingPhaseDetail == null)
                {
                    throw new NullReferenceException("Error grabbing phase details");
                }

                controllerEventLogsList.Add(controllerEventLogs);
                primaryPhaseDetails.Add(primaryPhaseDetail);
                opposingPhaseDetails.Add(opposingPhaseDetail);
            }

            return new TimeSpaceAverageBase()
            {
                ControllerEventLogsList = controllerEventLogsList,
                PrimaryPhaseDetails = primaryPhaseDetails,
                OpposingPhaseDetails = opposingPhaseDetails,
                ProgrammedCycleLength = programmedCycleLength,
                ProgramSplits = programmedSplitsForTimePeriod,
                Offset = offset,
            };
        }

        private List<DateOnly> GetDaysToProcess(DateOnly startDate, DateOnly endDate, int[] daysOfWeek)
        {
            List<DateOnly> datesToInclude = new List<DateOnly>();
            var days = endDate.DayNumber - startDate.DayNumber;

            for (int i = 0; i <= days; i++)
            {
                var date = startDate.AddDays(i);
                if (daysOfWeek.Contains(((int)date.DayOfWeek)))
                {
                    datesToInclude.Add(date);
                }
            }

            return datesToInclude;
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

        private static bool IsPlanSameForTimePeriod(List<Plan> planEventsForPeriod)
        {
            return planEventsForPeriod.Select(p => p.PlanNumber).Distinct().Count() == 1;
        }

        private List<IndianaEvent> GetApproachEvents(List<IndianaEvent> currentControllerEventLogs, List<DataLoggerEnum> eventCodes, TimeSpaceDiagramAverageOptions parameter)
        {
            var approachEvents = new List<IndianaEvent>();
            var daysToProcess = GetDaysToProcess(parameter.StartDate, parameter.EndDate, parameter.DaysOfWeek);
            foreach (DateOnly date in daysToProcess)
            {
                var start = date.ToDateTime(parameter.StartTime);
                var end = date.ToDateTime(parameter.EndTime);
                var events = currentControllerEventLogs.GetEventsByEventCodes(start.AddMinutes(-30), end.AddMinutes(30), eventCodes);
                approachEvents.AddRange(events);
            }

            return approachEvents;
        }

        private List<RouteLocation> GetLocationsFromRouteId(int routeId)
        {
            var routeLocations = _routeLocationsRepository.GetList().Where(l => l.RouteId == routeId).ToList();
            return routeLocations ?? new List<RouteLocation>();
        }
    }
}