﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.Common/AnalysisPhaseCollectionService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.IdentityModel.Tokens;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Business.Common
{
    public class AnalysisPhaseCollectionData
    {
        public AnalysisPhaseCollectionData()
        {
            Plans = new List<PlanSplitMonitorData>();
            AnalysisPhases = new List<AnalysisPhaseData>();
        }

        public IReadOnlyList<PlanSplitMonitorData> Plans { get; set; }
        public int MaxPhaseInUse { get; set; }
        public string locationId { get; set; }
        public List<AnalysisPhaseData> AnalysisPhases { get; set; }
        public Location Location { get; internal set; }
    }

    public class AnalysisPhaseCollectionService
    {
        private readonly PlanService planService;
        private readonly AnalysisPhaseService analysisPhaseService;

        public AnalysisPhaseCollectionService(
            PlanService planService,
            AnalysisPhaseService analysisPhaseService
            )
        {
            this.planService = planService;
            this.analysisPhaseService = analysisPhaseService;
        }

        public AnalysisPhaseCollectionData GetAnalysisPhaseCollectionData(
            string locationIdentifier,
            DateTime startTime,
            DateTime endTime,
            IReadOnlyList<IndianaEvent> planEvents,
            IReadOnlyList<IndianaEvent> cycleEvents,
            IReadOnlyList<IndianaEvent> splitsEvents,
            IReadOnlyList<IndianaEvent> pedestrianEvents,
            IReadOnlyList<IndianaEvent> terminationEvents,
            Location Location,
            int consecutiveCount)
        {
            if (Location.Approaches.IsNullOrEmpty())
            {
                throw new Exception("Approaches cannot be empty");
            }
            var analysisPhaseCollectionData = new AnalysisPhaseCollectionData();
            analysisPhaseCollectionData.locationId = locationIdentifier;
            var phasesInUse = cycleEvents.Where(d => d.EventCode == 1).Select(d => d.EventParam).Distinct();
            analysisPhaseCollectionData.Plans = planService.GetSplitMonitorPlans(startTime, endTime, locationIdentifier, planEvents.ToList());
            foreach (var phaseNumber in phasesInUse)
            {
                var aPhase = analysisPhaseService.GetAnalysisPhaseData(
                    phaseNumber,
                    pedestrianEvents,
                    cycleEvents,
                    terminationEvents,
                    consecutiveCount,
                    Location);
                analysisPhaseCollectionData.AnalysisPhases.Add(aPhase);
            }
            analysisPhaseCollectionData.AnalysisPhases = analysisPhaseCollectionData.AnalysisPhases.Where(a => a != null).OrderBy(i => i.PhaseNumber).ToList();
            analysisPhaseCollectionData.MaxPhaseInUse = FindMaxPhase(analysisPhaseCollectionData.AnalysisPhases);
            analysisPhaseCollectionData.Location = Location;
            if (analysisPhaseCollectionData.Plans.Count > 0)
            {
                foreach (var plan in analysisPhaseCollectionData.Plans)
                {
                    if (!splitsEvents.IsNullOrEmpty())
                    {
                        SetProgrammedSplits(plan, splitsEvents.ToList());
                        var highestSplit = FindHighestRecordedSplitPhase(plan);
                        FillMissingSplits(highestSplit, plan);
                    }
                    if (analysisPhaseCollectionData != null)
                    {
                        SetHighCycleCount(analysisPhaseCollectionData, plan);
                    }
                }
            }
            return analysisPhaseCollectionData;
        }

        private int FindMaxPhase(List<AnalysisPhaseData> analysisPhases)
        {
            return analysisPhases.Select(phase => phase.PhaseNumber).Concat(new[] { 0 }).Max();
        }

        public void SetProgrammedSplits(PlanSplitMonitorData plan, List<IndianaEvent> LocationEvents)
        {
            plan.Splits.Clear();
            var eventCodes = new List<short>();
            for (short i = 130; i <= 151; i++)
                eventCodes.Add(i);
            var splitsDt = LocationEvents.Where(s => s.Timestamp >= plan.Start && s.Timestamp < plan.Start.AddSeconds(2) && eventCodes.Contains(s.EventCode)).OrderBy(s => s.Timestamp); // controllerEventLogRepository.GetEventsByEventCodes(LocationId, plan.StartTime, plan.StartTime.AddSeconds(2), l);
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
                var Cycles = from cycle in phase.Cycles.Cycles
                             where cycle.StartTime > planSplitMonitorData.Start && cycle.EndTime < planSplitMonitorData.End
                             select cycle;

                if (Cycles.Count() > HighCycleCount)
                    HighCycleCount = Cycles.Count();
            }
            planSplitMonitorData.HighCycleCount = HighCycleCount;
        }
    }
}