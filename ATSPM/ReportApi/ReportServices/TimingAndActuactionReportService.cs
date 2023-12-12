using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.TimingAndActuation;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Timing and actuation report service
    /// </summary>
    public class TimingAndActuactionReportService : ReportServiceBase<TimingAndActuationsOptions, IEnumerable<TimingAndActuationsForPhaseResult>>
    {
        private readonly TimingAndActuationsForPhaseService timingAndActuationsForPhaseService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ISignalRepository signalRepository;
        private readonly PhaseService phaseService;

        /// <inheritdoc/>
        public TimingAndActuactionReportService(
            TimingAndActuationsForPhaseService timingAndActuationsForPhaseService,
            IControllerEventLogRepository controllerEventLogRepository,
            ISignalRepository signalRepository,
            PhaseService phaseService
            )
        {
            this.timingAndActuationsForPhaseService = timingAndActuationsForPhaseService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<TimingAndActuationsForPhaseResult>> ExecuteAsync(TimingAndActuationsOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(parameter.SignalIdentifier, parameter.Start);

            if (signal == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<TimingAndActuationsForPhaseResult>>(new NullReferenceException("Signal not found"));
            }

            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for signal");
                return await Task.FromException<IEnumerable<TimingAndActuationsForPhaseResult>>(new NullReferenceException("No Controller Event Logs found for signal"));
            }

            var phaseDetails = phaseService.GetPhases(signal);
            var tasks = new List<Task<TimingAndActuationsForPhaseResult>>();

            foreach (var phase in phaseDetails)
            {
                var eventCodes = new List<int> { };
                if (parameter.ShowAdvancedCount || parameter.ShowAdvancedDilemmaZone || parameter.ShowLaneByLaneCount || parameter.ShowStopBarPresence)
                    eventCodes.AddRange(new List<int> { 81, 82 });
                if (parameter.ShowPedestrianActuation)
                    eventCodes.AddRange(new List<int> { 89, 90 });
                if (parameter.ShowPedestrianIntervals)
                    eventCodes.AddRange(timingAndActuationsForPhaseService.GetPedestrianIntervalEventCodes(phase.Approach.IsPedestrianPhaseOverlap));
                if (parameter.PhaseEventCodesList != null)
                    eventCodes.AddRange(parameter.PhaseEventCodesList);
                tasks.Add(GetChartDataForPhase(parameter, controllerEventLogs, phase, eventCodes, false));
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

        private async Task<TimingAndActuationsForPhaseResult> GetChartDataForPhase(
            TimingAndActuationsOptions options,
            List<ControllerEventLog> controllerEventLogs,
            PhaseDetail phaseDetail,
            List<int> eventCodes,
            bool usePermissivePhase)
        {
            eventCodes.AddRange(timingAndActuationsForPhaseService.GetCycleCodes(phaseDetail.UseOverlap));
            var approachevents = controllerEventLogs.GetEventsByEventCodes(
                options.Start,
                options.End,
                eventCodes).ToList();
            var viewModel = timingAndActuationsForPhaseService.GetChartData(options, phaseDetail, approachevents, usePermissivePhase);
            viewModel.SignalDescription = phaseDetail.Approach.Location.SignalDescription();
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
