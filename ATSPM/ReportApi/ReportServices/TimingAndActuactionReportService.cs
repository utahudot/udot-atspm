using ATSPM.Application.Business;
using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.TimingAndActuation;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Timing and actuation report service
    /// </summary>
    public class TimingAndActuactionReportService : ReportServiceBase<TimingAndActuationsOptions, IEnumerable<TimingAndActuationsForPhaseResult>>
    {
        private readonly TimingAndActuationsForPhaseService timingAndActuationsForPhaseService;
        private readonly IIndianaEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository LocationRepository;
        private readonly PhaseService phaseService;

        /// <inheritdoc/>
        public TimingAndActuactionReportService(
            TimingAndActuationsForPhaseService timingAndActuationsForPhaseService,
            IIndianaEventLogRepository controllerEventLogRepository,
            ILocationRepository LocationRepository,
            PhaseService phaseService
            )
        {
            this.timingAndActuationsForPhaseService = timingAndActuationsForPhaseService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationRepository = LocationRepository;
            this.phaseService = phaseService;
        }

        /// <inheritdoc/>
        public override async Task<IEnumerable<TimingAndActuationsForPhaseResult>> ExecuteAsync(TimingAndActuationsOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.locationIdentifier, parameter.Start);

            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<IEnumerable<TimingAndActuationsForPhaseResult>>(new NullReferenceException("Location not found"));
            }

            var controllerEventLogs = controllerEventLogRepository.GetEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();

            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<IEnumerable<TimingAndActuationsForPhaseResult>>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var phaseDetails = phaseService.GetPhases(Location);
            var tasks = new List<Task<TimingAndActuationsForPhaseResult>>();

            foreach (var phase in phaseDetails)
            {
                var eventCodes = new List<DataLoggerEnum> { };
                if (parameter.ShowAdvancedCount || parameter.ShowAdvancedDilemmaZone || parameter.ShowLaneByLaneCount || parameter.ShowStopBarPresence)
                    eventCodes.AddRange(new List<DataLoggerEnum> { DataLoggerEnum.DetectorOff, DataLoggerEnum.DetectorOn });
                if (parameter.ShowPedestrianActuation)
                    eventCodes.AddRange(new List<DataLoggerEnum> { DataLoggerEnum.PedDetectorOff, DataLoggerEnum.PedDetectorOn });
                if (parameter.ShowPedestrianIntervals)
                    eventCodes.AddRange(timingAndActuationsForPhaseService.GetPedestrianIntervalEventCodes(phase.Approach.IsPedestrianPhaseOverlap));
                if (parameter.PhaseEventCodesList != null)
                    eventCodes.AddRange(parameter.PhaseEventCodesList.Select(e => (DataLoggerEnum)e));
                tasks.Add(GetChartDataForPhase(parameter, controllerEventLogs, phase, eventCodes, false));
            }
            var results = await Task.WhenAll(tasks);

            var finalResultcheck = results.Where(result => result != null).OrderBy(r => r.PhaseNumberSort).ToList();

            //if (finalResultcheck.IsNullOrEmpty())
            //{
            //    return Ok("No chart data found");
            //}
            //return Ok(finalResultcheck);

            return finalResultcheck;
        }

        private async Task<TimingAndActuationsForPhaseResult> GetChartDataForPhase(
            TimingAndActuationsOptions options,
            List<IndianaEvent> controllerEventLogs,
            PhaseDetail phaseDetail,
            List<DataLoggerEnum> eventCodes,
            bool usePermissivePhase)
        {
            eventCodes.AddRange(timingAndActuationsForPhaseService.GetCycleCodes(phaseDetail.UseOverlap));
            var approachevents = controllerEventLogs.GetEventsByEventCodes(
                options.Start.AddMinutes(-15),
                options.End.AddMinutes(15),
                eventCodes).ToList();
            var viewModel = timingAndActuationsForPhaseService.GetChartData(options, phaseDetail, approachevents, usePermissivePhase);
            viewModel.LocationDescription = phaseDetail.Approach.Location.LocationDescription();
            viewModel.ApproachDescription = phaseDetail.Approach.Description;
            return viewModel;
        }
    }
}
