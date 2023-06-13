using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.Common
{
    public class PlanSplitMonitorData : Plan
    {
        public PlanSplitMonitorData(DateTime start, DateTime end, int planNumber) : base(planNumber.ToString(), start, end)
        {
            Splits = new SortedDictionary<int, int>();
        }

        public SortedDictionary<int, int> Splits { get; set; }
        public int CycleLength { get; set; }
        public int OffsetLength { get; set; }
        public int CycleCount { get; set; }
    }

    public class PlanSplitMonitorService
    {
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public PlanSplitMonitorService(IControllerEventLogRepository controllerEventLogRepository)
        {

            this.controllerEventLogRepository = controllerEventLogRepository;
        }

        public PlanSplitMonitorData GetPlanSplitMonitor(DateTime start, DateTime end, int planNumber)
        {
            return new PlanSplitMonitorData(start, end, planNumber);
        }

        public void SetProgrammedSplits(string signalId, PlanSplitMonitorData plan)
        {
            plan.Splits.Clear();
            var l = new List<int>();
            for (var i = 130; i <= 151; i++)
                l.Add(i);
            var splitsDt = controllerEventLogRepository.GetSignalEventsByEventCodes(signalId, plan.StartTime, plan.StartTime.AddSeconds(2), l);
            foreach (var row in splitsDt)
            {
                if (row.EventCode == 132)
                    plan.CycleLength = row.EventParam;

                if (row.EventCode == 133)
                    plan.OffsetLength = row.EventParam;

                if (row.EventCode == 134 && !plan.Splits.ContainsKey(1))
                    plan.Splits.Add(1, row.EventParam);
                else if (row.EventCode == 134 && row.EventParam > 0)
                    plan.Splits[1] = row.EventParam;

                if (row.EventCode == 135 && !plan.Splits.ContainsKey(2))
                    plan.Splits.Add(2, row.EventParam);
                else if (row.EventCode == 135 && row.EventParam > 0)
                    plan.Splits[2] = row.EventParam;

                if (row.EventCode == 136 && !plan.Splits.ContainsKey(3))
                    plan.Splits.Add(3, row.EventParam);
                else if (row.EventCode == 136 && row.EventParam > 0)
                    plan.Splits[3] = row.EventParam;

                if (row.EventCode == 137 && !plan.Splits.ContainsKey(4))
                    plan.Splits.Add(4, row.EventParam);
                else if (row.EventCode == 137 && row.EventParam > 0)
                    plan.Splits[4] = row.EventParam;

                if (row.EventCode == 138 && !plan.Splits.ContainsKey(5))
                    plan.Splits.Add(5, row.EventParam);
                else if (row.EventCode == 138 && row.EventParam > 0)
                    plan.Splits[5] = row.EventParam;

                if (row.EventCode == 139 && !plan.Splits.ContainsKey(6))
                    plan.Splits.Add(6, row.EventParam);
                else if (row.EventCode == 139 && row.EventParam > 0)
                    plan.Splits[6] = row.EventParam;

                if (row.EventCode == 140 && !plan.Splits.ContainsKey(7))
                    plan.Splits.Add(7, row.EventParam);
                else if (row.EventCode == 140 && row.EventParam > 0)
                    plan.Splits[7] = row.EventParam;

                if (row.EventCode == 141 && !plan.Splits.ContainsKey(8))
                    plan.Splits.Add(8, row.EventParam);
                else if (row.EventCode == 141 && row.EventParam > 0)
                    plan.Splits[8] = row.EventParam;

                if (row.EventCode == 142 && !plan.Splits.ContainsKey(9))
                    plan.Splits.Add(9, row.EventParam);
                else if (row.EventCode == 142 && row.EventParam > 0)
                    plan.Splits[9] = row.EventParam;

                if (row.EventCode == 143 && !plan.Splits.ContainsKey(10))
                    plan.Splits.Add(10, row.EventParam);
                else if (row.EventCode == 143 && row.EventParam > 0)
                    plan.Splits[10] = row.EventParam;

                if (row.EventCode == 144 && !plan.Splits.ContainsKey(11))
                    plan.Splits.Add(11, row.EventParam);
                else if (row.EventCode == 144 && row.EventParam > 0)
                    plan.Splits[11] = row.EventParam;

                if (row.EventCode == 145 && !plan.Splits.ContainsKey(12))
                    plan.Splits.Add(12, row.EventParam);
                else if (row.EventCode == 145 && row.EventParam > 0)
                    plan.Splits[12] = row.EventParam;

                if (row.EventCode == 146 && !plan.Splits.ContainsKey(13))
                    plan.Splits.Add(13, row.EventParam);
                else if (row.EventCode == 146 && row.EventParam > 0)
                    plan.Splits[13] = row.EventParam;

                if (row.EventCode == 147 && !plan.Splits.ContainsKey(14))
                    plan.Splits.Add(14, row.EventParam);
                else if (row.EventCode == 147 && row.EventParam > 0)
                    plan.Splits[14] = row.EventParam;

                if (row.EventCode == 148 && !plan.Splits.ContainsKey(15))
                    plan.Splits.Add(15, row.EventParam);
                else if (row.EventCode == 148 && row.EventParam > 0)
                    plan.Splits[15] = row.EventParam;

                if (row.EventCode == 149 && !plan.Splits.ContainsKey(16))
                    plan.Splits.Add(16, row.EventParam);
                else if (row.EventCode == 149 && row.EventParam > 0)
                    plan.Splits[16] = row.EventParam;
            }

            if (plan.Splits.Count == 0)
                for (var i = 0; i < 16; i++)
                    plan.Splits.Add(i, 0);
        }

        public int FindHighestRecordedSplitPhase(PlanSplitMonitorData planSplitMonitorData)
        {
            var phase = 0;
            var maxkey = planSplitMonitorData.Splits.Max(x => x.Key);
            phase = maxkey;
            return phase;
        }

        public void FillMissingSplits(int highestSplit, PlanSplitMonitorData planSplitMonitorData)
        {
            for (var counter = 0; counter < highestSplit + 1; counter++)
                if (planSplitMonitorData.Splits.ContainsKey(counter))
                {
                }
                else
                {
                    planSplitMonitorData.Splits.Add(counter, 0);
                }
        }

        public void SetHighCycleCount(AnalysisPhaseCollectionData phases, PlanSplitMonitorData planSplitMonitorData)
        {
            //find all the phases cycles within the plan
            var HighCycleCount = 0;
            foreach (var phase in phases.AnalysisPhases)
            {
                var Cycles = from cycle in phase.Cycles.Items
                             where cycle.StartTime > planSplitMonitorData.StartTime && cycle.EndTime < planSplitMonitorData.EndTime
                             select cycle;

                if (Cycles.Count() > HighCycleCount)
                    HighCycleCount = Cycles.Count();
            }
            planSplitMonitorData.CycleCount = HighCycleCount;
        }
    }
}