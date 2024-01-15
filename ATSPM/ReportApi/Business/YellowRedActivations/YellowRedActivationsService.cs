using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.PedDelay;

namespace ATSPM.ReportApi.Business.YellowRedActivations
{
    public class YellowRedActivationsService
    {
        private readonly PlanService planService;
        private readonly CycleService cycleService;

        public YellowRedActivationsService(
            PlanService planService,
            CycleService cycleService)
        {
            this.planService = planService;
            this.cycleService = cycleService;
        }


        public YellowRedActivationsResult GetChartData(
            YellowRedActivationsOptions options,
            PhaseDetail phaseDetail,
            IReadOnlyList<ControllerEventLog> cycleEvents,
            IReadOnlyList<ControllerEventLog> detectorEvents,
            IReadOnlyList<ControllerEventLog> planEvents)
        {

            var cycles = cycleService.GetYellowRedActivationsCycles(
                options.Start,
                options.End,
                cycleEvents,
                detectorEvents,
                options.SevereLevelSeconds
                );

            var plans = planService.GetYellowRedActivationPlans(
                options.Start,
                options.End,
                cycles,
                phaseDetail.Approach.Location.LocationIdentifier,
                options.SevereLevelSeconds,
                planEvents).ToList();

            var detectorActivations = cycles.SelectMany(c => c.DetectorActivations).ToList();

            var phaseType = phaseDetail.Approach.GetPhaseType().ToString();

            return new YellowRedActivationsResult(
                phaseDetail.Approach.Location.LocationIdentifier,
                phaseDetail.Approach.Id,
                phaseDetail.Approach.Description,
                phaseDetail.Approach.ProtectedPhaseNumber,
                phaseDetail.Approach.PermissivePhaseNumber,
                phaseDetail.IsPermissivePhase,
                phaseType,
                options.Start,
                options.End,
                Convert.ToInt32(plans.Sum(p => p.Violations)),
                Convert.ToInt32(plans.Sum(p => p.SevereRedLightViolations)),
                Convert.ToInt32(plans.Sum(p => p.YellowOccurrences)),
                plans.Select(p => new YellowRedActivationsPlan(
                    p.PlanNumber.ToString(),
                    p.Start,
                    p.End,
                    Convert.ToInt32(p.Violations),
                    Convert.ToInt32(p.SevereRedLightViolations),
                    p.PercentViolations,
                    p.PercentSevereViolations,
                    p.AverageTRLV)).ToList(),
                cycles.Select(c => new DataPointForDouble(c.RedEvent, c.EndTime)).ToList(),
                cycles.Select(c => new DataPointForDouble(c.StartTime, c.RedClearanceEvent)).ToList(),
                cycles.Select(c => new DataPointForDouble(c.RedClearanceEvent, c.RedEvent)).ToList(),
                detectorActivations.Select(d => new DataPointForDouble(d.TimeStamp, d.YPoint)).ToList()
                );
        }
    }
}