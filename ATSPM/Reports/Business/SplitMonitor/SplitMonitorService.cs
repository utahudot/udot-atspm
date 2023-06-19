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
        private readonly AnalysisPhaseCollectionService analysisPhaseCollectionService;
        private readonly PlanService planService;

        public SplitMonitorService(
            AnalysisPhaseService analysisPhaseService,
            AnalysisPhaseCollectionService analysisPhaseCollectionService,
            PlanService planService
            )
        {
            this.analysisPhaseService = analysisPhaseService;
            this.analysisPhaseCollectionService = analysisPhaseCollectionService;
            this.planService = planService;
        }


        public List<SplitMonitorResult> GetChartData(
            SplitMonitorOptions options,
            IReadOnlyList<ControllerEventLog> planEvents,
            IReadOnlyList<ControllerEventLog> cycleEvents,
            IReadOnlyList<ControllerEventLog> pedEvents,
            IReadOnlyList<ControllerEventLog> splitsEvents,
            Signal signal)
        {
            var phaseCollection = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(
                signal.SignalId,
                options.Start, 
                options.End, 
                planEvents,
                cycleEvents,
                splitsEvents,
                pedEvents,
                signal,
                1
                );
            
            var result = new List<SplitMonitorResult>();
            foreach (var phase in phaseCollection.AnalysisPhases)
            {
                var splitMonitorResult = new SplitMonitorResult(phase.PhaseNumber, options.SignalId, options.Start, options.End)
                {
                    GapOuts = phase.Cycles.Items
                    .Where(c => c.TerminationEvent == 4)
                    .Select(c => new SplitMonitorEvent(c.StartTime, c.Duration.TotalSeconds))
                    .ToList(),
                    MaxOuts = phase.Cycles.Items
                    .Where(c => c.TerminationEvent == 5)
                    .Select(c => new SplitMonitorEvent(c.StartTime, c.Duration.TotalSeconds))
                    .ToList(),
                    ForceOffs = phase.Cycles.Items
                    .Where(c => c.TerminationEvent == 6)
                    .Select(c => new SplitMonitorEvent(c.StartTime, c.Duration.TotalSeconds))
                    .ToList(),
                    Unknowns = phase.Cycles.Items
                    .Where(c => c.TerminationEvent == 0)
                    .Select(c => new SplitMonitorEvent(c.StartTime, c.Duration.TotalSeconds))
                    .ToList(),
                    Peds = phase.Cycles.Items
                    .Where(c => c.HasPed)
                    .Select(c => new SplitMonitorEvent(c.StartTime, c.Duration.TotalSeconds))
                    .ToList(),
                };                
                splitMonitorResult.Plans = GetSplitMonitorPlansWithStatistics(options, phaseCollection, phase);
                result.Add(splitMonitorResult);
            }

            return result;
        }

        private List<PlanSplitMonitorData> GetSplitMonitorPlansWithStatistics(
            SplitMonitorOptions options, 
            AnalysisPhaseCollectionData phaseCollection, 
            AnalysisPhaseData phase)
        {
            var phasePlans = new List<PlanSplitMonitorData>();
            foreach (var plan in phaseCollection.Plans)
            {
                var cycles = phase.Cycles.Items.Where(x => x.StartTime >= plan.StartTime && x.StartTime < plan.EndTime).ToList();
                if (cycles.Any())
                {
                    var planCycleCount = Convert.ToDouble(cycles.Count());
                    var percentile = Convert.ToDouble(options.PercentileSplit) / 100;
                    phasePlans.Add(new PlanSplitMonitorData(plan.StartTime, plan.EndTime, plan.PlanNumber)
                    {
                        StartTime = plan.StartTime,
                        EndTime = plan.EndTime,
                        PlanNumber = plan.PlanNumber,
                        PercentSkips = planCycleCount > 0 ? Convert.ToDouble((plan.HighCycleCount) - planCycleCount) / plan.HighCycleCount : 0,
                        PercentGapOuts = planCycleCount > 0 ? Convert.ToDouble(cycles.Count(c => c.TerminationEvent == 4)) / planCycleCount : 0,
                        PercentMaxOuts = GetPercentMaxOuts(cycles, planCycleCount, plan.PlanNumber),
                        PercentForceOffs = GetPercentForceOffs(cycles, planCycleCount, plan.PlanNumber),
                        AverageSplit = planCycleCount > 0 ? Convert.ToDouble(cycles.Sum(c => c.Duration.TotalSeconds)) / planCycleCount : 0,
                        PercentileSplit = GetPercentSplit(planCycleCount, percentile, cycles),
                        Splits = plan.Splits
                    });
                }
            }
            return phasePlans;
        }

        private static double GetPercentForceOffs(List<AnalysisPhaseCycle> cycles, double planCycleCount, string planNumber)
        {
            if(planNumber != "254")
                return planCycleCount > 0 ? Convert.ToDouble(cycles.Count(c => c.TerminationEvent == 6)) / planCycleCount : 0;
            else
                return 0;
        }

        private static double GetPercentMaxOuts(List<AnalysisPhaseCycle> cycles, double planCycleCount, string planNumber)
        {
            if (planNumber == "254")
                return planCycleCount > 0 ? Convert.ToDouble(cycles.Count(c => c.TerminationEvent == 5)) / planCycleCount : 0;
            else
                return 0;
        }

        private double GetPercentSplit(double planCycleCount, double percentile, List<AnalysisPhaseCycle> cycles)
        {
            if (cycles.Count <= 2)
                return 0;

            var percentilIndex = percentile * planCycleCount;
            if (percentilIndex % 1 == 0)
            {
                return cycles.ElementAt(Convert.ToInt16(percentilIndex) - 1).Duration
                    .TotalSeconds;
            }
            else
            {
                var indexMod = percentilIndex % 1;
                //subtracting .5 leaves just the integer after the convert.
                //There was probably another way to do that, but this is easy.
                int indexInt = Convert.ToInt16(percentilIndex - .5);

                var step1 = cycles.ElementAt(Convert.ToInt16(indexInt) - 1).Duration.TotalSeconds;
                var step2 = cycles.ElementAt(Convert.ToInt16(indexInt)).Duration.TotalSeconds;
                var stepDiff = step2 - step1;
                var step3 = stepDiff * indexMod;
                return step1 + step3;
            }
        }
    }
}
