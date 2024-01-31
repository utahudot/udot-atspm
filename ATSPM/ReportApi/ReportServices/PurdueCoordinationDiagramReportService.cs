using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.PurdueCoordinationDiagram;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Purdue coordination diagram report service
    /// </summary>
    public class PurdueCoordinationDiagramReportService : ReportServiceBase<PurdueCoordinationDiagramOptions, IEnumerable<PurdueCoordinationDiagramResult>>
    {
        private readonly PurdueCoordinationDiagramService perdueCoordinationDiagramService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly LocationPhaseService LocationPhaseService;
        private readonly ILocationRepository LocationRepository;
        private readonly PhaseService phaseService;

        /// <inheritdoc/>
        public PurdueCoordinationDiagramReportService(
            PurdueCoordinationDiagramService perdueCoordinationDiagramService,
            IControllerEventLogRepository controllerEventLogRepository,
            LocationPhaseService LocationPhaseService,
            ILocationRepository LocationRepository,
            PhaseService phaseService)
        {
            this.perdueCoordinationDiagramService = perdueCoordinationDiagramService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationPhaseService = LocationPhaseService;
            this.LocationRepository = LocationRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<PurdueCoordinationDiagramResult>> ExecuteAsync(PurdueCoordinationDiagramOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.locationIdentifier, parameter.Start);
            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<PurdueCoordinationDiagramResult>>(new NullReferenceException("Location not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetLocationEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<IEnumerable<PurdueCoordinationDiagramResult>>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(Location);
            var tasks = new List<Task<PurdueCoordinationDiagramResult>>();
            foreach (var phase in phaseDetails)
            {
                tasks.Add(GetChartDataForApproach(parameter, phase, controllerEventLogs, planEvents));
            }

            var results = await Task.WhenAll(tasks);

            var finalResultcheck = results.Where(result => result != null).OrderBy(r => r.PhaseNumber).ToList();

            //if (finalResultcheck.IsNullOrEmpty())
            //{
            //    return Ok("No chart data found");
            //}
            //return Ok(finalResultcheck);

            return finalResultcheck;
        }

        private async Task<PurdueCoordinationDiagramResult> GetChartDataForApproach(
            PurdueCoordinationDiagramOptions options,
            PhaseDetail phaseDetail,
            IReadOnlyList<ControllerEventLog> controllerEventLogs,
            IReadOnlyList<ControllerEventLog> planEvents)
        {
            var LocationPhase = await LocationPhaseService.GetLocationPhaseData(
                phaseDetail,
                options.Start,
                options.End,
                options.BinSize,
                null,
                null,
                controllerEventLogs.ToList(),
                planEvents.ToList(),
                options.ShowVolumes);
            if (LocationPhase == null)
            {
                return null;
            }
            PurdueCoordinationDiagramResult viewModel = perdueCoordinationDiagramService.GetChartData(options, phaseDetail.Approach, LocationPhase);
            viewModel.LocationDescription = phaseDetail.Approach.Location.LocationDescription();
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
