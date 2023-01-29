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
    public class LeftTurnPedestrianAnalysisService
    {
        private readonly LeftTurnReportPreCheckService leftTurnReportPreCheckService;
        private readonly IPhasePedAggregationRepository phasePedAggregationRepository;

        public LeftTurnPedestrianAnalysisService(
            LeftTurnReportPreCheckService leftTurnReportPreCheckService,
            IPhasePedAggregationRepository phasePedAggregationRepository
            )
        {
            this.leftTurnReportPreCheckService = leftTurnReportPreCheckService;
            this.phasePedAggregationRepository = phasePedAggregationRepository;
        }

        public double GetPedestrianPercentage(string signalId, DirectionTypes directionTypeId, DateTime start, DateTime end, TimeSpan startTime, TimeSpan endTime)
        {
            var detectors = leftTurnReportPreCheckService.GetLeftTurnDetectors(signalId, directionTypeId);
            var approach = leftTurnReportPreCheckService.GetLTPhaseNumberPhaseTypeByDirection(signalId, directionTypeId);
            int opposingPhase = leftTurnReportPreCheckService.GetOpposingPhase(approach);
            double cycleAverage =  GetCycleAverage(signalId, start, end, startTime, endTime, opposingPhase);
            double pedCycleAverage = GetPedCycleAverage(signalId, start, end, startTime, endTime, opposingPhase);
            if(cycleAverage == 0)
                throw new ArithmeticException("Cycle average cannot be zero");
            return pedCycleAverage / cycleAverage;
        }

        private double GetPedCycleAverage(string signalId, DateTime start, DateTime end, TimeSpan startTime, TimeSpan endTime, int phase)
        {
            List<PhasePedAggregation> cycleAggregations = new List<PhasePedAggregation>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                cycleAggregations.AddRange(phasePedAggregationRepository.GetPhasePedsAggregationBySignalIdPhaseNumberAndDateRange(signalId, phase, tempDate.Date.Add(startTime), tempDate.Date.Add(endTime)));
            }
            return cycleAggregations.Average(a => a.PedCycles);
        }

        private double GetCycleAverage(string signalId, DateTime start, DateTime end, TimeSpan startTime, TimeSpan endTime, int phase)
        {
            List<PhaseCycleAggregation> cycleAggregations = new List<PhaseCycleAggregation>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                cycleAggregations.AddRange(phasePedAggregationRepository.GetApproachCyclesAggregationBySignalIdPhaseAndDateRange(signalId, phase, tempDate.Date.Add(startTime), tempDate.Date.Add(endTime)));
            }
            return cycleAggregations.Average(a => a.TotalRedToRedCycles);
        }
    }
}
