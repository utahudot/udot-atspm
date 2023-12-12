using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.ArrivalOnRed;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Arrival on red report service
    /// </summary>
    public class ArrivalOnRedReportService : ReportServiceBase<ArrivalOnRedOptions, IEnumerable<ArrivalOnRedResult>>
    {
        private readonly ArrivalOnRedService arrivalOnRedService;
        private readonly SignalPhaseService signalPhaseService;
        private readonly ILocationRepository signalRepository;
        private readonly PhaseService phaseService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        /// <inheritdoc/>
        public ArrivalOnRedReportService(
            ArrivalOnRedService arrivalOnRedService,
            SignalPhaseService signalPhaseService,
            IControllerEventLogRepository controllerEventLogRepository,
            ILocationRepository signalRepository,
            PhaseService phaseService
            )
        {
            this.arrivalOnRedService = arrivalOnRedService;
            this.signalPhaseService = signalPhaseService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.signalRepository = signalRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<ArrivalOnRedResult>> ExecuteAsync(ArrivalOnRedOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var signal = signalRepository.GetLatestVersionOfSignal(parameter.locationIdentifier, parameter.Start);
            if (signal == null)
            {
                //return BadRequest("Location not found");

                return await Task.FromException<IEnumerable<ArrivalOnRedResult>>(new NullReferenceException("Signal not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetSignalEventsBetweenDates(signal.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for signal");

                return await Task.FromException<IEnumerable<ArrivalOnRedResult>>(new NullReferenceException("No Controller Event Logs found for signal"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
            parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var phaseDetails = phaseService.GetPhases(signal);
            var tasks = new List<Task<ArrivalOnRedResult>>();
            foreach (var phase in phaseDetails)
            {
                tasks.Add(
                   GetChartDataByApproach(parameter, phase, controllerEventLogs, planEvents, signal.SignalDescription())
                );
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

        private async Task<ArrivalOnRedResult> GetChartDataByApproach(
            ArrivalOnRedOptions options,
            PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs,
            List<ControllerEventLog> planEvents,
            string signalDescription)
        {
            var signalPhase = await signalPhaseService.GetSignalPhaseData(
                phaseDetail,
                options.Start,
                options.End,
                options.SelectedBinSize,
                null,
                controllerEventLogs,
                planEvents,
                false
                );
            if (signalPhase == null)
            {
                return null;
            }
            ArrivalOnRedResult viewModel = arrivalOnRedService.GetChartData(options, signalPhase, phaseDetail.Approach);
            viewModel.SignalDescription = signalDescription;
            return viewModel;
        }
    }
}
