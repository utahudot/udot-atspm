using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.PedDelay
{

    public class PedPhaseService
    {
        private readonly IControllerEventLogRepository _controllerEventLogRepository;

        public PedPhaseService(IControllerEventLogRepository controllerEventLogRepository)
        {
            _controllerEventLogRepository = controllerEventLogRepository;
        }

        public PedPhaseData GetPedPhaseData(Approach approach, int timeBuffer, DateTime startDate, DateTime endDate,
            List<ControllerEventLog> plansData)
        {
            var controllerEventData = _controllerEventLogRepository.GetRecordsByParameterAndEvent(
                approach.SignalId,
                startDate,
                endDate,
                approach.GetPedDetectorsFromApproach(),
                approach.IsPedestrianPhaseOverlap ? new List<int> { 67, 68, 45, 90 } : new List<int> { 21, 22, 45, 90 }).ToList();

            var pedPhaseData = new PedPhaseData();
            pedPhaseData.StartDate = startDate;
            pedPhaseData.EndDate = endDate;
            pedPhaseData.SignalId = approach.SignalId;
            pedPhaseData.TimeBuffer = timeBuffer;
            pedPhaseData.Approach = approach;
            pedPhaseData.PhaseNumber = approach.ProtectedPhaseNumber;
            pedPhaseData.ApproachId = approach.Id;
            pedPhaseData.Plans = new List<PedPlan>();
            pedPhaseData.Cycles = new List<PedCycle>();
            pedPhaseData.PedBeginWalkEvents = new List<ControllerEventLog>();
            pedPhaseData.HourlyTotals = new List<PedHourlyTotal>();

            for (var i = 0; i < plansData.Count; i++)
            {
                //if this is the last plan then we want the end of the plan
                //to coincide with the end of the graph
                var endTime = i == plansData.Count - 1 ? endDate : plansData[i + 1].Timestamp;

                var plan = new PedPlan(pedPhaseData.PhaseNumber, plansData[i].Timestamp, endTime,
                    plansData[i].EventParam);

                plan.Events = (from e in controllerEventData
                               where e.Timestamp > plan.StartDate && e.Timestamp < plan.EndDate
                               select e).ToList();

                plan.UniquePedDetections = CountUniquePedDetections(plan.Events, pedPhaseData);
                pedPhaseData.Plans.Add(plan);
            }

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

            GetCycles(pedPhaseData, controllerEventData);
            AddCyclesToPlans(pedPhaseData);
            SetHourlyTotals(pedPhaseData);
            return pedPhaseData;
        }

        private void AddCyclesToPlans(PedPhaseData pedPhaseData)
        {
            foreach (var p in pedPhaseData.Plans)
            {
                var cycles = (from c in pedPhaseData.Cycles
                              where c.CallRegistered >= p.StartDate &&
                                    c.CallRegistered < p.EndDate
                              select c).ToList();
                p.Cycles = cycles;
            }
        }

        private void GetCycles(PedPhaseData pedPhaseData, List<ControllerEventLog> controllerEventLogs)
        {
            pedPhaseData.PedPresses = controllerEventLogs.Count(e => e.EventCode == 90);
            pedPhaseData.UniquePedDetections = CountUniquePedDetections(controllerEventLogs, pedPhaseData);

            controllerEventLogs = CombineSequential90s(controllerEventLogs);

            pedPhaseData.PedRequests = controllerEventLogs.Count(e => e.EventCode == 90);
            pedPhaseData.PedCallsRegisteredCount = controllerEventLogs.Count(e => e.EventCode == 45);

            Remove45s(controllerEventLogs);

            pedPhaseData.PedBeginWalkCount = controllerEventLogs.Count(e => e.EventCode == pedPhaseData.BeginWalkEvent);
            pedPhaseData.ImputedPedCallsRegistered = CountImputedPedCalls(controllerEventLogs, pedPhaseData);

            if (controllerEventLogs.Count > 1 && controllerEventLogs[0].EventCode == 90 && controllerEventLogs[1].EventCode == pedPhaseData.BeginWalkEvent)
            {
                pedPhaseData.Cycles.Add(new PedCycle(controllerEventLogs[1].Timestamp, controllerEventLogs[0].Timestamp));  // Middle of the event
            }

            for (var i = 0; i < controllerEventLogs.Count - 2; i++)
            {
                // there are four possibilities:
                // 1) 22, 90 , 21
                //   time between 90 and 21, count++
                // 2) 21, 90, 22
                //    time = 0 , count++
                // 3) 22, 90, 22 
                //    ignore this possibility
                // 4) 21, 90, 21
                //    time betweeen 90 and last 21, count++
                //
                if (controllerEventLogs[i].EventCode == pedPhaseData.BeginClearanceEvent &&
                    controllerEventLogs[i + 1].EventCode == 90 &&
                    controllerEventLogs[i + 2].EventCode == pedPhaseData.BeginWalkEvent)
                {
                    pedPhaseData.Cycles.Add(new PedCycle(controllerEventLogs[i + 2].Timestamp, controllerEventLogs[i + 1].Timestamp));  // this is case 1
                    i++;
                }
                else if (controllerEventLogs[i].EventCode == pedPhaseData.BeginWalkEvent &&
                         controllerEventLogs[i + 1].EventCode == 90 &&
                         controllerEventLogs[i + 2].EventCode == pedPhaseData.BeginClearanceEvent)
                {
                    pedPhaseData.Cycles.Add(new PedCycle(controllerEventLogs[i + 1].Timestamp, controllerEventLogs[i + 1].Timestamp));  // this is case 2
                    i++;
                }
                else if (controllerEventLogs[i].EventCode == pedPhaseData.BeginWalkEvent &&
                         controllerEventLogs[i + 1].EventCode == 90 &&
                         controllerEventLogs[i + 2].EventCode == pedPhaseData.BeginWalkEvent)
                {
                    pedPhaseData.Cycles.Add(new PedCycle(controllerEventLogs[i + 2].Timestamp, controllerEventLogs[i + 1].Timestamp));  // this is case 4
                    i++;
                }
                else if (controllerEventLogs[i].EventCode == pedPhaseData.BeginWalkEvent && (pedPhaseData.Cycles.Count == 0 || controllerEventLogs[i].Timestamp != pedPhaseData.Cycles.Last().BeginWalk))
                {
                    pedPhaseData.PedBeginWalkEvents.Add(controllerEventLogs[i]); // collected loose BeginWalkEvents for chart
                }
            }
            if (controllerEventLogs.Count >= 1)
            {
                if (controllerEventLogs[controllerEventLogs.Count - 1].EventCode == pedPhaseData.BeginWalkEvent)
                    pedPhaseData.PedBeginWalkEvents.Add(controllerEventLogs[controllerEventLogs.Count - 1]);
            }
            if (controllerEventLogs.Count >= 2)
            {
                if (controllerEventLogs[controllerEventLogs.Count - 2].EventCode == pedPhaseData.BeginWalkEvent)
                    pedPhaseData.PedBeginWalkEvents.Add(controllerEventLogs[controllerEventLogs.Count - 2]);
            }
        }

        private List<ControllerEventLog> CombineSequential90s(List<ControllerEventLog> controllerEventLogs)
        {
            var tempEvents = new List<ControllerEventLog>();
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

        private void Remove45s(List<ControllerEventLog> controllerEventLogs)
        {
            if (controllerEventLogs.Count(e => e.EventCode == 45) > 0)
            {
                controllerEventLogs = controllerEventLogs.Where(e => e.EventCode != 45).OrderBy(t => t.Timestamp).ToList();
            }
        }

        private int CountImputedPedCalls(List<ControllerEventLog> events, PedPhaseData pedPhaseData)
        {
            var tempEvents = events.Where(e => e.EventCode == 90 || e.EventCode == pedPhaseData.BeginWalkEvent).ToList();

            if (tempEvents.Count == 0) return 0;

            var previousEventCode = GetEventFromPreviousBin(
                pedPhaseData.SignalId,
                pedPhaseData.PhaseNumber,
                events.FirstOrDefault().Timestamp,
                new List<int> { pedPhaseData.BeginWalkEvent, 90 },
                TimeSpan.FromMinutes(15));
            tempEvents.Insert(0, previousEventCode);

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

        private int CountUniquePedDetections(List<ControllerEventLog> events, PedPhaseData pedPhaseData)
        {
            var tempEvents = events.Where(e => e.EventCode == 90).ToList();

            if (tempEvents.Count == 0) return 0;

            int pedDetections = 0;
            var previousEventCode = GetEventFromPreviousBin(
                pedPhaseData.SignalId,
                pedPhaseData.PhaseNumber,
                events.FirstOrDefault().Timestamp,
                new List<int> { pedPhaseData.BeginWalkEvent, 90 },
                TimeSpan.FromMinutes(15));

            if (previousEventCode != null)
            {
                tempEvents.Insert(0, previousEventCode);
            }
            else
            {
                pedDetections++;
            }

            var previousSelectedTimestamp = 0;

            for (var i = 1; i < tempEvents.Count; i++)
            {
                if (tempEvents[i].Timestamp.Subtract(tempEvents[previousSelectedTimestamp].Timestamp).TotalSeconds >= pedPhaseData.TimeBuffer)
                {
                    pedDetections++;
                    previousSelectedTimestamp = i;
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
                    var hourDelay = (from c in pedPhaseData.Cycles
                                     where c.CallRegistered >= dt &&
                                           c.CallRegistered < nextDt
                                     select c.Delay).Sum();
                    pedPhaseData.HourlyTotals.Add(new PedHourlyTotal(dt, hourDelay));
                    dt = dt.AddHours(1);
                    nextDt = nextDt.AddHours(1);
                }
            }
        }

        public ControllerEventLog GetEventFromPreviousBin(string signalId, int phase, DateTime currentTime, List<int> chosenEvents, TimeSpan lookbackTime)
        {
            var startTime = currentTime - lookbackTime;
            var eventRecord = _controllerEventLogRepository.GetEventsByEventCodesParam(
                signalId,
                startTime,
                currentTime,
                chosenEvents,
                phase)
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefault();

            return eventRecord;
        }
    }

}
