using ATSPM.Application.Reports.Business.PhaseTermination;
using System;
using System.Linq;

namespace Legacy.Common.Business.SplitMonitor
{
    public class SplitMonitorData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AnalysisPhaseCollectionData Phases { get; set; }
        public string SignalID { get; set; }
    }
    
    public class SplitMonitor
    {
        private readonly AnalysisPhaseCollectionService analysisPhaseCollectionService;
        private readonly PlanSplitMonitorService planSplitMonitorService;

        public SplitMonitor(AnalysisPhaseCollectionService analysisPhaseCollectionService, PlanSplitMonitorService planSplitMonitorService)
        {
            this.analysisPhaseCollectionService = analysisPhaseCollectionService;
            this.planSplitMonitorService = planSplitMonitorService;
        }

        public SplitMonitorData GetSplitMonitor(string signalID, DateTime startDate, DateTime endDate)
        {
            var splitMonitorData = new SplitMonitorData();
            splitMonitorData.SignalID = signalID;
            splitMonitorData.StartDate = startDate;
            splitMonitorData.EndDate = endDate;
            splitMonitorData.Phases = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(splitMonitorData.SignalID, splitMonitorData.StartDate, splitMonitorData.EndDate);
            if (splitMonitorData.Phases.Plans.Count > 0)
            {
                foreach (var plan in splitMonitorData.Phases.Plans)
                {
                    planSplitMonitorService.SetProgrammedSplits(splitMonitorData.SignalID, plan);
                    planSplitMonitorService.SetHighCycleCount(splitMonitorData.Phases, plan);
                }
            }
            
            foreach (var phase in splitMonitorData.Phases.AnalysisPhases)
            {
                if (phase.Cycles.Items.Count > 0)
                {
                    var maxSplitLength = 0;
                    foreach (var plan in splitMonitorData.Phases.Plans)
                    {
                        var highestSplit = planSplitMonitorService.FindHighestRecordedSplitPhase(plan);
                        planSplitMonitorService.FillMissingSplits(highestSplit, plan);
                        if (plan.Splits[phase.PhaseNumber] > maxSplitLength)
                            maxSplitLength = plan.Splits[phase.PhaseNumber];
                    }
                }
            }
            return splitMonitorData;
        }
    }
}
