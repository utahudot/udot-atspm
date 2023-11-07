using ATSPM.Application.Repositories;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.LeftTurnGapAnalysis;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Left turn gap analysis report service
    /// </summary>
    public class LeftTurnGapAnalysisReportService : ReportServiceBase<LeftTurnGapAnalysisOptions, IEnumerable<LeftTurnGapAnalysisResult>>
    {
        private readonly LeftTurnGapAnalysisService leftTurnGapAnalysisService;
        private readonly IApproachRepository approachRepository;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;

        /// <inheritdoc/>
        public LeftTurnGapAnalysisReportService(
            LeftTurnGapAnalysisService leftTurnGapAnalysisService,
            IApproachRepository approachRepository,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository)
        {
            this.leftTurnGapAnalysisService = leftTurnGapAnalysisService;
            this.approachRepository = approachRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<LeftTurnGapAnalysisResult>> ExecuteAsync(LeftTurnGapAnalysisOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(parameter.SignalIdentifier, parameter.Start);
            if (signal == null)
            {
                //return BadRequest("Signal not found");
                return await Task.FromException<IEnumerable<LeftTurnGapAnalysisResult>>(new NullReferenceException("Signal not found"));
            }
            var eventCodes = new List<int> { 1, 10, 81 };
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(
                signal.SignalIdentifier,
                parameter.Start,
                parameter.End)
                .ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for signal");
                return await Task.FromException<IEnumerable<LeftTurnGapAnalysisResult>>(new NullReferenceException("No Controller Event Logs found for signal"));
            }

            var tasks = new List<Task<LeftTurnGapAnalysisResult>>();
            var leftTurnGapData = new List<LeftTurnGapAnalysisResult>();
            //Get phase + check for opposing phase before creating chart
            var ebPhase = signal.Approaches.FirstOrDefault(x => x.ProtectedPhaseNumber == 6);
            if (ebPhase != null && signal.Approaches.Any(x => x.ProtectedPhaseNumber == 2))
            {
                tasks.Add(leftTurnGapAnalysisService.GetAnalysisForPhase(ebPhase, controllerEventLogs, parameter));
            }

            var nbPhase = signal.Approaches.FirstOrDefault(x => x.ProtectedPhaseNumber == 8);
            if (nbPhase != null && signal.Approaches.Any(x => x.ProtectedPhaseNumber == 4))
            {
                tasks.Add(leftTurnGapAnalysisService.GetAnalysisForPhase(nbPhase, controllerEventLogs, parameter));
            }

            var wbPhase = signal.Approaches.FirstOrDefault(x => x.ProtectedPhaseNumber == 2);
            if (wbPhase != null && signal.Approaches.Any(x => x.ProtectedPhaseNumber == 6))
            {
                tasks.Add(leftTurnGapAnalysisService.GetAnalysisForPhase(wbPhase, controllerEventLogs, parameter));
            }

            var sbPhase = signal.Approaches.FirstOrDefault(x => x.ProtectedPhaseNumber == 4);
            if (sbPhase != null && signal.Approaches.Any(x => x.ProtectedPhaseNumber == 8))
            {
                tasks.Add(leftTurnGapAnalysisService.GetAnalysisForPhase(sbPhase, controllerEventLogs, parameter));
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
    }
}
