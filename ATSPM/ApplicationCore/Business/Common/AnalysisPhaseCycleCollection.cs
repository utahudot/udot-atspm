using ATSPM.Data.Enums;
using ATSPM.Data.Models.EventLogModels;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.Common
{
    public class AnalysisPhaseCycleCollection
    {
        public List<AnalysisPhaseCycle> Cycles = new List<AnalysisPhaseCycle>();

        /// <summary>
        ///     Collection of phase events primarily used for the split monitor and Phase Termination Chart
        /// </summary>
        /// <param name="phasenumber"></param>
        /// <param name="locationId"></param>
        /// <param name="cycleEvents"></param>
        public AnalysisPhaseCycleCollection(
            int phasenumber,
            string locationId,
            List<IndianaEvent> cycleEvents,
            List<IndianaEvent> pedEvents,
            List<IndianaEvent> terminationEvents
            )
        {
            AnalysisPhaseCycle cycle = null;
            locationId = locationId;
            PhaseNumber = phasenumber;
            var combinedEvents = cycleEvents.Concat(terminationEvents).OrderBy(e => e.Timestamp).ToList();

            foreach (var cycleEvent in combinedEvents)
            {
                if (cycleEvent.EventCode == IndianaEnumerations.PhaseBeginGreen && cycleEvent.EventParam == phasenumber)
                    cycle = new AnalysisPhaseCycle(locationId, phasenumber, cycleEvent.Timestamp);

                if (cycle != null && cycleEvent.EventParam == phasenumber &&
                    (cycleEvent.EventCode == IndianaEnumerations.PhaseGapOut || cycleEvent.EventCode == IndianaEnumerations.PhaseMaxOut || cycleEvent.EventCode == IndianaEnumerations.PhaseForceOff))
                    cycle.SetTerminationEvent(cycleEvent.EventCode);

                if (cycle != null && cycleEvent.EventParam == phasenumber && cycleEvent.EventCode == IndianaEnumerations.PhaseBeginYellowChange)
                    cycle.YellowEvent = cycleEvent.Timestamp;

                if (cycle != null && cycleEvent.EventParam == phasenumber && cycleEvent.EventCode == IndianaEnumerations.PhaseEndRedClearance)
                {
                    cycle.SetEndTime(cycleEvent.Timestamp);
                    Cycles.Add(cycle);
                }
            }
            if (!pedEvents.IsNullOrEmpty())
            {
                foreach (var c in Cycles)
                {
                    var PedEventsForCycle = (from r in pedEvents
                                             where r.Timestamp >=
                                                   c.StartTime && r.Timestamp <= c.EndTime
                                             select r).ToList();

                    if ((c.EndTime - c.StartTime).Seconds > PedEventsForCycle.Count)
                    {
                        SetPedTimesForCycle(PedEventsForCycle, c);
                    }
                }
            }
        }

        public string locationId { get; set; }
        public int PhaseNumber { get; set; }


        public void SetPedTimesForCycle(List<IndianaEvent> PedEventsForCycle, AnalysisPhaseCycle Cycle)
        {
            if (PedEventsForCycle.Count > 0)
            {
                var eventsInOrder = PedEventsForCycle.OrderBy(r => r.Timestamp);
                if (eventsInOrder.Count() > 1)
                {
                    for (var i = 0; i < eventsInOrder.Count() - 1; i++)
                    {
                        var current = eventsInOrder.ElementAt(i);

                        var next = eventsInOrder.ElementAt(i + 1);


                        if (current.Timestamp.Ticks == next.Timestamp.Ticks)
                            continue;

                        //If the first event is 'Off', then set duration to 0
                        if (i == 0 && current.EventCode == IndianaEnumerations.PedestrianBeginSolidDontWalk)
                        {
                            Cycle.SetPedStart(Cycle.StartTime);
                            //cycle.SetPedEnd(current.TimeStamp);
                            Cycle.SetPedEnd(Cycle.StartTime);
                        }

                        //This is the prefered sequence; an 'On'  followed by an 'off'
                        if (current.EventCode == IndianaEnumerations.PedestrianBeginWalk && next.EventCode == IndianaEnumerations.PedestrianBeginSolidDontWalk)
                        {
                            if (Cycle.PedStartTime == DateTime.MinValue)
                                Cycle.SetPedStart(current.Timestamp);
                            else if (Cycle.PedStartTime > current.Timestamp)
                                Cycle.SetPedStart(current.Timestamp);

                            if (Cycle.PedEndTime == DateTime.MinValue)
                                Cycle.SetPedEnd(next.Timestamp);
                            else if (Cycle.PedEndTime < next.Timestamp)
                                Cycle.SetPedEnd(next.Timestamp);

                            continue;
                        }

                        //if we are at the penultimate event, and the last event is 'on' then set duration to 0.
                        if (i + 2 == eventsInOrder.Count() && next.EventCode == IndianaEnumerations.PedestrianBeginWalk)
                        {
                            Cycle.SetPedStart(Cycle.StartTime);
                            //cycle.SetPedEnd(cycle.YellowEvent);
                            Cycle.SetPedEnd(Cycle.StartTime);
                        }
                    }
                }
                else
                {
                    var current = eventsInOrder.First();
                    switch (current.EventCode)
                    {
                        //if the only event is off
                        case IndianaEnumerations.PedestrianBeginSolidDontWalk:
                            Cycle.SetPedStart(Cycle.StartTime);
                            Cycle.SetPedEnd(Cycle.StartTime);
                            //cycle.SetPedEnd(current.TimeStamp);

                            break;
                        //if the only event is on
                        case IndianaEnumerations.PedestrianBeginWalk:

                            Cycle.SetPedStart(current.Timestamp);
                            Cycle.SetPedEnd(current.Timestamp);
                            //cycle.SetPedEnd(cycle.YellowEvent);

                            break;
                    }
                }
            }
        }
    }
}