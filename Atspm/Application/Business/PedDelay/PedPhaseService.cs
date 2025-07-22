﻿#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.PedDelay/PedPhaseService.cs
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
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Business.PedDelay
{

    public class PedPhaseService
    {

        public PedPhaseService()
        {
        }

        public PedPhaseData GetPedPhaseData(PedDelayOptions options, Approach approach, //int timeBuffer, DateTime startDate, DateTime endDate,
            List<IndianaEvent> plansData, List<IndianaEvent> pedEvents)
        {
            var mainEvents = pedEvents.Where(p => p.Timestamp >= options.Start && p.Timestamp <= options.End).ToList();
            var previousEvents = pedEvents.Where(p => p.Timestamp < options.Start).ToList();

            var pedPhaseData = new PedPhaseData();
            pedPhaseData.StartDate = options.Start;
            pedPhaseData.EndDate = options.End;
            pedPhaseData.locationId = approach.Location.LocationIdentifier;
            pedPhaseData.TimeBuffer = options.TimeBuffer;
            pedPhaseData.Approach = approach;
            pedPhaseData.PhaseNumber = approach.ProtectedPhaseNumber;
            pedPhaseData.ApproachId = approach.Id;
            pedPhaseData.Plans = new List<PedPlan>();
            pedPhaseData.Cycles = new List<PedCycle>();
            pedPhaseData.PedBeginWalkEvents = new List<IndianaEvent>();
            pedPhaseData.HourlyTotals = new List<DataPointForDouble>();
            pedPhaseData.Plans = GetPedPlans(options, plansData, pedEvents, mainEvents, pedPhaseData);

            if (pedPhaseData.Approach.IsPedestrianPhaseOverlap)
            {
                pedPhaseData.BeginWalkEvent = 67;
                pedPhaseData.BeginClearanceEvent = 68;
            }
            else
            {
                pedPhaseData.BeginWalkEvent = 21;
                pedPhaseData.BeginClearanceEvent = 22;
            }

            GetCycles(pedPhaseData, mainEvents, previousEvents);
            AddCyclesToPlans(pedPhaseData);
            SetHourlyTotals(pedPhaseData);
            return pedPhaseData;
        }

        private List<PedPlan> GetPedPlans(
            PedDelayOptions options,
            List<IndianaEvent> plansData,
            List<IndianaEvent> pedEvents,
            List<IndianaEvent> mainEvents,
            PedPhaseData pedPhaseData)
        {
            var planService = new PlanService();
            var pedPlans = new List<PedPlan>();
            var planEvents = planService.SetFirstAndLastPlan(options.Start, options.End, options.LocationIdentifier, plansData.ToList());
            for (var i = 0; i < planEvents.Count; i++)
            {
                //if this is the last plan then we want the end of the plan
                //to coincide with the end of the graph
                var endTime = i == planEvents.Count - 1 ? options.End : planEvents[i + 1].Timestamp;

                var plan = new PedPlan(pedPhaseData.PhaseNumber, planEvents[i].Timestamp, endTime,
                    planEvents[i].EventParam);

                plan.Events = mainEvents.Where(e => e.Timestamp > plan.Start && e.Timestamp < plan.End).ToList();

                plan.PedPresses = plan.Events.Count(e => e.EventCode == 90);

                plan.UniquePedDetections = CountUniquePedDetections(
                    plan.Events,
                    pedEvents.Where(e => e.EventCode == 90 && e.Timestamp < plan.Start).ToList(),
                    pedPhaseData);
                pedPlans.Add(plan);
            }
            return pedPlans;
        }

        private void AddCyclesToPlans(PedPhaseData pedPhaseData)
        {
            foreach (var p in pedPhaseData.Plans)
            {
                var cycles = pedPhaseData.Cycles
                    .Where(c => c.CallRegistered >= p.Start && c.CallRegistered < p.End)
                    .ToList();
                p.Cycles = cycles;
            }
        }

        private void GetCycles(
            PedPhaseData pedPhaseData,
            List<IndianaEvent> mainEvents,
            List<IndianaEvent> previousEvents)
        {
            pedPhaseData.PedPresses = mainEvents.Count(e => e.EventCode == 90);
            pedPhaseData.UniquePedDetections = CountUniquePedDetections(mainEvents, previousEvents, pedPhaseData);

            mainEvents = CombineSequential90s(mainEvents);

            pedPhaseData.PedRequests = mainEvents.Count(e => e.EventCode == 90);
            pedPhaseData.PedCallsRegisteredCount = mainEvents.Count(e => e.EventCode == 45);

            //mainEvents = Remove45s(mainEvents);
            var pedEventCodes = new List<int> { 21, 22, 90 };
            mainEvents = mainEvents.Where(e => pedEventCodes.Contains(e.EventCode)).OrderBy(e => e.Timestamp).ToList();

            pedPhaseData.PedBeginWalkCount = mainEvents.Count(e => e.EventCode == pedPhaseData.BeginWalkEvent);
            pedPhaseData.ImputedPedCallsRegistered = CountImputedPedCalls(mainEvents, previousEvents, pedPhaseData);

            if (mainEvents.Count > 1 && mainEvents[0].EventCode == 90 && mainEvents[1].EventCode == pedPhaseData.BeginWalkEvent)
            {
                pedPhaseData.Cycles.Add(new PedCycle(mainEvents[1].Timestamp, mainEvents[0].Timestamp));  // Middle of the event
            }

            for (var i = 0; i < mainEvents.Count - 2; i++)
            {
                if (mainEvents[i].EventCode == pedPhaseData.BeginClearanceEvent &&
                mainEvents[i + 1].EventCode == 90 &&
                mainEvents[i + 2].EventCode == pedPhaseData.BeginWalkEvent)
                {
                    pedPhaseData.Cycles.Add(new PedCycle(mainEvents[i + 2].Timestamp, mainEvents[i + 1].Timestamp));  // this is case 1
                    i++;
                }
                else if (mainEvents[i].EventCode == pedPhaseData.BeginWalkEvent &&
                         mainEvents[i + 1].EventCode == 90 &&
                         mainEvents[i + 2].EventCode == pedPhaseData.BeginClearanceEvent)
                {
                    pedPhaseData.Cycles.Add(new PedCycle(mainEvents[i + 1].Timestamp, mainEvents[i + 1].Timestamp));  // this is case 2
                    i++;
                }
                else if (mainEvents[i].EventCode == pedPhaseData.BeginWalkEvent &&
                         mainEvents[i + 1].EventCode == 90 &&
                         mainEvents[i + 2].EventCode == pedPhaseData.BeginWalkEvent)
                {
                    pedPhaseData.Cycles.Add(new PedCycle(mainEvents[i + 2].Timestamp, mainEvents[i + 1].Timestamp));  // this is case 4
                    i++;
                }
                else if (mainEvents[i].EventCode == pedPhaseData.BeginWalkEvent && (pedPhaseData.Cycles.Count == 0 || mainEvents[i].Timestamp != pedPhaseData.Cycles.Last().BeginWalk))
                {
                    pedPhaseData.PedBeginWalkEvents.Add(mainEvents[i]); // collected loose BeginWalkmainEvents for chart
                }
            }

            if (mainEvents.Count >= 1)
            {
                if (mainEvents[mainEvents.Count - 1].EventCode == pedPhaseData.BeginWalkEvent)
                    pedPhaseData.PedBeginWalkEvents.Add(mainEvents[mainEvents.Count - 1]);
            }
            if (mainEvents.Count >= 2)
            {
                if (mainEvents[mainEvents.Count - 2].EventCode == pedPhaseData.BeginWalkEvent)
                    pedPhaseData.PedBeginWalkEvents.Add(mainEvents[mainEvents.Count - 2]);
            }
        }


        private List<IndianaEvent> CombineSequential90s(List<IndianaEvent> controllerEventLogs)
        {
            var tempEvents = new List<IndianaEvent>();
            for (int i = 0; i < controllerEventLogs.Count; i++)
            {
                if (controllerEventLogs[i].EventCode == 90)
                {
                    tempEvents.Add(controllerEventLogs[i]);

                    while (i + 1 < controllerEventLogs.Count && controllerEventLogs[i + 1].EventCode == 90)
                    {
                        i++;
                    }
                }
                else
                {
                    tempEvents.Add(controllerEventLogs[i]);
                }
            }
            return tempEvents.OrderBy(t => t.Timestamp).ToList();
        }

        private List<IndianaEvent> Remove45s(List<IndianaEvent> controllerEventLogs)
        {
            if (controllerEventLogs.Count(e => e.EventCode == 45) > 0)
            {
                controllerEventLogs = controllerEventLogs.Where(e => e.EventCode != 45).OrderBy(t => t.Timestamp).ToList();
            }
            return controllerEventLogs;
        }

        private int CountImputedPedCalls(List<IndianaEvent> mainEvents, List<IndianaEvent> previousEvents, PedPhaseData pedPhaseData)
        {
            if (mainEvents == null || mainEvents.Count == 0) return 0;
            var tempEvents = mainEvents.Where(e => e.EventCode == 90 || e.EventCode == pedPhaseData.BeginWalkEvent).ToList();
            var previousEventCode = GetPreviousEventCode(previousEvents, pedPhaseData);
            if (previousEventCode != null)
            {
                tempEvents.Insert(0, previousEventCode);
            }

            int pedCalls = 0;

            for (var i = 1; i < tempEvents.Count; i++)
            {
                if (tempEvents[i].EventCode == 90 && tempEvents[i - 1]?.EventCode == pedPhaseData.BeginWalkEvent)
                {
                    pedCalls++;
                }
            }
            return pedCalls;
        }

        private IndianaEvent GetPreviousEventCode(List<IndianaEvent> previousEvents, PedPhaseData pedPhaseData)
        {
            if (previousEvents == null || previousEvents.Count <= 0)
            {
                return null;
            }
            return previousEvents
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefault(e => e.EventCode == 90 || e.EventCode == pedPhaseData.BeginWalkEvent);
        }

        private int CountUniquePedDetections(List<IndianaEvent> mainEvents, List<IndianaEvent> previousEvents, PedPhaseData pedPhaseData)
        {
            var tempEvents = mainEvents.Where(e => e.EventCode == 90).ToList();

            if (tempEvents.Count == 0) return 0;

            int pedDetections = 0;
            var previousEventCode = GetPreviousEventCode(previousEvents, pedPhaseData);

            if (previousEventCode != null)
            {
                tempEvents.Insert(0, previousEventCode);
            }
            else
            {
                pedDetections++;
            }

            var previousSelectedTimeStamp = 0;

            for (var i = 1; i < tempEvents.Count; i++)
            {
                if (tempEvents[i].Timestamp.Subtract(tempEvents[previousSelectedTimeStamp].Timestamp).TotalSeconds >= pedPhaseData.TimeBuffer)
                {
                    pedDetections++;
                    previousSelectedTimeStamp = i;
                }
            }

            return pedDetections;
        }

        private void SetHourlyTotals(PedPhaseData pedPhaseData)
        {
            //Get Min Max and Average
            if (pedPhaseData.Cycles.Count > 0)
            {
                pedPhaseData.MinDelay = pedPhaseData.Cycles.Min(c => c.Delay);
                pedPhaseData.MaxDelay = pedPhaseData.Cycles.Max(c => c.Delay);
                pedPhaseData.AverageDelay = pedPhaseData.Cycles.Average(c => c.Delay);
                pedPhaseData.TotalDelay = pedPhaseData.Cycles.Sum(c => c.Delay);

                var dt = new DateTime(pedPhaseData.StartDate.Year, pedPhaseData.StartDate.Month, pedPhaseData.StartDate.Day, pedPhaseData.StartDate.Hour, 0, 0);
                var nextDt = dt.AddHours(1);
                while (dt < pedPhaseData.EndDate)
                {
                    var hourDelay = pedPhaseData.Cycles
                        .Where(c => c.CallRegistered >= dt && c.CallRegistered < nextDt)
                        .Select(c => c.Delay)
                        .Sum();
                    pedPhaseData.HourlyTotals.Add(new DataPointForDouble(dt, hourDelay));
                    dt = dt.AddHours(1);
                    nextDt = nextDt.AddHours(1);
                }
            }
        }


    }

}
