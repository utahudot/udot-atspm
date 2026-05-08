#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.Common/CycleService.cs
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

using Utah.Udot.Atspm.Business.ApproachSpeed;
using Utah.Udot.Atspm.Business.PreemptService;
using Utah.Udot.Atspm.Business.TimingAndActuation;
using Utah.Udot.Atspm.Business.TransitSignalPriority;
using Utah.Udot.Atspm.Business.YellowRedActivations;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.TempExtensions;

namespace Utah.Udot.Atspm.Business.Common
{
    public class CycleService
    {

        public CycleService()
        {
        }
        /// <summary>
        /// Needs event codes 1,8,9,61,63,64
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="cycleEvents"></param>
        /// <returns></returns>
        public List<RedToRedCycle> GetRedToRedCycles(
            DateTime startTime,
            DateTime endTime,
            List<IndianaEvent> cycleEvents)
        {
            var cycles = cycleEvents
                .Select((eventLog, index) => new { EventLog = eventLog, Index = index })
                .Where(item =>
                    item.Index < cycleEvents.Count - 3
                    && GetEventType(cycleEvents[item.Index].EventCode) == RedToRedCycle.EventType.ChangeToRed
                    && GetEventType(cycleEvents[item.Index + 1].EventCode) == RedToRedCycle.EventType.ChangeToGreen
                    && GetEventType(cycleEvents[item.Index + 2].EventCode) == RedToRedCycle.EventType.ChangeToYellow
                    && GetEventType(cycleEvents[item.Index + 3].EventCode) == RedToRedCycle.EventType.ChangeToRed)
                .Select(item => new RedToRedCycle(
                    cycleEvents[item.Index].Timestamp,
                    cycleEvents[item.Index + 1].Timestamp,
                    cycleEvents[item.Index + 2].Timestamp,
                    cycleEvents[item.Index + 3].Timestamp))
                .ToList();

            return cycles.Where(c => c.EndTime >= startTime && c.EndTime <= endTime || c.StartTime <= endTime && c.StartTime >= startTime).ToList();
        }




        /// <summary>
        /// Needs event codes 1,8,9,61,63,64
        /// </summary>
        /// <param name="approach"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="getPermissivePhase"></param>
        /// <param name="cycleEvents"></param>
        /// <returns></returns>
        public List<GreenToGreenCycle> GetGreenToGreenCycles(DateTime startTime, DateTime endTime, List<IndianaEvent> cycleEvents)
        {
            var cycles = new List<GreenToGreenCycle>();
            for (var i = 0; i < cycleEvents.Count; i++)
                if (i < cycleEvents.Count - 3
                    && GetEventType(cycleEvents[i].EventCode) == RedToRedCycle.EventType.ChangeToGreen
                    && GetEventType(cycleEvents[i + 1].EventCode) == RedToRedCycle.EventType.ChangeToYellow
                    && GetEventType(cycleEvents[i + 2].EventCode) == RedToRedCycle.EventType.ChangeToRed
                    && GetEventType(cycleEvents[i + 3].EventCode) == RedToRedCycle.EventType.ChangeToGreen)
                    cycles.Add(new GreenToGreenCycle(cycleEvents[i].Timestamp, cycleEvents[i + 1].Timestamp,
                        cycleEvents[i + 2].Timestamp, cycleEvents[i + 3].Timestamp));
            return cycles.Where(c => c.EndTime >= startTime && c.EndTime <= endTime || c.StartTime <= endTime && c.StartTime >= startTime).ToList();
        }

        public List<YellowRedActivationsCycle> GetYellowRedActivationsCycles(
            DateTime startTime,
            DateTime endTime,
            IReadOnlyList<IndianaEvent> cycleEvents,
            IReadOnlyList<IndianaEvent> detectorEvents,
            double severeViolationSeconds)
        {
            var cycles = new List<YellowRedActivationsCycle>();
            for (var item = 0; item < cycleEvents.Count; item++)
                if (item < cycleEvents.Count - 3
                    && GetYellowToRedEventType(cycleEvents[item].EventCode) == YellowRedEventType.BeginYellowClearance
                    && GetYellowToRedEventType(cycleEvents[item + 1].EventCode) == YellowRedEventType.BeginRedClearance
                    && GetYellowToRedEventType(cycleEvents[item + 2].EventCode) == YellowRedEventType.BeginRed
                    && GetYellowToRedEventType(cycleEvents[item + 3].EventCode) == YellowRedEventType.EndRed)
                {
                    cycles.Add(new YellowRedActivationsCycle(
                    cycleEvents[item].Timestamp,
                    cycleEvents[item + 1].Timestamp,
                    cycleEvents[item + 2].Timestamp,
                    cycleEvents[item + 3].Timestamp,
                    severeViolationSeconds,
                    detectorEvents
                    ));
                }
            return cycles.Where(c => c.EndTime >= startTime && c.EndTime <= endTime || c.StartTime <= endTime && c.StartTime >= startTime).ToList();


        }



        /// <summary>
        /// Needs event codes 1,8,9,61,63,64
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="approach"></param>
        /// <param name="detectorEvents"></param>
        /// <param name="getPermissivePhase"></param>
        /// <param name="pcdCycleTime"></param>
        /// <returns></returns>
        public async Task<List<CyclePcd>> GetPcdCycles(
            DateTime startDate,
            DateTime endDate,
            List<IndianaEvent> detectorEvents,
            List<IndianaEvent> cycleEvents,
            int? pcdCycleTime)
        {
            cycleEvents = cycleEvents.OrderBy(c => c.Timestamp).ToList();
            if (cycleEvents.Count <= 0)
            {
                return new List<CyclePcd>();
            }
            double pcdCycleShift = pcdCycleTime ?? 0;
            var cycles = new List<CyclePcd>();
            for (var i = 0; i < cycleEvents.Count; i++)
                if (i < cycleEvents.Count - 3
                    && GetEventType(cycleEvents[i].EventCode) == RedToRedCycle.EventType.ChangeToRed
                    && GetEventType(cycleEvents[i + 1].EventCode) == RedToRedCycle.EventType.ChangeToGreen
                    && GetEventType(cycleEvents[i + 2].EventCode) == RedToRedCycle.EventType.ChangeToYellow
                    && GetEventType(cycleEvents[i + 3].EventCode) == RedToRedCycle.EventType.ChangeToRed)
                    cycles.Add(new CyclePcd(cycleEvents[i].Timestamp, cycleEvents[i + 1].Timestamp,
                        cycleEvents[i + 2].Timestamp, cycleEvents[i + 3].Timestamp));
            if (cycles.Any())
            {
                var tasks = new List<Task>();
                foreach (var cycle in cycles)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        AddDetectorEventsToCycles(detectorEvents, cycle, pcdCycleShift);
                    }));
                }
                await Task.WhenAll(tasks);
            }
            return cycles.Where(c => c.EndTime >= startDate && c.EndTime <= endDate || c.StartTime <= endDate && c.StartTime >= startDate).ToList();
        }

        private static async Task AddDetectorEventsToCycles(List<IndianaEvent> detectorEvents, CyclePcd cycle, double pcdCycleShift)
        {
            var eventsForCycle = detectorEvents
                                    .Where(d => d.Timestamp >= cycle.StartTime.AddSeconds(-pcdCycleShift) &&
                                                                       d.Timestamp < cycle.EndTime.AddSeconds(pcdCycleShift)).ToList();
            foreach (var controllerEventLog in eventsForCycle)
                cycle.AddDetectorData(new DetectorDataPoint(cycle.StartTime, controllerEventLog.Timestamp,
                                               cycle.GreenEvent, cycle.YellowEvent));
        }
        private static async Task AddPhaseDropsCallsEventsToCycles(List<IndianaEvent> phaseDropsCalls, WaitTimeCycle cycle)
        {
            cycle.PhaseRegisterDroppedCalls = phaseDropsCalls.Where(d => d.Timestamp >= cycle.RedEvent && d.Timestamp < cycle.GreenEvent).OrderBy(e => e.Timestamp).ToList();
        }

        private static RedToRedCycle.EventType GetEventType(short eventCode)
        {
            return eventCode switch
            {
                1 => RedToRedCycle.EventType.ChangeToGreen,
                3 => RedToRedCycle.EventType.ChangeToEndMinGreen,
                61 => RedToRedCycle.EventType.ChangeToGreen,
                8 => RedToRedCycle.EventType.ChangeToYellow,
                63 => RedToRedCycle.EventType.ChangeToYellow,
                9 => RedToRedCycle.EventType.ChangeToRed,
                11 => RedToRedCycle.EventType.ChangeToEndOfRedClearance,
                64 => RedToRedCycle.EventType.ChangeToRed,
                66 => RedToRedCycle.EventType.OverLapDark,
                _ => RedToRedCycle.EventType.Unknown,
            };
        }

        /// <summary>
        /// Needs event codes 1,8,9,61,63,64
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="getPermissivePhase"></param>
        /// <param name="detector"></param>
        /// <returns></returns>
        public List<CycleSpeed> GetSpeedCycles(DateTime startDate, DateTime endDate, List<IndianaEvent> cycleEvents)
        {
            var mainEvents = cycleEvents.Where(c => c.Timestamp <= endDate && c.Timestamp >= startDate).ToList();
            var previousEvents = cycleEvents.Where(c => c.Timestamp < startDate).ToList();
            var nextEvents = cycleEvents.Where(c => c.Timestamp > endDate).ToList();
            var cycles = new List<CycleSpeed>();
            if (!mainEvents.IsNullOrEmpty())
            {
                if (GetEventType(mainEvents[mainEvents.Count - 1].EventCode) != RedToRedCycle.EventType.ChangeToRed || mainEvents.LastOrDefault().Timestamp < endDate)
                    //Get events to complete cycles
                    mainEvents.AddRange(nextEvents.OrderBy(e => e.Timestamp).Take(3));
                if (GetEventType(mainEvents[0].EventCode) != RedToRedCycle.EventType.ChangeToRed || mainEvents[0].Timestamp > startDate)
                    //Get events to start cycles
                    mainEvents.InsertRange(0, previousEvents.OrderByDescending(e => e.Timestamp).Take(3).OrderBy(e => e.Timestamp));
                for (var i = 0; i < mainEvents.Count; i++)
                    if (i < mainEvents.Count - 3
                        && GetEventType(mainEvents[i].EventCode) == RedToRedCycle.EventType.ChangeToRed
                        && GetEventType(mainEvents[i + 1].EventCode) == RedToRedCycle.EventType.ChangeToGreen
                        && GetEventType(mainEvents[i + 2].EventCode) == RedToRedCycle.EventType.ChangeToYellow
                        && GetEventType(mainEvents[i + 3].EventCode) == RedToRedCycle.EventType.ChangeToRed)
                        cycles.Add(new CycleSpeed(mainEvents[i].Timestamp, mainEvents[i + 1].Timestamp,
                            mainEvents[i + 2].Timestamp, mainEvents[i + 3].Timestamp));
            }
            return cycles;
        }

        public List<CycleSplitFail> GetSplitFailCycles(SplitFailOptions options, IReadOnlyList<IndianaEvent> cycleEvents, IReadOnlyList<IndianaEvent> terminationEvents)
        {
            var cycles = Enumerable.Range(0, cycleEvents.Count - 3)
                .Where(i => GetEventType(cycleEvents[i].EventCode) == RedToRedCycle.EventType.ChangeToGreen &&
                            GetEventType(cycleEvents[i + 1].EventCode) == RedToRedCycle.EventType.ChangeToYellow &&
                            GetEventType(cycleEvents[i + 2].EventCode) == RedToRedCycle.EventType.ChangeToRed &&
                            (GetEventType(cycleEvents[i + 3].EventCode) == RedToRedCycle.EventType.ChangeToGreen ||
                            cycleEvents[i + 3].EventCode == 66))
                .Select(i =>
                {
                    var termEvent = GetTerminationTypeBetweenStartAndEnd(cycleEvents[i].Timestamp, cycleEvents[i + 3].Timestamp, terminationEvents);
                    return new CycleSplitFail(cycleEvents[i].Timestamp, cycleEvents[i + 2].Timestamp, cycleEvents[i + 1].Timestamp,
                                              cycleEvents[i + 3].Timestamp, termEvent, options.FirstSecondsOfRed);
                })
                .Where(c => c.EndTime >= options.Start && c.EndTime <= options.End || c.StartTime <= options.End && c.StartTime >= options.Start)
                .ToList();

            return cycles;
        }

        public List<TransitSignalPriorityCycle> GetTransitSignalPriorityCycles(int phaseNumber, List<IndianaEvent> cycleEvents, List<IndianaEvent> terminationEvents, List<IndianaEvent> minGreenEvents)
        {
            if (cycleEvents.IsNullOrEmpty()) return new List<TransitSignalPriorityCycle>();
            var cycleEventsForPhase = cycleEvents.Where(c => c.EventParam == phaseNumber).OrderBy(c => c.Timestamp).ToList();
            var terminationEventsForPhase = terminationEvents.Where(c => c.EventParam == phaseNumber).ToList();

            var cleanTerminationEvents = CleanTerminationEvents(terminationEventsForPhase);
            var cycles = new List<TransitSignalPriorityCycle>();
            //var combinedEvents = cycleEvents.Concat(cleanTerminationEvents).Concat(minGreenEvents).Where(c => c.EventParam == phaseNumber).OrderBy(c => c.Timestamp).OrderBy(e => e.Timestamp).ToList();
            TransitSignalPriorityCycle cycle = null;
            foreach (var cycleEvent in cycleEventsForPhase)
            {
                if (cycleEvent.EventCode == (short)1)
                {
                    cycle = new TransitSignalPriorityCycle { GreenEvent = cycleEvent.Timestamp };
                }

                if (cycle != null && cycleEvent.EventCode == 8)
                    cycle.YellowEvent = cycleEvent.Timestamp;

                if (cycle != null && cycleEvent.EventCode == 10)
                {
                    cycle.RedEvent = cycleEvent.Timestamp;
                }

                if (cycle != null && cycleEvent.EventCode == 11 && cycle.YellowEvent != DateTime.MinValue && cycle.RedEvent != DateTime.MinValue)
                {
                    cycle.EndRedClearanceEvent = cycleEvent.Timestamp;
                    cycle.PhaseNumber = cycleEvent.EventParam;
                    cycles.Add(cycle);
                }
            }


            //Assign the termination event from the list of termation events to each cycle. If multiple are found use the last event. Also add the min green timestamp
            foreach (var c in cycles)
            {
                c.TerminationEvent = GetTerminationEventBetweenStartAndEnd(c.GreenEvent, c.EndRedClearanceEvent, cleanTerminationEvents);
                var minGreenEvent = minGreenEvents.Where(e => e.Timestamp >= c.GreenEvent && e.Timestamp <= c.EndRedClearanceEvent).FirstOrDefault();
                c.MinGreen = minGreenEvent == null ? c.YellowEvent : minGreenEvent.Timestamp;
            }
            //Filter out cycles that are outliers
            cycles = cycles.Where(c => c.MinGreenDurationSeconds >= 1 && c.RedDurationSeconds >= 1 && c.YellowDurationSeconds >= 1 && c.RedDurationSeconds < 10).ToList();
            return cycles;



        }

        private static CycleSplitFail.TerminationType GetTerminationTypeBetweenStartAndEnd(DateTime start,
            DateTime end, IReadOnlyList<IndianaEvent> terminationEvents)
        {
            var terminationType = CycleSplitFail.TerminationType.Unknown;
            var terminationEvent = terminationEvents.FirstOrDefault(t => t.Timestamp > start && t.Timestamp <= end);
            if (terminationEvent != null)
                terminationType = terminationEvent.EventCode switch
                {
                    4 => CycleSplitFail.TerminationType.GapOut,
                    5 => CycleSplitFail.TerminationType.MaxOut,
                    6 => CycleSplitFail.TerminationType.ForceOff,
                    _ => CycleSplitFail.TerminationType.Unknown,
                };
            return terminationType;
        }

        private static short? GetTerminationEventBetweenStartAndEnd(DateTime start,
            DateTime end, IReadOnlyList<IndianaEvent> terminationEvents)
        {
            return terminationEvents.Where(t => t.Timestamp > start && t.Timestamp <= end).OrderBy(t => t.Timestamp).LastOrDefault()?.EventCode;
        }

        private static YellowRedEventType GetYellowToRedEventType(short EventCode)
        {
            return EventCode switch
            {
                8 => YellowRedEventType.BeginYellowClearance,
                // overlap yellow
                63 => YellowRedEventType.BeginYellowClearance,
                9 => YellowRedEventType.BeginRedClearance,
                // overlap red
                64 => YellowRedEventType.BeginRedClearance,
                65 => YellowRedEventType.BeginRed,
                11 => YellowRedEventType.BeginRed,
                1 => YellowRedEventType.EndRed,
                // overlap green
                62 => YellowRedEventType.EndRed,
                _ => YellowRedEventType.Unknown,
            };
        }

        public enum YellowRedEventType
        {
            BeginYellowClearance,
            BeginRedClearance,
            BeginRed,
            EndRed,
            Unknown
        }


        public List<PreemptCycle> CreatePreemptCycle(List<IndianaEvent> preemptEvents)
        {
            var CycleCollection = new List<PreemptCycle>();
            PreemptCycle cycle = null;


            for (var x = 0; x < preemptEvents.Count; x++)
            {
                //It can happen that there is no defined terminaiton event.
                if (x + 1 < preemptEvents.Count)
                {
                    var timeBetweenEvents = preemptEvents[x + 1].Timestamp - preemptEvents[x].Timestamp;
                    if (cycle != null && timeBetweenEvents.TotalMinutes > 20 && preemptEvents[x].EventCode != 111 &&
                        preemptEvents[x].EventCode != 105)
                    {
                        EndCycle(cycle, preemptEvents[x], CycleCollection);
                        cycle = null;
                        continue;
                    }
                }

                switch (preemptEvents[x].EventCode)
                {
                    case 102:

                        if (cycle != null)
                            cycle.InputOn.Add(preemptEvents[x].Timestamp);

                        if (cycle == null && preemptEvents[x].Timestamp != preemptEvents[x + 1].Timestamp &&
                            preemptEvents[x + 1].EventCode == 105)
                            cycle = StartCycle(preemptEvents[x]);

                        break;

                    case 103:

                        if (cycle != null && cycle.GateDown == DateTime.MinValue)
                            cycle.GateDown = preemptEvents[x].Timestamp;


                        break;

                    case 104:

                        if (cycle != null)
                            cycle.InputOff.Add(preemptEvents[x].Timestamp);

                        break;

                    case 105:


                        ////If we run into an entry start after cycle start (event 102)
                        if (cycle != null && cycle.HasDelay)
                        {
                            cycle.EntryStarted = preemptEvents[x].Timestamp;
                            break;
                        }

                        if (cycle != null)
                        {
                            EndCycle(cycle, preemptEvents[x], CycleCollection);
                            cycle = StartCycle(preemptEvents[x]);
                            break;
                        }

                        if (cycle == null)
                            cycle = StartCycle(preemptEvents[x]);
                        break;

                    case 106:
                        if (cycle != null)
                        {
                            cycle.BeginTrackClearance = preemptEvents[x].Timestamp;

                            if (x + 1 < preemptEvents.Count && !DoesTrackClearEndNormal(preemptEvents, x))
                                cycle.BeginDwellService = FindNext111Event(preemptEvents, x);
                        }
                        break;

                    case 107:

                        if (cycle != null)
                        {
                            cycle.BeginDwellService = preemptEvents[x].Timestamp;

                            if (x + 1 < preemptEvents.Count && !DoesTheCycleEndNormal(preemptEvents, x))
                            {
                                cycle.BeginExitInterval = preemptEvents[x + 1].Timestamp;

                                EndCycle(cycle, preemptEvents[x + 1], CycleCollection);

                                cycle = null;
                            }
                        }


                        break;

                    case 108:
                        if (cycle != null)
                            cycle.LinkActive = preemptEvents[x].Timestamp;
                        break;

                    case 109:
                        if (cycle != null)
                            cycle.LinkInactive = preemptEvents[x].Timestamp;

                        break;

                    case 110:
                        if (cycle != null)
                            cycle.MaxPresenceExceeded = preemptEvents[x].Timestamp;
                        break;

                    case 111:
                        // 111 can usually be considered "cycle complete"
                        if (cycle != null)
                        {
                            cycle.BeginExitInterval = preemptEvents[x].Timestamp;

                            EndCycle(cycle, preemptEvents[x], CycleCollection);


                            cycle = null;
                        }
                        break;
                }


                if (x + 1 >= preemptEvents.Count && cycle != null)
                {
                    cycle.BeginExitInterval = preemptEvents[x].Timestamp;
                    EndCycle(cycle, preemptEvents[x], CycleCollection);
                    break;
                }
            }

            return CycleCollection;
        }

        private static DateTime FindNext111Event(List<IndianaEvent> DTTB, int counter)
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

        private static bool DoesTheCycleEndNormal(List<IndianaEvent> DTTB, int counter)
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

        private static bool DoesTrackClearEndNormal(List<IndianaEvent> DTTB, int counter)
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

        private void EndCycle(PreemptCycle cycle, IndianaEvent controller_Event_Log,
            List<PreemptCycle> CycleCollection)
        {
            cycle.CycleEnd = controller_Event_Log.Timestamp;
            cycle.Delay = GetDelay(cycle.HasDelay, cycle.EntryStarted, cycle.CycleStart);
            cycle.TimeToService = GetTimeToService(
                cycle.HasDelay,
                cycle.BeginTrackClearance,
                cycle.CycleStart,
                cycle.BeginDwellService,
                cycle.EntryStarted);
            cycle.DwellTime = GetDwellTime(cycle.CycleEnd, cycle.BeginDwellService);
            cycle.TimeToCallMaxOut = GetTimeToCallMaxOut(cycle.CycleStart, cycle.MaxPresenceExceeded);
            cycle.TimeToEndOfEntryDelay = GetTimeToEndOfEntryDelay(cycle.EntryStarted, cycle.CycleStart);
            cycle.TimeToTrackClear = GetTimeToTrackClear(cycle.BeginDwellService, cycle.BeginTrackClearance);
            cycle.TimeToGateDown = GetTimeToGateDown(cycle.CycleStart, cycle.GateDown);
            CycleCollection.Add(cycle);
        }

        private static double GetTimeToGateDown(DateTime cycleStart, DateTime gateDown)
        {
            if (cycleStart > DateTime.MinValue && gateDown > DateTime.MinValue && gateDown > cycleStart)
                return (gateDown - cycleStart).TotalSeconds;
            return 0;
        }

        private static double GetTimeToTrackClear(DateTime beginDwellService, DateTime beginTrackClearance)
        {
            if (beginDwellService > DateTime.MinValue && beginTrackClearance > DateTime.MinValue &&
                    beginDwellService > beginTrackClearance)
                return (beginDwellService - beginTrackClearance).TotalSeconds;
            return 0;
        }

        private static double GetTimeToEndOfEntryDelay(DateTime entryStarted, DateTime cycleStart)
        {
            if (cycleStart > DateTime.MinValue && entryStarted > DateTime.MinValue && entryStarted > cycleStart)
                return (entryStarted - cycleStart).TotalSeconds;
            return 0;
        }

        private static double GetTimeToCallMaxOut(DateTime CycleStart, DateTime MaxPresenceExceeded)
        {
            if (CycleStart > DateTime.MinValue && MaxPresenceExceeded > DateTime.MinValue &&
                   MaxPresenceExceeded > CycleStart)
                return (MaxPresenceExceeded - CycleStart).TotalSeconds;
            return 0;
        }

        private static double GetDwellTime(DateTime cycleEnd, DateTime beginDwellService)
        {
            if (cycleEnd > DateTime.MinValue && beginDwellService > DateTime.MinValue &&
                    cycleEnd >= beginDwellService)
                return (cycleEnd - beginDwellService).TotalSeconds;
            return 0;
        }

        private static double GetTimeToService(
            bool hasDelay,
            DateTime beginTrackClearance,
            DateTime cycleStart,
            DateTime beginDwellService,
            DateTime entryStarted)
        {
            if (beginTrackClearance > DateTime.MinValue && cycleStart > DateTime.MinValue &&
                   beginTrackClearance >= cycleStart)
            {
                if (hasDelay)
                    return (beginTrackClearance - entryStarted).TotalSeconds;
                return (beginTrackClearance - cycleStart).TotalSeconds;
            }

            if (beginDwellService > DateTime.MinValue && cycleStart > DateTime.MinValue &&
                beginDwellService >= cycleStart)
            {
                if (hasDelay)
                    return (beginDwellService - entryStarted).TotalSeconds;
                return (beginDwellService - cycleStart).TotalSeconds;
            }

            return 0;
        }

        private static double GetDelay(bool hasDelay, DateTime entryStarted, DateTime cycleStart)
        {
            if (hasDelay && entryStarted > DateTime.MinValue && cycleStart > DateTime.MinValue &&
                        entryStarted > cycleStart)
                return (entryStarted - cycleStart).TotalSeconds;

            return 0;
        }


        private static PreemptCycle StartCycle(IndianaEvent controller_Event_Log)
        {
            var cycle = new PreemptCycle();


            cycle.CycleStart = controller_Event_Log.Timestamp;

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

        public async Task<List<WaitTimeCycle>> GetWaitTimeCyclesAsync(
            List<IndianaEvent> cycleEvents,
            List<IndianaEvent> phaseCallsDrops,
            DateTime start,
            DateTime end)
        {
            cycleEvents = cycleEvents.OrderBy(c => c.Timestamp).ToList();
            if (cycleEvents.Count <= 0)
            {
                return new List<WaitTimeCycle>();
            }
            var cycles = new List<WaitTimeCycle>();
            for (var i = 0; i < cycleEvents.Count; i++)
                if (i < cycleEvents.Count - 3
                    && GetEventType(cycleEvents[i].EventCode) == RedToRedCycle.EventType.ChangeToEndOfRedClearance
                    && GetEventType(cycleEvents[i + 1].EventCode) == RedToRedCycle.EventType.ChangeToGreen)
                    cycles.Add(new WaitTimeCycle(cycleEvents[i].Timestamp, cycleEvents[i + 1].Timestamp));
            if (cycles.Any())
            {
                var tasks = new List<Task>();
                foreach (var cycle in cycles)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        AddPhaseDropsCallsEventsToCycles(phaseCallsDrops, cycle);
                    }));
                }
                await Task.WhenAll(tasks);
            }
            return cycles.Where(c => c.GreenEvent >= start && c.GreenEvent <= end || c.RedEvent <= end && c.RedEvent >= end).ToList();
        }

        public List<IndianaEvent> CleanTerminationEvents(IReadOnlyList<IndianaEvent> terminationEvents)
        {

            var sortedEvents = terminationEvents.OrderBy(x => x.Timestamp).ThenBy(y => y.EventCode).ToList();
            var duplicateList = new List<IndianaEvent>();
            for (int i = 0; i < sortedEvents.Count - 1; i++)
            {
                var event1 = sortedEvents[i];
                var event2 = sortedEvents[i + 1];
                if (event1.Timestamp == event2.Timestamp)
                {
                    if (event1.EventCode == 7)
                        duplicateList.Add(event1);
                    if (event2.EventCode == 7)
                        duplicateList.Add(event2);
                }
            }

            foreach (var e in duplicateList)
            {
                sortedEvents.Remove(e);
            }
            return sortedEvents;
        }

        public string GetPhaseSort(PhaseDetail phaseDetail)
        {
            return phaseDetail.IsPermissivePhase ?  // Check if the 'PhaseType' property of 'options' is true
                phaseDetail.Approach.IsPermissivePhaseOverlap ?  // If true, check if the 'IsPermissivePhaseOverlap' property of 'approach' is true
                    $"zOverlap - {phaseDetail.Approach.PermissivePhaseNumber.Value.ToString("D2")}-1"  // If true, concatenate "zOverlap - " with 'PermissivePhaseNumber' formatted as a two-digit string
                    : $"Phase - {phaseDetail.Approach.PermissivePhaseNumber.Value.ToString("D2")}-1" // If false, concatenate "Phase - " with 'PermissivePhaseNumber' formatted as a two-digit string
                :  // If 'PhaseType' is false
                phaseDetail.Approach.IsProtectedPhaseOverlap ?  // Check if the 'IsProtectedPhaseOverlap' property of 'approach' is true
                    $"zOverlap - {phaseDetail.Approach.ProtectedPhaseNumber.ToString("D2")}-2"  // If true, concatenate "zOverlap - " with 'ProtectedPhaseNumber' formatted as a two-digit string
                    : $"Phase - {phaseDetail.Approach.ProtectedPhaseNumber.ToString("D2")}-2";  // If false, concatenate "Phase = " with 'ProtectedPhaseNumber' formatted as a two-digit string
        }

        public List<CycleEventsDto> GetCycleEvents(
            PhaseDetail phaseDetail,
            List<IndianaEvent> controllerEventLogs,
            DateTime start,
            DateTime end)
        {

            List<short> cycleEventCodes = GetCycleCodes(phaseDetail.UseOverlap);
            var overlapLabel = phaseDetail.UseOverlap == true ? "Overlap" : "";
            string keyLabel = $"Cycles Intervals {phaseDetail.PhaseNumber} {overlapLabel}";
            var events = new List<CycleEventsDto>();
            if (controllerEventLogs.Any())
            {
                var tempEvents = controllerEventLogs.Where(c => cycleEventCodes.Contains(c.EventCode) && c.EventParam == phaseDetail.PhaseNumber)
                    .Select(e => new CycleEventsDto(e.Timestamp, (int)e.EventCode)).ToList();
                events.AddRange(tempEvents.Where(e => e.Start >= start
                                                        && e.Start <= end));
                var firstEvent = tempEvents.Where(e => e.Start < start).OrderByDescending(e => e.Start).FirstOrDefault();
                if (firstEvent != null)
                {
                    firstEvent.Start = start;
                    events.Insert(0, firstEvent);
                }
            }
            return events;
        }

        public List<short> GetCycleCodes(bool getOverlapCodes)
        {
            var phaseEventCodesForCycles = new List<short>
            {
                1,
                3,
                8,
                9,
                11
            };
            if (getOverlapCodes)
            {
                phaseEventCodesForCycles = new List<short>
                {
                    61,
                    62,
                    63,
                    64,
                    65
                };
            }

            return phaseEventCodesForCycles;
        }


        public Dictionary<string, List<DataPointForInt>> GetPhaseCustomEvents(
            string locationIdentifier,
            int phaseNumber,
            DateTime start,
            DateTime end,
            List<short>? phaseEventCodesList,
            List<IndianaEvent> controllerEventLogs)
        {
            var phaseCustomEvents = new Dictionary<string, List<DataPointForInt>>();
            if (phaseEventCodesList != null && phaseEventCodesList.Any())
            {
                foreach (var phaseEventCode in phaseEventCodesList)
                {

                    var phaseEvents = controllerEventLogs.Where(c => c.EventCode == phaseEventCode
                                                                        && c.Timestamp >= start
                                                                        && c.Timestamp <= end).ToList();
                    if (phaseEvents.Count > 0)
                    {
                        phaseCustomEvents.Add(
                            "Phase Events: " + phaseEventCode, phaseEvents.Select(s => new DataPointForInt(s.Timestamp, (int)s.EventCode)).ToList());
                    }

                    if (phaseCustomEvents.Count == 0)
                    {
                        var forceEventsForAllLanes = new List<IndianaEvent>();
                        var tempEvent1 = new IndianaEvent()
                        {
                            LocationIdentifier = locationIdentifier,
                            EventCode = phaseEventCode,
                            EventParam = Convert.ToByte(phaseNumber),
                            Timestamp = start.AddSeconds(-10)
                        };
                        forceEventsForAllLanes.Add(tempEvent1);
                        var tempEvent2 = new IndianaEvent()
                        {
                            LocationIdentifier = locationIdentifier,
                            EventCode = phaseEventCode,
                            EventParam = Convert.ToByte(phaseNumber),
                            Timestamp = start.AddSeconds(-9)
                        };
                        forceEventsForAllLanes.Add(tempEvent2);
                        phaseCustomEvents.Add(
                            "Phase Events: " + phaseEventCode, forceEventsForAllLanes.Select(s => new DataPointForInt(s.Timestamp, (int)s.EventCode))

                            .ToList());
                    }
                }
            }
            return phaseCustomEvents;
        }

        public List<DetectorEventDto> GetPedestrianEventsNew(
            Approach approach,
            DateTime start,
            DateTime end,
            List<IndianaEvent> controllerEventLogs)
        {
            var pedestrianEvents = new List<DetectorEventDto>();
            if (string.IsNullOrEmpty(approach.PedestrianDetectors) && approach.Location.PedsAre1to1 && approach.IsProtectedPhaseOverlap
                || !approach.Location.PedsAre1to1 && approach.PedestrianPhaseNumber.HasValue)
                return pedestrianEvents;
            var pedEventCodes = new List<short> { 90 };
            var pedDedectors = approach.GetPedDetectorsFromApproach();
            var pedEvents = controllerEventLogs.GetPedEvents(start, end, approach).ToList();
            foreach (var pedDetector in pedDedectors)
            {
                var lableName = $"Ped Det. Actuations, ph {approach.ProtectedPhaseNumber}, ch {pedDetector} ";
                var pedEventsForDetector = pedEvents.Where(c => pedEventCodes.Contains(c.EventCode)
                                                                && c.EventParam == pedDetector
                                                                && c.Timestamp >= start
                                                                && c.Timestamp <= end)
                                                   .ToList();
                if (pedEventsForDetector.Count > 0)
                {
                    var detectorEvents = new List<DetectorEventBase>();
                    for (var i = 0; i < pedEventsForDetector.Count; i++)
                    {
                        detectorEvents.Add(new DetectorEventBase(pedEvents[i].Timestamp, pedEvents[i].Timestamp));
                    }
                    pedestrianEvents.Add(new DetectorEventDto(lableName, detectorEvents));
                }
            }
            return pedestrianEvents;
        }

        public List<CycleEventsDto> GetPedestrianIntervals(
            Approach approach,
            List<IndianaEvent> controllerEventLogs,
            DateTime start,
            DateTime end)
        {
            List<short> overlapCodes = GetPedestrianIntervalEventCodes(approach.IsPedestrianPhaseOverlap);
            var pedPhase = approach.PedestrianPhaseNumber ?? approach.ProtectedPhaseNumber;
            return controllerEventLogs.Where(c => overlapCodes.Contains(c.EventCode)
                                                    && c.EventParam == pedPhase
                                                    && c.Timestamp >= start
                                                    && c.Timestamp <= end).Select(s => new CycleEventsDto(s.Timestamp, (int)s.EventCode)).ToList();
        }

        public List<short> GetPedestrianIntervalEventCodes(bool isPhaseOrOverlap)
        {
            var overlapCodes = new List<short>
            {
                21,
                22,
                23
            };
            if (isPhaseOrOverlap)
            {
                overlapCodes = new List<short> { 67, 68, 69 };
            }

            return overlapCodes;
        }


    }
}


