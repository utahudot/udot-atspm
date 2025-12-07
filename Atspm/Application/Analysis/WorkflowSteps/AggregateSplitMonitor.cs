#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregateSpeedItemEvents.cs
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

using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// Transforms <see cref="IndianaEvent"/> into <see cref="DetectorEventCountAggregation"/>
    /// where <see cref="IndianaEvent.EventCode"/> equals <see cref="IndianaEnumerations.VehicleDetectorOn"/>
    /// and <see cref="IndianaEvent.EventParam"/> equals <see cref="Detector.DetectorChannel"/>.
    /// </summary>
    public class AggregateSplitMonitor : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<PhaseSplitMonitorAggregation>>
    {
        private readonly TimeSpan _binSize;

        /// <inheritdoc/>
        public AggregateSplitMonitor(TimeSpan binSize, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _binSize = binSize;
        }

        /// <inheritdoc/>
        protected override Task<IEnumerable<PhaseSplitMonitorAggregation>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            return (Task<IEnumerable<PhaseSplitMonitorAggregation>>)SplitMonitorAgg(input);
        }

        private IEnumerable<PhaseSplitMonitorAggregation> SplitMonitorAgg(Tuple<Location, IEnumerable<IndianaEvent>> input)
        {
            var phaseSplitMonitor = new List<PhaseSplitMonitorAggregation>();
            var location = input.Item1;
            var approaches = location.Approaches;
            var indianaEvents = input.Item2;
            var locationIdentifier = location.LocationIdentifier;
            DateTime binStart = indianaEvents.Select(i => i.Timestamp).OrderBy(i => i).FirstOrDefault();
            DateTime binEnd = indianaEvents.Select(i => i.Timestamp).OrderBy(i => i).LastOrDefault();
            var results = new ConcurrentBag<PhaseSplitMonitorAggregation>();

            var tuple = new Tuple<Location, IEnumerable<IndianaEvent>, DateTime, DateTime>(location, indianaEvents, binStart, binEnd);
            var aggregatedEvents = SplitMonitorAggregationCalculation(tuple);

            foreach (var result in aggregatedEvents)
            {
                if (result == null) continue;
                result.Start = binStart;
                result.End = binEnd;
                result.LocationIdentifier = location.LocationIdentifier;
                results.Add(result);
            }
            var enumerable = results.AsEnumerable();
            return enumerable;

        }

        private IEnumerable<PhaseSplitMonitorAggregation> SplitMonitorAggregationCalculation(Tuple<Location, IEnumerable<IndianaEvent>, DateTime, DateTime> input)
        {
            var phaseSplitMonitor = new List<PhaseSplitMonitorAggregation>();
            var location = input.Item1;
            var indianaEvents = input.Item2;
            var start = input.Item3;
            var end = input.Item4;

            var pedEvents = indianaEvents.Where(e =>
                    new List<short>
                    {
                    21,
                    23
                    }.Contains(e.EventCode)
                    && e.Timestamp >= start
                    && e.Timestamp <= end).ToList();
            var cycleEvents = indianaEvents.Where(e =>
                new List<short>
                {
                    1,
                    4,
                    5,
                    6,
                    7,
                    8,
                    11
                }.Contains(e.EventCode)
                && e.Timestamp >= start
                && e.Timestamp <= end).ToList();
            var splitsEventCodes = new List<short>();
            for (short i = 130; i <= 149; i++)
                splitsEventCodes.Add(i);
            var splitsEvents = indianaEvents.Where(e =>
                splitsEventCodes.Contains(e.EventCode)
                && e.Timestamp >= start
                && e.Timestamp <= end).ToList();
            var terminationEvents = indianaEvents.Where(e =>
                new List<short>
                {
                    4,
                    5,
                    6,
                    7
                }.Contains(e.EventCode)
                && e.Timestamp >= start
                && e.Timestamp <= end).ToList();


            var aggregatedSplitMonitor = GetAggregatedSplitMonitor(input,
                   cycleEvents,
                   pedEvents,
                   splitsEvents,
                   terminationEvents,
                   location);

            return aggregatedSplitMonitor;


            //var enumerable = phaseSplitMonitor.AsEnumerable();
            //return enumerable;
        }

        private List<PhaseSplitMonitorAggregation> GetAggregatedSplitMonitor(Tuple<Location, IEnumerable<IndianaEvent>, DateTime, DateTime> input, List<IndianaEvent> cycleEvents, List<IndianaEvent> pedEvents, List<IndianaEvent> splitsEvents, List<IndianaEvent> terminationEvents, Location location)
        {
            var listPhaseInformation = new List<PhaseSplitMonitorAggregation>();


            var phaseCollection = GetAnalysisPhaseCollectionData(
                    location.LocationIdentifier,
                    input.Item3,
                    input.Item4,
                    new List<IndianaEvent>(),
                    cycleEvents,
                    splitsEvents,
                    pedEvents,
                    terminationEvents,
                    location,
                    1
                    );

            if (phaseCollection == null)
            {
                return null;
            }

            var highCycleCount = GetHighCycleCount(phaseCollection);

            foreach (var phase in phaseCollection.AnalysisPhases)
            {
                var phaseData = GetChartDataForPhase(input, phase, highCycleCount);
                listPhaseInformation.Add(phaseData);
            }

            return listPhaseInformation;
        }

        private PhaseSplitMonitorAggregation GetChartDataForPhase(Tuple<Location, IEnumerable<IndianaEvent>, DateTime, DateTime> input, AnalysisPhaseData phase, int highCycleCount)
        {
            var cycles = phase.Cycles.Cycles.ToList();
            int skippedPhases = highCycleCount - cycles.Count();

            var aggregatedPhaseSplitMonitor = new PhaseSplitMonitorAggregation();
            aggregatedPhaseSplitMonitor.LocationIdentifier = input.Item1.LocationIdentifier;
            aggregatedPhaseSplitMonitor.BinStartTime = input.Item3;
            aggregatedPhaseSplitMonitor.PhaseNumber = phase.PhaseNumber;
            aggregatedPhaseSplitMonitor.Start = input.Item3;
            aggregatedPhaseSplitMonitor.End = input.Item4;
            aggregatedPhaseSplitMonitor.EightyFifthPercentileSplit = GetPercentSplit(highCycleCount, .85, cycles);
            aggregatedPhaseSplitMonitor.SkippedCount = skippedPhases;

            return aggregatedPhaseSplitMonitor;
        }

        public int GetHighCycleCount(AnalysisPhaseCollectionData phases)
        {
            //find all the phases cycles within the plan
            var HighCycleCount = 0;
            foreach (var phase in phases.AnalysisPhases)
            {
                var Cycles = from cycle in phase.Cycles.Cycles
                             select cycle;

                if (Cycles.Count() > HighCycleCount)
                    HighCycleCount = Cycles.Count();
            }
            return HighCycleCount;
        }

        private double GetPercentSplit(double highCycleCount, double percentile, List<AnalysisPhaseCycle> cycles)
        {
            if (cycles.Count <= 2)
                return 0;
            var orderedCycles = cycles.OrderBy(c => c.Duration.TotalSeconds).ToList();

            var percentilIndex = percentile * orderedCycles.Count;
            if ((percentilIndex % 1).AreEqual(0))
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

        public IReadOnlyList<PlanSplitMonitorData> GetSplitMonitorPlans(
            DateTime startDate,
            DateTime endDate,
            string locationId,
            IList<IndianaEvent> planEvents)
        {
            //var planEvents = SetFirstAndLastPlan(startDate, endDate, locationId, events.ToList());
            var plans = new List<PlanSplitMonitorData>();
            for (var i = 0; i < planEvents.Count; i++)
                if (planEvents.Count - 1 == i)
                {
                    if (planEvents[i].Timestamp != endDate)
                        plans.Add(new PlanSplitMonitorData(planEvents[i].Timestamp, endDate, planEvents[i].EventParam.ToString()));
                }
                else
                {
                    if (planEvents[i].Timestamp != planEvents[i + 1].Timestamp)
                        plans.Add(new PlanSplitMonitorData(planEvents[i].Timestamp, planEvents[i + 1].Timestamp,
                            planEvents[i].EventParam.ToString()));
                }
            return plans;
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
            analysisPhaseCollectionData.Plans = GetSplitMonitorPlans(startTime, endTime, locationIdentifier, planEvents.ToList());
            foreach (var phaseNumber in phasesInUse)
            {
                var aPhase = GetAnalysisPhaseData(
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
        private static List<PhaseDetail> GetPhases(Location Location)
        {
            if (Location.Approaches == null || Location.Approaches.Count == 0)
            {
                return new List<PhaseDetail>();
            }

            var phaseDetails = new List<PhaseDetail>();
            foreach (var approach in Location.Approaches)
            {
                if (approach.ProtectedPhaseNumber != 0)
                {
                    phaseDetails.Add(new PhaseDetail
                    {
                        PhaseNumber = approach.ProtectedPhaseNumber,
                        UseOverlap = approach.IsProtectedPhaseOverlap,
                        IsPermissivePhase = false,
                        Approach = approach
                    });
                }
                if (approach.PermissivePhaseNumber != null)
                {
                    phaseDetails.Add(new PhaseDetail
                    {
                        PhaseNumber = approach.PermissivePhaseNumber.Value,
                        UseOverlap = approach.IsPermissivePhaseOverlap,
                        IsPermissivePhase = true,
                        Approach = approach
                    });
                }
            }
            return phaseDetails;
        }

        public AnalysisPhaseData GetAnalysisPhaseData(
            int phaseNumber,
            IReadOnlyList<IndianaEvent> pedestrianEvents,
            IReadOnlyList<IndianaEvent> cycleEvents,
            IReadOnlyList<IndianaEvent> terminationEvents,
            int consecutiveCount,
            Location location
            )
        {
            var cleanTerminationEventsForPhase = CleanTerminationEvents(terminationEvents, phaseNumber);
            if (location.Approaches.IsNullOrEmpty())
            {
                return null;
            }
            var analysisPhaseData = new AnalysisPhaseData();
            var phase = GetPhases(location).Find(p => p.PhaseNumber == phaseNumber);
            SetPhaseDescription(analysisPhaseData, phase, phaseNumber);
            analysisPhaseData.PhaseNumber = phaseNumber;
            var cycleEventCodes = new List<short> { 1, 8, 11 };
            var phaseEvents = cycleEvents.ToList().Where(p => p.EventParam == phaseNumber && cycleEventCodes.Contains(p.EventCode)).ToList();
            if (!pedestrianEvents.IsNullOrEmpty())
            {
                analysisPhaseData.PedestrianEvents = pedestrianEvents.Where(t => t.EventParam == phaseNumber && (t.EventCode == 21 || t.EventCode == 23)).ToList();
            }
            else
            {
                analysisPhaseData.PedestrianEvents = new List<IndianaEvent>();
            }
            analysisPhaseData.Cycles = new AnalysisPhaseCycleCollection(phaseNumber, analysisPhaseData.locationIdentifier, phaseEvents, analysisPhaseData.PedestrianEvents, cleanTerminationEventsForPhase);
            if (!cleanTerminationEventsForPhase.IsNullOrEmpty())
            {
                analysisPhaseData.TerminationEvents = cleanTerminationEventsForPhase.Where(t => t.EventParam == phaseNumber && (t.EventCode == 4 || t.EventCode == 5 || t.EventCode == 6)).ToList();
            }
            else
            {
                analysisPhaseData.TerminationEvents = new List<IndianaEvent>();
            }
            analysisPhaseData.ConsecutiveGapOuts = FindConsecutiveEvents(analysisPhaseData.TerminationEvents, 4, consecutiveCount) ?? new List<IndianaEvent>();
            analysisPhaseData.ConsecutiveMaxOut = FindConsecutiveEvents(analysisPhaseData.TerminationEvents, 5, consecutiveCount) ?? new List<IndianaEvent>();
            analysisPhaseData.ConsecutiveForceOff = FindConsecutiveEvents(analysisPhaseData.TerminationEvents, 6, consecutiveCount) ?? new List<IndianaEvent>();
            analysisPhaseData.UnknownTermination = FindUnknownTerminationEvents(cleanTerminationEventsForPhase.ToList(), phaseNumber) ?? new List<IndianaEvent>();
            analysisPhaseData.PercentMaxOuts = FindPercentageConsecutiveEvents(analysisPhaseData.TerminationEvents, 5);
            analysisPhaseData.PercentForceOffs = FindPercentageConsecutiveEvents(analysisPhaseData.TerminationEvents, 6);
            analysisPhaseData.TotalPhaseTerminations = analysisPhaseData.TerminationEvents.Count;
            analysisPhaseData.Location = location;
            return analysisPhaseData;
        }

        private static void SetPhaseDescription(AnalysisPhaseData analysisPhaseData, PhaseDetail phase, int phaseNumber)
        {
            if (phase == null)
            {
                analysisPhaseData.PhaseDescription = $"Phase {phaseNumber} (Unconfigured)";
            }
            else
            {
                analysisPhaseData.PhaseDescription = phase.GetApproachDescription();
            }
        }

        public List<IndianaEvent> CleanTerminationEvents(IReadOnlyList<IndianaEvent> terminationEvents,
            int phasenumber)
        {

            var sortedEvents = terminationEvents.Where(t => t.EventParam == phasenumber).OrderBy(x => x.Timestamp).ThenBy(y => y.EventCode).ToList();
            var duplicateList = new List<IndianaEvent>();
            for (int i = 0; i < sortedEvents.Count - 1; i++)
            {
                var event1 = sortedEvents[i];
                var event2 = sortedEvents[i + 1];
                if (event1.Timestamp == event2.Timestamp)
                {
                    if (event1.EventCode == 7)
                        duplicateList.Add(event1);
                    if (event2.EventCode == 7)
                        duplicateList.Add(event2);
                }
            }

            foreach (var e in duplicateList)
            {
                sortedEvents.Remove(e);
            }
            return sortedEvents;
        }

        private List<IndianaEvent> FindConsecutiveEvents(List<IndianaEvent> terminationEvents,
            short eventtype, int consecutiveCount)
        {
            var ConsecutiveEvents = new List<IndianaEvent>();
            var runningConsecCount = 0;
            // Order the events by datestamp
            var eventsInOrder = terminationEvents.OrderBy(TerminationEvent => TerminationEvent.Timestamp);
            foreach (var termEvent in eventsInOrder)
            {
                if (termEvent.EventCode == eventtype)
                    runningConsecCount++;
                else
                    runningConsecCount = 0;

                if (runningConsecCount >= consecutiveCount)
                    ConsecutiveEvents.Add(termEvent);
            }
            return ConsecutiveEvents;
        }

        private List<IndianaEvent> FindUnknownTerminationEvents(List<IndianaEvent> terminationEvents, int phaseNumber)
        {
            return terminationEvents.Where(t => t.EventCode == 7 && t.EventParam == phaseNumber).ToList();
        }


        private double FindPercentageConsecutiveEvents(List<IndianaEvent> terminationEvents, short eventtype)
        {
            double percentile = 0;
            double total = terminationEvents.Count(t => t.EventCode != 7);
            //Get all termination events of the event type
            var terminationEventsOfType = terminationEvents.Count(terminationEvent => terminationEvent.EventCode == eventtype);

            if (terminationEvents.Any())
                percentile = terminationEventsOfType / total;
            return percentile;
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

        private int FindMaxPhase(List<AnalysisPhaseData> analysisPhases)
        {
            return analysisPhases.Select(phase => phase.PhaseNumber).Concat(new[] { 0 }).Max();
        }

    }
}
