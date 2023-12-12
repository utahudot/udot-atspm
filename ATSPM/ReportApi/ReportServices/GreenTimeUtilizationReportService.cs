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
        private readonly ILocationRepository signalRepository;
        private readonly PhaseService phaseService;

        /// <inheritdoc/>
        public GreenTimeUtilizationReportService(
            GreenTimeUtilizationService GreenTimeUtilizationService,
            IControllerEventLogRepository controllerEventLogRepository,
            ILocationRepository signalRepository,
            PhaseService phaseService)
        {
            greenTimeUtilizationService = GreenTimeUtilizationService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<GreenTimeUtilizationResult>> ExecuteAsync(GreenTimeUtilizationOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(parameter.locationIdentifier, parameter.Start);
            if (signal == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<GreenTimeUtilizationResult>>(new NullReferenceException("Signal not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for signal");
                return await Task.FromException<IEnumerable<GreenTimeUtilizationResult>>(new NullReferenceException("No Controller Event Logs found for signal"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(signal);
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
            viewModel.SignalDescription = phaseDetail.Approach.Location.SignalDescription();
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
