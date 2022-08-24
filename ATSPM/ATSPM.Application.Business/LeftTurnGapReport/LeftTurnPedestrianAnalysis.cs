using ATSPM.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public static class LeftTurnPedestrianAnalysis
    {
        private static IPhasePedAggregationRepository _phasePedAggregationRepository;
        private static IApproachCycleAggregationRepository _approachCycleAggregationRepository;

        public static double GetPedestrianPercentage(
            string signalId,
            int directionTypeId,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            IPhasePedAggregationRepository phasePedAggregationRepository,
            IApproachCycleAggregationRepository approachCycleAggregationRepository)
        {
            _approachCycleAggregationRepository = approachCycleAggregationRepository;
            _phasePedAggregationRepository = phasePedAggregationRepository;

            var detectors = LeftTurnReportPreCheck.GetLeftTurnDetectors(signalId, directionTypeId);
            var approach = LeftTurnReportPreCheck.GetLTPhaseNumberPhaseTypeByDirection(signalId, directionTypeId);
            int opposingPhase = LeftTurnReportPreCheck.GetOpposingPhase(approach);
            double cycleAverage = GetCycleAverage(signalId, start, end, startTime, endTime, opposingPhase);
            double pedCycleAverage = GetPedCycleAverage(signalId, start, end, startTime, endTime, opposingPhase);
            if (cycleAverage == 0)
                throw new ArithmeticException("Cycle average cannot be zero");
            return pedCycleAverage / cycleAverage;
        }

        private static double GetPedCycleAverage(
            string signalId,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            int phase)
        {
            List<Models.PhasePedAggregation> cycleAggregations = new List<Models.PhasePedAggregation>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                cycleAggregations.AddRange(_phasePedAggregationRepository.GetPhasePedsAggregationBySignalIdPhaseNumberAndDateRange(
                    signalId,
                    phase,
                    tempDate.Date.Add(startTime),
                    tempDate.Date.Add(endTime)));
            }
            return cycleAggregations.Average(a => a.PedCycles);
        }

        private static double GetCycleAverage(
            string signalId,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            int phase)
        {
            List<Models.PhaseCycleAggregation> cycleAggregations = new List<Models.PhaseCycleAggregation>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                cycleAggregations.AddRange(_approachCycleAggregationRepository.GetApproachCyclesAggregationBySignalIdPhaseAndDateRange(
                    signalId,
                    phase,
                    tempDate.Date.Add(startTime),
                    tempDate.Date.Add(endTime)));
            }
            return cycleAggregations.Average(a => a.TotalRedToRedCycles);
        }
    }
}
