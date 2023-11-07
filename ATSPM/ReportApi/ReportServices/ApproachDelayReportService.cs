using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.AppoachDelay;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Approach delay report service
    /// </summary>
    public class ApproachDelayReportService : ReportServiceBase<ApproachDelayOptions, IEnumerable<ApproachDelayResult>>
    {
        private readonly ApproachDelayService _approachDelayService;
        private readonly SignalPhaseService _signalPhaseService;
        private readonly ISignalRepository _signalRepository;
        private readonly IControllerEventLogRepository _controllerEventLogRepository;
        private readonly PhaseService _phaseService;

        /// <inheritdoc/>
        public ApproachDelayReportService(
            ApproachDelayService approachDelayService,
            SignalPhaseService signalPhaseService,
            ISignalRepository signalRepository,
            IControllerEventLogRepository controllerEventLogRepository,
            PhaseService phaseService
            )
        {
            _approachDelayService = approachDelayService;
            _signalPhaseService = signalPhaseService;
            _signalRepository = signalRepository;
            _controllerEventLogRepository = controllerEventLogRepository;
            _phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<ApproachDelayResult>> ExecuteAsync(ApproachDelayOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var signal = _signalRepository.GetLatestVersionOfSignal(parameter.SignalIdentifier, parameter.Start);

            if (signal == null)
            {
                //return BadRequest("Signal not found");
                return await Task.FromException<IEnumerable<ApproachDelayResult>>(new NullReferenceException("Signal not found"));
            }

            var controllerEventLogs = _controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for signal");
                return await Task.FromException<IEnumerable<ApproachDelayResult>>(new NullReferenceException("No Controller Event Logs found for signal"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
                parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var phaseDetails = _phaseService.GetPhases(signal);
            var tasks = new List<Task<ApproachDelayResult>>();

            foreach (var phase in phaseDetails)
            {
                tasks.Add(GetChartDataByApproach(parameter, phase, controllerEventLogs, planEvents, signal.SignalDescription()));
            }

            var results = await Task.WhenAll(tasks);
            //var finalResultcheck = results.Where(result => result != null).ToList();

            //if (finalResultcheck.IsNullOrEmpty())
            //{
            //    return Ok("No chart data found");
            //}
            //return Ok(finalResultcheck);

            return results;
        }

        protected async Task<ApproachDelayResult> GetChartDataByApproach(
            ApproachDelayOptions options,
            PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            string signalDescription)
        {
            var signalPhase = await _signalPhaseService.GetSignalPhaseData(
                phaseDetail,
                options.Start,
                options.End,
                options.BinSize,
                null,
                controllerEventLogs,
                planEvents,
                false);
            if (signalPhase == null)
            {
                return null;
            }
            ApproachDelayResult viewModel = _approachDelayService.GetChartData(
                options,
                phaseDetail,
                signalPhase);
            viewModel.SignalDescription = signalDescription;
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
