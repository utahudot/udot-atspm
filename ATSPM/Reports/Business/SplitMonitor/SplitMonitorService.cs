using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Parquet.Data.Rows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.SplitMonitor
{
    public class SplitMonitorData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AnalysisPhaseCollectionData Phases { get; set; }
        public string SignalID { get; set; }
    }

    public class SplitMonitorService
    {
        private readonly AnalysisPhaseService analysisPhaseService;
        private readonly PlanSplitMonitorService planSplitMonitorService;
        private readonly PlanService planService;

        public SplitMonitorService(
            AnalysisPhaseService analysisPhaseService,
            PlanSplitMonitorService planSplitMonitorService,
            PlanService planService
            )
        {
            this.analysisPhaseService = analysisPhaseService;
            this.planSplitMonitorService = planSplitMonitorService;
            this.planService = planService;
        }


        public SplitMonitorResult GetChartData(
            SplitMonitorOptions options,
            IReadOnlyList<ControllerEventLog> planEvents,
            IReadOnlyList<ControllerEventLog> phaseEvents,
            Signal signal)
        {
            var phase = analysisPhaseService.GetAnalysisPhaseData(options.PhaseNumber, signal, phaseEvents.ToList());
            var plans = planService.GetSplitMonitorPlans(options.Start, options.End, options.SignalId, planEvents.ToList());
            var maxSplitLength = 0;
            var programedSplits = new List<Split>();
            var gapOuts = new List<SplitMonitorGapOut>();
            var maxOuts = new List<SplitMonitorMaxOut>();
            var forceOffs = new List<SplitMonitorForceOff>();
            var unknowns = new List<SplitMonitorUnknown>();
            var peds = new List<Peds>();
            
            foreach (var plan in plans)
            {
                var highestSplit = planSplitMonitorService.FindHighestRecordedSplitPhase(plan);
                planSplitMonitorService.FillMissingSplits(highestSplit, plan);
                if (plan.Splits[phase.PhaseNumber] > maxSplitLength)
                    maxSplitLength = plan.Splits[phase.PhaseNumber];
                programedSplits.Add(new Split(plan.StartTime, plan.Splits[phase.PhaseNumber]));
                programedSplits.Add(new Split(plan.EndTime, plan.Splits[phase.PhaseNumber]));
            }
            foreach (var cycle in phase.Cycles.Items)
            {
                if (cycle.TerminationEvent == 4)
                    gapOuts.Add(new SplitMonitorGapOut(cycle.StartTime, cycle.Duration.TotalSeconds));
                if (cycle.TerminationEvent == 5)
                    maxOuts.Add(new SplitMonitorMaxOut(cycle.StartTime, cycle.Duration.TotalSeconds));
                if (cycle.TerminationEvent == 6)
                    forceOffs.Add(new SplitMonitorForceOff(cycle.StartTime, cycle.Duration.TotalSeconds));
                if (cycle.TerminationEvent == 0)
                    unknowns.Add(new SplitMonitorUnknown(cycle.StartTime, cycle.Duration.TotalSeconds));
                if (cycle.HasPed && options.ShowPedActivity)
                {
                    if (cycle.PedDuration == 0)
                    {
                        if (cycle.PedStartTime == DateTime.MinValue)
                            cycle.SetPedStart(cycle.StartTime);
                        if (cycle.PedEndTime == DateTime.MinValue)
                            cycle.SetPedEnd(cycle.YellowEvent);
                    }
                    peds.Add(new Peds(cycle.PedStartTime, cycle.PedDuration));
                }
            }                    

            return new SplitMonitorResult(
                options.ApproachId,
                options.SignalId,
                options.Start,
                options.End,
                options.PhaseNumber,
                plans.ToList(),
                programedSplits,
                gapOuts,
                maxOuts,
                forceOffs,
                unknowns,
                peds);
        }
    }
}
