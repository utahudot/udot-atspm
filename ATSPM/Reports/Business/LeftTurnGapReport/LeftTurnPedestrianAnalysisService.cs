using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.LeftTurnGapReport
{
    public class LeftTurnPedestrianAnalysisService
    {
        private readonly LeftTurnReportPreCheckService leftTurnReportPreCheckService;

        public LeftTurnPedestrianAnalysisService(
            LeftTurnReportPreCheckService leftTurnReportPreCheckService)
        {
            this.leftTurnReportPreCheckService = leftTurnReportPreCheckService;
        }
        public PedActuationResult GetPedestrianPercentage(
            Signal signal,
            Approach approach,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            int[] daysOfWeek,
            List<PhasePedAggregation> phasePedAggregations,
            int opposingPhase,
            List<PhaseCycleAggregation> phaseCycleAggregations
            )
        {
            var detectors = leftTurnReportPreCheckService.GetLeftTurnDetectors(approach);
            var cycleAverage = GetCycleAverage(
                start,
                end,
                startTime,
                endTime,
                daysOfWeek,
                phaseCycleAggregations);
            var pedCycleAverage = GetPedCycleAverage(
                start,
                end,
                startTime,
                endTime,
                daysOfWeek,
                phasePedAggregations);

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
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            int[] daysOfWeek,
            List<PhasePedAggregation> phasePedAggregations
            )
        {
            List<double> hourlyPedCycles = new List<double>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                hourlyPedCycles.Add(phasePedAggregations
                    .Where(p => p.BinStartTime >= tempDate.Date.Add(startTime)
                        && p.BinStartTime < tempDate.Date.Add(endTime))
                    .Sum(p => p.PedCycles));
            }
            foreach (var aggregation in phasePedAggregations)
            {
                aggregation.BinStartTime = aggregation.BinStartTime.AddMinutes(-aggregation.BinStartTime.Minute % 15);
            }
            double averagePedCycles = 0;
            if (phasePedAggregations.Any())
            {
                averagePedCycles = hourlyPedCycles.Average(a => a);
            }
            Dictionary<DateTime, double> cycleList = new Dictionary<DateTime, double>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                if (daysOfWeek.Contains((int)start.DayOfWeek))
                    for (var tempstart = tempDate.Date.Add(startTime); tempstart < tempDate.Add(endTime); tempstart = tempstart.AddMinutes(15))
                    {
                        if (phasePedAggregations.Where(c => c.BinStartTime >= tempstart && c.BinStartTime < tempstart.AddMinutes(15)).Any())
                        {
                            cycleList.Add(tempstart, phasePedAggregations.Where(c => c.BinStartTime >= tempstart && c.BinStartTime < tempstart.AddMinutes(15)).Average(c => c.PedCycles));
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
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            int[] daysOfWeek,
            List<PhaseCycleAggregation> cycleAggregations)
        {
            List<double> hourlyCycles = new List<double>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                hourlyCycles.Add(cycleAggregations
                    .Where(p => p.BinStartTime >= tempDate.Date.Add(startTime)
                        && p.BinStartTime < tempDate.Date.Add(endTime))
                    .Sum(p => p.TotalRedToRedCycles));
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