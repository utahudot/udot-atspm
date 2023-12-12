using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.PreemptService;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Approach delay report service
    /// </summary>
    public class PreemptServiceReportService : ReportServiceBase<PreemptServiceOptions, PreemptServiceResult>
    {
        private readonly PreemptServiceService preemptServiceService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;

        /// <inheritdoc/>
        public PreemptServiceReportService(
            PreemptServiceService preemptServiceService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository
            )
        {
            this.preemptServiceService = preemptServiceService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
        }

        /// <inheritdoc/>
        public override async Task<PreemptServiceResult> ExecuteAsync(PreemptServiceOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(parameter.SignalIdentifier, parameter.Start);
            if (signal == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<PreemptServiceResult>(new NullReferenceException("Signal not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(parameter.SignalIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for signal");
                return await Task.FromException<PreemptServiceResult>(new NullReferenceException("No Controller Event Logs found for signal"));
            }
            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var preemptEvents = controllerEventLogs.GetEventsByEventCodes(parameter.Start, parameter.End, new List<int>() { 105 });
            PreemptServiceResult result = preemptServiceService.GetChartData(
                parameter,
                planEvents.ToList(),
                preemptEvents.ToList());
            result.SignalDescription = signal.SignalDescription();
            //return Ok(viewModel);

            return result;
        }

    }
}
