﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.WaitTime/WaitTimeService.cs
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

using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.PhaseTermination;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.TempExtensions;

namespace Utah.Udot.Atspm.Business.WaitTime
{
    public class WaitTimeService
    {
        private readonly CycleService cycleService;

        public WaitTimeService(CycleService cycleService)
        {
            this.cycleService = cycleService;
        }

        public class WaitTimeTracker
        {
            public DateTime Time;
            public double WaitTimeSeconds;
        }

        public void GetDetectionTypes(Approach approach, out bool useDroppingAlgorithm, out string detectionTypesForApproach)
        {
            var hasAdvanceDetection = approach.GetAllDetectorsOfDetectionType(Data.Enums.DetectionTypes.AC).Any();
            var hasStopBarDetection = approach.GetAllDetectorsOfDetectionType(Data.Enums.DetectionTypes.SBP).Any();
            if (hasAdvanceDetection && hasStopBarDetection)
            {
                useDroppingAlgorithm = true;
                detectionTypesForApproach = "Advanced Count Detection + Stop Bar Presence Detection";
            }
            else if (hasAdvanceDetection)
            {
                useDroppingAlgorithm = false;
                detectionTypesForApproach = "Advanced Count Detection";
            }
            else if (hasStopBarDetection)
            {
                useDroppingAlgorithm = true;
                detectionTypesForApproach = "Stop Bar Presence Detection";
            }
            else
            {
                useDroppingAlgorithm = false;
                detectionTypesForApproach = "No Detection Found";
            }
        }


        public async Task<WaitTimeResult> GetChartData(
            WaitTimeOptions options,
            PhaseDetail phaseDetail,
            IReadOnlyList<IndianaEvent> events,
            AnalysisPhaseData analysisPhaseData,
            IReadOnlyList<PlanSplitMonitorData> plans
            )
        {
            bool useDroppingAlgorithm;
            string detectionTypesForApproach;
            GetDetectionTypes(phaseDetail.Approach, out useDroppingAlgorithm, out detectionTypesForApproach);
            var detectorsForVolume = phaseDetail.Approach.GetDetectorsForMetricType(32);
            var channels = detectorsForVolume.Select(x => x.DetectorChannel).ToList();
            var volume = new VolumeCollection(
           options.Start,
            options.End,
               events.Where(e => e.EventCode == 82 && channels.Contains(e.EventParam)).ToList(),
               options.BinSize);
            var cycleEvents = events.Where(x =>
                (x.EventCode == 11 || x.EventCode == 1)
                && x.EventParam == phaseDetail.PhaseNumber);
            //var greenList = events.Where(x =>
            //x.EventCode == PHASE_BEGIN_GREEN
            //&& x.EventParam == phaseDetail.PhaseNumber)
            //.OrderBy(x => x.Timestamp);
            var orderedPhaseRegisterList = events.Where(x =>
                (x.EventCode == 43 ||
                x.EventCode == 44)
                && x.EventParam == phaseDetail.PhaseNumber);
            var waitTimeTrackerList = new List<WaitTimeTracker>();
            var gapOuts = new List<DataPointForDouble>();
            var maxOuts = new List<DataPointForDouble>();
            var forceOffs = new List<DataPointForDouble>();
            var unknowns = new List<DataPointForDouble>();
            try
            {
                var waitTimeCycles = await cycleService.GetWaitTimeCyclesAsync(cycleEvents.ToList(), orderedPhaseRegisterList.ToList(), options.Start, options.End);
                foreach (var cycle in waitTimeCycles)
                {
                    ////Find the corresponding green
                    //var green = greenList.Where(x => x.Timestamp > red.Timestamp).OrderBy(x => x.Timestamp)
                    //    .FirstOrDefault();
                    //if (green == null)
                    //    continue;

                    //Find all events between the red and green
                    //var phaseCallList = orderedPhaseRegisterList
                    //    .Where(x => x.Timestamp >= cycle.RedEvent && x.Timestamp < cycle.GreenEvent)
                    //    .OrderBy(x => x.Timestamp).ToList();

                    if (!cycle.PhaseRegisterDroppedCalls.Any())
                        continue;

                    var exportList = new List<string>();
                    foreach (var row in cycle.PhaseRegisterDroppedCalls)
                    {
                        exportList.Add($"{row.LocationIdentifier}, {row.Timestamp}, {row.EventCode}, {row.EventParam}");
                    }

                    WaitTimeTracker waitTimeTrackerToFill = null;
                    if (useDroppingAlgorithm &&
                        cycle.PhaseRegisterDroppedCalls.Any(x => x.EventCode == 44))
                    {
                        var lastDroppedPhaseCall =
                            cycle.PhaseRegisterDroppedCalls.LastOrDefault(x => x.EventCode == 44);
                        if (lastDroppedPhaseCall != null)
                        {
                            var lastIndex = cycle.PhaseRegisterDroppedCalls.IndexOf(lastDroppedPhaseCall);
                            if (lastIndex + 1 >= cycle.PhaseRegisterDroppedCalls.Count)
                                continue;
                            var nextPhaseCall = cycle.PhaseRegisterDroppedCalls[lastIndex + 1];

                            waitTimeTrackerToFill = new WaitTimeTracker
                            {
                                Time = cycle.GreenEvent,
                                WaitTimeSeconds = (cycle.GreenEvent - nextPhaseCall.Timestamp).TotalSeconds
                            };
                        }
                    }
                    else if (cycle.PhaseRegisterDroppedCalls.Any(x => x.EventCode == 43))
                    {
                        var firstPhaseCall = cycle.PhaseRegisterDroppedCalls.First(x => x.EventCode == 43);
                        //waitTimeTrackerList.Add(new WaitTimeTracker { Time = green.TimeStamp, WaitTimeSeconds = (green.TimeStamp - firstPhaseCall.TimeStamp).TotalSeconds });
                        waitTimeTrackerToFill = new WaitTimeTracker
                        {
                            Time = cycle.GreenEvent,
                            WaitTimeSeconds = (cycle.GreenEvent - firstPhaseCall.Timestamp).TotalSeconds
                        };
                    }
                    else
                    {
                        continue;
                    }

                    //Toss anything longer than 6 minutes - usually a bad value as a result of missing data
                    if (waitTimeTrackerToFill == null || waitTimeTrackerToFill.WaitTimeSeconds > 360)
                        continue;
                    var priorPhase = analysisPhaseData.Cycles.Cycles.FirstOrDefault(x => x.EndTime == cycle.RedEvent);
                    if (priorPhase != null)
                    {
                        waitTimeTrackerList.Add(waitTimeTrackerToFill);
                        switch (priorPhase.TerminationEvent)
                        {
                            case 4: //Gap Out
                                gapOuts.Add(new DataPointForDouble(waitTimeTrackerToFill.Time,
                                    waitTimeTrackerToFill.WaitTimeSeconds));
                                break;
                            case 5: //Max Out
                                maxOuts.Add(new DataPointForDouble(waitTimeTrackerToFill.Time,
                                    waitTimeTrackerToFill.WaitTimeSeconds));
                                break;
                            case 6: //Force Off
                                forceOffs.Add(new DataPointForDouble(waitTimeTrackerToFill.Time,
                                    waitTimeTrackerToFill.WaitTimeSeconds));
                                break;
                            case 0:
                                unknowns.Add(new DataPointForDouble(waitTimeTrackerToFill.Time,
                                    waitTimeTrackerToFill.WaitTimeSeconds));
                                break;
                        }
                    }
                }
            }
            catch
            {
                throw;
            }

            var averageWaitTime = new List<DataPointForDouble>();
            try
            {
                if (waitTimeTrackerList.Any())
                {
                    for (var lowerTimeLimit = options.Start;
                        lowerTimeLimit < options.End;
                        lowerTimeLimit = lowerTimeLimit.AddMinutes(15))
                    {
                        var upperTimeLimit = lowerTimeLimit.AddMinutes(15);
                        var items = waitTimeTrackerList.Where(x => x.Time > lowerTimeLimit && x.Time < upperTimeLimit);
                        if (items.Any())
                        {
                            var avg = items.Average(x => x.WaitTimeSeconds);
                            averageWaitTime.Add(new DataPointForDouble(upperTimeLimit, avg));
                        }
                    }
                }


                var splits = new List<DataPointForDouble>();
                foreach (var plan in plans)
                {
                    var splitForPhase = plan.Splits.Where(s => s.Key == phaseDetail.PhaseNumber).FirstOrDefault();
                    splits.Add(new DataPointForDouble(plan.Start, splitForPhase.Value));
                }
                var waitTimePlans = GetWaitTimePlans(plans, waitTimeTrackerList);
                //}

                var result = new WaitTimeResult(
                    phaseDetail.Approach.Location.LocationIdentifier,
                    phaseDetail.Approach.Id,
                    phaseDetail.Approach.Description,
                    phaseDetail.Approach.ProtectedPhaseNumber,
                    options.Start,
                    options.End,
                    detectionTypesForApproach,
                    waitTimePlans,
                    gapOuts,
                    maxOuts,
                    forceOffs,
                    unknowns,
                    averageWaitTime,
                    volume.Items.Select(v => new DataPointForInt(v.StartTime, v.HourlyVolume)).ToList(),
                    splits
                    );
                result.LocationDescription = phaseDetail.Approach.Location.LocationDescription();
                return result;
            }
            catch
            {
                throw;
            }
        }

        private IReadOnlyList<PlanWaitTime> GetWaitTimePlans(IReadOnlyList<PlanSplitMonitorData> plans,
            IReadOnlyList<WaitTimeTracker> waitTimeTrackerList)
        {
            var planWaitTimes = new List<PlanWaitTime>();
            foreach (var plan in plans)
            {
                planWaitTimes.Add(
                    new PlanWaitTime(
                        plan.PlanNumber,
                        plan.Start,
                        plan.End,
                        waitTimeTrackerList
                            .Where(x => x.Time > plan.Start && x.Time < plan.End)
                            .Select(x => x.WaitTimeSeconds)
                            .DefaultIfEmpty(0)
                            .Average(),
                        waitTimeTrackerList
                            .Where(x => x.Time > plan.Start && x.Time < plan.End)
                            .Select(x => x.WaitTimeSeconds)
                            .DefaultIfEmpty(0)
                            .Max()
                    ));
            }
            return planWaitTimes;
        }
    }
}