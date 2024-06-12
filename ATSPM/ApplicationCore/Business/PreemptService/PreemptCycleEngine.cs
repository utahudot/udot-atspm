#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Business.PreemptService/PreemptCycleEngine.cs
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
using ATSPM.Data.Models.EventLogModels;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Business.PreemptService
{
    public class PreemptCycleEngine
    {
        public List<PreemptCycle> CreatePreemptCycle(List<IndianaEvent> controllerEvents)
        {
            var CycleCollection = new List<PreemptCycle>();
            PreemptCycle cycle = null;


            //foreach (MOE.Common.Models.Controller_Event_Log row in controllerEvents.Events)
            for (var x = 0; x < controllerEvents.Count; x++)
            {
                //It can happen that there is no defined terminaiton event.
                if (x + 1 < controllerEvents.Count)
                {
                    var t = controllerEvents[x + 1].Timestamp - controllerEvents[x].Timestamp;
                    if (cycle != null && t.TotalMinutes > 20 && controllerEvents[x].EventCode != 111 &&
                        controllerEvents[x].EventCode != 105)
                    {
                        EndCycle(cycle, controllerEvents[x], CycleCollection);
                        cycle = null;
                        continue;
                    }
                }

                switch (controllerEvents[x].EventCode)
                {
                    case 102:
                        cycle?.InputOn.Add(controllerEvents[x].Timestamp);
                        if (cycle == null && controllerEvents[x].Timestamp != controllerEvents[x + 1].Timestamp &&
                            controllerEvents[x + 1].EventCode == 105)
                            cycle = StartCycle(controllerEvents[x]);
                        break;

                    case 103:
                        if (cycle != null && cycle.GateDown == DateTime.MinValue)
                            cycle.GateDown = controllerEvents[x].Timestamp;
                        break;

                    case 104:
                        cycle?.InputOff.Add(controllerEvents[x].Timestamp);
                        break;

                    case 105:
                        ////If we run into an entry start after cycle start (event 102)
                        if (cycle != null && cycle.HasDelay)
                        {
                            cycle.EntryStarted = controllerEvents[x].Timestamp;
                            break;
                        }

                        if (cycle != null)
                        {
                            EndCycle(cycle, controllerEvents[x], CycleCollection);
                            cycle = StartCycle(controllerEvents[x]);
                            break;
                        }

                        cycle ??= StartCycle(controllerEvents[x]);
                        break;

                    case 106:
                        if (cycle != null)
                        {
                            cycle.BeginTrackClearance = controllerEvents[x].Timestamp;

                            if (x + 1 < controllerEvents.Count)
                                if (!DoesTrackClearEndNormal(controllerEvents, x))
                                    cycle.BeginDwellService = FindNext111Event(controllerEvents, x);
                        }
                        break;

                    case 107:

                        if (cycle != null)
                        {
                            cycle.BeginDwellService = controllerEvents[x].Timestamp;

                            if (x + 1 < controllerEvents.Count)
                                if (!DoesTheCycleEndNormal(controllerEvents, x))
                                {
                                    cycle.BeginExitInterval = controllerEvents[x + 1].Timestamp;

                                    EndCycle(cycle, controllerEvents[x + 1], CycleCollection);

                                    cycle = null;
                                }
                        }
                        break;

                    case 108:
                        if (cycle != null)
                            cycle.LinkActive = controllerEvents[x].Timestamp;
                        break;

                    case 109:
                        if (cycle != null)
                            cycle.LinkInactive = controllerEvents[x].Timestamp;
                        break;

                    case 110:
                        if (cycle != null)
                            cycle.MaxPresenceExceeded = controllerEvents[x].Timestamp;
                        break;

                    case 111:
                        // 111 can usually be considered "cycle complete"
                        if (cycle != null)
                        {
                            cycle.BeginExitInterval = controllerEvents[x].Timestamp;
                            EndCycle(cycle, controllerEvents[x], CycleCollection);
                            cycle = null;
                        }
                        break;
                }


                if (x + 1 >= controllerEvents.Count && cycle != null)
                {
                    cycle.BeginExitInterval = controllerEvents[x].Timestamp;
                    EndCycle(cycle, controllerEvents[x], CycleCollection);
                    break;
                }
            }

            return CycleCollection;
        }

        private DateTime FindNext111Event(List<IndianaEvent> DTTB, int counter)
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

        private bool DoesTheCycleEndNormal(List<IndianaEvent> ControllerEventLogs, int counter)
        {
            var foundEvent111 = false;

            for (var x = counter; x < ControllerEventLogs.Count; x++)
                switch (ControllerEventLogs[x].EventCode)
                {
                    case 102:
                        foundEvent111 = false;
                        x = ControllerEventLogs.Count;
                        break;
                    case 105:
                        foundEvent111 = false;
                        x = ControllerEventLogs.Count;
                        break;

                    case 111:
                        foundEvent111 = true;
                        x = ControllerEventLogs.Count;
                        break;
                }

            return foundEvent111;
        }

        private bool DoesTrackClearEndNormal(List<IndianaEvent> DTTB, int counter)
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

        private void EndCycle(PreemptCycle cycle, IndianaEvent controllerEventLog,
            List<PreemptCycle> CycleCollection)
        {
            cycle.CycleEnd = controllerEventLog.Timestamp;
            CycleCollection.Add(cycle);
        }


        private PreemptCycle StartCycle(IndianaEvent controllerEventLog)
        {
            var cycle = new PreemptCycle
            {
                CycleStart = controllerEventLog.Timestamp
            };

            if (controllerEventLog.EventCode == 105)
            {
                cycle.EntryStarted = controllerEventLog.Timestamp;
                cycle.HasDelay = false;
            }

            if (controllerEventLog.EventCode == 102)
            {
                cycle.StartInputOn = controllerEventLog.Timestamp;
                cycle.HasDelay = true;
            }

            return cycle;
        }
    }
}