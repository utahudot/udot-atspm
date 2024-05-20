using ATSPM.Application.Business;
using ATSPM.Application.Business.Common;
using ATSPM.Application.Business.TimingAndActuation;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.Repositories.EventLogRepositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Extensions;
using System.ComponentModel.DataAnnotations;

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
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.LocationIdentifier, parameter.Start);

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
                var eventCodes = new List<short> { };
                eventCodes.AddRange(new List<short> { 81, 82, 89, 90 });
                eventCodes.AddRange(timingAndActuationsForPhaseService.GetPedestrianIntervalEventCodes(phase.Approach.IsPedestrianPhaseOverlap));
                if (parameter.PhaseEventCodesList != null)
                    eventCodes.AddRange(parameter.PhaseEventCodesList);
                tasks.Add(GetChartDataForPhase(parameter, controllerEventLogs, phase, eventCodes, phase.IsPermissivePhase));
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
            List<short> eventCodes,
            bool usePermissivePhase)
        {
            eventCodes.AddRange(timingAndActuationsForPhaseService.GetCycleCodes(phaseDetail.UseOverlap));
            var approachevents = controllerEventLogs.GetEventsByEventCodes(
                options.Start.AddMinutes(-15),
                options.End.AddMinutes(15),
                eventCodes).ToList();
            var viewModel = timingAndActuationsForPhaseService.GetChartData(options, phaseDetail, approachevents, usePermissivePhase);
            viewModel.LocationDescription = phaseDetail.Approach.Location.LocationDescription();
            string approachDescription = GetApproachDescription(phaseDetail);
            viewModel.ApproachDescription = approachDescription;
            return viewModel;
        }

        private static string GetApproachDescription(PhaseDetail phaseDetail)
        {
            DirectionTypes direction = phaseDetail.Approach.DirectionTypeId;
            string directionTypeName = direction.GetAttributeOfType<DisplayAttribute>().Name;
            MovementTypes movementType = phaseDetail.Approach.Detectors.ToList()[0].MovementType;
            string movementTypeName = movementType.GetAttributeOfType<DisplayAttribute>().Name;
            string approachDescription = $"{directionTypeName} {movementTypeName} Ph{phaseDetail.PhaseNumber}";
            return approachDescription;
        }
    }
}
