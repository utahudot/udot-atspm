using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.ReportApi.Business.LeftTurnGapReport
{
    public class LeftTurnGapDurationAnalysis
    {

        public LeftTurnGapDurationAnalysis()
        {
        }

        public GapDurationResult GetPercentOfGapDuration(
            Approach approach,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            int[] daysOfWeek,
            Location signal,
            int totalActivations,
            List<PhaseLeftTurnGapAggregation> phaseLeftTurnGapAggregations,
            int opposingPhase)
        {
            int numberOfOposingLanes = GetNumberOfOpposingLanes(signal, opposingPhase);
            double criticalGap = GetCriticalGap(numberOfOposingLanes);

            var gapDurationResult = new GapDurationResult
            {
                Capacity = GetGapSummedTotal(criticalGap, phaseLeftTurnGapAggregations),
                AcceptableGaps = GetGapsList(start, end, startTime, endTime, criticalGap, daysOfWeek, phaseLeftTurnGapAggregations),
                Demand = CalculateGapDemand(criticalGap, totalActivations),
                Direction = approach.DirectionType.Abbreviation + approach.Detectors.FirstOrDefault()?.MovementType,
                OpposingDirection = GetOpposingPhaseDirection(signal, opposingPhase)
            };
            if (gapDurationResult.Capacity == 0)
                throw new ArithmeticException("Gap Count cannot be zero");
            gapDurationResult.GapDurationPercent = gapDurationResult.Demand / gapDurationResult.Capacity;
            return gapDurationResult;
        }

        private Dictionary<DateTime, double> GetGapsList(
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            double criticalGap,
            int[] daysOfWeek,
            List<PhaseLeftTurnGapAggregation> phaseLeftTurnGapAggregations
            )
        {
            List<PhaseLeftTurnGapAggregation> amAggregations = new List<PhaseLeftTurnGapAggregation>();
            int gapColumn = GetGapColumn(criticalGap);
            Dictionary<DateTime, double> acceptableGaps = new Dictionary<DateTime, double>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                for (var tempStart = tempDate.Date.Add(startTime); tempStart < tempDate.Date.Add(endTime); tempStart = tempStart.AddMinutes(15))
                {
                    if (daysOfWeek.Contains((int)start.DayOfWeek))
                    {
                        var leftTurnGaps = phaseLeftTurnGapAggregations.Where(p => p.BinStartTime >= tempStart && p.BinStartTime < tempStart.Add(startTime).AddMinutes(15));
                        int count = 0;
                        double sum = 0;
                        if (gapColumn == 12)
                        {
                            count = leftTurnGaps.Sum(l => l.GapCount6 + l.GapCount7 + l.GapCount8 + l.GapCount9);
                            sum = leftTurnGaps.Sum(l => l.SumGapDuration1 / 4.1);
                        }
                        else
                        {
                            count = leftTurnGaps.Sum(l => l.GapCount7 + l.GapCount8 + l.GapCount9);
                            sum = leftTurnGaps.Sum(l => l.SumGapDuration2 / 5.3);
                        }

                        acceptableGaps.Add(tempStart, sum);
                    }
                }
            }
            return acceptableGaps;
        }

        private double GetGapSummedTotal(
            double criticalGap,
            List<PhaseLeftTurnGapAggregation> phaseLeftTurnGapAggregations)
        {
            int gapColumn = GetGapColumn(criticalGap);
            switch (gapColumn)
            {
                case 1: return phaseLeftTurnGapAggregations.Sum(p => p.GapCount1);
                case 2: return phaseLeftTurnGapAggregations.Sum(p => p.GapCount2);
                case 3: return phaseLeftTurnGapAggregations.Sum(p => p.GapCount3);
                case 4: return phaseLeftTurnGapAggregations.Sum(p => p.GapCount4);
                case 5: return phaseLeftTurnGapAggregations.Sum(p => p.GapCount5);
                case 6: return phaseLeftTurnGapAggregations.Sum(p => p.GapCount6);
                case 7: return phaseLeftTurnGapAggregations.Sum(p => p.GapCount7);
                case 8: return phaseLeftTurnGapAggregations.Sum(p => p.GapCount8);
                case 9: return phaseLeftTurnGapAggregations.Sum(p => p.GapCount9);
                case 10: return phaseLeftTurnGapAggregations.Sum(p => p.GapCount10);
                case 11: return phaseLeftTurnGapAggregations.Sum(p => p.GapCount11);
                case 12: return phaseLeftTurnGapAggregations.Sum(p => p.SumGapDuration1);
                case 13: return phaseLeftTurnGapAggregations.Sum(p => p.SumGapDuration2);
                case 14: return phaseLeftTurnGapAggregations.Sum(p => p.SumGapDuration3);
                default: throw new Exception("Gap Column not found");
            }
        }



        public int GetNumberOfOpposingLanes(Location signal, int opposingPhase)
        {
            List<MovementTypes> thruMovements = new List<MovementTypes>() { MovementTypes.T, MovementTypes.TR, MovementTypes.TL };
            return signal
                .Approaches
                .SelectMany(a => a.Detectors)
                .Where(d => d.DetectionTypes.First().Id == DetectionTypes.LLC && thruMovements.Contains(d.MovementType))
                .Count(d => d.Approach.ProtectedPhaseNumber == opposingPhase);
        }

        public string GetOpposingPhaseDirection(Location signal, int opposingPhase)
        {
            return signal
                .Approaches
                .Where(d => d.ProtectedPhaseNumber == opposingPhase)
                .FirstOrDefault()?.DirectionType.Abbreviation;
        }

        //static functions

        public static int GetGapColumn(double criticalGap)
        {
            return criticalGap switch
            {
                4.1 => 12,
                5.3 => 13,
                _ => 12
            };
        }

        public static double GetCriticalGap(int numberOfOposingLanes)
        {
            if (numberOfOposingLanes <= 2)
            {
                return 4.1;
            }
            else
            {
                return 5.3;
            }
        }

        public static double CalculateGapDemand(double criticalGap, int totalActivations)
        {
            return totalActivations * criticalGap;
        }
    }
}
