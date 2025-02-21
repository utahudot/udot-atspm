#region license
// Copyright 2024 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.PreemptServiceRequest/PreemptServiceRequestService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Business.SplitMonitor;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Business.TransitSignalPriority;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.PhaseTermination;

namespace Utah.Udot.Atspm.Business.TransitSignalPriorityRequest
{
    public class TransitSignalPriorityService
    {
        private static readonly short[] EventCodes =
            (new short[] { 1, 4, 5, 6, 7, 8, 11 })
            .Concat(Enumerable.Range(130, 20).Select(x => (short)x))
            .ToArray();

        private static readonly int _maxDegreeOfParallelism = 5;

        private readonly IServiceProvider _serviceProvider;
        private readonly PlanService _planService;
        private readonly CycleService _cycleService;
        private readonly ILogger<TransitSignalPriorityService> _logger;

        // New block that gets all data for a single inputParameters (across multiple dates)
        private readonly TransformBlock<TransitSignalLoadParameters, (TransitSignalLoadParameters, List<IndianaEvent>)?> _loadLocationDataBlock;
        private readonly TransformBlock<(TransitSignalLoadParameters, List<IndianaEvent>), (TransitSignalLoadParameters, Dictionary<string, List<IndianaEvent>>)> _classifyEventsBlock;
        private readonly TransformBlock<(TransitSignalLoadParameters, Dictionary<string, List<IndianaEvent>>), (TransitSignalLoadParameters, List<TransitSignalPriorityPlan>)> _processCyclesBlock;
        private readonly TransformBlock<(TransitSignalLoadParameters, List<IndianaEvent>)?, (TransitSignalLoadParameters, List<IndianaEvent>)> _filteringBlock;
        private readonly TransformBlock<(TransitSignalLoadParameters, List<TransitSignalPriorityPlan>), (TransitSignalLoadParameters, List<TransitSignalPriorityPlan>)> _calculateTSPMaxBlock;

        public TransitSignalPriorityService(
            IServiceProvider serviceProvider,
            PlanService planService,
            CycleService cycleService,
            ILogger<TransitSignalPriorityService> logger)
        {
            _serviceProvider = serviceProvider;
            _planService = planService;
            _cycleService = cycleService;
            _logger = logger;

            // Block to load inputParameters and all events (aggregated over all dates)
            _loadLocationDataBlock = new TransformBlock<TransitSignalLoadParameters, (TransitSignalLoadParameters, List<IndianaEvent>)?>(
                async input =>
                {
                    try
                    {
                        // Start fetching location concurrently with event retrieval
                        var locationTask = GetLocation(input.LocationIdentifier, input.Dates.First());

                        var eventTasks = input.Dates.Select(async date =>
                        {
                            try
                            {
                                var events = await GetEvents(input.LocationIdentifier, date);
                                return events ?? new List<IndianaEvent>(); // Ensure non-null
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error getting events for location {input.LocationIdentifier} on {date}");
                                return new List<IndianaEvent>(); // Return empty list on failure
                            }
                        }).ToList();

                        // Await location retrieval
                        input.Location = await locationTask;
                        if (input.Location == null)
                        {
                            _logger.LogWarning($"Location not found for {input.LocationIdentifier} on {input.Dates.First()}");
                            return null;
                        }

                        _logger.LogInformation($"Location found: {input.LocationIdentifier}");

                        // Await all event retrieval tasks
                        var eventsForAllDates = (await Task.WhenAll(eventTasks)).SelectMany(e => e).ToList();

                        return (input, eventsForAllDates);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Unexpected error processing {input.LocationIdentifier}");
                        return null;
                    }
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded } // No explicit limitation on concurrent tasks
            );





            _filteringBlock = new TransformBlock<(TransitSignalLoadParameters, List<IndianaEvent>)?, (TransitSignalLoadParameters, List<IndianaEvent>)>(
                item =>
                {
                    if (item.HasValue)
                    {
                        return item.Value;
                    }
                    // Decide how to handle null values. Here, we return a default value that can be filtered later.
                    return default;
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }
);

            // Reuse (or slightly adjust) the existing classification block.
            _classifyEventsBlock = new TransformBlock<(TransitSignalLoadParameters, List<IndianaEvent>), (TransitSignalLoadParameters, Dictionary<string, List<IndianaEvent>>)>(
                tuple =>
                {
                    var (location, events) = tuple;
                    var categorizedEvents = new Dictionary<string, List<IndianaEvent>>
                    {
                        { "planEvents", new List<IndianaEvent>() },
                        { "cycleEvents", new List<IndianaEvent>() },
                        { "splitsEvents", new List<IndianaEvent>() },
                        { "terminationEvents", new List<IndianaEvent>() }
                    };

                    foreach (var ev in events)
                    {
                        if (new List<short> { 131 }.Contains(ev.EventCode))
                            categorizedEvents["planEvents"].Add(ev);

                        if (new List<short> { 1,8,11 }.Contains(ev.EventCode))
                            categorizedEvents["cycleEvents"].Add(ev);

                        if (Enumerable.Range(130, 20).Contains(ev.EventCode))
                            categorizedEvents["splitsEvents"].Add(ev);

                        if (new List<short> { 4, 5, 6, 7 }.Contains(ev.EventCode))
                            categorizedEvents["terminationEvents"].Add(ev);
                    }

                    return (location, categorizedEvents);
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }
            );

            _processCyclesBlock = new TransformBlock<(TransitSignalLoadParameters, Dictionary<string, List<IndianaEvent>>), (TransitSignalLoadParameters, List<TransitSignalPriorityPlan>)>(
                async tuple =>
                {
                    var (inputParameters, eventGroups) = tuple;

                    // Extract event lists to local variables.
                    var cycleEvents = eventGroups["cycleEvents"];
                    var terminationEvents = eventGroups["terminationEvents"];
                    var planEvents = eventGroups["planEvents"];
                    var splitsEvents = eventGroups["splitsEvents"];

                    var phases = cycleEvents.Select(c => c.EventParam).Distinct();
                    var cycles = new List<TransitSignalPriorityCycle>();

                    foreach (var phase in phases)
                    {
                        if(cycleEvents.Any(cycleEvents => cycleEvents.EventParam == phase))
                        cycles.AddRange(_cycleService.GetTransitSignalPriorityCycles(
                            phase,
                            cycleEvents.Where(cycleEvents => cycleEvents.EventParam == phase).ToList(),
                            terminationEvents.Where(terminationEvents => terminationEvents.EventParam == phase).ToList()
                            ));
                    }
                    var plans = GetTransitSignalPriorityPlans(
                        inputParameters.Dates.OrderBy(d => d).First(), 
                        inputParameters.Dates.OrderBy(d => d).Last(), 
                        inputParameters.LocationIdentifier,
                        planEvents,
                        splitsEvents,
                        cycles
                        );


                    // Clear the event lists to free up memory after processing.
                    cycleEvents.Clear();
                    terminationEvents.Clear();
                    planEvents.Clear();
                    splitsEvents.Clear();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    return (inputParameters, plans);
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }
            );

            _calculateTSPMaxBlock = new TransformBlock<(TransitSignalLoadParameters, List<TransitSignalPriorityPlan>), (TransitSignalLoadParameters, List<TransitSignalPriorityPlan>)>(
                input =>
                {
                    var (parameters, plans) = input;

                    foreach (var plan in plans)
                    {
                        foreach (var phase in plan.Phases)
                        {
                            phase.SkipsGreaterThan70TSPMax = phase.ProgrammedSplit - phase.MinTime;
                            phase.ForceOffsGreaterThan40TSPMax = phase.ProgrammedSplit - ((phase.MinTime + phase.PercentileSplit50th) / 2);
                            phase.ForceOffsGreaterThan60TSPMax = phase.ProgrammedSplit - phase.PercentileSplit50th;
                            phase.ForceOffsGreaterThan80TSPMax = phase.ProgrammedSplit - phase.PercentileSplit85th;

                            if (phase.PercentSkips > 70)
                            {
                                phase.RecommendedTSPMax = phase.SkipsGreaterThan70TSPMax;
                            }
                            else if (phase.PercentMaxOutsForceOffs < 60)
                            {
                                phase.RecommendedTSPMax = phase.ForceOffsGreaterThan60TSPMax;
                            }
                            else if (phase.PercentMaxOutsForceOffs < 80)
                            {
                                phase.RecommendedTSPMax = phase.ForceOffsGreaterThan80TSPMax;
                            }
                            else
                            {
                                phase.RecommendedTSPMax = null; // Not recommended
                            }

                            // Ensure RecommendedTSPMax is null if it is negative
                            if (phase.RecommendedTSPMax.HasValue && phase.RecommendedTSPMax < 0)
                            {
                                phase.RecommendedTSPMax = null;
                            }
                        }
                    }

                    return (parameters, plans);
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }
    );

        }

        private async Task<Location> GetLocation(string locationIdentifier, DateTime date)
        {
            // Create a scope to get the inputParameters.
            using (var scope = _serviceProvider.CreateScope())
            {
                var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();
                return locationRepository.GetLatestVersionOfLocation(locationIdentifier, date);
            }            
        }

        private async Task<List<IndianaEvent>> GetEvents(string locationIdentifier, DateTime date)
        {
            using (var innerScope = _serviceProvider.CreateScope())
            {
                var eventLogRepository = innerScope.ServiceProvider.GetRequiredService<IIndianaEventLogRepository>();
                var events = eventLogRepository
                    .GetEventsByEventCodes(locationIdentifier, date, date.AddDays(1), EventCodes)
                    .ToList();
                _logger.LogInformation($"Found {events.Count} events for location {locationIdentifier} on {date}");

                // Perform manual garbage collection after retrieving event logs.
                GC.Collect();
                GC.WaitForPendingFinalizers();

                return events;
            }
        }

        public async Task<List<TransitSignalPriorityResult>> GetChartDataAsync(TransitSignalPriorityOptions options)
        {


            // Link the blocks, and add a predicate to filteringBlock's output if needed.
            _loadLocationDataBlock.LinkTo(
                _filteringBlock,
                new DataflowLinkOptions { PropagateCompletion = true }
            );

            // Optionally, filter out any default values in filteringBlock (if default is a valid non-data marker).
            _filteringBlock.LinkTo(
                _classifyEventsBlock,
                new DataflowLinkOptions { PropagateCompletion = true },
                item => item != default
            );
            _classifyEventsBlock.LinkTo(
                _processCyclesBlock,
                new DataflowLinkOptions { PropagateCompletion = true }
            );
            _processCyclesBlock.LinkTo(
                _calculateTSPMaxBlock,
                new DataflowLinkOptions { PropagateCompletion = true }
            );

            // For each inputParameters, post one item containing all dates.
            foreach (var locationIdentifier in options.LocationIdentifiers)
            {
                var inputParmeters = new TransitSignalLoadParameters
                {
                    LocationIdentifier = locationIdentifier,
                    Dates = options.Dates.ToList()
                };
                _logger.LogInformation($"Posting location {locationIdentifier} with {options.Dates.ToList().Count} dates");
                bool posted = _loadLocationDataBlock.Post(inputParmeters);
                if (!posted)
                    _logger.LogError($"Failed to post data for location {locationIdentifier}");
            }
            _loadLocationDataBlock.Complete();
            var results = new List<TransitSignalPriorityResult>();
            while (await _calculateTSPMaxBlock.OutputAvailableAsync())
            {
                while (_calculateTSPMaxBlock.TryReceive(out var result))
                {
                    results.Add(new TransitSignalPriorityResult { LocationIdentifier = result.Item1.LocationIdentifier, TransitSignalPlans = result.Item2 });
                }
            }
            await _calculateTSPMaxBlock.Completion;

            return results;
        }

        public List<TransitSignalPriorityPlan> GetTransitSignalPriorityPlans(
            DateTime startDate,
            DateTime endDate,
            string locationId,
            IReadOnlyList<IndianaEvent> planEvents,
            IReadOnlyList<IndianaEvent> splitEvents,
            List<TransitSignalPriorityCycle> cycles)
        {
            if (planEvents == null)
                throw new ArgumentNullException(nameof(planEvents));
            if (splitEvents == null)
                throw new ArgumentNullException(nameof(splitEvents));
            if (cycles == null)
                throw new ArgumentNullException(nameof(cycles));
            var plans = _planService.GetTransitSignalPriorityBasicPlans(startDate, endDate, locationId, planEvents);

            // Build the complete list of phases from cycles (fill in any gaps)
            var completePhases = GetCompletePhaseList(cycles);


            // Process each distinct plan number
            var tspPlans = new List<TransitSignalPriorityPlan>();
            var distinctPlanNumbers = plans.Select(p => p.PlanNumber).Distinct().ToList();

            foreach (var planNumber in distinctPlanNumbers)
            {             
                var tspPlan = new TransitSignalPriorityPlan
                {
                    PlanNumber = Convert.ToInt32(planNumber)
                };

                // Get all plans that share the current plan number
                var plansForNumber = plans.Where(p => p.PlanNumber == planNumber).ToList();

                // Aggregate cycles that occur within any of these plans' time windows
                var planCycles = new List<TransitSignalPriorityCycle>();
                var validSplitEvents = new List<IndianaEvent>();
                var programmedSplits = new SortedDictionary<int, int>();
                foreach (var plan in plansForNumber)
                {
                    planCycles.AddRange(cycles.Where(c => c.GreenEvent >= plan.Start && c.GreenEvent < plan.End).ToList());
                    validSplitEvents.AddRange(splitEvents.Where(s => s.Timestamp >= plan.Start && s.Timestamp < plan.End).ToList());
                    SetProgrammedSplits(programmedSplits, validSplitEvents);
                }

                if (!planCycles.Any())
                    continue;

                // Group cycles by phase based on the complete phase list
                var phaseCyclesDict = GetPhaseCyclesDictionary(planCycles, completePhases);
                if (!phaseCyclesDict.Any())
                    continue;

                // Determine the highest cycle count among all phases for percentage calculations
                int maxCycleCount = phaseCyclesDict.Values.Max(cycleList => cycleList.Count);

                // Process each phase that has cycles and a corresponding programmed split
                foreach (var phasePair in phaseCyclesDict)
                {
                    int phaseNumber = phasePair.Key;
                    var cyclesForPhase = phasePair.Value;

                    if (cyclesForPhase.Any() && programmedSplits.ContainsKey(phaseNumber))
                    {
                        double skippedCycles = maxCycleCount - cyclesForPhase.Count;
                        double percentSkips = maxCycleCount > 0 ? (skippedCycles / maxCycleCount) * 100 : 0;
                        double percentGapOuts = maxCycleCount > 0
                            ? (cyclesForPhase.Count(c => c.TerminationEvent == 4) / (double)maxCycleCount) * 100
                            : 0;
                        double averageSplit = cyclesForPhase.Any()
                            ? cyclesForPhase.Average(c => c.DurationSeconds)
                            : 0;

                        var tspPhase = new TransitSignalPhase
                        {
                            PhaseNumber = phaseNumber,
                            PercentSkips = percentSkips,
                            PercentGapOuts = percentGapOuts,
                            PercentMaxOutsForceOffs = GetPercentMaxOutForceOffs(planNumber, maxCycleCount, cyclesForPhase) * 100,
                            AverageSplit = averageSplit,
                            MinTime = cyclesForPhase.Min(c => c.DurationSeconds),
                            ProgrammedSplit = programmedSplits[phaseNumber],
                            PercentileSplit85th = GetPercentSplit(cyclesForPhase.Count, 0.85, cyclesForPhase),
                            PercentileSplit50th = GetPercentSplit(cyclesForPhase.Count, 0.5, cyclesForPhase),
                            MinGreen = cyclesForPhase.Min(c => c.GreenDurationSeconds),
                            Yellow = cyclesForPhase.Min(c => c.YellowDurationSeconds),
                            RedClearance = cyclesForPhase.Min(c => c.RedDurationSeconds)
                        };

                        tspPlan.Phases.Add(tspPhase);
                    }
                }

                tspPlans.Add(tspPlan);
            }

            return tspPlans;
        }


        private static double GetPercentMaxOutForceOffs(string planNumber, int highCycleCount, List<TransitSignalPriorityCycle> cycles)
        {
            if (highCycleCount == 0)
            {
                return 0;
            }
            return planNumber == "254" ? Convert.ToDouble(cycles.Count(c => c.TerminationEvent == 5)) / highCycleCount :
                                        Convert.ToDouble(cycles.Count(c => c.TerminationEvent == 6)) / highCycleCount;
        }

        /// <summary>
        /// Creates plan objects based on cleaned events and the given time range.
        /// </summary>
        private List<Plan> CreatePlansFromEvents(List<IndianaEvent> cleanedEvents, DateTime startDate, DateTime endDate)
        {
            var plans = new List<Plan>();

            for (int i = 0; i < cleanedEvents.Count; i++)
            {
                DateTime planStart = cleanedEvents[i].Timestamp;
                DateTime planEnd = (i == cleanedEvents.Count - 1) ? endDate : cleanedEvents[i + 1].Timestamp;

                // If the plan's duration is longer than a day, adjust the end to be at the start of the next day.
                if ((planEnd - planStart).TotalDays > 1)
                {
                    planEnd = planStart.Date.AddDays(1);
                }

                plans.Add(new Plan(cleanedEvents[i].EventParam.ToString(), planStart, planEnd));
            }

            return plans;
        }

        /// <summary>
        /// Builds a complete list of phases based on the cycles provided,
        /// filling in any missing phase numbers between the minimum and maximum.
        /// </summary>
        private List<int> GetCompletePhaseList(List<TransitSignalPriorityCycle> cycles)
        {
            var phaseNumbers = cycles.Select(c => c.PhaseNumber).Distinct().ToList();
            if (!phaseNumbers.Any())
                return new List<int>();

            int minPhase = phaseNumbers.Min();
            int maxPhase = phaseNumbers.Max();

            return Enumerable.Range(minPhase, maxPhase - minPhase + 1).ToList();
        }

        /// <summary>
        /// Groups the given plan cycles into a dictionary keyed by phase number.
        /// </summary>
        private Dictionary<int, List<TransitSignalPriorityCycle>> GetPhaseCyclesDictionary(List<TransitSignalPriorityCycle> planCycles, List<int> allPhases)
        {
            var dictionary = new Dictionary<int, List<TransitSignalPriorityCycle>>();

            foreach (var phase in allPhases)
            {
                // Even if there are no cycles for a phase, add an empty list so the phase is represented.
                var cyclesForPhase = planCycles.Where(c => c.PhaseNumber == phase).ToList();
                dictionary.Add(phase, cyclesForPhase);
            }

            return dictionary;
        }


        private double GetPercentSplit(double highCycleCount, double percentile, List<TransitSignalPriorityCycle> cycles)
        {
            if (cycles.Count <= 2)
                return 0;
            var orderedCycles = cycles.OrderBy(c => c.DurationSeconds).ToList();

            var percentilIndex = percentile * orderedCycles.Count;
            if ((percentilIndex % 1).AreEqual(0))
            {
                return orderedCycles.ElementAt(Convert.ToInt16(percentilIndex) - 1).DurationSeconds;
            }
            else
            {
                var indexMod = percentilIndex % 1;
                //subtracting .5 leaves just the integer after the convert.
                //There was probably another way to do that, but this is easy.
                int indexInt = Convert.ToInt16(percentilIndex - .5);

                var step1 = orderedCycles.ElementAt(Convert.ToInt16(indexInt) - 1).DurationSeconds;
                var step2 = orderedCycles.ElementAt(Convert.ToInt16(indexInt)).DurationSeconds;
                var stepDiff = step2 - step1;
                var step3 = stepDiff * indexMod;
                return step1 + step3;
            }
        }

        public void SetProgrammedSplits(SortedDictionary<int, int> splits, List<IndianaEvent> locationEvents)
        {
            var eventCodes = new List<short>();
            for (short i = 130; i <= 151; i++)
                eventCodes.Add(i);
            var splitsDt = locationEvents.Where(s => eventCodes.Contains(s.EventCode)).OrderBy(s => s.Timestamp);
            foreach (var row in splitsDt)
            {

                if (row.EventCode == 134 && !splits.ContainsKey(1))
                    splits.Add(1, row.EventParam);
                else if (row.EventCode == 134 && row.EventParam > 0)
                    splits[1] = row.EventParam;

                if (row.EventCode == 135 && !splits.ContainsKey(2))
                    splits.Add(2, row.EventParam);
                else if (row.EventCode == 135 && row.EventParam > 0)
                    splits[2] = row.EventParam;

                if (row.EventCode == 136 && !splits.ContainsKey(3))
                    splits.Add(3, row.EventParam);
                else if (row.EventCode == 136 && row.EventParam > 0)
                    splits[3] = row.EventParam;

                if (row.EventCode == 137 && !splits.ContainsKey(4))
                    splits.Add(4, row.EventParam);
                else if (row.EventCode == 137 && row.EventParam > 0)
                    splits[4] = row.EventParam;

                if (row.EventCode == 138 && !splits.ContainsKey(5))
                    splits.Add(5, row.EventParam);
                else if (row.EventCode == 138 && row.EventParam > 0)
                    splits[5] = row.EventParam;

                if (row.EventCode == 139 && !splits.ContainsKey(6))
                    splits.Add(6, row.EventParam);
                else if (row.EventCode == 139 && row.EventParam > 0)
                    splits[6] = row.EventParam;

                if (row.EventCode == 140 && !splits.ContainsKey(7))
                    splits.Add(7, row.EventParam);
                else if (row.EventCode == 140 && row.EventParam > 0)
                    splits[7] = row.EventParam;

                if (row.EventCode == 141 && !splits.ContainsKey(8))
                    splits.Add(8, row.EventParam);
                else if (row.EventCode == 141 && row.EventParam > 0)
                    splits[8] = row.EventParam;

                if (row.EventCode == 142 && !splits.ContainsKey(9))
                    splits.Add(9, row.EventParam);
                else if (row.EventCode == 142 && row.EventParam > 0)
                    splits[9] = row.EventParam;

                if (row.EventCode == 143 && !splits.ContainsKey(10))
                    splits.Add(10, row.EventParam);
                else if (row.EventCode == 143 && row.EventParam > 0)
                    splits[10] = row.EventParam;

                if (row.EventCode == 144 && !splits.ContainsKey(11))
                    splits.Add(11, row.EventParam);
                else if (row.EventCode == 144 && row.EventParam > 0)
                    splits[11] = row.EventParam;

                if (row.EventCode == 145 && !splits.ContainsKey(12))
                    splits.Add(12, row.EventParam);
                else if (row.EventCode == 145 && row.EventParam > 0)
                    splits[12] = row.EventParam;

                if (row.EventCode == 146 && !splits.ContainsKey(13))
                    splits.Add(13, row.EventParam);
                else if (row.EventCode == 146 && row.EventParam > 0)
                    splits[13] = row.EventParam;

                if (row.EventCode == 147 && !splits.ContainsKey(14))
                    splits.Add(14, row.EventParam);
                else if (row.EventCode == 147 && row.EventParam > 0)
                    splits[14] = row.EventParam;

                if (row.EventCode == 148 && !splits.ContainsKey(15))
                    splits.Add(15, row.EventParam);
                else if (row.EventCode == 148 && row.EventParam > 0)
                    splits[15] = row.EventParam;

                if (row.EventCode == 149 && !splits.ContainsKey(16))
                    splits.Add(16, row.EventParam);
                else if (row.EventCode == 149 && row.EventParam > 0)
                    splits[16] = row.EventParam;
            }

            if (splits.Count == 0)
                for (var i = 0; i < 16; i++)
                    splits.Add(i, 0);
        }
    }
}
