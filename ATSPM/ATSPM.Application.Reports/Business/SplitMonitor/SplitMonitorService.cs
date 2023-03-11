using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.PhaseTermination;
using System;
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
        private readonly AnalysisPhaseCollectionService analysisPhaseCollectionService;
        private readonly PlanSplitMonitorService planSplitMonitorService;

        public SplitMonitorService(AnalysisPhaseCollectionService analysisPhaseCollectionService)
        {
            this.analysisPhaseCollectionService = analysisPhaseCollectionService;
            planSplitMonitorService = planSplitMonitorService;
        }

        public SplitMonitorResult GetChartData(SplitMonitorOptions options)
        {
            var splitMonitorData = new SplitMonitorResult();
            splitMonitorData.SignalId = options.SignalId;
            splitMonitorData.Start = options.StartDate;
            splitMonitorData.End = options.EndDate;
            var phases = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(options.SignalId, options.StartDate, options.EndDate);
            if (phases.AnalysisPhases.Count > 0)
            {
                var phasesInOrder = phases.AnalysisPhases.Select(r => r).OrderBy(r => r.PhaseNumber);


                foreach (var phase in phasesInOrder)
                {
                    if (phase.Cycles.Items.Count > 0)
                    {
                        var maxSplitLength = 0;
                        foreach (var plan in phases.Plans)
                        {
                            var highestSplit = planSplitMonitorService.FindHighestRecordedSplitPhase(plan);
                            planSplitMonitorService.FillMissingSplits(highestSplit, plan);
                            if (plan.Splits[phase.PhaseNumber] > maxSplitLength)
                                maxSplitLength = plan.Splits[phase.PhaseNumber];
                            splitMonitorData.ProgramedSplits.Add(new Split(plan.StartTime, plan.Splits[phase.PhaseNumber]));
                            splitMonitorData.ProgramedSplits.Add(new Split(plan.EndTime, plan.Splits[phase.PhaseNumber]));
                        }
                        foreach (var cycle in phase.Cycles.Items)
                        {
                            if (cycle.TerminationEvent == 4)
                                splitMonitorData.GapOuts.Add(new SplitMonitorGapOut(cycle.StartTime, cycle.Duration.TotalSeconds));
                            if (cycle.TerminationEvent == 5)
                                splitMonitorData.MaxOuts.Add(new SplitMonitorMaxOut(cycle.StartTime, cycle.Duration.TotalSeconds));
                            if (cycle.TerminationEvent == 6)
                                splitMonitorData.ForceOffs.Add(new SplitMonitorForceOff(cycle.StartTime, cycle.Duration.TotalSeconds));
                            if (cycle.TerminationEvent == 0)
                                splitMonitorData.Unknowns.Add(new SplitMonitorUnknown(cycle.StartTime, cycle.Duration.TotalSeconds));
                            if (cycle.HasPed && options.ShowPedActivity)
                            {
                                if (cycle.PedDuration == 0)
                                {
                                    if (cycle.PedStartTime == DateTime.MinValue)
                                        cycle.SetPedStart(cycle.StartTime);
                                    if (cycle.PedEndTime == DateTime.MinValue)
                                        cycle.SetPedEnd(cycle.YellowEvent);
                                }
                                splitMonitorData.Peds.Add(new Peds(cycle.PedStartTime, cycle.PedDuration));
                            }
                        }
                    }


                }
            }
            return splitMonitorData;
        }
    }
}
