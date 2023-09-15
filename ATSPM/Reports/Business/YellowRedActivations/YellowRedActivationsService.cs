using ATSPM.Application.Reports.Business.Common;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.YellowRedActivations
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
            Approach approach,
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
                approach,
                options.SevereLevelSeconds,
                planEvents).ToList();

            var detectorActivations = cycles.SelectMany(c => c.DetectorActivations).ToList();

            return new YellowRedActivationsResult(
                approach.Signal.SignalIdentifier,
                approach.Id,
                approach.Description,
                options.UsePermissivePhase ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber,
                options.Start,
                options.End,
                Convert.ToInt32(plans.Sum(p => p.Violations)),
                Convert.ToInt32(plans.Sum(p => p.SevereRedLightViolations)),
                Convert.ToInt32(plans.Sum(p => p.YellowOccurrences)),
                plans.Select(p => new YellowRedActivationsPlan(
                    p.PlanNumber.ToString(),
                    p.StartTime,
                    p.EndTime,
                    Convert.ToInt32(p.Violations),
                    Convert.ToInt32(p.SevereRedLightViolations),
                    p.PercentViolations,
                    p.PercentSevereViolations,
                    p.AverageTRLV)).ToList(),
                cycles.Select(c => new YellowRedActivationEvent(c.RedEvent, c.RedBeginY)).ToList(),
                cycles.Select(c => new YellowRedActivationEvent(c.YellowClearanceEvent, c.YellowClearanceBeginY)).ToList(),
                cycles.Select(c => new YellowRedActivationEvent(c.RedClearanceEvent, c.RedClearanceBeginY)).ToList(),
                detectorActivations.Select(d => new YellowRedActivationEvent(d.TimeStamp, d.YPoint)).ToList()
                );
        }
    }
}