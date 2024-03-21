using ATSPM.Application.Business.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.PedDelay
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
                pedPhaseData.BeginWalkEvent = DataLoggerEnum.PedestrianOverlapBeginWalk;
                pedPhaseData.BeginClearanceEvent = DataLoggerEnum.PedestrianOverlapBeginClearance;
            }
            else
            {
                pedPhaseData.BeginWalkEvent = DataLoggerEnum.PedestrianBeginWalk;
                pedPhaseData.BeginClearanceEvent = DataLoggerEnum.PedestrianBeginChangeInterval;
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
            var planEvents = planService.GetPlanEvents(options.Start, options.End, options.LocationIdentifier, plansData.ToList());
            for (var i = 0; i < planEvents.Count; i++)
            {
                //if this is the last plan then we want the end of the plan
                //to coincide with the end of the graph
                var endTime = i == planEvents.Count - 1 ? options.End : planEvents[i + 1].Timestamp;

                var plan = new PedPlan(pedPhaseData.PhaseNumber, planEvents[i].Timestamp, endTime,
                    planEvents[i].EventParam);

                plan.Events = mainEvents.Where(e => e.Timestamp > plan.Start && e.Timestamp < plan.End).ToList();

                plan.UniquePedDetections = CountUniquePedDetections(
                    plan.Events,
                    pedEvents.Where(e => e.EventCode == DataLoggerEnum.PedDetectorOn && e.Timestamp < plan.Start).ToList(),
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
            pedPhaseData.PedPresses = mainEvents.Count(e => e.EventCode == DataLoggerEnum.PedDetectorOn);
            pedPhaseData.UniquePedDetections = CountUniquePedDetections(mainEvents, previousEvents, pedPhaseData);

            mainEvents = CombineSequential90s(mainEvents);

            pedPhaseData.PedRequests = mainEvents.Count(e => e.EventCode == DataLoggerEnum.PedDetectorOn);
            pedPhaseData.PedCallsRegisteredCount = mainEvents.Count(e => e.EventCode == DataLoggerEnum.PedestrianCallRegistered);

            mainEvents = Remove45s(mainEvents);

            pedPhaseData.PedBeginWalkCount = mainEvents.Count(e => e.EventCode == pedPhaseData.BeginWalkEvent);
            pedPhaseData.ImputedPedCallsRegistered = CountImputedPedCalls(mainEvents, previousEvents, pedPhaseData);

            if (mainEvents.Count > 1 && mainEvents[0].EventCode == DataLoggerEnum.PedDetectorOn && mainEvents[1].EventCode == pedPhaseData.BeginWalkEvent)
            {
                pedPhaseData.Cycles.Add(new PedCycle(mainEvents[1].Timestamp, mainEvents[0].Timestamp));  // Middle of the event
            }

            for (var i = 0; i < mainEvents.Count - 1; i++)
            {
                if (mainEvents[i].EventCode == pedPhaseData.BeginClearanceEvent &&
                mainEvents[i + 1].EventCode == DataLoggerEnum.PedDetectorOn &&
                mainEvents[i + 2].EventCode == pedPhaseData.BeginWalkEvent)
                {
                    pedPhaseData.Cycles.Add(new PedCycle(mainEvents[i + 2].Timestamp, mainEvents[i + 1].Timestamp));  // this is case 1
                    i++;
                }
                else if (mainEvents[i].EventCode == pedPhaseData.BeginWalkEvent &&
                         mainEvents[i + 1].EventCode == DataLoggerEnum.PedDetectorOn &&
                         mainEvents[i + 2].EventCode == pedPhaseData.BeginClearanceEvent)
                {
                    pedPhaseData.Cycles.Add(new PedCycle(mainEvents[i + 1].Timestamp, mainEvents[i + 1].Timestamp));  // this is case 2
                    i++;
                }
                else if (mainEvents[i].EventCode == pedPhaseData.BeginWalkEvent &&
                         mainEvents[i + 1].EventCode == DataLoggerEnum.PedDetectorOn &&
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
                if (controllerEventLogs[i].EventCode == DataLoggerEnum.PedDetectorOn)
                {
                    tempEvents.Add(controllerEventLogs[i]);

                    while (i + 1 < controllerEventLogs.Count && controllerEventLogs[i + 1].EventCode == DataLoggerEnum.PedDetectorOn)
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
            if (controllerEventLogs.Count(e => e.EventCode == DataLoggerEnum.PedestrianCallRegistered) > 0)
            {
                controllerEventLogs = controllerEventLogs.Where(e => e.EventCode != DataLoggerEnum.PedestrianCallRegistered).OrderBy(t => t.Timestamp).ToList();
            }
            return controllerEventLogs;
        }

        private int CountImputedPedCalls(List<IndianaEvent> mainEvents, List<IndianaEvent> previousEvents, PedPhaseData pedPhaseData)
        {
            if (mainEvents == null || mainEvents.Count == 0) return 0;
            var tempEvents = mainEvents.Where(e => e.EventCode == DataLoggerEnum.PedDetectorOn || e.EventCode == pedPhaseData.BeginWalkEvent).ToList();
            var previousEventCode = GetPreviousEventCode(previousEvents, pedPhaseData);
            if (previousEventCode != null)
            {
                tempEvents.Insert(0, previousEventCode);
            }

            int pedCalls = 0;

            for (var i = 1; i < tempEvents.Count; i++)
            {
                if (tempEvents[i].EventCode == DataLoggerEnum.PedDetectorOn && tempEvents[i - 1]?.EventCode == pedPhaseData.BeginWalkEvent)
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
                .FirstOrDefault(e => e.EventCode == DataLoggerEnum.PedDetectorOn || e.EventCode == pedPhaseData.BeginWalkEvent);
        }

        private int CountUniquePedDetections(List<IndianaEvent> mainEvents, List<IndianaEvent> previousEvents, PedPhaseData pedPhaseData)
        {
            var tempEvents = mainEvents.Where(e => e.EventCode == DataLoggerEnum.PedDetectorOn).ToList();

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
