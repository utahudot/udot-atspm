using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.TimeSpaceDiagram;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Time space diagram report service
    /// </summary>
    public class TimeSpaceDiagramReportService : ReportServiceBase<TimeSpaceDiagramOptions, IEnumerable<TimeSpaceDiagramResult>>
    {
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository LocationRepository;
        private readonly TimeSpaceDiagramForPhaseService timeSpaceDiagramReportService;
        private readonly PhaseService phaseService;
        private readonly IRouteLocationsRepository routeLocationsRepository;
        private readonly static double defaultDistanceToNextLocation = 1584;

        public TimeSpaceDiagramReportService(IIndianaEventLogRepository controllerEventLogRepository, ILocationRepository locationRepository, TimeSpaceDiagramForPhaseService timeSpaceDiagramReportService, PhaseService phaseService, IRouteLocationsRepository routeLocationsRepository)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            LocationRepository = locationRepository;
            this.timeSpaceDiagramReportService = timeSpaceDiagramReportService;
            this.phaseService = phaseService;
            this.routeLocationsRepository = routeLocationsRepository;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<TimeSpaceDiagramResult>> ExecuteAsync(TimeSpaceDiagramOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var routeLocations = GetLocationsFromRouteId(parameter.RouteId);
            if (routeLocations.Count == 0)
            {
                throw new Exception($"No locations present for route");
            }

            var eventCodes = new List<DataLoggerEnum>() { DataLoggerEnum.DetectorOn, DataLoggerEnum.DetectorOff };
            var tasks = new List<Task<TimeSpaceDiagramResult>>();
            routeLocations.Sort((r1, r2) => r1.Order - r2.Order);

            //Throw exception when no distance is found
            foreach (var routeLocation in routeLocations)
            {
                if (routeLocation.NextLocationDistance == null && routeLocation.PreviousLocationDistance == null)
                {
                    throw new Exception($"Distance not configured for route: {parameter.RouteId}");
                }
            }

            var (controllerEventLogsList, phaseDetails) = ProcessRouteLocations(routeLocations, parameter);

            for (var i = 0; i < routeLocations.Count; i++)
            {
                var nextLocationDistance = i == routeLocations.Count - 1 ? 0 : routeLocations[i].NextLocationDistance.Distance;
                var previousLocationDistance = i == 0 ? 0 : routeLocations[i].PreviousLocationDistance.Distance;
                tasks.Add(GetChartDataForPhase(parameter,
                    controllerEventLogsList[i],
                    phaseDetails[i],
                    eventCodes,
                    nextLocationDistance,
                    previousLocationDistance,
                    isFirstElement: i == 0,
                    isLastElement: i == routeLocations.Count - 1
                ));
            }

            var results = await Task.WhenAll(tasks);
            return results;
        }

        private (List<List<IndianaEvent>> controllerEventLogsList, List<PhaseDetail> phaseDetails) ProcessRouteLocations(IEnumerable<RouteLocation> routeLocations, TimeSpaceDiagramOptions parameter)
        {
            var controllerEventLogsList = new List<List<IndianaEvent>>();
            var phaseDetails = new List<PhaseDetail>();

            foreach (var routeLocation in routeLocations)
            {
                var location = LocationRepository.GetLatestVersionOfLocation(routeLocation.LocationIdentifier, parameter.Start);

                if (location == null)
                {
                    throw new NullReferenceException("Issue fetching location from route");
                }

                var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

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
            TimeSpaceDiagramOptions parameter,
            List<IndianaEvent> currentControllerEventLogs,
            PhaseDetail currentPhase,
            List<DataLoggerEnum> eventCodes,
            double distanceToNextLocation,
            double distanceToPreviousLocation,
            bool isFirstElement,
            bool isLastElement)
        {
            eventCodes.AddRange(timeSpaceDiagramReportService.GetCycleCodes(currentPhase.UseOverlap));
            var approachEvents = currentControllerEventLogs.GetEventsByEventCodes(
                parameter.Start.AddMinutes(-15),
                parameter.End.AddMinutes(15),
                eventCodes).ToList();
            var viewModel = timeSpaceDiagramReportService.GetChartData(parameter,
                currentPhase,
                approachEvents,
                distanceToNextLocation,
                distanceToPreviousLocation,
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
