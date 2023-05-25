using ATSPM.Application.Reports.Business.LeftTurnGapReport;
using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Legacy.Common.Business.LeftTurnGapReport
{
    public class LeftTurnSplitFailAnalysisService
    {
        private readonly LeftTurnReportPreCheckService leftTurnReportPreCheckService;
        private readonly IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository;

        public LeftTurnSplitFailAnalysisService(
            LeftTurnReportPreCheckService leftTurnReportPreCheckService,
            IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository)
        {
            this.leftTurnReportPreCheckService = leftTurnReportPreCheckService;
            this.approachSplitFailAggregationRepository = approachSplitFailAggregationRepository;
        }

        //public double GetSplitFailPercent(
        //    string signalId,
        //    DirectionTypes directionTypeId,
        //    DateTime start,
        //    DateTime end,
        //    TimeSpan startTime,
        //    TimeSpan endTime)
        //{
        //    var detectors = leftTurnReportPreCheckService.GetLeftTurnDetectors(signalId, directionTypeId);
        //    var approach = leftTurnReportPreCheckService.GetLTPhaseNumberPhaseTypeByDirection(signalId, directionTypeId);
        //    var phase = leftTurnReportPreCheckService.GetOpposingPhase(approach);
        //    List<ApproachSplitFailAggregation> splitFailsAggregates = new List<ApproachSplitFailAggregation>();
        //    for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
        //    {
        //        splitFailsAggregates.AddRange(approachSplitFailAggregationRepository.GetApproachSplitFailsAggregationByApproachIdAndDateRange(detectors.First().ApproachId, tempDate.Date.Add(startTime), tempDate.Date.Add(endTime),
        //            true));
        //        splitFailsAggregates.AddRange(approachSplitFailAggregationRepository.GetApproachSplitFailsAggregationByApproachIdAndDateRange(detectors.First().ApproachId, tempDate.Date.Add(startTime), tempDate.Date.Add(endTime),
        //            false));
        //    }
        //    int cycles = splitFailsAggregates.Sum(s => s.Cycles);
        //    int splitFails = splitFailsAggregates.Sum(s => s.SplitFailures);
        //    if (cycles == 0)
        //        throw new ArithmeticException("Cycles cannot be zero");
        //    return splitFails / cycles;
        //}
    }
}
