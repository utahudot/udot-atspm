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
            var cycles = new List<RedToRedCycle>();
            for (var i = 0; i < cycleEvents.Count; i++)
                if (i < cycleEvents.Count - 3
                    && GetEventType(cycleEvents[i].EventCode) == RedToRedCycle.EventType.ChangeToRed
                    && GetEventType(cycleEvents[i + 1].EventCode) == RedToRedCycle.EventType.ChangeToGreen
                    && GetEventType(cycleEvents[i + 2].EventCode) == RedToRedCycle.EventType.ChangeToYellow
                    && GetEventType(cycleEvents[i + 3].EventCode) == RedToRedCycle.EventType.ChangeToRed)
                {
                    cycles.Add(new RedToRedCycle(cycleEvents[i].Timestamp, cycleEvents[i + 1].Timestamp,
                        cycleEvents[i + 2].Timestamp, cycleEvents[i + 3].Timestamp));
                    i += 2;
                }

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
        public List<GreenToGreenCycle> GetGreenToGreenCycles(Approach approach, DateTime startTime, DateTime endTime,
            bool getPermissivePhase, List<ControllerEventLog> cycleEvents)
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
            Approach approach,
            List<ControllerEventLog> detectorEvents,
            List<ControllerEventLog> cycleEvents,
            int? pcdCycleTime)
        {
            double pcdCycleShift = pcdCycleTime ?? 0;
            //var cycleEvents = GetCycleEvents(getPermissivePhase, startDate.AddSeconds(-900), endDate.AddSeconds(900), approach);
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


        /// <summary>
        /// Needs event codes 1,3,8,9,11,61,63,64
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="approach"></param>
        /// <param name="getPermissivePhase"></param>
        /// <returns></returns>
        //public List<TimingAndActuationCycle> GetTimingAndActuationCycles(DateTime startDate, DateTime endDate,
        //    Approach approach, bool getPermissivePhase)
        //{
        //    var cycleEvents = GetDetailedCycleEvents(getPermissivePhase, startDate, endDate, approach);
        //    if (cycleEvents != null && cycleEvents.Count > 0 && (GetEventType(cycleEvents.LastOrDefault().EventCode) !=
        //        RedToRedCycle.EventType.ChangeToRed || cycleEvents.LastOrDefault().Timestamp < endDate))
        //        GetEventsToCompleteCycle(getPermissivePhase, endDate, approach, cycleEvents);
        //    var cycles = new List<TimingAndActuationCycle>();
        //    DateTime dummyTime;
        //    for (var i = 0; i < cycleEvents.Count; i++)
        //    {
        //        dummyTime = new DateTime(1900, 1, 1);
        //        if (i < cycleEvents.Count - 5
        //            && GetEventType(cycleEvents[i].EventCode) == RedToRedCycle.EventType.ChangeToGreen
        //            && GetEventType(cycleEvents[i + 1].EventCode) == RedToRedCycle.EventType.ChangeToEndMinGreen
        //            && GetEventType(cycleEvents[i + 2].EventCode) == RedToRedCycle.EventType.ChangeToYellow
        //            && GetEventType(cycleEvents[i + 3].EventCode) == RedToRedCycle.EventType.ChangeToRed
        //            && GetEventType(cycleEvents[i + 4].EventCode) == RedToRedCycle.EventType.ChangeToEndOfRedClearance
        //            && GetEventType(cycleEvents[i + 5].EventCode) == RedToRedCycle.EventType.ChangeToGreen
        //        )
        //            cycles.Add(new TimingAndActuationCycle(cycleEvents[i].Timestamp, cycleEvents[i + 1].Timestamp,
        //                cycleEvents[i + 2].Timestamp, cycleEvents[i + 3].Timestamp, cycleEvents[i + 4].Timestamp,
        //                cycleEvents[i + 5].Timestamp, dummyTime));
        //    }

        //    //// If there are no 5 part cycles, Try to get a 3 or 4 part cycle.
        //    //get 4 part series is 61, 63,64 and maybe 66
        //    if (cycles.Count != 0) return cycles;
        //    {
        //        var endRedEvent = new DateTime();
        //        dummyTime = new DateTime(1900, 1, 1);
        //        for (var i = 0; i < cycleEvents.Count; i++)
        //        {
        //            if (i < cycleEvents.Count - 5
        //                && GetEventType(cycleEvents[i].EventCode) == RedToRedCycle.EventType.ChangeToGreen
        //                && GetEventType(cycleEvents[i + 1].EventCode) == RedToRedCycle.EventType.ChangeToYellow
        //                && GetEventType(cycleEvents[i + 2].EventCode) == RedToRedCycle.EventType.ChangeToRed
        //            )
        //            {
        //                var overlapDarkTime = cycleEvents[i + 3].Timestamp;
        //                endRedEvent = cycleEvents[i + 4].Timestamp;

        //                if (GetEventType(cycleEvents[i + 3].EventCode) != RedToRedCycle.EventType.OverLapDark)
        //                {
        //                    endRedEvent = cycleEvents[i + 3].Timestamp;
        //                }

        //                cycles.Add(new TimingAndActuationCycle(cycleEvents[i].Timestamp, dummyTime,
        //                    cycleEvents[i + 1].Timestamp,
        //                    dummyTime, cycleEvents[i + 2].Timestamp, endRedEvent, overlapDarkTime));
        //            }
        //        }
        //    }
        //    return cycles;
        //}

        private RedToRedCycle.EventType GetEventType(int eventCode)
        {
            switch (eventCode)
            {
                case 1:
                    return RedToRedCycle.EventType.ChangeToGreen;
                case 3:
                    return RedToRedCycle.EventType.ChangeToEndMinGreen;
                case 61:
                    return RedToRedCycle.EventType.ChangeToGreen;
                case 8:
                    return RedToRedCycle.EventType.ChangeToYellow;
                case 63:
                    return RedToRedCycle.EventType.ChangeToYellow;
                case 9:
                    return RedToRedCycle.EventType.ChangeToRed;
                case 11:
                    return RedToRedCycle.EventType.ChangeToEndOfRedClearance;
                case 64:
                    return RedToRedCycle.EventType.ChangeToRed;
                case 66:
                    return RedToRedCycle.EventType.OverLapDark;
                default:
                    return RedToRedCycle.EventType.Unknown;
            }
        }

        /// <summary>
        /// Needs event codes 1,8,9,61,63,64
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="getPermissivePhase"></param>
        /// <param name="detector"></param>
        /// <returns></returns>
        public List<CycleSpeed> GetSpeedCycles(DateTime startDate, DateTime endDate, bool getPermissivePhase,
            Detector detector, List<ControllerEventLog> cycleEvents)
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
                switch (terminationEvent.EventCode)
                {
                    case 4:
                        terminationType = CycleSplitFail.TerminationType.GapOut;
                        break;
                    case 5:
                        terminationType = CycleSplitFail.TerminationType.MaxOut;
                        break;
                    case 6:
                        terminationType = CycleSplitFail.TerminationType.ForceOff;
                        break;
                    default:
                        terminationType = CycleSplitFail.TerminationType.Unknown;
                        break;
                }
            return terminationType;
        }


        private EventType GetYellowToRedEventType(int EventCode)
        {
            switch (EventCode)
            {
                case 8:
                    return EventType.BeginYellowClearance;
                // overlap yellow
                case 63:
                    return EventType.BeginYellowClearance;

                case 9:
                    return EventType.BeginRedClearance;
                // overlap red
                case 64:
                    return EventType.BeginRedClearance;

                case 65:
                    return EventType.BeginRed;
                case 11:
                    return EventType.BeginRed;

                case 1:
                    return EventType.EndRed;
                // overlap green
                case 61:
                    return EventType.EndRed;

                default:
                    return EventType.Unknown;
            }
        }

        public enum EventType
        {
            BeginYellowClearance,
            BeginRedClearance,
            BeginRed,
            EndRed,
            Unknown
        }

    }
}