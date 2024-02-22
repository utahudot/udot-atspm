using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.TempExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.ReportApi.Business.SplitMonitor
{
    public class SplitMonitorData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AnalysisPhaseCollectionData Phases { get; set; }
        public string locationId { get; set; }
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


        public async Task<IEnumerable<SplitMonitorResult>> GetChartData(
            SplitMonitorOptions options,
            IReadOnlyList<ControllerEventLog> planEvents,
            IReadOnlyList<ControllerEventLog> cycleEvents,
            IReadOnlyList<ControllerEventLog> pedEvents,
            IReadOnlyList<ControllerEventLog> splitsEvents,
            IReadOnlyList<ControllerEventLog> terminationEvents,
            Location Location)
        {
            var phaseCollection = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(
                Location.LocationIdentifier,
                options.Start,
                options.End,
                planEvents,
                cycleEvents,
                splitsEvents,
                pedEvents,
                terminationEvents,
                Location,
                1
                );

            var tasks = new List<Task<SplitMonitorResult>>();
            foreach (var phase in phaseCollection.AnalysisPhases)
            {
                tasks.Add(GetChartDataForPhase(options, phaseCollection, phase));
            }

            var results = await Task.WhenAll(tasks);

            return results.Where(result => result != null).OrderBy(r => r.PhaseNumber);
        }

        private async Task<SplitMonitorResult> GetChartDataForPhase(SplitMonitorOptions options, AnalysisPhaseCollectionData phaseCollection, AnalysisPhaseData phase)
        {
            var plans = GetSplitMonitorPlansWithStatistics(options, phaseCollection, phase);
            var test = plans.SelectMany(p => p.Splits.Where(s => s.Key == phase.PhaseNumber));
            var splits = new List<DataPointForDouble>();
            foreach (var plan in plans)
            {
                var splitForPhase = plan.Splits.Where(s => s.Key == phase.PhaseNumber).FirstOrDefault();
                splits.Add(new DataPointForDouble(plan.Start, splitForPhase.Value));
            }

            var splitMonitorResult = new SplitMonitorResult(phase.PhaseNumber, phase.PhaseDescription, options.locationIdentifier, options.Start, options.End)
            {
                PercentileSplit = options.PercentileSplit,
                ProgrammedSplits = splits,
                GapOuts = phase.Cycles.Cycles
                                .Where(c => c.TerminationEvent == 4)
                                .Select(c => new DataPointForDouble(c.StartTime, c.Duration.TotalSeconds))
                                .ToList(),
                MaxOuts = phase.Cycles.Cycles
                                .Where(c => c.TerminationEvent == 5)
                                .Select(c => new DataPointForDouble(c.StartTime, c.Duration.TotalSeconds))
                                .ToList(),
                ForceOffs = phase.Cycles.Cycles
                                .Where(c => c.TerminationEvent == 6)
                                .Select(c => new DataPointForDouble(c.StartTime, c.Duration.TotalSeconds))
                                .ToList(),
                Unknowns = phase.Cycles.Cycles
                                .Where(c => c.TerminationEvent == 0)
                                .Select(c => new DataPointForDouble(c.StartTime, c.Duration.TotalSeconds))
                                .ToList(),
                Peds = phase.Cycles.Cycles
                                .Where(c => c.HasPed)
                                .Select(c => new DataPointForDouble(c.PedStartTime, c.PedDuration))
                                .ToList(),
                Plans = plans.Select(p => new PlanSplitMonitorDTO
                {
                    PlanNumber = p.PlanNumber,
                    PlanDescription = p.PlanDescription,
                    Start = p.Start,
                    End = p.End,
                    AverageSplit = p.AverageSplit,
                    PercentSkips = p.PercentSkips * 100,
                    PercentGapOuts = p.PercentGapOuts * 100,
                    PercentMaxOuts = p.PercentMaxOuts * 100,
                    PercentForceOffs = p.PercentForceOffs * 100,
                    PercentileSplit = p.PercentileSplit,

                }).ToList(),
                LocationDescription = phase.Location.LocationDescription()
            };

            return splitMonitorResult;
        }

        private List<PlanSplitMonitorData> GetSplitMonitorPlansWithStatistics(
            SplitMonitorOptions options,
            AnalysisPhaseCollectionData phaseCollection,
            AnalysisPhaseData phase)
        {
            var phasePlans = new List<PlanSplitMonitorData>();
            foreach (var plan in phaseCollection.Plans)
            {
                var cycles = phase.Cycles.Cycles.Where(x => x.StartTime >= plan.Start && x.EndTime < plan.End).ToList();
                if (cycles.Any())
                {
                    //var planCycleCount = Convert.ToDouble(cycles.Count());
                    var highCycleCount = plan.HighCycleCount;
                    double SkippedPhases = plan.HighCycleCount - cycles.Count();
                    var percentile = Convert.ToDouble(options.PercentileSplit) / 100;
                    phasePlans.Add(new PlanSplitMonitorData(plan.Start, plan.End, plan.PlanNumber)
                    {
                        Start = plan.Start,
                        End = plan.End,
                        PlanNumber = plan.PlanNumber,
                        PercentSkips = highCycleCount > 0 ? SkippedPhases / highCycleCount : 0,
                        PercentGapOuts = highCycleCount > 0 ? Convert.ToDouble(cycles.Count(c => c.TerminationEvent == 4)) / highCycleCount : 0,
                        PercentMaxOuts = GetPercentMaxOuts(cycles, highCycleCount, plan.PlanNumber),
                        PercentForceOffs = GetPercentForceOffs(cycles, highCycleCount, plan.PlanNumber),
                        AverageSplit = cycles.Count > 0 ? Convert.ToDouble(cycles.Sum(c => c.Duration.TotalSeconds)) / cycles.Count : 0,
                        PercentileSplit = GetPercentSplit(highCycleCount, percentile, cycles),
                        Splits = plan.Splits
                    });
                }
            }
            return phasePlans;
        }

        private static double GetPercentForceOffs(List<AnalysisPhaseCycle> cycles, double highCycleCounts, string planNumber)
        {
            if (planNumber != "254")
                return highCycleCounts > 0 ? Convert.ToDouble(cycles.Count(c => c.TerminationEvent == 6)) / highCycleCounts : 0;
            else
                return 0;
        }

        private static double GetPercentMaxOuts(List<AnalysisPhaseCycle> cycles, double highCycleCount, string planNumber)
        {
            if (planNumber == "254")
                return highCycleCount > 0 ? Convert.ToDouble(cycles.Count(c => c.TerminationEvent == 5)) / highCycleCount : 0;
            else
                return 0;
        }

        private double GetPercentSplit(double highCycleCount, double percentile, List<AnalysisPhaseCycle> cycles)
        {
            if (cycles.Count <= 2)
                return 0;
            var orderedCycles = cycles.OrderBy(c => c.Duration.TotalSeconds).ToList();

            var percentilIndex = percentile * orderedCycles.Count;
            if (percentilIndex % 1 == 0)
            {
                return orderedCycles.ElementAt(Convert.ToInt16(percentilIndex) - 1).Duration
                    .TotalSeconds;
            }
            else
            {
                var indexMod = percentilIndex % 1;
                //subtracting .5 leaves just the integer after the convert.
                //There was probably another way to do that, but this is easy.
                int indexInt = Convert.ToInt16(percentilIndex - .5);

                var step1 = orderedCycles.ElementAt(Convert.ToInt16(indexInt) - 1).Duration.TotalSeconds;
                var step2 = orderedCycles.ElementAt(Convert.ToInt16(indexInt)).Duration.TotalSeconds;
                var stepDiff = step2 - step1;
                var step3 = stepDiff * indexMod;
                return step1 + step3;
            }
        }
    }
}
