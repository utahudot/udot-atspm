using ATSPM.Application.Repositories;
using ATSPM.ReportApi.Business;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.PhaseTermination;
using ATSPM.ReportApi.TempExtensions;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Purdue phase termination report service
    /// </summary>
    public class PurduePhaseTerminationReportService : ReportServiceBase<PurduePhaseTerminationOptions, PhaseTerminationResult>
    {
        private readonly AnalysisPhaseCollectionService analysisPhaseCollectionService;
        private readonly IControllerEventLogRepository controllerEventLogRepository;
        private readonly ILocationRepository LocationRepository;

        /// <inheritdoc/>
        public PurduePhaseTerminationReportService(
            AnalysisPhaseCollectionService analysisPhaseCollectionService,
            IControllerEventLogRepository controllerEventLogRepository,
            ILocationRepository LocationRepository)
        {
            this.analysisPhaseCollectionService = analysisPhaseCollectionService;
            this.controllerEventLogRepository = controllerEventLogRepository;
            this.LocationRepository = LocationRepository;
        }

        /// <inheritdoc/>
        public override async Task<PhaseTerminationResult> ExecuteAsync(PurduePhaseTerminationOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var Location = LocationRepository.GetLatestVersionOfLocation(parameter.locationIdentifier, parameter.Start);
            if (Location == null)
            {
                //return BadRequest("Location not found");
                return await Task.FromException<PhaseTerminationResult>(new NullReferenceException("Location not found"));
            }
            var controllerEventLogs = controllerEventLogRepository.GetLocationEventsBetweenDates(Location.LocationIdentifier, parameter.Start.AddHours(-12), parameter.End.AddHours(12)).ToList();
            if (controllerEventLogs.IsNullOrEmpty())
            {
                //return Ok("No Controller Event Logs found for Location");
                return await Task.FromException<PhaseTerminationResult>(new NullReferenceException("No Controller Event Logs found for Location"));
            }

            var planEvents = controllerEventLogs.GetPlanEvents(
                parameter.Start.AddHours(-12),
                parameter.End.AddHours(12)).ToList();
            var terminationEvents = controllerEventLogs.Where(e =>
                new List<int> { 4, 5, 6, 7 }.Contains(e.EventCode)
                && e.Timestamp >= parameter.Start
                && e.Timestamp <= parameter.End).ToList();
            var pedEvents = controllerEventLogs.Where(e =>
                new List<int> { 21, 23 }.Contains(e.EventCode)
                && e.Timestamp >= parameter.Start
                && e.Timestamp <= parameter.End).ToList();
            var cycleEvents = controllerEventLogs.Where(e =>
                new List<int> { 1, 11 }.Contains(e.EventCode)
                && e.Timestamp >= parameter.Start
                && e.Timestamp <= parameter.End).ToList();
            var splitsEventCodes = new List<int>();
            for (var i = 130; i <= 151; i++)
                splitsEventCodes.Add(i);
            var splitsEvents = controllerEventLogs.Where(e =>
                splitsEventCodes.Contains(e.EventCode)
                && e.Timestamp >= parameter.Start
                && e.Timestamp <= parameter.End).ToList();
            controllerEventLogs = null;
            GC.Collect();

            var phaseCollectionData = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(
                parameter.locationIdentifier,
                parameter.Start,
                parameter.End,
                planEvents,
                cycleEvents,
                splitsEvents,
                pedEvents,
                terminationEvents,
                Location,
                parameter.SelectedConsecutiveCount);
            var phases = new List<Phase>();
            foreach (var phase in phaseCollectionData.AnalysisPhases)
            {
                phases.Add(new Phase(
                    phase.PhaseNumber,
                    phase.ConsecutiveGapOuts.Select(g => g.Timestamp).ToList(),
                    phase.ConsecutiveMaxOut.Select(g => g.Timestamp).ToList(),
                    phase.ConsecutiveForceOff.Select(g => g.Timestamp).ToList(),
                    phase.PedestrianEvents.Select(g => g.Timestamp).ToList(),
                    phase.UnknownTermination.Select(g => g.Timestamp).ToList()
                    ));
            }

            var plans = phaseCollectionData.Plans.Select(p => new Plan(p.PlanNumber.ToString(), p.Start, p.End)).ToList();
            var result = new PhaseTerminationResult(
                phaseCollectionData.locationId,
                parameter.Start,
                parameter.End,
                parameter.SelectedConsecutiveCount,
                plans,
                phases
                );
            result.LocationDescription = Location.LocationDescription();
            //return Ok(result);

            return result;
        }
    }
}
