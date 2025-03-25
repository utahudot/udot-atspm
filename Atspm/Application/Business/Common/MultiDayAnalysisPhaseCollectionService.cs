//#region license
//// Copyright 2024 Utah Departement of Transportation
//// for Application - Utah.Udot.Atspm.Business.Common/MultiDayAnalysisPhaseCollectionService.cs
//// 
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
//// 
//// http://www.apache.org/licenses/LICENSE-2.0
//// 
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.
//#endregion

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Utah.Udot.Atspm.Data.Models.EventLogModels;
//using Utah.Udot.Atspm.Extensions;

//namespace Utah.Udot.Atspm.Business.Common
//{
//    /// <summary>
//    /// Service for aggregating analysis phase collection data across specific dates.
//    /// </summary>
//    public class MultiDayAnalysisPhaseCollectionService
//    {
//        private readonly AnalysisPhaseCollectionService _dailyService;

//        public MultiDayAnalysisPhaseCollectionService(AnalysisPhaseCollectionService dailyService)
//        {
//            _dailyService = dailyService;
//        }

//        /// <summary>
//        /// Retrieves and combines analysis phase collection data for specific dates selected by the user.
//        /// </summary>
//        /// <param name="locationIdentifier">Unique identifier for the location.</param>
//        /// <param name="selectedDates">A collection of specific dates to process.</param>
//        /// <param name="planEvents">All plan events across the period.</param>
//        /// <param name="cycleEvents">All cycle events across the period.</param>
//        /// <param name="splitsEvents">All splits events across the period.</param>
//        /// <param name="pedestrianEvents">All pedestrian events across the period.</param>
//        /// <param name="terminationEvents">All termination events across the period.</param>
//        /// <param name="location">Location details.</param>
//        /// <param name="consecutiveCount">Consecutive count parameter.</param>
//        /// <returns>A merged AnalysisPhaseCollectionData instance.</returns>
//        public AnalysisPhaseCollectionData GetCombinedAnalysisPhaseCollectionDataForDates(
//            string locationIdentifier,
//            IEnumerable<DateTime> selectedDates,
//            List<IndianaEvent> planEvents,
//            List<IndianaEvent> cycleEvents,
//            List<IndianaEvent> splitsEvents,
//            List<IndianaEvent> pedestrianEvents,
//            List<IndianaEvent> terminationEvents,
//            Location location,
//            int consecutiveCount)
//        {
//            var dailyResults = new List<AnalysisPhaseCollectionData>();

//            // Process each selected date.
//            foreach (var date in selectedDates)
//            {
//                // Define the day boundaries for the selected date.
//                var dayStart = date.Date;
//                var dayEnd = dayStart.AddDays(1);

//                // Filter events for the current day.
//                var dayPlanEvents = planEvents.Where(e => e.Timestamp >= dayStart && e.Timestamp < dayEnd).ToList();
//                var dayCycleEvents = cycleEvents.Where(e => e.Timestamp >= dayStart && e.Timestamp < dayEnd).ToList();
//                var daySplitsEvents = splitsEvents.Where(e => e.Timestamp >= dayStart && e.Timestamp < dayEnd).ToList();
//                var dayPedestrianEvents = pedestrianEvents.Where(e => e.Timestamp >= dayStart && e.Timestamp < dayEnd).ToList();
//                var dayTerminationEvents = terminationEvents.Where(e => e.Timestamp >= dayStart && e.Timestamp < dayEnd).ToList();

//                // Process the day only if events exist.
//                if (dayPlanEvents.Any() ||
//                    dayCycleEvents.Any() ||
//                    daySplitsEvents.Any() ||
//                    dayPedestrianEvents.Any() ||
//                    dayTerminationEvents.Any())
//                {
//                    var dailyData = _dailyService.GetAnalysisPhaseCollectionData(
//                        locationIdentifier,
//                        dayStart,
//                        dayEnd,
//                        dayPlanEvents,
//                        dayCycleEvents,
//                        daySplitsEvents,
//                        dayPedestrianEvents,
//                        dayTerminationEvents,
//                        location,
//                        consecutiveCount);

//                    dailyResults.Add(dailyData);
//                }
//            }

//            // Create the combined container.
//            var combinedData = new AnalysisPhaseCollectionData
//            {
//                locationId = locationIdentifier,
//                Location = location
//            };

//            // --- Merge Plan Data ---
//            var allPlans = dailyResults.SelectMany(d => d.Plans).ToList();
//            // Assumes that PlanSplitMonitorData has a unique identifier: PlanNumber.
//            var mergedPlans = allPlans
//                .GroupBy(p => p.PlanNumber)
//                .Select(g => MergePlanGroup(g))
//                .ToList();
//            combinedData.Plans = mergedPlans;

//            // --- Merge Analysis Phase Data ---
//            var allPhases = dailyResults.SelectMany(d => d.AnalysisPhases)
//                .GroupBy(ap => ap.PhaseNumber)
//                .Select(g => MergeAnalysisPhases(g))
//                .OrderBy(ap => ap.PhaseNumber)
//                .ToList();
//            combinedData.AnalysisPhases = allPhases;

//            combinedData.MaxPhaseInUse = combinedData.AnalysisPhases
//                .Select(ap => ap.PhaseNumber)
//                .Concat(new[] { 0 })
//                .Max();

//            return combinedData;
//        }

//        /// <summary>
//        /// Merges a collection of PlanSplitMonitorData objects that share the same plan number.
//        /// Instead of averaging percentages, this logic aggregates the raw data and then recalculates
//        /// the percentage and split metrics.
//        /// </summary>
//        /// <param name="plans">The group of plans to merge.</param>
//        /// <returns>A single merged PlanSplitMonitorData instance.</returns>
//        private PlanSplitMonitorData MergePlanGroup(IEnumerable<PlanSplitMonitorData> plans)
//        {
//            var planGroup = plans.ToList();

//            // Create a merged plan using the earliest start and latest end.
//            var merged = new PlanSplitMonitorData(
//                planGroup.Min(p => p.Start),
//                planGroup.Max(p => p.End),
//                planGroup.First().PlanNumber)
//            {
//                // For CycleLength and OffsetLength, take the first non-zero value.
//                CycleLength = planGroup.Select(p => p.CycleLength).FirstOrDefault(v => v != 0),
//                OffsetLength = planGroup.Select(p => p.OffsetLength).FirstOrDefault(v => v != 0),
//                // Sum counts that should be aggregated.
//                HighCycleCount = planGroup.Sum(p => p.HighCycleCount),
//                // For properties like MinTime and ProgrammedSplit, choose an appropriate aggregation.
//                MinTime = planGroup.Min(p => p.MinTime),
//                ProgrammedSplit = planGroup.Select(p => p.ProgrammedSplit).FirstOrDefault(v => v != 0)
//            };

//            // Merge the Splits dictionaries by summing values for matching keys.
//            foreach (var plan in planGroup)
//            {
//                foreach (var kvp in plan.Splits)
//                {
//                    if (merged.Splits.ContainsKey(kvp.Key))
//                    {
//                        merged.Splits[kvp.Key] += kvp.Value;
//                    }
//                    else
//                    {
//                        merged.Splits.Add(kvp.Key, kvp.Value);
//                    }
//                }
//            }

//            // Recalculate percentage metrics based on the merged raw data.
//            RecalculatePercentages(merged, planGroup);
//            // Recalculate split-related metrics based on the merged splits.
//            RecalculateSplitMetrics(merged);

//            return merged;
//        }

//        /// <summary>
//        /// Merges a collection of AnalysisPhaseData objects that share the same phase number.
//        /// Adjust the merge logic as necessary.
//        /// </summary>
//        /// <param name="phases">The group of phase data to merge.</param>
//        /// <returns>A single merged AnalysisPhaseData instance.</returns>
//        private AnalysisPhaseData MergeAnalysisPhases(IEnumerable<AnalysisPhaseData> phases)
//        {
//            var phaseGroup = phases.ToList();
//            var merged = new AnalysisPhaseData
//            {
//                PhaseNumber = phaseGroup.First().PhaseNumber,
//                PhaseDescription = phaseGroup.First().PhaseDescription,
//                locationId = phaseGroup.First().locationId,
//                locationIdentifier = phaseGroup.First().locationIdentifier,
//                // For these fields, you might choose to recalculate based on underlying data if available.
//                PercentMaxOuts = phaseGroup.Average(p => p.PercentMaxOuts),
//                PercentForceOffs = phaseGroup.Average(p => p.PercentForceOffs),
//                TotalPhaseTerminations = phaseGroup.Sum(p => p.TotalPhaseTerminations),
//                Direction = phaseGroup.First().Direction,
//                IsOverlap = phaseGroup.Any(p => p.IsOverlap),
//                PedestrianEvents = phaseGroup.SelectMany(p => p.PedestrianEvents).OrderBy(e => e.Timestamp).ToList(),
//                TerminationEvents = phaseGroup.SelectMany(p => p.TerminationEvents).OrderBy(e => e.Timestamp).ToList(),
//                UnknownTermination = phaseGroup.SelectMany(p => p.UnknownTermination).OrderBy(e => e.Timestamp).ToList(),
//                Location = phaseGroup.First().Location
//            };

//            // Merge consecutive event lists.
//            merged.ConsecutiveGapOuts = phaseGroup.SelectMany(p => p.ConsecutiveGapOuts).OrderBy(e => e.Timestamp).ToList();
//            merged.ConsecutiveMaxOut = phaseGroup.SelectMany(p => p.ConsecutiveMaxOut).OrderBy(e => e.Timestamp).ToList();
//            merged.ConsecutiveForceOff = phaseGroup.SelectMany(p => p.ConsecutiveForceOff).OrderBy(e => e.Timestamp).ToList();

//            // Merge cycles from each AnalysisPhaseData.
//            var allCycles = phaseGroup
//                .SelectMany(p => p.Cycles.Cycles)
//                .OrderBy(c => c.StartTime)
//                .ToList();
//            // Rebuild the cycle collection; adjust the constructor and merging logic as needed.
//            merged.Cycles = new AnalysisPhaseCycleCollection(
//                merged.PhaseNumber,
//                merged.locationIdentifier,
//                allCycles,
//                new List<IndianaEvent>(), // Optionally, merge pedestrian events within cycles.
//                new List<IndianaEvent>()  // Optionally, merge termination events within cycles.
//            );

//            return merged;
//        }

//        /// <summary>
//        /// Recalculates percentage metrics for the merged plan.
//        /// This placeholder should be replaced with logic that aggregates underlying raw counts
//        /// (e.g., total cycles, skips count, etc.) and then computes percentages accordingly.
//        /// </summary>
//        /// <param name="merged">The merged PlanSplitMonitorData object.</param>
//        /// <param name="planGroup">The list of daily PlanSplitMonitorData objects that were merged.</param>
//        private void RecalculatePercentages(PlanSplitMonitorData merged, List<PlanSplitMonitorData> planGroup)
//        {
//            // Example (pseudocode):
//            // merged.TotalCycles = planGroup.Sum(p => p.TotalCycles);
//            // merged.SkipsCount = planGroup.Sum(p => p.SkipsCount);
//            // merged.GapOutsCount = planGroup.Sum(p => p.GapOutsCount);
//            // merged.MaxOutsCount = planGroup.Sum(p => p.MaxOutsCount);
//            // merged.ForceOffsCount = planGroup.Sum(p => p.ForceOffsCount);
//            //
//            // merged.PercentSkips = (merged.TotalCycles > 0) ? (double)merged.SkipsCount / merged.TotalCycles : 0;
//            // merged.PercentGapOuts = (merged.TotalCycles > 0) ? (double)merged.GapOutsCount / merged.TotalCycles : 0;
//            // merged.PercentMaxOuts = (merged.TotalCycles > 0) ? (double)merged.MaxOutsCount / merged.TotalCycles : 0;
//            // merged.PercentForceOffs = (merged.TotalCycles > 0) ? (double)merged.ForceOffsCount / merged.TotalCycles : 0;

//            // Without the raw counts available, set default values or implement your logic here.
//            merged.PercentSkips = 0;
//            merged.PercentGapOuts = 0;
//            merged.PercentMaxOuts = 0;
//            merged.PercentForceOffs = 0;
//        }

//        /// <summary>
//        /// Recalculates split-related metrics for the merged plan.
//        /// This placeholder implementation computes the average split and example percentiles based on the merged splits.
//        /// Adjust the calculations as needed.
//        /// </summary>
//        /// <param name="merged">The merged PlanSplitMonitorData object.</param>
//        private void RecalculateSplitMetrics(PlanSplitMonitorData merged)
//        {
//            if (merged.Splits.Any())
//            {
//                // Calculate the average split from the merged splits.
//                merged.AverageSplit = merged.Splits.Values.Average();

//                // For percentile calculations, sort the split values.
//                var sortedSplits = merged.Splits.Values.OrderBy(v => v).ToList();
//                int count = sortedSplits.Count;

//                // Example: 50th percentile.
//                merged.PercentileSplit50th = sortedSplits[count / 2];

//                // Example: 85th percentile.
//                merged.PercentileSplit85th = sortedSplits[(int)(count * 0.85)];

//                // For this example, assign the 50th percentile as the default percentile split.
//                merged.PercentileSplit = merged.PercentileSplit50th;
//            }
//            else
//            {
//                merged.AverageSplit = 0;
//                merged.PercentileSplit50th = 0;
//                merged.PercentileSplit85th = 0;
//                merged.PercentileSplit = 0;
//            }
//        }
//    }
//}
