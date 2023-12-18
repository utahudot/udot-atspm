using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.TempExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.ReportApi.Business.WaitTime
{
    public class WaitTimeService
    {

        public const int PHASE_BEGIN_GREEN = 1;
        public const int PHASE_END_RED_CLEARANCE = 11;
        public const int PHASE_CALL_REGISTERED = 43;
        public const int PHASE_CALL_DROPPED = 44;
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


        public async Task<WaitTimeResult> GetChartData(
            WaitTimeOptions options,
            PhaseDetail phaseDetail,
            IReadOnlyList<ControllerEventLog> controllerEventLogs,
            AnalysisPhaseData analysisPhaseData,
            IReadOnlyList<PlanSplitMonitorData> plans,
            VolumeCollection volumeCollection
            )
        {
            bool useDroppingAlgorithm;
            string detectionTypesForApproach;
            GetDetectionTypes(phaseDetail.Approach, out useDroppingAlgorithm, out detectionTypesForApproach);
            var redList = controllerEventLogs.Where(x =>
                x.EventCode == PHASE_END_RED_CLEARANCE
                && x.EventParam == phaseDetail.PhaseNumber)
                .OrderBy(x => x.Timestamp);
            var greenList = controllerEventLogs.Where(x =>
            x.EventCode == PHASE_BEGIN_GREEN
            && x.EventParam == phaseDetail.PhaseNumber)
                .OrderBy(x => x.Timestamp);
            var orderedPhaseRegisterList = controllerEventLogs.Where(x =>
                (x.EventCode == PHASE_CALL_REGISTERED ||
                x.EventCode == PHASE_CALL_DROPPED)
                && x.EventParam == phaseDetail.PhaseNumber);
            var waitTimeTrackerList = new List<WaitTimeTracker>();
            var gapOuts = new List<DataPointForDouble>();
            var maxOuts = new List<DataPointForDouble>();
            var forceOffs = new List<DataPointForDouble>();
            var unknowns = new List<DataPointForDouble>();
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
                        exportList.Add($"{row.LocationIdentifier}, {row.Timestamp}, {row.EventCode}, {row.EventParam}");
                    }

                    WaitTimeTracker waitTimeTrackerToFill = null;
                    if (useDroppingAlgorithm &&
                        phaseCallList.Any(x => x.EventCode == PHASE_CALL_DROPPED))
                    {
                        var lastDroppedPhaseCall =
                            phaseCallList.LastOrDefault(x => x.EventCode == PHASE_CALL_DROPPED);
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
                    else if (phaseCallList.Any(x => x.EventCode == PHASE_CALL_REGISTERED))
                    {
                        var firstPhaseCall = phaseCallList.First(x => x.EventCode == PHASE_CALL_REGISTERED);
                        //waitTimeTrackerList.Add(new WaitTimeTracker { Time = green.TimeStamp, WaitTimeSeconds = (green.TimeStamp - firstPhaseCall.TimeStamp).TotalSeconds });
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
                            averageWaitTime.Add(new DataPointForDouble(upperTimeLimit, avg));
                        }
                    }
                }


                var splits = plans.Select(p => new PlanSplit(p.Start, p.End, phaseDetail.PhaseNumber, p.Splits[phaseDetail.PhaseNumber]));
                var waitTimePlans = GetWaitTimePlans(plans, waitTimeTrackerList);
                //}

                return new WaitTimeResult(
                    "Wait Time",
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
                    volumeCollection.Items.Select(v => new DataPointForInt(v.StartTime, v.HourlyVolume)).ToList()
                    );
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