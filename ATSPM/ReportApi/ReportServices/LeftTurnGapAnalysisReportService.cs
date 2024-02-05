using ATSPM.Application.Repositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
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
        private readonly ILocationRepository LocationRepository;

        /// <inheritdoc/>
        public LeftTurnGapAnalysisReportService(
            LeftTurnGapAnalysisService leftTurnGapAnalysisService,
            IApproachRepository approachRepository,
            IControllerEventLogRepository controllerEventLogRepository,
            ILocationRepository LocationRepository)
        {
            this.leftTurnGapAnalysisService = leftTurnGapAnalysisService;
            this.approachRepository = approachRepository;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationRepository = LocationRepository;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<LeftTurnGapAnalysisResult>> ExecuteAsync(LeftTurnGapAnalysisOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.locationIdentifier, parameter.Start);
            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<LeftTurnGapAnalysisResult>>(new NullReferenceException("Location not found"));
            }
            var eventCodes = new List<int> { 1, 10, 81 };
            var controllerEventLogs = controllerEventLogRepository.GetLocationEventsBetweenDates(
                Location.LocationIdentifier,
                parameter.Start,
                parameter.End)
                .ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<IEnumerable<LeftTurnGapAnalysisResult>>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var tasks = new List<Task<LeftTurnGapAnalysisResult>>();
            var leftTurnGapData = new List<LeftTurnGapAnalysisResult>();
            //Get phase + check for opposing phase before creating chart
            var ebPhase = Location.Approaches.FirstOrDefault(x => x.ProtectedPhaseNumber == 6);
            if (ebPhase != null && Location.Approaches.Any(x => x.ProtectedPhaseNumber == 2))
            {
                tasks.Add(leftTurnGapAnalysisService.GetAnalysisForPhase(ebPhase, controllerEventLogs, parameter));
            }

            var nbPhase = Location.Approaches.FirstOrDefault(x => x.ProtectedPhaseNumber == 8);
            if (nbPhase != null && Location.Approaches.Any(x => x.ProtectedPhaseNumber == 4))
            {
                tasks.Add(leftTurnGapAnalysisService.GetAnalysisForPhase(nbPhase, controllerEventLogs, parameter));
            }

            var wbPhase = Location.Approaches.FirstOrDefault(x => x.ProtectedPhaseNumber == 2);
            if (wbPhase != null && Location.Approaches.Any(x => x.ProtectedPhaseNumber == 6))
            {
                tasks.Add(leftTurnGapAnalysisService.GetAnalysisForPhase(wbPhase, controllerEventLogs, parameter));
            }

            var sbPhase = Location.Approaches.FirstOrDefault(x => x.ProtectedPhaseNumber == 4);
            if (sbPhase != null && Location.Approaches.Any(x => x.ProtectedPhaseNumber == 8))
            {
                tasks.Add(leftTurnGapAnalysisService.GetAnalysisForPhase(sbPhase, controllerEventLogs, parameter));
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
    }
}
