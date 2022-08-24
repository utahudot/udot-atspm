using ATSPM.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public static class LeftTurnGapOutAnalysis
    {
        public static double GetPercentOfGapDuration(
            string signalId,
            int directionTypeId,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository,
            IDetectorRepository detectorRepository)
        {
            var approach = LeftTurnReportPreCheck.GetLTPhaseNumberPhaseTypeByDirection(signalId, directionTypeId);
            int opposingPhase = LeftTurnReportPreCheck.GetOpposingPhase(approach);
            int numberOfOposingLanes = GetNumberOfOpposingLanes(signalId, opposingPhase, detectorRepository);
            double criticalGap = GetCriticalGap(numberOfOposingLanes);

            int gapCountTotal = GetGapSummedTotal(
                signalId,
                opposingPhase,
                start,
                end,
                startTime,
                endTime,
                criticalGap,
                phaseLeftTurnGapAggregationRepository);
            //int gapForAllGapsGreaterThanCriticalGapTotal = GetGapForAllGapsGreaterThanCriticalGapTotal(signalId, opposingPhase, start, end, startTime, endTime, criticalGap);
            double gapDemand = GetGapDemand(
                signalId,
                directionTypeId,
                start,
                end,
                startTime,
                endTime,
                criticalGap,
                detectorEventCountAggregationRepository);
            if (gapCountTotal == 0)
                throw new ArithmeticException("Gap Count cannot be zero");
            return gapDemand / gapCountTotal;
        }

        private static double GetGapDemand(
            string signalId,
            int directionTypeId,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            double criticalGap,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository)
        {
            var detectors = LeftTurnReportPreCheck.GetLeftTurnDetectors(signalId, directionTypeId);
            int totalActivations = 0;
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                foreach (var detector in detectors)
                {
                    totalActivations += detectorEventCountAggregationRepository.GetDetectorEventCountSumAggregationByDetectorIdAndDateRange(
                        detector.Id,
                        tempDate.Date.Add(startTime),
                        tempDate.Date.Add(endTime));
                }
            }
            return totalActivations * criticalGap;
        }

        private static int GetGapSummedTotal(
            string signalId,
            int phaseNumber,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            double criticalGap,
            IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository)
        {
            List<Models.PhaseLeftTurnGapAggregation> amAggregations = new List<Models.PhaseLeftTurnGapAggregation>();
            int gapColumn = 1;
            if (criticalGap == 4.1)
                gapColumn = 6;
            else if (criticalGap == 5.3)
                gapColumn = 7;
            int gapTotal = 0;
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                gapTotal += Convert.ToInt32(phaseLeftTurnGapAggregationRepository.GetSummedGapsBySignalIdPhaseNumberAndDateRange(
                    signalId,
                    phaseNumber,
                    tempDate.Date.Add(startTime),
                    tempDate.Date.Add(endTime),
                    gapColumn));
            }
            return gapTotal;
        }

        private static double GetCriticalGap(int numberOfOposingLanes)
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

        public static int GetNumberOfOpposingLanes(
            string signalId,
            int opposingPhase,
            IDetectorRepository detectorRepository)
        {
            return detectorRepository.GetDetectorsBySignalID(signalId).Count(d => d.Approach.ProtectedPhaseNumber == opposingPhase);
        }
    }
}
