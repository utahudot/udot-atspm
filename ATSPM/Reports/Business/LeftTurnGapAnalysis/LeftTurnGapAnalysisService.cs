﻿using ATSPM.Data.Models;
using ATSPM.Application.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Data.Enums;
using Microsoft.Extensions.Options;
using ATSPM.Application.Repositories;

namespace ATSPM.Application.Reports.Business.LeftTurnGapAnalysis
{
    public class LeftTurnGapAnalysisService
    {
        public const int EVENT_GREEN = 1;
        public const int EVENT_RED = 10;
        public const int EVENT_DET = 81;

        public LeftTurnGapAnalysisService()
        {
        }

        public LeftTurnGapAnalysisResult GetChartData(
            LeftTurnGapAnalysisOptions options,
            Approach approach,
            List<ControllerEventLog> eventLogs)
        {
            var phaseEvents = new List<ControllerEventLog>();

            phaseEvents.AddRange(eventLogs.Where(x =>
                x.EventParam == approach.ProtectedPhaseNumber &&
                (x.EventCode == EVENT_GREEN || x.EventCode == EVENT_RED)));

            var detectorsToUse = new List<Data.Models.Detector>();
            var detectionTypeStr = "Detector Type: Lane-By-Lane Count";

            //Use only lane-by-lane count detectors if they exists, otherwise check for stop bar
            detectorsToUse = approach.GetAllDetectorsOfDetectionType(DetectionTypes.LLC);

            if (!detectorsToUse.Any())
            {
                detectorsToUse = approach.GetAllDetectorsOfDetectionType(DetectionTypes.SBP);
                detectionTypeStr = "Detector Type: Stop Bar Presence";

                //If no detectors of either type for this approach, skip it
                if (!detectorsToUse.Any())
                    return null;
            }

            foreach (var detector in detectorsToUse)
            {
                // Check for thru, right, thru-right, and thru-left
                if (!IsThruDetector(detector)) continue;

                phaseEvents.AddRange(eventLogs.Where(x =>
                    x.EventCode == EVENT_DET && x.EventParam == detector.DetChannel));
            }

            if (phaseEvents.Any())
            {
                return GetData(phaseEvents, options, detectionTypeStr, approach);
            }
            return null;
        }

        private bool IsThruDetector(Data.Models.Detector detector)
        {
            return detector.MovementTypeId == MovementTypes.T || detector.MovementTypeId == MovementTypes.R ||
                   detector.MovementTypeId == MovementTypes.TR || detector.MovementTypeId == MovementTypes.TL;
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
            List<ControllerEventLog> events,
            LeftTurnGapAnalysisOptions options,
            string detectionTypeStr,
            Approach approach)
        {
            var percentTurnableSeries = new List<PercentTurnableSeries>();
            var greenList = events.Where(x => x.EventCode == EVENT_GREEN && x.Timestamp >= options.StartDate && x.Timestamp < options.EndDate)
                .OrderBy(x => x.Timestamp).ToList();
            var redList = events.Where(x => x.EventCode == EVENT_RED && x.Timestamp >= options.StartDate && x.Timestamp < options.EndDate)
                .OrderBy(x => x.Timestamp).ToList();
            var orderedDetectorCallList = events.Where(x => x.EventCode == EVENT_DET && x.Timestamp >= options.StartDate && x.Timestamp < options.EndDate)
                .OrderBy(x => x.Timestamp).ToList();

            var eventBeforeStart = events.Where(e => e.Timestamp < options.StartDate && (e.EventCode == EVENT_GREEN || e.EventCode == EVENT_RED)).OrderByDescending(e => e.Timestamp).FirstOrDefault();
            if (eventBeforeStart != null && eventBeforeStart.EventCode == EVENT_GREEN)
            {
                eventBeforeStart.Timestamp = options.StartDate;
                greenList.Insert(0, eventBeforeStart);
            }
            if (eventBeforeStart != null && eventBeforeStart.EventCode == EVENT_RED)
            {
                eventBeforeStart.Timestamp = options.StartDate;
                redList.Insert(0, eventBeforeStart);
            }

            var eventAfterEnd = events.Where(e => e.Timestamp > options.EndDate && (e.EventCode == EVENT_GREEN || e.EventCode == EVENT_RED)).OrderBy(e => e.Timestamp).FirstOrDefault();
            if (eventAfterEnd != null && eventAfterEnd.EventCode == EVENT_GREEN)
            {
                eventAfterEnd.Timestamp = options.EndDate;
                greenList.Add(eventAfterEnd);
            }
            if (eventAfterEnd != null && eventAfterEnd.EventCode == EVENT_RED)
            {
                eventAfterEnd.Timestamp = options.EndDate;
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
            var gaps1 = new List<GapCount>();
            var gaps2 = new List<GapCount>();
            var gaps3 = new List<GapCount>();
            var gaps4 = new List<GapCount>();
            var gaps5 = new List<GapCount>();
            var gaps6 = new List<GapCount>();
            var gaps7 = new List<GapCount>();
            var gaps8 = new List<GapCount>();
            var gaps9 = new List<GapCount>();
            var gaps10 = new List<GapCount>();
            var gaps11 = new List<GapCount>();

            for (var lowerTimeLimit = options.StartDate; lowerTimeLimit < options.EndDate; lowerTimeLimit = lowerTimeLimit.AddMinutes(options.BinSize))
            {
                var upperTimeLimit = lowerTimeLimit.AddMinutes(options.BinSize);
                var items = phaseTrackerList.Where(x => x.GreenTime >= lowerTimeLimit && x.GreenTime < upperTimeLimit).ToList();


                if (!items.Any()) continue;
                gaps1.Add(new GapCount(upperTimeLimit, items.Sum(x => x.GapCounter1)));
                gaps2.Add(new GapCount(upperTimeLimit, items.Sum(x => x.GapCounter2)));
                gaps3.Add(new GapCount(upperTimeLimit, items.Sum(x => x.GapCounter3)));
                gaps4.Add(new GapCount(upperTimeLimit, items.Sum(x => x.GapCounter4)));
                var localTotal = items.Sum(x => x.GapCounter1) + items.Sum(x => x.GapCounter2)
                                                               + items.Sum(x => x.GapCounter3) +
                                                               items.Sum(x => x.GapCounter4);
                if (options.Gap5Min.HasValue)
                {
                    int sum = items.Sum(x => x.GapCounter5);
                    gaps5.Add(new GapCount(upperTimeLimit, sum));
                    localTotal += sum;
                }
                if (options.Gap6Min.HasValue)
                {
                    int sum = items.Sum(x => x.GapCounter6);
                    gaps6.Add(new GapCount(upperTimeLimit, sum));
                    localTotal += sum;
                }
                if (options.Gap7Min.HasValue)
                {
                    int sum = items.Sum(x => x.GapCounter7);
                    gaps7.Add(new GapCount(upperTimeLimit, sum));
                    localTotal += sum;
                }
                if (options.Gap8Min.HasValue)
                {
                    int sum = items.Sum(x => x.GapCounter8);
                    gaps8.Add(new GapCount(upperTimeLimit, sum));
                    localTotal += sum;
                }
                if (options.Gap9Min.HasValue)
                {
                    int sum = items.Sum(x => x.GapCounter9);
                    gaps9.Add(new GapCount(upperTimeLimit, sum));
                    localTotal += sum;
                }
                if (options.Gap10Min.HasValue)
                {
                    int sum = items.Sum(x => x.GapCounter10);
                    gaps10.Add(new GapCount(upperTimeLimit, sum));
                    localTotal += sum;
                }
                if (options.Gap11Min.HasValue)
                {
                    int sum = items.Sum(x => x.GapCounter11);
                    gaps11.Add(new GapCount(upperTimeLimit, sum));
                    localTotal += sum;
                }
                percentTurnableSeries.Add(new PercentTurnableSeries(upperTimeLimit, items.Average(x => x.PercentPhaseTurnable) * 100));

                if (localTotal > highestTotal)
                    highestTotal = localTotal;
            }

            return new LeftTurnGapAnalysisResult(
                approach.Signal.SignalId,
                approach.Id,
                approach.ProtectedPhaseNumber,
                approach.Description,
                options.StartDate,
                options.EndDate,
                detectionTypeStr,
                options.Gap1Max,
                gaps1,
                options.Gap2Max,
                gaps2,
                options.Gap3Max,
                gaps3,
                options.Gap4Max,
                gaps4,
                options.Gap5Max,
                gaps5,
                options.Gap6Max,
                gaps6,
                options.Gap7Max,
                gaps7,
                options.Gap8Max,
                gaps8,
                options.Gap9Max,
                gaps9,
                options.Gap10Max,
                gaps10,
                options.Gap11Max,
                gaps11,
                percentTurnableSeries,
                sumDuration1,
                sumDuration2,
                sumDuration3,
                sumGreenTime,
                highestTotal,
                detectionTypeStr
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
            List<ControllerEventLog> greenList,
            List<ControllerEventLog> redList,
            List<ControllerEventLog> orderedDetectorCallList,
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

                    var gapsList = new List<ControllerEventLog>();
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