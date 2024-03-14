using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class PedActuationService
    {
        private readonly LeftTurnReportService leftTurnReportPreCheckService;

        public PedActuationService(
            LeftTurnReportService leftTurnReportService)
        {
            this.leftTurnReportPreCheckService = leftTurnReportService;
        }
        public PedActuationResult GetPedestrianPercentage(
            Location Location,
            Approach approach,
            PedActuationOptions options,
            TimeSpan startTime,
            TimeSpan endTime,
            List<PhasePedAggregation> phasePedAggregations,
            int opposingPhase,
            List<PhaseCycleAggregation> phaseCycleAggregations
            )
        {
            var cycleAverage = GetCycleAverage(
                options,
                startTime,
                endTime,
                phaseCycleAggregations);
            var pedCycleAverage = GetPedCycleAverage(
                options,
                startTime,
                endTime,
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
            result.Direction = approach.DirectionType.Abbreviation + approach.Detectors.FirstOrDefault()?.MovementType;
            result.OpposingDirection = Location.Approaches.Where(a => a.ProtectedPhaseNumber == opposingPhase).FirstOrDefault()?.DirectionType.Abbreviation;

            return result;
        }

        private PedCycleAverageResult GetPedCycleAverage(
            PedActuationOptions options,
            TimeSpan startTime,
            TimeSpan endTime,
            List<PhasePedAggregation> phasePedAggregations
            )
        {
            List<double> hourlyPedCycles = new List<double>();
            for (var tempDate = options.Start.Date; tempDate <= options.End; tempDate = tempDate.AddDays(1))
            {
                hourlyPedCycles.Add(phasePedAggregations
                    .Where(p => p.Start >= tempDate.Date.Add(startTime)
                        && p.Start < tempDate.Date.Add(endTime))
                    .Sum(p => p.PedCycles));
            }
            foreach (var aggregation in phasePedAggregations)
            {
                aggregation.Start = aggregation.Start.AddMinutes(-aggregation.Start.Minute % 15);
            }
            double averagePedCycles = 0;
            if (phasePedAggregations.Any())
            {
                averagePedCycles = hourlyPedCycles.Average(a => a);
            }
            Dictionary<DateTime, double> cycleList = new Dictionary<DateTime, double>();
            for (var tempDate = options.Start.Date; tempDate <= options.End; tempDate = tempDate.AddDays(1))
            {
                if (options.DaysOfWeek.Contains((int)tempDate.DayOfWeek))
                    for (var tempstart = tempDate.Date.Add(startTime); tempstart < tempDate.Add(endTime); tempstart = tempstart.AddMinutes(15))
                    {
                        if (phasePedAggregations.Where(c => c.Start >= tempstart && c.Start < tempstart.AddMinutes(15)).Any())
                        {
                            cycleList.Add(tempstart, phasePedAggregations.Where(c => c.Start >= tempstart && c.Start < tempstart.AddMinutes(15)).Average(c => c.PedCycles));
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
            PedActuationOptions options,
            TimeSpan startTime,
            TimeSpan endTime,
            List<PhaseCycleAggregation> cycleAggregations)
        {
            List<double> hourlyCycles = new List<double>();
            for (var tempDate = options.Start.Date; tempDate < options.End; tempDate = tempDate.AddDays(1))
            {
                hourlyCycles.Add(cycleAggregations
                    .Where(p => p.Start >= tempDate.Date.Add(startTime)
                        && p.Start < tempDate.Date.Add(endTime))
                    .Sum(p => p.TotalRedToRedCycles));
            }
            Dictionary<DateTime, double> cycleList = GetAverageCycles(options.Start, options.End, startTime, endTime, options.DaysOfWeek, cycleAggregations);
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
            List<PhaseCycleAggregation> cycleAggregations)
        {
            Dictionary<DateTime, double> cycleList = new Dictionary<DateTime, double>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                if (daysOfWeek.Contains((int)tempDate.DayOfWeek))
                    for (var tempstart = tempDate.Date.Add(startTime); tempstart < tempDate.Add(endTime); tempstart = tempstart.AddMinutes(15))
                    {
                        cycleList.Add(tempstart, cycleAggregations.Where(c => c.Start >= tempstart && c.Start < tempstart.AddMinutes(15)).Average(c => c.TotalRedToRedCycles));
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