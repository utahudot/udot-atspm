using ATSPM.Application.Repositories;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.PreemptServiceRequest;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Preempt request report service
    /// </summary>
    public class PreemptRequestReportService : ReportServiceBase<PreemptServiceRequestOptions, PreemptServiceRequestResult>
    {
        private readonly PreemptServiceRequestService preemptServiceRequestService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;

        /// <inheritdoc/>
        public PreemptRequestReportService(
            PreemptServiceRequestService preemptServiceRequestService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository)
        {
            this.preemptServiceRequestService = preemptServiceRequestService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
        }

        /// <inheritdoc/>
        public override async Task<PreemptServiceRequestResult> ExecuteAsync(PreemptServiceRequestOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(parameter.SignalIdentifier, parameter.Start);
            if (signal == null)
            {
                //return BadRequest("Signal not found");
                return await Task.FromException<PreemptServiceRequestResult>(new NullReferenceException("Signal not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(parameter.SignalIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for signal");
                return await Task.FromException<PreemptServiceRequestResult>(new NullReferenceException("No Controller Event Logs found for signal"));
            }
            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var events = controllerEventLogs.GetEventsByEventCodes(parameter.Start, parameter.End, new List<int>() { 102 });
            PreemptServiceRequestResult result = preemptServiceRequestService.GetChartData(parameter, planEvents, events);
            result.SignalDescription = signal.SignalDescription();
            //return Ok(viewModel);

            return result;
        }
    }
}
