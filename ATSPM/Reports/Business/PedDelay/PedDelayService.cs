using ATSPM.Application.Reports.Business.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.PedDelay
{
    public class PedDelayService
    {

        public PedDelayService()
        {

        }


        public PedDelayResult GetChartData(
            PedDelayOptions options,
            PedPhaseData pedPhase,
            List<RedToRedCycle> redToRedCycles)
        {
            int currentRedToRedCycle = 0;
            var delayByCycleLengthDataPoints = new Dictionary<PedCycle, double>();
            var plans = new List<PedDelayPlan>();
            var cycleLengths = new List<CycleLengths>();
            var pedestrianDelay = new List<PedestrianDelay>();
            var startOfWalk = new List<StartBeginWalk>();
            var percentDelayByCycleLength = new List<PercentDelayByCycleLength>();

            foreach (var pedPlan in pedPhase.Plans)
            {
                foreach (var pedCycle in pedPlan.Cycles)
                {
                    pedestrianDelay.Add(new PedestrianDelay(pedCycle.BeginWalk, pedCycle.Delay));

                    if (options.ShowPedBeginWalk)
                    {
                        startOfWalk.Add(new StartBeginWalk(pedCycle.BeginWalk, pedCycle.Delay)); //add ped walk to top of delay
                    }

                    if (options.ShowPercentDelay)
                    {
                        AddDelayByCycleLengthDataPoint(
                            pedCycle,
                            ref currentRedToRedCycle,
                            delayByCycleLengthDataPoints,
                            redToRedCycles);
                    }
                }
            }

            if (options.ShowCycleLength)
            {
                foreach (var cycle in redToRedCycles)
                {
                    cycleLengths.Add(new CycleLengths(cycle.EndTime, cycle.RedLineY));
                }
            }

            if (options.ShowPedBeginWalk)
            {
                foreach (var e in pedPhase.PedBeginWalkEvents)
                {
                    startOfWalk.Add(new StartBeginWalk(e.Timestamp, 0));
                }
            }

            if (options.ShowPercentDelay)
            {
                var delayByCycleLengthStepChart = CreateDelayByCycleLengthStepChart(
                    delayByCycleLengthDataPoints,
                    pedPhase);
                foreach (var cycle in delayByCycleLengthStepChart)
                {
                    percentDelayByCycleLength.Add(new PercentDelayByCycleLength(cycle.Key, cycle.Value));
                }
            }
            return new PedDelayResult(
                pedPhase.Approach.Signal.SignalIdentifier,
                pedPhase.Approach.Id,
                pedPhase.Approach.ProtectedPhaseNumber,
                pedPhase.Approach.Description,
                options.Start,
                options.End,
                pedPhase.PedPresses,
                pedPhase.Plans.Sum(p => p.CyclesWithPedRequests),
                options.TimeBuffer,
                pedPhase.UniquePedDetections,
                pedPhase.MinDelay,
                pedPhase.MaxDelay,
                pedPhase.AverageDelay,
                plans,
                cycleLengths,
                pedestrianDelay,
                startOfWalk,
                percentDelayByCycleLength
                );
        }

        protected void AddDelayByCycleLengthDataPoint(
            PedCycle pc,
            ref int currentRedToRedCycle,
            Dictionary<PedCycle, double> delayByCycleLengthDataPoints,
            List<RedToRedCycle> redToRedCycles)
        {
            while (currentRedToRedCycle < redToRedCycles.Count)
            {
                if (redToRedCycles[currentRedToRedCycle].EndTime > pc.BeginWalk)
                {
                    double cycle1;
                    if (currentRedToRedCycle > 0)
                    {
                        cycle1 = redToRedCycles[currentRedToRedCycle - 1].RedLineY;
                    }
                    else
                    {
                        cycle1 = redToRedCycles[currentRedToRedCycle].RedLineY;
                    }

                    var cycle2 = redToRedCycles[currentRedToRedCycle].RedLineY;
                    var average = (cycle1 + cycle2) / 2;
                    delayByCycleLengthDataPoints.Add(pc, pc.Delay / average * 100);
                    break;
                }
                currentRedToRedCycle++;
            }
        }

        protected Dictionary<DateTime, double> CreateDelayByCycleLengthStepChart(
            Dictionary<PedCycle, double> delayByCycleLengthDataPoints,
            PedPhaseData pedPhase)
        {
            var delayByCycleLengthStepChart = new Dictionary<DateTime, double>();
            var startTime = pedPhase.StartDate;
            while (startTime <= pedPhase.EndDate)
            {
                var endTime = startTime.AddMinutes(30);
                var cycles = delayByCycleLengthDataPoints.Where(c => c.Key.BeginWalk >= startTime && c.Key.BeginWalk < endTime).ToList();
                double average = 0;
                if (cycles.Count > 0)
                {
                    average = cycles.Average(c => c.Value);
                }
                delayByCycleLengthStepChart.Add(startTime, average);
                startTime = startTime.AddMinutes(30);
            }
            return delayByCycleLengthStepChart;
        }


    }
}