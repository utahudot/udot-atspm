using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.PurdueCoordinationDiagram;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Purdue coordination diagram report service
    /// </summary>
    public class PurdueCoordinationDiagramReportService : ReportServiceBase<PurdueCoordinationDiagramOptions, IEnumerable<PurdueCoordinationDiagramResult>>
    {
        private readonly PurdueCoordinationDiagramService perdueCoordinationDiagramService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly SignalPhaseService signalPhaseService;
        private readonly ISignalRepository signalRepository;
        private readonly PhaseService phaseService;

        public PurdueCoordinationDiagramReportService(
            PurdueCoordinationDiagramService perdueCoordinationDiagramService,
            IControllerEventLogRepository controllerEventLogRepository,
            SignalPhaseService signalPhaseService,
            ISignalRepository signalRepository,
            PhaseService phaseService)
        {
            this.perdueCoordinationDiagramService = perdueCoordinationDiagramService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalPhaseService = signalPhaseService;
            this.signalRepository = signalRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<PurdueCoordinationDiagramResult>> ExecuteAsync(PurdueCoordinationDiagramOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(parameter.SignalIdentifier, parameter.Start);
            if (signal == null)
            {
                //return BadRequest("Signal not found");
                return await Task.FromException<IEnumerable<PurdueCoordinationDiagramResult>>(new NullReferenceException("Signal not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.SignalIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for signal");
                return await Task.FromException<IEnumerable<PurdueCoordinationDiagramResult>>(new NullReferenceException("No Controller Event Logs found for signal"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(signal);
            var tasks = new List<Task<PurdueCoordinationDiagramResult>>();
            foreach (var phase in phaseDetails)
            {
                tasks.Add(GetChartDataForApproach(parameter, phase, controllerEventLogs, planEvents));
            }

            var results = await Task.WhenAll(tasks);

            var finalResultcheck = results.Where(result => result != null).ToList();

            //if (finalResultcheck.IsNullOrEmpty())
            //{
            //    return Ok("No chart data found");
            //}
            //return Ok(finalResultcheck);

            return results;
        }

        private async Task<PurdueCoordinationDiagramResult> GetChartDataForApproach(
            PurdueCoordinationDiagramOptions options,
            PhaseDetail phaseDetail,
            IReadOnlyList<ControllerEventLog> controllerEventLogs,
            IReadOnlyList<ControllerEventLog> planEvents)
        {
            var signalPhase = await signalPhaseService.GetSignalPhaseData(
                phaseDetail,
                options.Start,
                options.End,
                options.SelectedBinSize,
                null,
                controllerEventLogs.ToList(),
                planEvents.ToList(),
                options.ShowVolumes);
            if (signalPhase == null)
            {
                return null;
            }
            PurdueCoordinationDiagramResult viewModel = perdueCoordinationDiagramService.GetChartData(options, phaseDetail.Approach, signalPhase);
            viewModel.SignalDescription = phaseDetail.Approach.Signal.SignalDescription();
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
