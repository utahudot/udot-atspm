using ATSPM.Data.Models;
using ATSPM.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.LeftTurnGapReport
{
    public class LeftTurnPedestrianAnalysisService
    {
        private readonly ISignalRepository _signalRepository;
        private readonly IApproachRepository _approachRepository;
        private readonly IPhasePedAggregationRepository _phasePedAggregationRepository;
        private readonly IApproachCycleAggregationRepository _approachCycleAggregationRepository;

        public LeftTurnPedestrianAnalysisService(
            ISignalRepository signalRepository,
            IApproachRepository approachRepository,
            IPhasePedAggregationRepository phasePedAggregationRepository,
            IApproachCycleAggregationRepository approachCycleAggregationRepository)
        {
            _signalRepository = signalRepository;
            _approachRepository = approachRepository;
            _phasePedAggregationRepository = phasePedAggregationRepository;
            _approachCycleAggregationRepository = approachCycleAggregationRepository;
        }
        public PedActuationResult GetPedestrianPercentage(
            string signalId,
            int approachId,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            int[] daysOfWeek)
        {
            var signal = _signalRepository.GetLatestVersionOfSignal(signalId, start);
            var approach = signal.Approaches.Where(a => a.Id == approachId).FirstOrDefault();
            var detectors = LeftTurnReportPreCheck.GetLeftTurnDetectors(approachId, _approachRepository);
            int opposingPhase = LeftTurnReportPreCheck.GetOpposingPhase(approach);
            var cycleAverage = GetCycleAverage(signalId, start, end, startTime, endTime, opposingPhase, daysOfWeek);
            var pedCycleAverage = GetPedCycleAverage(signalId, start, end, startTime, endTime, opposingPhase, daysOfWeek);

            Dictionary<DateTime, double> cycleList = new Dictionary<DateTime, double>();
            foreach (var avg in cycleAverage.CycleAverageList)
            {
                if (avg.Value != 0)
                {
                    var pedAvg = pedCycleAverage.PedCycleAverageList.FirstOrDefault(p => p.Key == avg.Key);
                    cycleList.Add(avg.Key, pedAvg.Value / avg.Value);
                }
            }
            var result = new PedActuationResult();
            if (cycleAverage.CycleAverage == 0)
            {
                result.CyclesWithPedCallsPercent = 0;
                result.CyclesWithPedCallsNum = 0;
            }
            else
            {
                result.CyclesWithPedCallsPercent = pedCycleAverage.PedCycleAverage / cycleAverage.CycleAverage;
                result.CyclesWithPedCallsNum = (int)pedCycleAverage.PedCycleAverage;
            }
            result.PercentCyclesWithPedsList = cycleList;
            result.Direction = approach.DirectionType.Abbreviation + approach.Detectors.FirstOrDefault()?.MovementType.Abbreviation;
            result.OpposingDirection = signal.Approaches.Where(a => a.ProtectedPhaseNumber == opposingPhase).FirstOrDefault()?.DirectionType.Abbreviation;

            return result;
        }

        private PedCycleAverageResult GetPedCycleAverage(
            string signalId,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            int phase,
            int[] daysOfWeek)
        {
            List<PhasePedAggregation> cycleAggregations = new List<PhasePedAggregation>();
            List<double> hourlyPedCycles = new List<double>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                if (daysOfWeek.Contains((int)start.DayOfWeek))
                {
                    var pedAgg = _phasePedAggregationRepository.GetPhasePedsAggregationBySignalIdPhaseNumberAndDateRange(
                        signalId,
                        phase,
                        tempDate.Date.Add(startTime),
                        tempDate.Date.Add(endTime));
                    hourlyPedCycles.Add(pedAgg.Sum(p => p.PedCycles));
                    cycleAggregations.AddRange(pedAgg);
                }
            }
            double averagePedCycles = 0;
            if (cycleAggregations.Any())
            {
                averagePedCycles = hourlyPedCycles.Average(a => a);
            }
            Dictionary<DateTime, double> cycleList = new Dictionary<DateTime, double>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                if (daysOfWeek.Contains((int)start.DayOfWeek))
                    for (var tempstart = tempDate.Date.Add(startTime); tempstart < tempDate.Add(endTime); tempstart = tempstart.AddMinutes(15))
                    {
                        if (cycleAggregations.Where(c => c.BinStartTime >= tempstart && c.BinStartTime < tempstart.AddMinutes(15)).Any())
                        {
                            cycleList.Add(tempstart, cycleAggregations.Where(c => c.BinStartTime >= tempstart && c.BinStartTime < tempstart.AddMinutes(15)).Average(c => c.PedCycles));
                        }
                        else
                        {
                            cycleList.Add(tempstart, 0);
                        }
                    }
            }
            var pedCycleAverageResult = new PedCycleAverageResult
            {
                PedCycleAverageList = cycleList,
                PedCycleAverage = averagePedCycles
            };
            return pedCycleAverageResult;
        }

        private CycleAverageResult GetCycleAverage(
            string signalId,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            int phase,
            int[] daysOfWeek)
        {
            List<PhaseCycleAggregation> cycleAggregations = new List<PhaseCycleAggregation>();
            List<double> hourlyCycles = new List<double>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                if (daysOfWeek.Contains((int)start.DayOfWeek))
                {
                    var cyclesAgg = _approachCycleAggregationRepository.GetApproachCyclesAggregationBySignalIdPhaseAndDateRange(signalId, phase, tempDate.Date.Add(startTime), tempDate.Date.Add(endTime));
                    hourlyCycles.Add(cyclesAgg.Sum(c => c.TotalRedToRedCycles));
                    cycleAggregations.AddRange(cyclesAgg);
                }
            }
            Dictionary<DateTime, double> cycleList = GetAverageCycles(start, end, startTime, endTime, daysOfWeek, cycleAggregations);
            var cycleAverage = new CycleAverageResult
            {
                CycleAverage = hourlyCycles.Average(a => a),
                CycleAverageList = cycleList
            };
            return cycleAverage;
        }

        public static Dictionary<DateTime, double> GetAverageCycles(
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            int[] daysOfWeek,
            List<ATSPM.Data.Models.PhaseCycleAggregation> cycleAggregations)
        {
            Dictionary<DateTime, double> cycleList = new Dictionary<DateTime, double>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                if (daysOfWeek.Contains((int)start.DayOfWeek))
                    for (var tempstart = tempDate.Date.Add(startTime); tempstart < tempDate.Add(endTime); tempstart = tempstart.AddMinutes(15))
                    {
                        cycleList.Add(tempstart, cycleAggregations.Where(c => c.BinStartTime >= tempstart && c.BinStartTime < tempstart.AddMinutes(15)).Average(c => c.TotalRedToRedCycles));
                    }
            }

            return cycleList;
        }
    }

}

public class CycleAverageResult
{
    public double CycleAverage { get; set; }
    public Dictionary<DateTime, double> CycleAverageList { get; set; }
}
public class PedCycleAverageResult
{
    public double PedCycleAverage { get; set; }
    public Dictionary<DateTime, double> PedCycleAverageList { get; set; }
}