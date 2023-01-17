using System;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Application.Reports.ViewModels.ControllerEventLog;
using ATSPM.Data.Models;

namespace Legacy.Common.Business.PEDDelay
{
    public class PedPhase 
    {
        public PedPhase(Approach approach, Signal signal, int timeBuffer, DateTime startDate, DateTime endDate,
            PlansBase plansData) : base(signal.SignalID, startDate, endDate, approach.GetPedDetectorsFromApproach(), approach.IsPedestrianPhaseOverlap ? new List<int> {67, 68, 45, 90} : new List<int> { 21, 22, 45, 90 })
        {
            SignalID = signal.SignalId;
            TimeBuffer = timeBuffer;
            StartDate = startDate;
            EndDate = endDate;
            Approach = approach;
            PhaseNumber = approach.ProtectedPhaseNumber;
            ApproachID = approach.Id;
            EndDate = endDate;
            Plans = new List<PedPlan>();
            Cycles = new List<PedCycle>();
            PedBeginWalkEvents = new List<ControllerEventLog>();
            HourlyTotals = new List<PedHourlyTotal>();

            for (var i = 0; i < plansData.Events.Count; i++)
            {
                //if this is the last plan then we want the end of the plan
                //to coincide with the end of the graph
                var endTime = i == plansData.Events.Count - 1 ? endDate : plansData.Events[i + 1].Timestamp;

                var plan = new PedPlan(PhaseNumber, plansData.Events[i].Timestamp, endTime,
                    plansData.Events[i].EventParam);

                plan.Events = (from e in Events
                                where e.Timestamp > plan.StartDate && e.Timestamp < plan.EndDate
                                select e).ToList();

                plan.UniquePedDetections = CountUniquePedDetections(plan.Events);
                Plans.Add(plan);
            }

            if (Approach.IsPedestrianPhaseOverlap)
            {
                BeginWalkEvent = 67;
                BeginClearanceEvent = 68;
            }
            else
            {
                BeginWalkEvent = 21;
                BeginClearanceEvent = 22;
            }

            GetCycles();
            AddCyclesToPlans();
            SetHourlyTotals();
        }
        public Approach Approach { get; set; }
        public int ApproachID { get; set; }
        public int PhaseNumber { get; }
        public string SignalID { get; }
        public List<PedCycle> Cycles { get; }
        public List<PedPlan> Plans { get; }
        public List<PedHourlyTotal> HourlyTotals { get; }
        public double MinDelay { get; private set; }
        public double AverageDelay { get; private set; }
        public double MaxDelay { get; private set; }
        public double TotalDelay { get; set; }
        public int TimeBuffer { get; set; }
        public int PedPresses { get; private set; }
        public int UniquePedDetections { get; set; }
        public int PedRequests { get; private set; }
        public int ImputedPedCallsRegistered { get; set; }
        public int PedBeginWalkCount { get; set; }
        public List<ControllerEventLog> PedBeginWalkEvents { get; set; }
        public int PedCallsRegisteredCount { get; set; }
        private int BeginWalkEvent { get; set; }
        private int BeginClearanceEvent { get; set; }
        private ControllerEventLogResult EventCollection { get; set; }

        private void AddCyclesToPlans()
        {
            foreach (var p in Plans)
            {
                var cycles = (from c in Cycles
                              where c.CallRegistered >= p.StartDate &&
                                    c.CallRegistered < p.EndDate
                              select c).ToList();
                p.Cycles = cycles;
            }
        }

        private void GetCycles()
        {
            PedPresses = EventCollection.Events.Count(e => e.EventCode == 90);
            UniquePedDetections = CountUniquePedDetections(EventCollection.Events);

            CombineSequential90s();

            PedRequests = (EventCollection.Events.Count(e => e.EventCode == 90));
            PedCallsRegisteredCount = EventCollection.Events.Count(e => e.EventCode == 45);

            Remove45s();

            PedBeginWalkCount = EventCollection.Events.Count(e => e.EventCode == BeginWalkEvent);
            ImputedPedCallsRegistered = CountImputedPedCalls(EventCollection.Events);

            if (EventCollection.Events.Count > 1 && EventCollection.Events[0].EventCode == 90 && EventCollection.Events[1].EventCode == BeginWalkEvent)
            {
                Cycles.Add(new PedCycle(EventCollection.Events[1].Timestamp, EventCollection.Events[0].Timestamp));  // Middle of the event
            }

            for (var i = 0; i < EventCollection.Events.Count - 2; i++)
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
                if (EventCollection.Events[i].EventCode == BeginClearanceEvent &&
                    EventCollection.Events[i + 1].EventCode == 90 &&
                    EventCollection.Events[i + 2].EventCode == BeginWalkEvent)
                {
                    Cycles.Add(new PedCycle(EventCollection.Events[i + 2].Timestamp, EventCollection.Events[i + 1].Timestamp));  // this is case 1
                    i++;
                }
                else if (EventCollection.Events[i].EventCode == BeginWalkEvent &&
                         EventCollection.Events[i + 1].EventCode == 90 &&
                         EventCollection.Events[i + 2].EventCode == BeginClearanceEvent)
                {
                    Cycles.Add(new PedCycle(EventCollection.Events[i + 1].Timestamp, EventCollection.Events[i + 1].Timestamp));  // this is case 2
                    i++;
                }
                else if (EventCollection.Events[i].EventCode == BeginWalkEvent &&
                         EventCollection.Events[i + 1].EventCode == 90 &&
                         EventCollection.Events[i + 2].EventCode == BeginWalkEvent)
                {
                    Cycles.Add(new PedCycle(EventCollection.Events[i + 2].Timestamp, EventCollection.Events[i + 1].Timestamp));  // this is case 4
                    i++;
                }
                else if (EventCollection.Events[i].EventCode == BeginWalkEvent && (Cycles.Count == 0 || EventCollection.Events[i].Timestamp != Cycles.Last().BeginWalk))
                {
                    PedBeginWalkEvents.Add(EventCollection.Events[i]); // collected loose BeginWalkEvents for chart
                }
            }
            if (EventCollection.Events.Count >= 1)
            {
                if (EventCollection.Events[EventCollection.Events.Count - 1].EventCode == BeginWalkEvent)
                    PedBeginWalkEvents.Add(EventCollection.Events[EventCollection.Events.Count - 1]);
            }
            if (EventCollection.Events.Count >= 2)
            {
                if (EventCollection.Events[EventCollection.Events.Count - 2].EventCode == BeginWalkEvent)
                    PedBeginWalkEvents.Add(EventCollection.Events[EventCollection.Events.Count - 2]);
            }
        }

        private void CombineSequential90s()
        {
            var tempEvents = new List<ControllerEventLog>();
            for (int i = 0; i < EventCollection.Events.Count; i++)
            {
                if (EventCollection.Events[i].EventCode == 90)
                {
                    tempEvents.Add(EventCollection.Events[i]);

                    while (i + 1 < EventCollection.Events.Count && EventCollection.Events[i + 1].EventCode == 90)
                    {
                        i++;
                    }
                }
                else
                {
                    tempEvents.Add(EventCollection.Events[i]);
                }
            }
            EventCollection.Events = tempEvents.OrderBy(t => t.Timestamp).ToList();
        }

        private void Remove45s()
        {
            EventCollection.Events = EventCollection.Events.Where(e => e.EventCode != 45).OrderBy(t => t.Timestamp).ToList();
        }

        private int CountImputedPedCalls(List<ControllerEventLog> events)
        {
            var tempEvents = events.Where(e => e.EventCode == 90 || e.EventCode == BeginWalkEvent).ToList();
            
            if (tempEvents.Count == 0) return 0;

            var previousEventCode = GetEventFromPreviousBin(SignalID, PhaseNumber, events.FirstOrDefault().Timestamp, new List<int> { BeginWalkEvent, 90 }, TimeSpan.FromMinutes(15));
            tempEvents.Insert(0, previousEventCode);

            int pedCalls = 0;

            for (var i = 1; i < tempEvents.Count; i++)
            {
                if (tempEvents[i].EventCode == 90 && tempEvents[i - 1]?.EventCode == BeginWalkEvent)
                {
                    pedCalls++;
                }
            }
            return pedCalls;
        }

        private int CountUniquePedDetections(List<ControllerEventLog> events)
        {
            var tempEvents = events.Where(e => e.EventCode == 90).ToList();

            if (tempEvents.Count == 0) return 0;

            int pedDetections = 0;
            var previousEventCode = GetEventFromPreviousBin(SignalID, PhaseNumber, events.FirstOrDefault().Timestamp, new List<int> { 90 }, TimeSpan.FromSeconds(TimeBuffer));

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
                if (tempEvents[i].Timestamp.Subtract(tempEvents[previousSelectedTimestamp].Timestamp).TotalSeconds >= TimeBuffer)
                {
                    pedDetections++;
                    previousSelectedTimestamp = i;
                }
            }

            return pedDetections;
        }

        private void SetHourlyTotals()
        {
            //Get Min Max and Average
            if (Cycles.Count > 0)
            {
                MinDelay = Cycles.Min(c => c.Delay);
                MaxDelay = Cycles.Max(c => c.Delay);
                AverageDelay = Cycles.Average(c => c.Delay);
                TotalDelay = Cycles.Sum(c => c.Delay);

                var dt = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, StartDate.Hour, 0, 0);
                var nextDt = dt.AddHours(1);
                while (dt < EndDate)
                {
                    var hourDelay = (from c in Cycles
                                     where c.CallRegistered >= dt &&
                                           c.CallRegistered < nextDt
                                     select c.Delay).Sum();
                    HourlyTotals.Add(new PedHourlyTotal(dt, hourDelay));
                    dt = dt.AddHours(1);
                    nextDt = nextDt.AddHours(1);
                }
            }
        }

        public static ControllerEventLog GetEventFromPreviousBin(string signalID, int phase, DateTime currentTime, List<int> chosenEvents, TimeSpan lookbackTime)
        {
            var db = new SPM();
            var startTime = currentTime - lookbackTime;
            var eventRecord = (from s in db.Controller_Event_Log
                               where s.SignalID == signalID &&
                                     s.EventParam == phase &&
                                     s.Timestamp > startTime &&
                                     s.Timestamp < currentTime &&
                                     chosenEvents.Contains(s.EventCode)
                               orderby s.Timestamp descending
                               select s
            ).FirstOrDefault();

            return eventRecord;
        }
    }
}
