using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.GreenTimeUtilization;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Green time utilization report service
    /// </summary>
    public class GreenTimeUtilizationReportService : ReportServiceBase<GreenTimeUtilizationOptions, IEnumerable<GreenTimeUtilizationResult>>
    {
        private readonly GreenTimeUtilizationService greenTimeUtilizationService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository LocationRepository;
        private readonly PhaseService phaseService;

        /// <inheritdoc/>
        public GreenTimeUtilizationReportService(
            GreenTimeUtilizationService GreenTimeUtilizationService,
            IControllerEventLogRepository controllerEventLogRepository,
            ILocationRepository LocationRepository,
            PhaseService phaseService)
        {
            greenTimeUtilizationService = GreenTimeUtilizationService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationRepository = LocationRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<GreenTimeUtilizationResult>> ExecuteAsync(GreenTimeUtilizationOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.locationIdentifier, parameter.Start);
            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<GreenTimeUtilizationResult>>(new NullReferenceException("Location not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetLocationEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<IEnumerable<GreenTimeUtilizationResult>>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(Location);
            var tasks = new List<Task<GreenTimeUtilizationResult>>();
            foreach (var phase in phaseDetails)
            {
                tasks.Add(GetChartDataForApproach(parameter, phase, controllerEventLogs, planEvents, false));
            }

            var results = await Task.WhenAll(tasks);

            var finalResultcheck = results.Where(result => result != null).ToList();

            //if (finalResultcheck.IsNullOrEmpty())
            //{
            //    return Ok("No chart data found");
            //}
            //return Ok(finalResultcheck);

            return finalResultcheck;
        }

        private async Task<GreenTimeUtilizationResult> GetChartDataForApproach(
            GreenTimeUtilizationOptions options,
            PhaseDetail phaseDetail,
            IReadOnlyList<ControllerEventLog> controllerEventLogs,
            IReadOnlyList<ControllerEventLog> planEvents,
            bool usePermissivePhase)
        {
            var detectorEvents = controllerEventLogs.GetDetectorEvents(options.MetricTypeId, phaseDetail.Approach, options.Start, options.End, true, false).ToList();
            var cycleEvents = controllerEventLogs.GetEventsByEventCodes(options.Start, options.End, new List<int>() { 1, 8, 11 }).ToList();

            GreenTimeUtilizationResult viewModel = greenTimeUtilizationService.GetChartData(
                phaseDetail,
                options,
                detectorEvents,
                cycleEvents,
                planEvents.ToList(),
                controllerEventLogs.ToList()
                );
            viewModel.LocationDescription = phaseDetail.Approach.Location.LocationDescription();
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
