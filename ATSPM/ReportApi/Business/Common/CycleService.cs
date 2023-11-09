using ATSPM.Data.Models;
using ATSPM.ReportApi.Business.ApproachSpeed;
using ATSPM.ReportApi.Business.PreemptService;
using ATSPM.ReportApi.Business.SplitFail;
using ATSPM.ReportApi.Business.YellowRedActivations;
//using System.IO;

namespace ATSPM.ReportApi.Business.Common
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
            List<ControllerEventLog> cycleEvents)
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
        public List<GreenToGreenCycle> GetGreenToGreenCycles(DateTime startTime, DateTime endTime, List<ControllerEventLog> cycleEvents)
        {
            //    if (cycleEvents != null && cycleEvents.Count > 0 && (GetEventType(cycleEvents.LastOrDefault().EventCode) !=
            //        RedToRedCycle.EventType.ChangeToGreen || cycleEvents.LastOrDefault().TimeStamp < endTime))
            //        GetEventsToCompleteCycle(getPermissivePhase, endTime, approach, cycleEvents);
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
            IReadOnlyList<ControllerEventLog> cycleEvents,
            IReadOnlyList<ControllerEventLog> detectorEvents,
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


            //var cycles = cycleEvents
            //    .Select((eventLog, index) => new { EventLog = eventLog, Index = index })
            //    .Where(item =>
            //        item.Index < cycleEvents.Count - 3
            //        && GetYellowToRedEventType(cycleEvents[item.Index].EventCode) == YellowRedEventType.BeginYellowClearance
            //        && GetYellowToRedEventType(cycleEvents[item.Index + 1].EventCode) == YellowRedEventType.BeginRedClearance
            //        && GetYellowToRedEventType(cycleEvents[item.Index + 2].EventCode) == YellowRedEventType.BeginRed
            //        && GetYellowToRedEventType(cycleEvents[item.Index + 3].EventCode) == YellowRedEventType.EndRed)
            //    .Select(item => new YellowRedActivationsCycle(
            //        cycleEvents[item.Index].Timestamp,
            //        cycleEvents[item.Index + 1].Timestamp,
            //        cycleEvents[item.Index + 2].Timestamp,
            //        cycleEvents[item.Index + 3].Timestamp,
            //        severeViolationSeconds,
            //        detectorEvents
            //        ))
            //    .ToList();
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
            List<ControllerEventLog> detectorEvents,
            List<ControllerEventLog> cycleEvents,
            int? pcdCycleTime)
        {
            cycleEvents = cycleEvents.OrderBy(c => c.Timestamp).ToList();
            var min = cycleEvents.Min(c => c.Timestamp);
            var max = cycleEvents.Max(c => c.Timestamp);
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
                //Parallel.ForEach(cycles, cycle =>
                //{
                //    AddDetectorEventsToCycles(detectorEvents, cycle, pcdCycleShift);
                //});
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

        private async void AddDetectorEventsToCycles(List<ControllerEventLog> detectorEvents, CyclePcd cycle, double pcdCycleShift)
        {
            var eventsForCycle = detectorEvents
                                    .Where(d => d.Timestamp >= cycle.StartTime.AddSeconds(-pcdCycleShift) &&
                                                                       d.Timestamp < cycle.EndTime.AddSeconds(pcdCycleShift)).ToList();
            foreach (var controllerEventLog in eventsForCycle)
                cycle.AddDetectorData(new DetectorDataPoint(cycle.StartTime, controllerEventLog.Timestamp,
                                               cycle.GreenEvent, cycle.YellowEvent));
        }

        private RedToRedCycle.EventType GetEventType(int eventCode)
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
        public List<CycleSpeed> GetSpeedCycles(DateTime startDate, DateTime endDate, List<ControllerEventLog> cycleEvents)
        {
            var mainEvents = cycleEvents.Where(c => c.Timestamp <= endDate && c.Timestamp >= startDate).ToList();
            var previousEvents = cycleEvents.Where(c => c.Timestamp < startDate).ToList();
            var nextEvents = cycleEvents.Where(c => c.Timestamp > endDate).ToList();
            if (mainEvents.Any() && (GetEventType(mainEvents.Last().EventCode) !=
                RedToRedCycle.EventType.ChangeToRed || mainEvents.LastOrDefault().Timestamp < endDate))
                //Get events to complete cycles
                mainEvents.AddRange(nextEvents.OrderBy(e => e.Timestamp).Take(3));
            if (mainEvents.Any() && (GetEventType(mainEvents.First().EventCode) !=
                RedToRedCycle.EventType.ChangeToRed || mainEvents.FirstOrDefault().Timestamp > startDate))
                //Get events to start cycles
                mainEvents.InsertRange(0, previousEvents.OrderByDescending(e => e.Timestamp).Take(3).OrderBy(e => e.Timestamp));
            var cycles = new List<CycleSpeed>();
            if (mainEvents != null)
                for (var i = 0; i < mainEvents.Count; i++)
                    if (i < mainEvents.Count - 3
                        && GetEventType(mainEvents[i].EventCode) == RedToRedCycle.EventType.ChangeToRed
                        && GetEventType(mainEvents[i + 1].EventCode) == RedToRedCycle.EventType.ChangeToGreen
                        && GetEventType(mainEvents[i + 2].EventCode) == RedToRedCycle.EventType.ChangeToYellow
                        && GetEventType(mainEvents[i + 3].EventCode) == RedToRedCycle.EventType.ChangeToRed)
                        cycles.Add(new CycleSpeed(mainEvents[i].Timestamp, mainEvents[i + 1].Timestamp,
                            mainEvents[i + 2].Timestamp, mainEvents[i + 3].Timestamp));


            return cycles;
        }



        public List<CycleSplitFail> GetSplitFailCycles(SplitFailOptions options, IReadOnlyList<ControllerEventLog> cycleEvents, IReadOnlyList<ControllerEventLog> terminationEvents)
        {
            var cycles = Enumerable.Range(0, cycleEvents.Count - 3)
                .Where(i => GetEventType(cycleEvents[i].EventCode) == RedToRedCycle.EventType.ChangeToGreen &&
                            GetEventType(cycleEvents[i + 1].EventCode) == RedToRedCycle.EventType.ChangeToYellow &&
                            GetEventType(cycleEvents[i + 2].EventCode) == RedToRedCycle.EventType.ChangeToRed &&
                            (GetEventType(cycleEvents[i + 3].EventCode) == RedToRedCycle.EventType.ChangeToGreen ||
                            cycleEvents[i + 3].EventCode == 66))
                .Select(i =>
                {
                    var termEvent = GetTerminationEventBetweenStartAndEnd(cycleEvents[i].Timestamp, cycleEvents[i + 3].Timestamp, terminationEvents);
                    return new CycleSplitFail(cycleEvents[i].Timestamp, cycleEvents[i + 2].Timestamp, cycleEvents[i + 1].Timestamp,
                                              cycleEvents[i + 3].Timestamp, termEvent, options.FirstSecondsOfRed);
                })
                .Where(c => c.EndTime >= options.Start && c.EndTime <= options.End || c.StartTime <= options.End && c.StartTime >= options.Start)
                .ToList();

            return cycles;
        }

        private CycleSplitFail.TerminationType GetTerminationEventBetweenStartAndEnd(DateTime start,
            DateTime end, IReadOnlyList<ControllerEventLog> terminationEvents)
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


        private YellowRedEventType GetYellowToRedEventType(int EventCode)
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
                61 => YellowRedEventType.EndRed,
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


        public List<PreemptCycle> CreatePreemptCycle(List<ControllerEventLog> preemptEvents)
        {
            var CycleCollection = new List<PreemptCycle>();
            PreemptCycle cycle = null;


            //foreach (MOE.Common.Models.Controller_Event_Log row in DTTB.Events)
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

                            if (x + 1 < preemptEvents.Count)
                                if (!DoesTrackClearEndNormal(preemptEvents, x))
                                    cycle.BeginDwellService = FindNext111Event(preemptEvents, x);
                        }
                        break;

                    case 107:

                        if (cycle != null)
                        {
                            cycle.BeginDwellService = preemptEvents[x].Timestamp;

                            if (x + 1 < preemptEvents.Count)
                                if (!DoesTheCycleEndNormal(preemptEvents, x))
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

        private double GetTimeToGateDown(DateTime cycleStart, DateTime gateDown)
        {
            if (cycleStart > DateTime.MinValue && gateDown > DateTime.MinValue && gateDown > cycleStart)
                return (gateDown - cycleStart).TotalSeconds;
            return 0;
        }

        private double GetTimeToTrackClear(DateTime beginDwellService, DateTime beginTrackClearance)
        {
            if (beginDwellService > DateTime.MinValue && beginTrackClearance > DateTime.MinValue &&
                    beginDwellService > beginTrackClearance)
                return (beginDwellService - beginTrackClearance).TotalSeconds;
            return 0;
        }

        private double GetTimeToEndOfEntryDelay(DateTime entryStarted, DateTime cycleStart)
        {
            if (cycleStart > DateTime.MinValue && entryStarted > DateTime.MinValue && entryStarted > cycleStart)
                return (entryStarted - cycleStart).TotalSeconds;
            return 0;
        }

        private double GetTimeToCallMaxOut(DateTime CycleStart, DateTime MaxPresenceExceeded)
        {
            if (CycleStart > DateTime.MinValue && MaxPresenceExceeded > DateTime.MinValue &&
                   MaxPresenceExceeded > CycleStart)
                return (MaxPresenceExceeded - CycleStart).TotalSeconds;
            return 0;
        }

        private double GetDwellTime(DateTime cycleEnd, DateTime beginDwellService)
        {
            if (cycleEnd > DateTime.MinValue && beginDwellService > DateTime.MinValue &&
                    cycleEnd >= beginDwellService)
                return (cycleEnd - beginDwellService).TotalSeconds;
            return 0;
        }

        private double GetTimeToService(
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

        private double GetDelay(bool hasDelay, DateTime entryStarted, DateTime cycleStart)
        {
            if (hasDelay && entryStarted > DateTime.MinValue && cycleStart > DateTime.MinValue &&
                        entryStarted > cycleStart)
                return (entryStarted - cycleStart).TotalSeconds;

            return 0;
        }


        private PreemptCycle StartCycle(ControllerEventLog controller_Event_Log)
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
    }
}


