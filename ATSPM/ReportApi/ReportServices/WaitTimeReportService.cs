using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.WaitTime;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Wait time report service
    /// </summary>
    public class WaitTimeReportService : ReportServiceBase<WaitTimeOptions, IEnumerable<WaitTimeResult>>
    {
        private readonly AnalysisPhaseCollectionService analysisPhaseCollectionService;
        private readonly WaitTimeService waitTimeService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository signalRepository;
        private readonly PhaseService phaseService;

        /// <inheritdoc/>
        public WaitTimeReportService(
            AnalysisPhaseCollectionService analysisPhaseCollectionService,
            WaitTimeService waitTimeService,
            IControllerEventLogRepository controllerEventLogRepository,
            ILocationRepository signalRepository,
            PhaseService phaseService
            )
        {
            this.analysisPhaseCollectionService = analysisPhaseCollectionService;
            this.waitTimeService = waitTimeService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<WaitTimeResult>> ExecuteAsync(WaitTimeOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(parameter.locationIdentifier, parameter.Start);

            if (signal == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<WaitTimeResult>>(new NullReferenceException("Signal not found"));
            }

            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for signal");
                return await Task.FromException<IEnumerable<WaitTimeResult>>(new NullReferenceException("No Controller Event Logs found for signal"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var phaseEvents = controllerEventLogRepository.GetSignalEventsByEventCodes(
            parameter.locationIdentifier,
            parameter.Start,
                parameter.End,
                new List<int>() {
                    82,
                    WaitTimeService.PHASE_BEGIN_GREEN,
                    WaitTimeService.PHASE_CALL_DROPPED,
                    WaitTimeService.PHASE_END_RED_CLEARANCE,
                    WaitTimeService.PHASE_CALL_REGISTERED}
                );
            var terminationEvents = controllerEventLogs.Where(e =>
                new List<int> { 4, 5, 6, 7 }.Contains(e.EventCode)
                && e.Timestamp >= parameter.Start
                && e.Timestamp <= parameter.End).ToList();
            var splitsEventCodes = new List<int>();
            for (var i = 130; i <= 151; i++)
                splitsEventCodes.Add(i);
            var splitsEvents = controllerEventLogs.Where(e =>
                splitsEventCodes.Contains(e.EventCode)
                && e.Timestamp >= parameter.Start
                && e.Timestamp <= parameter.End).ToList();
            var volume = new VolumeCollection(
            parameter.Start,
                parameter.End,
                phaseEvents.Where(e => e.EventCode == 82).ToList(),
                parameter.BinSize);
            var analysisPhaseDataCollection = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(
                signal.LocationIdentifier,
            parameter.Start,
                parameter.End,
                planEvents,
                phaseEvents,
                splitsEvents,
                null,
                terminationEvents,
                signal,
                1);
            var phaseDetails = phaseService.GetPhases(signal);
            var tasks = new List<Task<WaitTimeResult>>();
            foreach (var phaseDetail in phaseDetails)
            {
                tasks.Add(waitTimeService.GetChartData(
                parameter,
                phaseDetail,
                phaseEvents,
                analysisPhaseDataCollection.AnalysisPhases.Where(a => a.PhaseNumber == phaseDetail.PhaseNumber).First(),
                analysisPhaseDataCollection.Plans,
                volume
                ));
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
