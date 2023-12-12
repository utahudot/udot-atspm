using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.YellowRedActivations;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Yellow and red activations report service
    /// </summary>
    public class YellowRedActivationsReportService : ReportServiceBase<YellowRedActivationsOptions, IEnumerable<YellowRedActivationsResult>>
    {
        private readonly YellowRedActivationsService yellowRedActivationsService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;
        private readonly PhaseService phaseService;

        /// <inheritdoc/>
        public YellowRedActivationsReportService(
            YellowRedActivationsService yellowRedActivationsService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository,
            PhaseService phaseService)
        {
            this.yellowRedActivationsService = yellowRedActivationsService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<YellowRedActivationsResult>> ExecuteAsync(YellowRedActivationsOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(parameter.SignalIdentifier, parameter.Start);

            if (signal == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<YellowRedActivationsResult>>(new NullReferenceException("Signal not found"));
            }

            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for signal");
                return await Task.FromException<IEnumerable<YellowRedActivationsResult>>(new NullReferenceException("No Controller Event Logs found for signal"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(signal);
            var tasks = new List<Task<YellowRedActivationsResult>>();
            foreach (var phaseDetail in phaseDetails)
            {
                tasks.Add(GetChartDataForApproach(parameter, phaseDetail, controllerEventLogs, planEvents, signal.SignalDescription()));
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

        private async Task<YellowRedActivationsResult> GetChartDataForApproach(
            YellowRedActivationsOptions options,
            PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            string signalDescription)
        {
            var cycleEvents = controllerEventLogs.GetEventsByEventCodes(
                options.Start.AddSeconds(-900),
                options.End.AddSeconds(900),
                GetYellowRedActivationsCycleEventCodes(phaseDetail.UseOverlap),
                phaseDetail.PhaseNumber)
                .OrderBy(e => e.Timestamp)
            .ToList();
            var detectorEvents = controllerEventLogRepository.GetDetectorEvents(
                options.MetricTypeId,
                phaseDetail.Approach,
                options.Start,
                options.End,
                true,
                false);

            var viewModel = yellowRedActivationsService.GetChartData(
                options,
                phaseDetail,
                cycleEvents,
                detectorEvents,
                planEvents);
            viewModel.SignalDescription = signalDescription;
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }

        private List<int> GetYellowRedActivationsCycleEventCodes(bool useOverlap)
        {
            return useOverlap
                ? new List<int> { 62, 63, 64 }
                : new List<int> { 1, 8, 9, 11 };
        }
    }
}
