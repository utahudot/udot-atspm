using ATSPM.Application.Business;
using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.TimeSpaceDiagram;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    public class TimeSpaceDiagramAverageReportService : ReportServiceBase<TimeSpaceDiagramAverageOptions, IEnumerable<TimeSpaceDiagramAverageResult>>
    {
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository locationRepository;
        private readonly PhaseService phaseService;
        private readonly IRouteLocationsRepository routeLocationsRepository;
        private readonly PlanService planService;
        private readonly TimeSpaceAverageService timeSpaceAverageService;
        private readonly IRouteRepository routeRepository;

        public TimeSpaceDiagramAverageReportService(IIndianaEventLogRepository controllerEventLogRepository,
            ILocationRepository locationRepository,
            PhaseService phaseService,
            IRouteLocationsRepository routeLocationsRepository,
            PlanService planService,
            TimeSpaceAverageService timeSpaceAverageService,
            IRouteRepository routeRepository)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.locationRepository = locationRepository;
            this.phaseService = phaseService;
            this.routeLocationsRepository = routeLocationsRepository;
            this.planService = planService;
            this.timeSpaceAverageService = timeSpaceAverageService;
            this.routeRepository = routeRepository;
        }

        public override async Task<IEnumerable<TimeSpaceDiagramAverageResult>> ExecuteAsync(TimeSpaceDiagramAverageOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var routeLocations = GetLocationsFromRouteId(parameter.RouteId);
            var routeName = GetRouteNameFromId(parameter.RouteId);
            if (routeLocations.Count == 0)
            {
                throw new Exception($"No locations present for route");
            }

            var eventCodes = new List<short>() { 81, 82 };
            var tasks = new List<Task<TimeSpaceDiagramAverageResult>>();
            routeLocations.Sort((r1, r2) => r1.Order - r2.Order);

            //Throw exception when no distance is found
            foreach (var routeLocation in routeLocations)
            {
                if (routeLocation.NextLocationDistance == null && routeLocation.PreviousLocationDistance == null)
                {
                    throw new Exception($"Distance not configured for route: {routeName}");
                }
            }

            var averageParamsBase = ProcessRouteLocations(routeLocations, parameter);

            var results = await Task.WhenAll(GetChartData(parameter, routeLocations, averageParamsBase, eventCodes));
            return results;
        }

        private object GetRouteNameFromId(int routeId)
        {
            var routeName = routeRepository.GetList().Where(r => r.Id == routeId)?.FirstOrDefault()?.Name;
            return routeName != null ? routeName : "";
        }

        private async Task<TimeSpaceDiagramAverageResult> GetChartDataForPhase(
            TimeSpaceDiagramAverageOptions parameter,
            RouteLocation currRouteLocation,
            List<IndianaEvent> currentControllerEventLogs,
            PhaseDetail primaryPhase,
            List<IndianaEvent> programSplits,
            int offset,
            int cycleLength,
            List<short> eventCodes,
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
            var viewModel = timeSpaceAverageService.GetChartData(
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
            List<short> eventCodes)
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
                    averageParamsBase.ProgramSplits[i],
                    averageParamsBase.Offset[i],
                    averageParamsBase.ProgrammedCycleLength[i],
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
                    averageParamsBase.ProgramSplits[i],
                    averageParamsBase.Offset[i],
                    averageParamsBase.ProgrammedCycleLength[i],
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
            var programmedCycleLength = new List<int>();
            var offset = new List<int>();
            var programmedSplitsForTimePeriod = new List<List<IndianaEvent>>();
            var daysToProcess = GetDaysToProcess(parameter.StartDate, parameter.EndDate, parameter.DaysOfWeek);

            if (daysToProcess.Count == 0)
            {
                throw new Exception("No Data for Days Selected");
            }

            foreach (var routeLocation in routeLocations)
            {
                var planEventsForPeriod = new List<Plan>();
                var location = locationRepository.GetLatestVersionOfLocation(routeLocation.LocationIdentifier);

                if (location == null)
                {
                    throw new Exception("Issue fetching location from route");
                }

                var controllerEventLogs = new List<IndianaEvent>();
                int currentProgrammedCycleLength = 0;
                int currentOffset = 0;
                var currentProgrammedSplitsForTimePeriod = new List<IndianaEvent>();

                var primaryPhaseDetail = phaseService.GetPhases(location).Find(p => p.PhaseNumber == routeLocation.PrimaryPhase);
                var opposingPhaseDetail = phaseService.GetPhases(location).Find(p => p.PhaseNumber == routeLocation.OpposingPhase);
                //var phaseToSearch = routeLocation.PrimaryPhase;
                //var phaseDetail = _phaseService.GetPhases(location).Find(p => p.PhaseNumber == phaseToSearch);

                if (primaryPhaseDetail == null || opposingPhaseDetail == null)
                {
                    throw new Exception("Error grabbing phase details");
                }

                if (parameter.SpeedLimit == null && primaryPhaseDetail.Approach.Mph == null)
                {
                    throw new Exception($"Speed not configured in route for all phases");
                }

                if (parameter.SpeedLimit == null && opposingPhaseDetail.Approach.Mph == null)
                {
                    throw new Exception($"Speed not configured in route for all phases");
                }

                foreach (DateOnly date in daysToProcess)
                {
                    var start = date.ToDateTime(parameter.StartTime);
                    var end = date.ToDateTime(parameter.EndTime);
                    var logs = controllerEventLogRepository.GetEventsBetweenDates(location.LocationIdentifier, start.AddHours(-12), end.AddHours(12)).ToList();
                    var planEvents = logs.GetPlanEvents(start.AddHours(-12), end.AddHours(12));
                    var plan = planService.GetBasicPlans(start, end, routeLocation.LocationIdentifier, planEvents).FirstOrDefault();



                    if (currentProgrammedCycleLength == 0)
                    {
                        var programmedCycleForPlan = logs.GetEventsByEventCodes(start.AddHours(-12), end.AddHours(12), new List<short>() { 132 });
                        currentProgrammedCycleLength = GetEventOverallapingTime(start, programmedCycleForPlan, "CycleLength").FirstOrDefault().EventParam;
                    }

                    if (currentOffset == 0)
                    {
                        var offsets = logs.GetEventsByEventCodes(start.AddHours(-12), end.AddHours(12), new List<short>() { 133 });
                        currentOffset = GetEventOverallapingTime(start, offsets, "Offset").FirstOrDefault().EventParam;
                    }

                    if (!currentProgrammedSplitsForTimePeriod.Any())
                    {
                        var programmedSplits = logs.GetEventsByEventCodes(
                            start.AddHours(-12),
                            end.AddHours(12),
                            new List<short>() {
                            134,
                            135,
                            136,
                            137,
                            138,
                            139,
                            140 });
                        currentProgrammedSplitsForTimePeriod.AddRange(GetEventOverallapingTime(start, programmedSplits, "Program Splits"));
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



                controllerEventLogsList.Add(controllerEventLogs);
                primaryPhaseDetails.Add(primaryPhaseDetail);
                opposingPhaseDetails.Add(opposingPhaseDetail);
                programmedCycleLength.Add(currentProgrammedCycleLength);
                offset.Add(currentOffset);
                programmedSplitsForTimePeriod.Add(currentProgrammedSplitsForTimePeriod);
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

        private List<IndianaEvent> GetApproachEvents(List<IndianaEvent> currentControllerEventLogs, List<short> eventCodes, TimeSpaceDiagramAverageOptions parameter)
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
            var routeLocations = routeLocationsRepository.GetList().Where(l => l.RouteId == routeId).ToList();
            return routeLocations ?? new List<RouteLocation>();
        }
    }
}
