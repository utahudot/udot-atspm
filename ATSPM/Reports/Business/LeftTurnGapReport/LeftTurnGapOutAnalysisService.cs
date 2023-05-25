using System;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Data.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using ATSPM.Data.Enums;
using ATSPM.Application.Reports.Business.LeftTurnGapReport;

namespace Legacy.Common.Business.LeftTurnGapReport
{
    public class LeftTurnGapOutAnalysisService
    {
        private readonly LeftTurnReportPreCheckService leftTurnReportPreCheckService;
        private readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;
        private readonly IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository;
        private readonly IDetectorRepository detectorRepository;
        private readonly ISignalRepository signalRepository;

        public LeftTurnGapOutAnalysisService(
            LeftTurnReportPreCheckService leftTurnReportPreCheckService,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository,
            IDetectorRepository detectorRepository,
            ISignalRepository signalRepository

            )
        {
            this.leftTurnReportPreCheckService = leftTurnReportPreCheckService;
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            this.phaseLeftTurnGapAggregationRepository = phaseLeftTurnGapAggregationRepository;
            this.detectorRepository = detectorRepository;
            this.signalRepository = signalRepository;
        }

        //public double GetPercentOfGapDuration(
        //    string signalId,
        //    DirectionTypes directionType,
        //    DateTime start,
        //    DateTime end,
        //    TimeSpan startTime,
        //    TimeSpan endTime)
        //{
        //    //var approach = leftTurnReportPreCheckService. .GetLTPhaseNumberPhaseTypeByDirection(signalId, directionType);
        //    int opposingPhase = leftTurnReportPreCheckService.GetOpposingPhase(approach);
        //    int numberOfOposingLanes = GetNumberOfOpposingLanes(signalId, opposingPhase, start);
        //    double criticalGap = GetCriticalGap(numberOfOposingLanes);

        //    int gapCountTotal = GetGapSummedTotal(signalId, opposingPhase, start, end, startTime, endTime, criticalGap);
        //    //int gapForAllGapsGreaterThanCriticalGapTotal = GetGapForAllGapsGreaterThanCriticalGapTotal(signalId, opposingPhase, start, end, startTime, endTime, criticalGap);
        //    double gapDemand = GetGapDemand(signalId, directionType, start, end, startTime, endTime, criticalGap);
        //    if (gapCountTotal == 0)
        //        throw new ArithmeticException("Gap Count cannot be zero");
        //    return (gapDemand / gapCountTotal);
        //}

        private double GetGapDemand(
            Approach approach,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            double criticalGap)
        {
            var detectors = leftTurnReportPreCheckService.GetLeftTurnDetectors(approach);
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

        private int GetGapSummedTotal(
            string signalId,
            int phaseNumber,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            double criticalGap)
        {
            
            List<PhaseLeftTurnGapAggregation> amAggregations = new List<PhaseLeftTurnGapAggregation>();
            int gapColumn = 1;
            if (criticalGap == 4.1)
                gapColumn = 6;
            else if (criticalGap == 5.3)
                gapColumn = 7;
            int gapTotal = 0;
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
               gapTotal += phaseLeftTurnGapAggregationRepository.GetSummedGapsBySignalIdPhaseNumberAndDateRange(
                    signalId, phaseNumber, tempDate.Date.Add(startTime), tempDate.Date.Add(endTime), gapColumn);
            }
            return gapTotal;
        }

        private double GetCriticalGap(int numberOfOposingLanes)
        {
            if(numberOfOposingLanes <= 2)
            {
                return 4.1;
            }
            else
            {
                return 5.3;          
            }
        }

        public int GetNumberOfOpposingLanes(string signalId, int opposingPhase, DateTime dateOfSignal)
        {
            return signalRepository.GetLatestVersionOfSignal(signalId, dateOfSignal).Approaches
                .SelectMany(a => a.Detectors)
                .Count(d => d.Approach.ProtectedPhaseNumber == opposingPhase);
        }
    }
}
