using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.PreempDetail;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Preempt detail report service
    /// </summary>
    public class PreemptDetailReportService : ReportServiceBase<PreemptDetailOptions, PreemptDetailResult>
    {
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly PreemptDetailService preemptDetailService;
        private readonly ISignalRepository signalRepository;

        /// <inheritdoc/>
        public PreemptDetailReportService(
            IControllerEventLogRepository controllerEventLogRepository,
            PreemptDetailService preemptDetailService,
            ISignalRepository signalRepository)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.preemptDetailService = preemptDetailService;
            this.signalRepository = signalRepository;
        }

        /// <inheritdoc/>
        public override async Task<PreemptDetailResult> ExecuteAsync(PreemptDetailOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(parameter.SignalIdentifier, parameter.Start);
            if (signal == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<PreemptDetailResult>(new NullReferenceException("Signal not found"));
            }
            var codes = new List<int>();

            for (var i = 101; i <= 111; i++)
                codes.Add(i);

            var events = controllerEventLogRepository.GetSignalEventsByEventCodes(
                parameter.SignalIdentifier,
                parameter.Start,
                parameter.End,
                codes).ToList();

            if (events.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for signal");
                return await Task.FromException<PreemptDetailResult>(new NullReferenceException("No Controller Event Logs found for signal"));
            }


            var result = preemptDetailService.GetChartData(parameter, events);
            //viewModel.Details = signal.SignalDescription();
            result.Summary.SignalDescription = signal.SignalDescription();
            //return Ok(viewModel);

            return result;
        }
    }
}
