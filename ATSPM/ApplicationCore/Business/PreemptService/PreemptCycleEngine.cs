using ATSPM.Data.Models;
using System;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.PreemptService
{
    public class PreemptCycleEngine
    {
        public List<PreemptCycle> CreatePreemptCycle(List<ControllerEventLog> DTTB)
        {
            var CycleCollection = new List<PreemptCycle>();
            PreemptCycle cycle = null;


            //foreach (MOE.Common.Models.Controller_Event_Log row in DTTB.Events)
            for (var x = 0; x < DTTB.Count; x++)
            {
                //It can happen that there is no defined terminaiton event.
                if (x + 1 < DTTB.Count)
                {
                    var t = DTTB[x + 1].Timestamp - DTTB[x].Timestamp;
                    if (cycle != null && t.TotalMinutes > 20 && DTTB[x].EventCode != 111 &&
                        DTTB[x].EventCode != 105)
                    {
                        EndCycle(cycle, DTTB[x], CycleCollection);
                        cycle = null;
                        continue;
                    }
                }

                switch (DTTB[x].EventCode)
                {
                    case 102:
                        cycle?.InputOn.Add(DTTB[x].Timestamp);
                        if (cycle == null && DTTB[x].Timestamp != DTTB[x + 1].Timestamp &&
                            DTTB[x + 1].EventCode == 105)
                            cycle = StartCycle(DTTB[x]);
                        break;

                    case 103:
                        if (cycle != null && cycle.GateDown == DateTime.MinValue)
                            cycle.GateDown = DTTB[x].Timestamp;
                        break;

                    case 104:
                        cycle?.InputOff.Add(DTTB[x].Timestamp);
                        break;

                    case 105:
                        ////If we run into an entry start after cycle start (event 102)
                        if (cycle != null && cycle.HasDelay)
                        {
                            cycle.EntryStarted = DTTB[x].Timestamp;
                            break;
                        }

                        if (cycle != null)
                        {
                            EndCycle(cycle, DTTB[x], CycleCollection);
                            cycle = StartCycle(DTTB[x]);
                            break;
                        }

                        cycle ??= StartCycle(DTTB[x]);
                        break;

                    case 106:
                        if (cycle != null)
                        {
                            cycle.BeginTrackClearance = DTTB[x].Timestamp;

                            if (x + 1 < DTTB.Count)
                                if (!DoesTrackClearEndNormal(DTTB, x))
                                    cycle.BeginDwellService = FindNext111Event(DTTB, x);
                        }
                        break;

                    case 107:

                        if (cycle != null)
                        {
                            cycle.BeginDwellService = DTTB[x].Timestamp;

                            if (x + 1 < DTTB.Count)
                                if (!DoesTheCycleEndNormal(DTTB, x))
                                {
                                    cycle.BeginExitInterval = DTTB[x + 1].Timestamp;

                                    EndCycle(cycle, DTTB[x + 1], CycleCollection);

                                    cycle = null;
                                }
                        }
                        break;

                    case 108:
                        if (cycle != null)
                            cycle.LinkActive = DTTB[x].Timestamp;
                        break;

                    case 109:
                        if (cycle != null)
                            cycle.LinkInactive = DTTB[x].Timestamp;
                        break;

                    case 110:
                        if (cycle != null)
                            cycle.MaxPresenceExceeded = DTTB[x].Timestamp;
                        break;

                    case 111:
                        // 111 can usually be considered "cycle complete"
                        if (cycle != null)
                        {
                            cycle.BeginExitInterval = DTTB[x].Timestamp;
                            EndCycle(cycle, DTTB[x], CycleCollection);
                            cycle = null;
                        }
                        break;
                }


                if (x + 1 >= DTTB.Count && cycle != null)
                {
                    cycle.BeginExitInterval = DTTB[x].Timestamp;
                    EndCycle(cycle, DTTB[x], CycleCollection);
                    break;
                }
            }

            return CycleCollection;
        }

        private DateTime FindNext111Event(List<ControllerEventLog> DTTB, int counter)
        {
            var Next111Event = new DateTime();
            for (var x = counter; x < DTTB.Count; x++)
                if (DTTB[x].EventCode == 111)
                {
                    Next111Event = DTTB[x].Timestamp;
                    x = DTTB.Count;
                }
            return Next111Event;
        }

        private bool DoesTheCycleEndNormal(List<ControllerEventLog> DTTB, int counter)
        {
            var foundEvent111 = false;

            for (var x = counter; x < DTTB.Count; x++)
                switch (DTTB[x].EventCode)
                {
                    case 102:
                        foundEvent111 = false;
                        x = DTTB.Count;
                        break;
                    case 105:
                        foundEvent111 = false;
                        x = DTTB.Count;
                        break;

                    case 111:
                        foundEvent111 = true;
                        x = DTTB.Count;
                        break;
                }

            return foundEvent111;
        }

        private bool DoesTrackClearEndNormal(List<ControllerEventLog> DTTB, int counter)
        {
            var foundEvent107 = false;

            for (var x = counter; x < DTTB.Count; x++)
                switch (DTTB[x].EventCode)
                {
                    case 107:
                        foundEvent107 = true;
                        x = DTTB.Count;
                        break;

                    case 111:
                        foundEvent107 = false;
                        x = DTTB.Count;
                        break;
                }

            return foundEvent107;
        }

        private void EndCycle(PreemptCycle cycle, ControllerEventLog controller_Event_Log,
            List<PreemptCycle> CycleCollection)
        {
            cycle.CycleEnd = controller_Event_Log.Timestamp;
            CycleCollection.Add(cycle);
        }


        private PreemptCycle StartCycle(ControllerEventLog controller_Event_Log)
        {
            var cycle = new PreemptCycle
            {
                CycleStart = controller_Event_Log.Timestamp
            };

            if (controller_Event_Log.EventCode == 105)
            {
                cycle.EntryStarted = controller_Event_Log.Timestamp;
                cycle.HasDelay = false;
            }

            if (controller_Event_Log.EventCode == 102)
            {
                cycle.StartInputOn = controller_Event_Log.Timestamp;
                cycle.HasDelay = true;
            }

            return cycle;
        }
    }
}