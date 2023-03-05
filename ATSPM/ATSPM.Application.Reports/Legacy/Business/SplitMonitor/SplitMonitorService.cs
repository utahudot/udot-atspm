using ATSPM.Application.Reports.Business.Common;
using ATSPM.Application.Reports.Business.PhaseTermination;
using ATSPM.Application.Reports.Business.SplitMonitor;
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
    
    public class SplitMonitorService
    {
        private readonly AnalysisPhaseCollectionService analysisPhaseCollectionService;
        private readonly PlanSplitMonitorService planSplitMonitorService;

        public SplitMonitorService(AnalysisPhaseCollectionService analysisPhaseCollectionService, PlanSplitMonitorService planSplitMonitorService)
        {
            this.analysisPhaseCollectionService = analysisPhaseCollectionService;
            this.planSplitMonitorService = planSplitMonitorService;
        }

        //public SplitMonitorResult GetChartData(SplitMonitorOptions options)
        //{
        //    var splitMonitorData = new SplitMonitorResult();
        //    splitMonitorData.SignalId = options.SignalId;
        //    splitMonitorData.Start = options.StartDate;
        //    splitMonitorData.End = options.EndDate;
        //    var phases = analysisPhaseCollectionService.GetAnalysisPhaseCollectionData(splitMonitorData.SignalID, splitMonitorData.StartDate, splitMonitorData.EndDate);
        //    if (phases.Plans.Count > 0)
        //    {
        //        foreach (var plan in phases.Plans)
        //        {
        //            planSplitMonitorService.SetProgrammedSplits(splitMonitorData.SignalId, plan);
        //            planSplitMonitorService.SetHighCycleCount(phases, plan);
        //        }
        //    }
            
        //    foreach (var phase in phases.AnalysisPhases)
        //    {
        //        if (phase.Cycles.Items.Count > 0)
        //        {
        //            var maxSplitLength = 0;
        //            foreach (var plan in phases.Plans)
        //            {
        //                var highestSplit = planSplitMonitorService.FindHighestRecordedSplitPhase(plan);
        //                planSplitMonitorService.FillMissingSplits(highestSplit, plan);
        //                if (plan.Splits[phase.PhaseNumber] > maxSplitLength)
        //                    maxSplitLength = plan.Splits[phase.PhaseNumber];
        //            }
        //        }
        //    }
        //    splitMonitorData
        //    return splitMonitorData;
        //}
    }
}
