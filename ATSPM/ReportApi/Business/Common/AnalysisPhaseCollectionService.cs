using ATSPM.Data.Models;
using Microsoft.IdentityModel.Tokens;

namespace ATSPM.ReportApi.Business.Common
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
        private readonly PhaseService phaseService;

        public AnalysisPhaseCollectionService(
            PlanService planService,
            AnalysisPhaseService analysisPhaseService,
            PhaseService phaseService
            )
        {
            this.planService = planService;
            this.analysisPhaseService = analysisPhaseService;
            this.phaseService = phaseService;
        }

        //public AnalysisPhaseCollectionData GetAnalysisPhaseCollectionData(
        //    string locationId,
        //    DateTime startTime,
        //    DateTime endTime,
        //    int consecutivecount)
        //{
        //    var Location = _LocationRepository.GetLatestVersionOfLocation(locationId, startTime);
        //    var analysisPhaseCollectionData = new AnalysisPhaseCollectionData();
        //    analysisPhaseCollectionData.LocationId = locationId;
        //    var ptedt = controllerEventLogRepository.GetLocationEventsByEventCodes(
        //        analysisPhaseCollectionData.LocationId,
        //        startTime,
        //        endTime,
        //        new List<int> { 1, 11, 4, 5, 6, 7, 21, 23 });
        //    var dapta = controllerEventLogRepository.GetLocationEventsByEventCodes(
        //        analysisPhaseCollectionData.LocationId,
        //        startTime,
        //        endTime,
        //        new List<int> { 1 });
        //    ptedt = ptedt.OrderByDescending(i => i.TimeStamp).ToList();
        //    var phasesInUse = dapta.Where(r => r.EventCode == 1).Select(r => r.EventParam).Distinct();
        //    analysisPhaseCollectionData.Plans = planService.GetSplitMonitorPlans(startTime, endTime, analysisPhaseCollectionData.LocationId);
        //    foreach (var row in phasesInUse)
        //    {
        //        var aPhase = analysisPhaseService.GetAnalysisPhaseData(row, ptedt, consecutivecount, Location);
        //        analysisPhaseCollectionData.AnalysisPhases.Add(aPhase);
        //    }
        //    analysisPhaseCollectionData.AnalysisPhases = analysisPhaseCollectionData.AnalysisPhases.OrderBy(i => i.PhaseNumber).ToList();
        //    analysisPhaseCollectionData.MaxPhaseInUse = FindMaxPhase(analysisPhaseCollectionData.AnalysisPhases);
        //    return analysisPhaseCollectionData;
        //}

        public AnalysisPhaseCollectionData GetAnalysisPhaseCollectionData(
            string locationIdentifier,
            DateTime startTime,
            DateTime endTime,
            IReadOnlyList<ControllerEventLog> planEvents,
            IReadOnlyList<ControllerEventLog> cycleEvents,
            IReadOnlyList<ControllerEventLog> splitsEvents,
            IReadOnlyList<ControllerEventLog> pedestrianEvents,
            IReadOnlyList<ControllerEventLog> terminationEvents,
            Location Location,
            int consecutiveCount)
        {
            if (Location.Approaches.IsNullOrEmpty())
            {
                return null;
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

        //public AnalysisPhaseCollectionData GetAnalysisPhaseCollection(
        //    string locationId,
        //    DateTime startTime,
        //    DateTime endTime,
        //    IReadOnlyList<ControllerEventLog> terminationEvents,
        //    IReadOnlyList<ControllerEventLog> planEvents,
        //    IReadOnlyList<ControllerEventLog> phaseEvents,
        //    IReadOnlyList<ControllerEventLog> pedEvents)
        //{
        //    var analysisPhaseCollectionData = new AnalysisPhaseCollectionData();
        //    var cel = ControllerEventLogRepositoryFactory.Create();
        //    var ptedt = cel.GetLocationEventsByEventCodes(locationId, startTime, endTime,
        //        new List<int> { 1, 11, 4, 5, 6, 7, 21, 23 });
        //    var dapta = cel.GetLocationEventsByEventCodes(locationId, startTime, endTime, new List<int> { 1 });
        //    var phasesInUse = dapta.Where(d => d.EventCode == 1).Select(d => d.EventParam).Distinct();
        //    Plans = PlanFactory.GetSplitMonitorPlans(startTime, endTime, locationId);
        //    foreach (var row in phasesInUse)
        //    {
        //        var aPhase = new AnalysisPhase(row, locationId, ptedt);
        //        Items.Add(aPhase);
        //    }
        //    OrderPhases();
        //    return analysisPhaseCollectionData;
        //}

        private int FindMaxPhase(List<AnalysisPhaseData> analysisPhases)
        {
            return analysisPhases.Select(phase => phase.PhaseNumber).Concat(new[] { 0 }).Max();
        }

        public void SetProgrammedSplits(PlanSplitMonitorData plan, List<ControllerEventLog> LocationEvents)
        {
            plan.Splits.Clear();
            var eventCodes = new List<int>();
            for (var i = 130; i <= 151; i++)
                eventCodes.Add(i);
            var splitsDt = LocationEvents.Where(s => eventCodes.Contains(s.EventCode)); // controllerEventLogRepository.GetLocationEventsByEventCodes(locationId, plan.StartTime, plan.StartTime.AddSeconds(2), l);
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
                             where cycle.StartTime > planSplitMonitorData.Start && cycle.EndTime < planSplitMonitorData.End
                             select cycle;

                if (Cycles.Count() > HighCycleCount)
                    HighCycleCount = Cycles.Count();
            }
            planSplitMonitorData.HighCycleCount = HighCycleCount;
        }
    }
}