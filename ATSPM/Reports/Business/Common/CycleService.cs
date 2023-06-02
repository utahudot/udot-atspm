using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
//using System.IO;
using System.Linq;
using ATSPM.Application.Repositories;
using ATSPM.Application.Reports.Business.SplitFail;
using ATSPM.Application.Reports.Business.YellowRedActivations;
using ATSPM.Application.Reports.Business.ApproachSpeed;

namespace ATSPM.Application.Reports.Business.Common
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
        public List<GreenToGreenCycle> GetGreenToGreenCycles( DateTime startTime, DateTime endTime,List<ControllerEventLog> cycleEvents)
        {
            //    if (cycleEvents != null && cycleEvents.Count > 0 && (GetEventType(cycleEvents.LastOrDefault().EventCode) !=
            //        RedToRedCycle.EventType.ChangeToGreen || cycleEvents.LastOrDefault().Timestamp < endTime))
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
            var cycles = cycleEvents
                .Select((eventLog, index) => new { EventLog = eventLog, Index = index })
                .Where(item =>
                    item.Index < cycleEvents.Count - 3
                    && GetYellowToRedEventType(cycleEvents[item.Index].EventCode) == YellowRedEventType.BeginYellowClearance
                    && GetYellowToRedEventType(cycleEvents[item.Index + 1].EventCode) == YellowRedEventType.BeginRedClearance
                    && GetYellowToRedEventType(cycleEvents[item.Index + 2].EventCode) == YellowRedEventType.BeginRed
                    && GetYellowToRedEventType(cycleEvents[item.Index + 3].EventCode) == YellowRedEventType.EndRed)
                .Select(item => new YellowRedActivationsCycle(
                    cycleEvents[item.Index].Timestamp,
                    cycleEvents[item.Index + 1].Timestamp,
                    cycleEvents[item.Index + 2].Timestamp,
                    cycleEvents[item.Index + 3].Timestamp,
                    severeViolationSeconds,
                    detectorEvents
                    ))
                .ToList();
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
        public List<CyclePcd> GetPcdCycles(
            DateTime startDate,
            DateTime endDate,
            List<ControllerEventLog> detectorEvents,
            List<ControllerEventLog> cycleEvents,
            int? pcdCycleTime)
        {
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
                foreach (var cycle in cycles)
                {
                    var eventsForCycle = detectorEvents
                        .Where(d => d.Timestamp >= cycle.StartTime.AddSeconds(-pcdCycleShift) &&
                                    d.Timestamp < cycle.EndTime.AddSeconds(pcdCycleShift)).ToList();
                    foreach (var controllerEventLog in eventsForCycle)
                        cycle.AddDetectorData(new DetectorDataPoint(cycle.StartTime, controllerEventLog.Timestamp,
                            cycle.GreenEvent, cycle.YellowEvent));
                }

            //var totalSortedEvents = cycles.Sum(d => d.DetectorEvents.Count);
            return cycles.Where(c => c.EndTime >= startDate && c.EndTime <= endDate || c.StartTime <= endDate && c.StartTime >= startDate).ToList();
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
            var mainEvents = cycleEvents.Where(c => c.Timestamp<=endDate && c.Timestamp>=startDate).ToList();
            var previousEvents = cycleEvents.Where(c => c.Timestamp < startDate).ToList();
            var nextEvents = cycleEvents.Where(c => c.Timestamp > endDate).ToList();
            if (mainEvents.Any() && (GetEventType(mainEvents.Last().EventCode) !=
                RedToRedCycle.EventType.ChangeToRed || mainEvents.LastOrDefault().Timestamp < endDate))
                //Get events to complete cycles
                mainEvents.AddRange(nextEvents.OrderBy(e => e.Timestamp).Take(3));
            if (mainEvents.Any() && (GetEventType(mainEvents.First().EventCode) !=
                RedToRedCycle.EventType.ChangeToRed || mainEvents.LastOrDefault().Timestamp > startDate))
                //Get events to start cycles
                mainEvents.InsertRange(0, nextEvents.OrderByDescending(e => e.Timestamp).Take(3).OrderBy(e => e.Timestamp));
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

    }
}