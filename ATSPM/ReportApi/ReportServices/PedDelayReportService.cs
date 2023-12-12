using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.PedDelay;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Ped delay report service
    /// </summary>
    public class PedDelayReportService : ReportServiceBase<PedDelayOptions, IEnumerable<PedDelayResult>>
    {
        private readonly PedDelayService pedDelayService;
        private readonly PedPhaseService pedPhaseService;
        private readonly CycleService cycleService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository signalRepository;
        private readonly PhaseService phaseService;

        /// <inheritdoc/>
        public PedDelayReportService(
            PedDelayService pedDelayService,
            PedPhaseService pedPhaseService,
            CycleService cycleService,
            IControllerEventLogRepository controllerEventLogRepository,
            ILocationRepository signalRepository,
            PhaseService phaseService)
        {
            this.pedDelayService = pedDelayService;
            this.pedPhaseService = pedPhaseService;
            this.cycleService = cycleService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<PedDelayResult>> ExecuteAsync(PedDelayOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(parameter.locationIdentifier, parameter.Start);
            if (signal == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<PedDelayResult>>(new NullReferenceException("Signal not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for signal");
                return await Task.FromException<IEnumerable<PedDelayResult>>(new NullReferenceException("No Controller Event Logs found for signal"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(signal);
            var tasks = new List<Task<PedDelayResult>>();
            foreach (var phase in phaseDetails)
            {
                tasks.Add(GetChartDataForApproach(parameter, phase, planEvents, controllerEventLogs));
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

        private async Task<PedDelayResult> GetChartDataForApproach(
            PedDelayOptions options,
            PhaseDetail phaseDetail,
            IReadOnlyList<ControllerEventLog>
            planEvents,
            IReadOnlyList<ControllerEventLog> events)
        {
            var cycleEvents = events.GetCycleEventsWithTimeExtension(phaseDetail.PhaseNumber, phaseDetail.UseOverlap, options.Start, options.End);
            var pedEvents = events.GetPedEvents(options.Start, options.End, phaseDetail.Approach);
            if (pedEvents.IsNullOrEmpty())
                return null;
            var pedPhaseData = pedPhaseService.GetPedPhaseData(
                options,
                phaseDetail.Approach,
                planEvents.ToList(),
                pedEvents.ToList());

            var cycles = cycleService.GetRedToRedCycles(
                options.Start,
                options.End,
                cycleEvents.ToList());

            PedDelayResult viewModel = pedDelayService.GetChartData(
                options,
                pedPhaseData,
                cycles
                );
            viewModel.SignalDescription = phaseDetail.Approach.Location.SignalDescription();
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
