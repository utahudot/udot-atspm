using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.Application.Business;
using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.PedDelay;
using ATSPM.Application.TempExtensions;
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
        private readonly ILocationRepository LocationRepository;
        private readonly PhaseService phaseService;

        /// <inheritdoc/>
        public PedDelayReportService(
            PedDelayService pedDelayService,
            PedPhaseService pedPhaseService,
            CycleService cycleService,
            IControllerEventLogRepository controllerEventLogRepository,
            ILocationRepository LocationRepository,
            PhaseService phaseService)
        {
            this.pedDelayService = pedDelayService;
            this.pedPhaseService = pedPhaseService;
            this.cycleService = cycleService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationRepository = LocationRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<PedDelayResult>> ExecuteAsync(PedDelayOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.locationIdentifier, parameter.Start);
            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<PedDelayResult>>(new NullReferenceException("Location not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetLocationEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<IEnumerable<PedDelayResult>>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(Location);
            var tasks = new List<Task<PedDelayResult>>();
            foreach (var phase in phaseDetails)
            {
                tasks.Add(GetChartDataForApproach(parameter, phase, planEvents, controllerEventLogs));
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
            viewModel.LocationDescription = phaseDetail.Approach.Location.LocationDescription();
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
