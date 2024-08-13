﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PedDelay/PedDelayService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Utah.Udot.Atspm.Business.Common;

namespace Utah.Udot.Atspm.Business.PedDelay
{
    public class PedDelayService
    {

        public PedDelayService()
        {

        }


        public PedDelayResult GetChartData(
            PedDelayOptions options,
            PedPhaseData pedPhase,
            List<Common.RedToRedCycle> redToRedCycles)
        {
            int currentRedToRedCycle = 0;
            var delayByCycleLengthDataPoints = new Dictionary<PedCycle, double>();
            var plans = new List<PedDelayPlan>();
            var cycleLengths = new List<DataPointForDouble>();
            var pedestrianDelay = new List<DataPointForDouble>();
            var startOfWalk = new List<DataPointForDouble>();
            var percentDelayByCycleLength = new List<DataPointForDouble>();

            foreach (var pedPlan in pedPhase.Plans)
            {
                foreach (var pedCycle in pedPlan.Cycles)
                {
                    pedestrianDelay.Add(new DataPointForDouble(pedCycle.BeginWalk, pedCycle.Delay));

                    if (options.ShowPedBeginWalk)
                    {
                        startOfWalk.Add(new DataPointForDouble(pedCycle.BeginWalk, pedCycle.Delay)); //add ped walk to top of delay
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
                    cycleLengths.Add(new DataPointForDouble(cycle.EndTime, cycle.RedLineY));
                }
            }

            if (options.ShowPedBeginWalk)
            {
                foreach (var e in pedPhase.PedBeginWalkEvents)
                {
                    startOfWalk.Add(new DataPointForDouble(e.Timestamp, 0));
                }
            }

            if (options.ShowPercentDelay)
            {
                var delayByCycleLengthStepChart = CreateDelayByCycleLengthStepChart(
                    delayByCycleLengthDataPoints,
                    pedPhase);
                foreach (var cycle in delayByCycleLengthStepChart)
                {
                    percentDelayByCycleLength.Add(new DataPointForDouble(cycle.Key, cycle.Value));
                }
            }
            var pedDelayPlans = new List<PedDelayPlan>();
            foreach (var plan in pedPhase.Plans)
            {
                var vehicleCycles = redToRedCycles.Where(r => r.StartTime >= plan.Start && r.EndTime < plan.End).ToList();
                var averageCycleLength = vehicleCycles.Any() ? vehicleCycles.Average(c => c.TotalTimeSeconds) : 0;
                var pedRecallMessage = vehicleCycles.Count > 0 && (double)plan.PedBeginWalkCount / vehicleCycles.Count * 100 >= options.PedRecallThreshold ? "Ped Recall On" : "";
                pedDelayPlans.Add(new PedDelayPlan(
                    plan.PlanNumber.ToString(),
                    plan.Start,
                    plan.End,
                    pedRecallMessage,
                    Convert.ToInt32(plan.CyclesWithPedRequests),
                    plan.UniquePedDetections,
                    plan.PedPresses,
                    plan.AvgDelay,
                    averageCycleLength));
            }

            return new PedDelayResult(
                pedPhase.Approach.Location.LocationIdentifier,
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
                pedDelayPlans,
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
            List<Common.RedToRedCycle> redToRedCycles)
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