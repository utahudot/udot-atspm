﻿using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.Common;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.WaitTime
{
    public class WaitTimeService
    {
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
                detectionTypesForApproach = "Advance + Stop Bar Detection";
            }
            else if (hasAdvanceDetection)
            {
                useDroppingAlgorithm = false;
                detectionTypesForApproach = "Advance Detection";
            }
            else if (hasStopBarDetection)
            {
                useDroppingAlgorithm = true;
                detectionTypesForApproach = "Stop Bar Detection";
            }
            else
            {
                useDroppingAlgorithm = false;
                detectionTypesForApproach = "No Detection Found";
            }
        }


        public WaitTimeResult GetChartData(
            WaitTimeOptions options,
            Approach approach,
            List<ControllerEventLog> controllerEventLogs,
            AnalysisPhaseData analysisPhaseData,
            List<PlanSplitMonitorData> plans,
            VolumeCollection volumeCollection
            )
        {
            bool useDroppingAlgorithm;
            string detectionTypesForApproach;
            GetDetectionTypes(approach, out useDroppingAlgorithm, out detectionTypesForApproach);
            var redList = controllerEventLogs.Where(x => x.EventCode == WaitTimeOptions.PHASE_END_RED_CLEARANCE)
                .OrderBy(x => x.Timestamp);
            var greenList = controllerEventLogs.Where(x => x.EventCode == WaitTimeOptions.PHASE_BEGIN_GREEN)
                .OrderBy(x => x.Timestamp);
            var orderedPhaseRegisterList = controllerEventLogs.Where(x =>
                x.EventCode == WaitTimeOptions.PHASE_CALL_REGISTERED ||
                x.EventCode == WaitTimeOptions.PHASE_CALL_DROPPED);
            var waitTimeTrackerList = new List<WaitTimeTracker>();
            var gapOuts = new List<WaitTimePoint>();
            var maxOuts = new List<WaitTimePoint>();
            var forceOffs = new List<WaitTimePoint>();
            var unknowns = new List<WaitTimePoint>();
            try
            {
                foreach (var red in redList)
                {
                    //Find the corresponding green
                    var green = greenList.Where(x => x.Timestamp > red.Timestamp).OrderBy(x => x.Timestamp)
                        .FirstOrDefault();
                    if (green == null)
                        continue;

                    //Find all events between the red and green
                    var phaseCallList = orderedPhaseRegisterList
                        .Where(x => x.Timestamp >= red.Timestamp && x.Timestamp < green.Timestamp)
                        .OrderBy(x => x.Timestamp).ToList();

                    if (!phaseCallList.Any())
                        continue;

                    var exportList = new List<string>();
                    foreach (var row in phaseCallList)
                    {
                        exportList.Add($"{row.SignalId}, {row.Timestamp}, {row.EventCode}, {row.EventParam}");
                    }

                    WaitTimeTracker waitTimeTrackerToFill = null;
                    if (useDroppingAlgorithm &&
                        phaseCallList.Any(x => x.EventCode == WaitTimeOptions.PHASE_CALL_DROPPED))
                    {
                        var lastDroppedPhaseCall =
                            phaseCallList.LastOrDefault(x => x.EventCode == WaitTimeOptions.PHASE_CALL_DROPPED);
                        if (lastDroppedPhaseCall != null)
                        {
                            var lastIndex = phaseCallList.IndexOf(lastDroppedPhaseCall);
                            if (lastIndex + 1 >= phaseCallList.Count)
                                continue;
                            var nextPhaseCall = phaseCallList[lastIndex + 1];

                            waitTimeTrackerToFill = new WaitTimeTracker
                            {
                                Time = green.Timestamp,
                                WaitTimeSeconds = (green.Timestamp - nextPhaseCall.Timestamp).TotalSeconds
                            };
                        }
                    }
                    else if (phaseCallList.Any(x => x.EventCode == WaitTimeOptions.PHASE_CALL_REGISTERED))
                    {
                        var firstPhaseCall = phaseCallList.First(x => x.EventCode == WaitTimeOptions.PHASE_CALL_REGISTERED);
                        //waitTimeTrackerList.Add(new WaitTimeTracker { Time = green.Timestamp, WaitTimeSeconds = (green.Timestamp - firstPhaseCall.Timestamp).TotalSeconds });
                        waitTimeTrackerToFill = new WaitTimeTracker
                        {
                            Time = green.Timestamp,
                            WaitTimeSeconds = (green.Timestamp - firstPhaseCall.Timestamp).TotalSeconds
                        };
                    }
                    else
                    {
                        continue;
                    }

                    //Toss anything longer than 6 minutes - usually a bad value as a result of missing data
                    if (waitTimeTrackerToFill.WaitTimeSeconds > 360)
                        continue;
                    var priorPhase = analysisPhaseData.Cycles.Items.FirstOrDefault(x => x.EndTime == red.Timestamp);
                    if (priorPhase != null)
                    {
                        waitTimeTrackerList.Add(waitTimeTrackerToFill);
                        switch (priorPhase.TerminationEvent)
                        {
                            case 4: //Gap Out
                                gapOuts.Add(new WaitTimePoint(waitTimeTrackerToFill.Time,
                                    waitTimeTrackerToFill.WaitTimeSeconds));
                                break;
                            case 5: //Max Out
                                maxOuts.Add(new WaitTimePoint(waitTimeTrackerToFill.Time,
                                    waitTimeTrackerToFill.WaitTimeSeconds));
                                break;
                            case 6: //Force Off
                                forceOffs.Add(new WaitTimePoint(waitTimeTrackerToFill.Time,
                                    waitTimeTrackerToFill.WaitTimeSeconds));
                                break;
                            case 0:
                                unknowns.Add(new WaitTimePoint(waitTimeTrackerToFill.Time,
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

            var averageWaitTime = new List<WaitTimePoint>();
            try
            {
                if (!waitTimeTrackerList.Any())
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
                            averageWaitTime.Add(new WaitTimePoint(upperTimeLimit, avg));
                        }
                    }
                }


                var splits = plans.Select(p => new PlanSplit(p.StartTime, p.EndTime, approach.ProtectedPhaseNumber, p.Splits[approach.ProtectedPhaseNumber]));
                var waitTimePlans = GetWaitTimePlans(plans, waitTimeTrackerList);
                //}

                return new WaitTimeResult(
                    "Wait Time",
                    approach.Id,
                    approach.Description,
                    approach.ProtectedPhaseNumber,
                    options.Start,
                    options.End,
                    detectionTypesForApproach,
                    waitTimePlans,
                    gapOuts,
                    maxOuts,
                    forceOffs,
                    unknowns,
                    averageWaitTime,
                    volumeCollection.Items.Select(v => new WaitTime.Volume(v.StartTime, v.HourlyVolume)).ToList()
                    );
            }
            catch
            {
                throw;
            }
        }

        private List<PlanWaitTime> GetWaitTimePlans(List<PlanSplitMonitorData> plans,
            List<WaitTimeTracker> waitTimeTrackerList)
        {
            var planWaitTimes = new List<PlanWaitTime>();
            foreach (var plan in plans)
            {
                planWaitTimes.Add(
                    new PlanWaitTime(
                        plan.PlanNumber,
                        plan.StartTime,
                        plan.EndTime,
                        waitTimeTrackerList
                            .Where(x => x.Time > plan.StartTime && x.Time < plan.EndTime)
                            .Select(x => x.WaitTimeSeconds)
                            .DefaultIfEmpty(0)
                            .Average(),
                        waitTimeTrackerList
                            .Where(x => x.Time > plan.StartTime && x.Time < plan.EndTime)
                            .Select(x => x.WaitTimeSeconds)
                            .DefaultIfEmpty(0)
                            .Max()
                    ));
            }
            return planWaitTimes;
        }
    }
}