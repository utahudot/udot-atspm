using ATSPM.Application.Business.Common;
using ATSPM.Application.Extensions;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ATSPM.Application.Business.LeftTurnGapAnalysis
{
    public class LeftTurnGapAnalysisService
    {
        public const short EVENT_GREEN = 1;
        public const short EVENT_RED = 10;
        public const short EVENT_DET = 81;

        public LeftTurnGapAnalysisService()
        {
        }

        public async Task<LeftTurnGapAnalysisResult> GetAnalysisForPhase(Approach approach, List<IndianaEvent> eventLogs, LeftTurnGapAnalysisOptions options, Approach leftTurnPhase)
        {
            var cycleEventsByPhase = new List<IndianaEvent>();

            cycleEventsByPhase.AddRange(eventLogs.Where(x =>
                x.EventParam == approach.ProtectedPhaseNumber &&
                (x.EventCode == EVENT_GREEN || x.EventCode == EVENT_RED)));

            var detectorsToUse = new List<Detector>();
            var detectionTypeStr = "Lane-By-Lane Count";

            //Use only lane-by-lane count detectors if they exists, otherwise check for stop bar
            detectorsToUse = approach.GetAllDetectorsOfDetectionType(DetectionTypes.LLC);

            if (!detectorsToUse.Any())
            {
                detectorsToUse = approach.GetAllDetectorsOfDetectionType(DetectionTypes.SBP);
                detectionTypeStr = "Stop Bar Presence";

                //If no detectors of either type for this approach, skip it
                if (!detectorsToUse.Any())
                    return null;
            }
            var detectorEvents = new List<IndianaEvent>();
            foreach (var detector in detectorsToUse)
            {
                // Check for thru, right, thru-right, and thru-left
                if (IsThruDetector(detector))
                {
                    detectorEvents.AddRange(eventLogs.Where(x =>
                        x.EventCode == EVENT_DET && x.EventParam == detector.DetectorChannel));
                }
            }

            if (detectorEvents.Any() && cycleEventsByPhase.Any())
            {
                var result = GetData(cycleEventsByPhase, detectorEvents, options, detectionTypeStr, approach);
                result.PhaseDescription = $"{leftTurnPhase.DirectionType.Description} Left Phase {leftTurnPhase.ProtectedPhaseNumber} crossing {approach.DirectionType.Description} {string.Join(',', detectorsToUse.Select(d => d.MovementType.GetDescription()))} Phase {approach.ProtectedPhaseNumber}";
                result.ApproachDescription = approach.Description;
                result.LocationDescription = approach.Location.LocationDescription();
                return result;
            }
            return null;
        }

        private bool IsThruDetector(Detector detector)
        {
            return detector.MovementType == MovementTypes.T || detector.MovementType == MovementTypes.R ||
                   detector.MovementType == MovementTypes.TR || detector.MovementType == MovementTypes.TL;
        }

        private int GetOpposingPhase(int phase)
        {
            switch (phase)
            {
                case 2:
                    return 6;
                case 4:
                    return 8;
                case 6:
                    return 2;
                case 8:
                    return 4;
                default:
                    return 0;
            }
        }

        protected LeftTurnGapAnalysisResult GetData(
            List<IndianaEvent> cycleEventsByPhase,
            List<IndianaEvent> detectorEvents,
            LeftTurnGapAnalysisOptions options,
            string detectionTypeStr,
            Approach approach)
        {
            var percentTurnableSeries = new List<DataPointForDouble>();
            var greenList = cycleEventsByPhase.Where(x => x.EventCode == EVENT_GREEN && x.Timestamp >= options.Start && x.Timestamp < options.End)
                .OrderBy(x => x.Timestamp).ToList();
            var redList = cycleEventsByPhase.Where(x => x.EventCode == EVENT_RED && x.Timestamp >= options.Start && x.Timestamp < options.End)
                .OrderBy(x => x.Timestamp).ToList();
            var orderedDetectorCallList = detectorEvents.Where(x => x.EventCode == EVENT_DET && x.Timestamp >= options.Start && x.Timestamp < options.End)
                .OrderBy(x => x.Timestamp).ToList();

            var eventBeforeStart = cycleEventsByPhase.Where(e => e.Timestamp < options.Start && (e.EventCode == EVENT_GREEN || e.EventCode == EVENT_RED)).OrderByDescending(e => e.Timestamp).FirstOrDefault();
            if (eventBeforeStart != null && eventBeforeStart.EventCode == EVENT_GREEN)
            {
                eventBeforeStart.Timestamp = options.Start;
                greenList.Insert(0, eventBeforeStart);
            }
            if (eventBeforeStart != null && eventBeforeStart.EventCode == EVENT_RED)
            {
                eventBeforeStart.Timestamp = options.Start;
                redList.Insert(0, eventBeforeStart);
            }

            var eventAfterEnd = cycleEventsByPhase.Where(e => e.Timestamp > options.End && (e.EventCode == EVENT_GREEN || e.EventCode == EVENT_RED)).OrderBy(e => e.Timestamp).FirstOrDefault();
            if (eventAfterEnd != null && eventAfterEnd.EventCode == EVENT_GREEN)
            {
                eventAfterEnd.Timestamp = options.End;
                greenList.Add(eventAfterEnd);
            }
            if (eventAfterEnd != null && eventAfterEnd.EventCode == EVENT_RED)
            {
                eventAfterEnd.Timestamp = options.End;
                redList.Add(eventAfterEnd);
            }

            double sumDuration1;
            double sumDuration2;
            double sumDuration3;
            double sumGreenTime;

            var phaseTrackerList = GetGapsFromControllerData(
                greenList,
                redList,
                orderedDetectorCallList,
                options,
                out sumDuration1,
                out sumDuration2,
                out sumDuration3,
                out sumGreenTime);

            var highestTotal = 0;
            var gaps1 = new List<DataPointForInt>();
            var gaps2 = new List<DataPointForInt>();
            var gaps3 = new List<DataPointForInt>();
            var gaps4 = new List<DataPointForInt>();
            var gaps5 = new List<DataPointForInt>();
            var gaps6 = new List<DataPointForInt>();
            var gaps7 = new List<DataPointForInt>();
            var gaps8 = new List<DataPointForInt>();
            var gaps9 = new List<DataPointForInt>();
            var gaps10 = new List<DataPointForInt>();

            for (var lowerTimeLimit = options.Start; lowerTimeLimit < options.End; lowerTimeLimit = lowerTimeLimit.AddMinutes(options.BinSize))
            {
                var upperTimeLimit = lowerTimeLimit.AddMinutes(options.BinSize);
                var items = phaseTrackerList.Where(x => x.GreenTime >= lowerTimeLimit && x.GreenTime < upperTimeLimit).ToList();


                if (!items.Any()) continue;
                gaps1.Add(new DataPointForInt(upperTimeLimit, items.Sum(x => x.GapCounter1)));
                gaps2.Add(new DataPointForInt(upperTimeLimit, items.Sum(x => x.GapCounter2)));
                gaps3.Add(new DataPointForInt(upperTimeLimit, items.Sum(x => x.GapCounter3)));
                gaps4.Add(new DataPointForInt(upperTimeLimit, items.Sum(x => x.GapCounter4)));
                var localTotal = items.Sum(x => x.GapCounter1) + items.Sum(x => x.GapCounter2)
                                                               + items.Sum(x => x.GapCounter3) +
                                                               items.Sum(x => x.GapCounter4);
                if (options.Gap5Min.HasValue)
                {
                    int sum = items.Sum(x => x.GapCounter5);
                    gaps5.Add(new DataPointForInt(upperTimeLimit, sum));
                    localTotal += sum;
                }
                if (options.Gap6Min.HasValue)
                {
                    int sum = items.Sum(x => x.GapCounter6);
                    gaps6.Add(new DataPointForInt(upperTimeLimit, sum));
                    localTotal += sum;
                }
                if (options.Gap7Min.HasValue)
                {
                    int sum = items.Sum(x => x.GapCounter7);
                    gaps7.Add(new DataPointForInt(upperTimeLimit, sum));
                    localTotal += sum;
                }
                if (options.Gap8Min.HasValue)
                {
                    int sum = items.Sum(x => x.GapCounter8);
                    gaps8.Add(new DataPointForInt(upperTimeLimit, sum));
                    localTotal += sum;
                }
                if (options.Gap9Min.HasValue)
                {
                    int sum = items.Sum(x => x.GapCounter9);
                    gaps9.Add(new DataPointForInt(upperTimeLimit, sum));
                    localTotal += sum;
                }
                if (options.Gap10Min.HasValue)
                {
                    int sum = items.Sum(x => x.GapCounter10);
                    gaps10.Add(new DataPointForInt(upperTimeLimit, sum));
                    localTotal += sum;
                }
                percentTurnableSeries.Add(new DataPointForDouble(upperTimeLimit, items.Average(x => x.PercentPhaseTurnable) * 100));

                if (localTotal > highestTotal)
                    highestTotal = localTotal;
            }

            return new LeftTurnGapAnalysisResult(
                approach.Location.LocationIdentifier,
                approach.Id,
                approach.ProtectedPhaseNumber,
                approach.Description,
                options.Start,
                options.End,
                detectionTypeStr,
                options.Gap1Min,
                options.Gap1Max,
                gaps1,
                options.Gap2Min,
                options.Gap2Max,
                gaps2,
                options.Gap3Min,
                options.Gap3Max,
                gaps3,
                options.Gap4Min,
                options.Gap4Max,
                gaps4,
                options.Gap5Min,
                options.Gap5Max,
                gaps5,
                options.Gap6Min,
                options.Gap6Max,
                gaps6,
                options.Gap7Min,
                options.Gap7Max,
                gaps7,
                options.Gap8Min,
                options.Gap8Max,
                gaps8,
                options.Gap9Min,
                options.Gap9Max,
                gaps9,
                options.Gap10Min,
                options.Gap10Max,
                gaps10,
                percentTurnableSeries,
                sumDuration1,
                sumDuration2,
                sumDuration3,
                sumGreenTime,
                highestTotal,
                detectionTypeStr,
                options.BinSize,
                options.TrendLineGapThreshold
                );
        }

        public void AddGapToCounters(PhaseLeftTurnGapTracker phaseTracker, double gap, LeftTurnGapAnalysisOptions options)
        {
            if (gap > options.Gap1Min && gap <= options.Gap1Max)
            {
                phaseTracker.GapCounter1++;
            }
            else if (gap > options.Gap2Min && gap <= options.Gap2Max)
            {
                phaseTracker.GapCounter2++;
            }
            else if (gap > options.Gap3Min && gap <= options.Gap3Max)
            {
                phaseTracker.GapCounter3++;
            }

            if (options.Gap4Max == null)
            {
                if (gap > options.Gap4Min)
                {
                    phaseTracker.GapCounter4++;
                }
            }
            else
            {
                if (gap > options.Gap4Min && gap <= options.Gap4Max.Value)
                {
                    phaseTracker.GapCounter4++;
                }
            }

            if (options.Gap5Min != null)
            {
                if (options.Gap5Max != null && gap > options.Gap5Min.Value &&
                    gap > options.Gap5Min && gap <= options.Gap5Max.Value)
                {
                    phaseTracker.GapCounter5++;
                }
            }
            if (options.Gap6Min != null)
            {
                if (options.Gap6Max != null && gap > options.Gap6Min.Value &&
                    gap > options.Gap6Min && gap <= options.Gap6Max.Value)
                {
                    phaseTracker.GapCounter6++;
                }
            }
            if (options.Gap7Min != null)
            {
                if (options.Gap7Max != null && gap > options.Gap7Min.Value &&
                    gap > options.Gap7Min && gap <= options.Gap7Max.Value)
                {
                    phaseTracker.GapCounter7++;
                }
            }
            if (options.Gap8Min != null)
            {
                if (options.Gap8Max != null && gap > options.Gap8Min.Value &&
                    gap > options.Gap8Min && gap <= options.Gap8Max.Value)
                {
                    phaseTracker.GapCounter8++;
                }
            }
            if (options.Gap9Min != null)
            {
                if (options.Gap9Max != null && gap > options.Gap9Min.Value &&
                    gap > options.Gap9Min && gap <= options.Gap9Max.Value)
                {
                    phaseTracker.GapCounter9++;
                }
            }
            if (options.Gap10Min != null)
            {
                if (options.Gap10Max != null && gap > options.Gap10Min.Value &&
                    gap > options.Gap10Min && gap <= options.Gap10Max.Value)
                {
                    phaseTracker.GapCounter10++;
                }
            }
            if (options.Gap10Max != null && gap > options.Gap10Max.Value)
            {
                phaseTracker.GapCounter11++;
            }

        }

        private List<PhaseLeftTurnGapTracker> GetGapsFromControllerData(
            List<IndianaEvent> greenList,
            List<IndianaEvent> redList,
            List<IndianaEvent> orderedDetectorCallList,
            LeftTurnGapAnalysisOptions options,
            out double sumDuration1,
            out double sumDuration2,
            out double sumDuration3,
            out double sumGreenTime)
        {

            var phaseTrackerList = new List<PhaseLeftTurnGapTracker>();
            sumDuration1 = 0;
            sumDuration2 = 0;
            sumDuration3 = 0;
            sumGreenTime = 0;
            if (redList.Any() && greenList.Any())
            {
                foreach (var green in greenList)
                {
                    //Find the corresponding red
                    var red = redList.Where(x => x.Timestamp > green.Timestamp).OrderBy(x => x.Timestamp)
                        .FirstOrDefault();
                    if (red == null)
                        continue;

                    double trendLineGapTimeCounter = 0;

                    var phaseTracker = new PhaseLeftTurnGapTracker
                    { GreenTime = green.Timestamp };

                    var gapsList = new List<IndianaEvent>();
                    gapsList.Add(green);
                    gapsList.AddRange(orderedDetectorCallList.Where(x =>
                        x.Timestamp > green.Timestamp && x.Timestamp < red.Timestamp));
                    gapsList.Add(red);
                    for (var i = 1; i < gapsList.Count; i++)
                    {
                        var gap = gapsList[i].Timestamp.TimeOfDay.TotalSeconds -
                                  gapsList[i - 1].Timestamp.TimeOfDay.TotalSeconds;

                        if (gap < 0) continue;

                        AddGapToCounters(phaseTracker, gap, options);

                        if (gap >= options.TrendLineGapThreshold)
                        {
                            trendLineGapTimeCounter += gap;
                        }

                        if (options.SumDurationGap1.HasValue &&
                            gap >= options.SumDurationGap1.Value)
                        {
                            sumDuration1 += gap;
                        }

                        if (options.SumDurationGap2.HasValue &&
                            gap >= options.SumDurationGap2.Value)
                        {
                            sumDuration2 += gap;
                        }

                        if (options.SumDurationGap3.HasValue &&
                            gap >= options.SumDurationGap3.Value)
                        {
                            sumDuration3 += gap;
                        }
                    }

                    //Decimal rounding errors can cause the number to be > 100
                    var percentTurnable =
                        Math.Min(trendLineGapTimeCounter / (red.Timestamp - green.Timestamp).TotalSeconds, 100);
                    sumGreenTime += (red.Timestamp - green.Timestamp).TotalSeconds;
                    phaseTracker.PercentPhaseTurnable = percentTurnable;
                    phaseTrackerList.Add(phaseTracker);
                }
            }
            return phaseTrackerList;
        }

    }

    public class PhaseLeftTurnGapTracker
    {
        public DateTime GreenTime;
        public int GapCounter1;
        public int GapCounter2;
        public int GapCounter3;
        public int GapCounter4;
        public int GapCounter5;
        public int GapCounter6;
        public int GapCounter7;
        public int GapCounter8;
        public int GapCounter9;
        public int GapCounter10;
        public int GapCounter11;
        public double PercentPhaseTurnable;
    }
}
