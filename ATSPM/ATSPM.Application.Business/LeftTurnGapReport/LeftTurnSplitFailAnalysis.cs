using ATSPM.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public static class LeftTurnSplitFailAnalysis
    {
        public static double GetSplitFailPercent(
            string signalId,
            int directionTypeId,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            IApproachSplitFailAggregationRepository splitFailAggregationRepository)
        {
            var detectors = LeftTurnReportPreCheck.GetLeftTurnDetectors(signalId, directionTypeId);
            var approach = LeftTurnReportPreCheck.GetLTPhaseNumberPhaseTypeByDirection(signalId, directionTypeId);
            var phase = LeftTurnReportPreCheck.GetOpposingPhase(approach);
            List<Models.ApproachSplitFailAggregation> splitFailsAggregates = new List<Models.ApproachSplitFailAggregation>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                splitFailsAggregates.AddRange(splitFailAggregationRepository.GetApproachSplitFailsAggregationByApproachIdAndDateRange(
                    detectors.First().ApproachId,
                    tempDate.Date.Add(startTime),
                    tempDate.Date.Add(endTime),
                    true));
                splitFailsAggregates.AddRange(splitFailAggregationRepository.GetApproachSplitFailsAggregationByApproachIdAndDateRange(
                    detectors.First().ApproachId,
                    tempDate.Date.Add(startTime),
                    tempDate.Date.Add(endTime),
                    false));
            }
            int cycles = splitFailsAggregates.Sum(s => s.Cycles);
            int splitFails = splitFailsAggregates.Sum(s => s.SplitFailures);
            if (cycles == 0)
                throw new ArithmeticException("Cycles cannot be zero");
            return splitFails / cycles;
        }
    }
}
