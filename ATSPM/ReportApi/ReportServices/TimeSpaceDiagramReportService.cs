using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.Infrastructure.Repositories;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.PhaseTermination;
using ATSPM.ReportApi.Business.TimeSpaceDiagram;
using ATSPM.ReportApi.Business.TimingAndActuation;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Time space diagram report service
    /// </summary>
    public class TimeSpaceDiagramReportService : ReportServiceBase<TimeSpaceDiagramOption, IEnumerable<TimeSpaceDiagramResult>>
    {
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository LocationRepository;
        private readonly TimeSpaceDiagramForPhaseService timeSpaceDiagramReportService;
        private readonly PhaseService phaseService;
        private readonly IRouteLocationsRepository routeLocationsRepository;
        private readonly static double defaultDistanceToNextLocation = 1584;

        public TimeSpaceDiagramReportService(IControllerEventLogRepository controllerEventLogRepository, ILocationRepository locationRepository, TimeSpaceDiagramForPhaseService timeSpaceDiagramReportService, PhaseService phaseService, IRouteLocationsRepository routeLocationsRepository)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            LocationRepository = locationRepository;
            this.timeSpaceDiagramReportService = timeSpaceDiagramReportService;
            this.phaseService = phaseService;
            this.routeLocationsRepository = routeLocationsRepository;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<TimeSpaceDiagramResult>> ExecuteAsync(TimeSpaceDiagramOption parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var routeLocations = GetLocationsFromRouteId(parameter.RouteId);
            var eventCodes = new List<int>() { 81, 82 };
            var tasks = new List<Task<TimeSpaceDiagramResult>>();
            routeLocations.Sort((r1, r2) => r1.Order - r2.Order);

            //if(parameter.OpposingPhase == true)
            //{
            //    routeLocations.Reverse();
            //}

            var (controllerEventLogsList, phaseDetails) = ProcessRouteLocations(routeLocations, parameter);

            for (var i = 0; i < routeLocations.Count; i++)
            {
                var nextLocationDistance = routeLocations[i].NextLocationDistance == null ? defaultDistanceToNextLocation : routeLocations[i].NextLocationDistance.Distance;
                tasks.Add(GetChartDataForPhase(parameter,
                    controllerEventLogsList[i],
                    phaseDetails[i],
                    eventCodes,
                    nextLocationDistance,
                    isFirstElement: i == 0,
                    isLastElement: i == routeLocations.Count - 1
                ));
            }

            var results = await Task.WhenAll(tasks);
            return results;
        }

        private (List<List<ControllerEventLog>> controllerEventLogsList, List<PhaseDetail> phaseDetails) ProcessRouteLocations(IEnumerable<RouteLocation> routeLocations, TimeSpaceDiagramOption parameter)
        {
            var controllerEventLogsList = new List<List<ControllerEventLog>>();
            var phaseDetails = new List<PhaseDetail>();

            foreach (var routeLocation in routeLocations)
            {
                var location = LocationRepository.GetLatestVersionOfLocation(routeLocation.LocationIdentifier, parameter.Start);

                if (location == null)
                {
                    throw new NullReferenceException("Issue fetching location from route");
                }

                var controllerEventLogs = controllerEventLogRepository.GetLocationEventsBetweenDates(location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

                if (controllerEventLogs.IsNullOrEmpty())
                {
                    throw new NullReferenceException("No Controller Event Logs found for Location");
                }

                var phaseToSearch = /*parameter.OpposingPhase == true ? routeLocation.OpposingPhase :*/ routeLocation.PrimaryPhase;
                var phaseDetail = phaseService.GetPhases(location).Find(p => p.PhaseNumber == phaseToSearch);

                if (phaseDetail == null)
                {
                    throw new NullReferenceException("Error grabbing phase details");
                }

                controllerEventLogsList.Add(controllerEventLogs);
                phaseDetails.Add(phaseDetail);
            }

            return (controllerEventLogsList, phaseDetails);
        }

        private async Task<TimeSpaceDiagramResult> GetChartDataForPhase(
            TimeSpaceDiagramOption parameter, 
            List<ControllerEventLog> currentControllerEventLogs, 
            PhaseDetail currentPhase,
            List<int> eventCodes,
            double distanceToNextLocation,
            bool isFirstElement,
            bool isLastElement)
        {
            eventCodes.AddRange(timeSpaceDiagramReportService.GetCycleCodes(currentPhase.UseOverlap));
            var approachevents = currentControllerEventLogs.GetEventsByEventCodes(
                parameter.Start.AddMinutes(-15),
                parameter.End.AddMinutes(15),
                eventCodes).ToList();
            var viewModel = timeSpaceDiagramReportService.GetChartData(parameter,
                currentPhase,
                approachevents,
                distanceToNextLocation,
                isFirstElement,
                isLastElement);
            viewModel.LocationDescription = currentPhase.Approach.Location.LocationDescription();
            viewModel.ApproachDescription = currentPhase.Approach.Description;
            return viewModel;
        }

        private List<RouteLocation> GetLocationsFromRouteId(int routeId)
        {
            var routeLocations = routeLocationsRepository.GetList().Where(l => l.RouteId == routeId).ToList();
            return routeLocations ?? new List<RouteLocation>();
        }
    }
}
