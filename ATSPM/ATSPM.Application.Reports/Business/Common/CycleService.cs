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
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public CycleService(IControllerEventLogRepository controllerEventLogRepository)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
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
        /// <returns></returns>
        public List<RedToRedCycle> GetRedToRedCycles(Approach approach, DateTime startTime, DateTime endTime)
        {
            var cycleEventNumbers = approach.IsPermissivePhaseOverlap
                      ? new List<int> { 61, 63, 64 }
                      : new List<int> { 1, 8, 9 };
            var cycleEvents = controllerEventLogRepository.GetEventsByEventCodesParam(approach.SignalId, startTime, endTime.AddSeconds(900),
                cycleEventNumbers,
                approach.ProtectedPhaseNumber);
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
        public List<TimingAndActuationCycle> GetTimingAndActuationCycles(DateTime startDate, DateTime endDate,
            Approach approach, bool getPermissivePhase)
        {
            var cycleEvents = GetDetailedCycleEvents(getPermissivePhase, startDate, endDate, approach);
            if (cycleEvents != null && cycleEvents.Count > 0 && (GetEventType(cycleEvents.LastOrDefault().EventCode) !=
                RedToRedCycle.EventType.ChangeToRed || cycleEvents.LastOrDefault().Timestamp < endDate))
                GetEventsToCompleteCycle(getPermissivePhase, endDate, approach, cycleEvents);
            var cycles = new List<TimingAndActuationCycle>();
            DateTime dummyTime;
            for (var i = 0; i < cycleEvents.Count; i++)
            {
                dummyTime = new DateTime(1900, 1, 1);
                if (i < cycleEvents.Count - 5
                    && GetEventType(cycleEvents[i].EventCode) == RedToRedCycle.EventType.ChangeToGreen
                    && GetEventType(cycleEvents[i + 1].EventCode) == RedToRedCycle.EventType.ChangeToEndMinGreen
                    && GetEventType(cycleEvents[i + 2].EventCode) == RedToRedCycle.EventType.ChangeToYellow
                    && GetEventType(cycleEvents[i + 3].EventCode) == RedToRedCycle.EventType.ChangeToRed
                    && GetEventType(cycleEvents[i + 4].EventCode) == RedToRedCycle.EventType.ChangeToEndOfRedClearance
                    && GetEventType(cycleEvents[i + 5].EventCode) == RedToRedCycle.EventType.ChangeToGreen
                )
                    cycles.Add(new TimingAndActuationCycle(cycleEvents[i].Timestamp, cycleEvents[i + 1].Timestamp,
                        cycleEvents[i + 2].Timestamp, cycleEvents[i + 3].Timestamp, cycleEvents[i + 4].Timestamp,
                        cycleEvents[i + 5].Timestamp, dummyTime));
            }

            //// If there are no 5 part cycles, Try to get a 3 or 4 part cycle.
            //get 4 part series is 61, 63,64 and maybe 66
            if (cycles.Count != 0) return cycles;
            {
                var endRedEvent = new DateTime();
                dummyTime = new DateTime(1900, 1, 1);
                for (var i = 0; i < cycleEvents.Count; i++)
                {
                    if (i < cycleEvents.Count - 5
                        && GetEventType(cycleEvents[i].EventCode) == RedToRedCycle.EventType.ChangeToGreen
                        && GetEventType(cycleEvents[i + 1].EventCode) == RedToRedCycle.EventType.ChangeToYellow
                        && GetEventType(cycleEvents[i + 2].EventCode) == RedToRedCycle.EventType.ChangeToRed
                    )
                    {
                        var overlapDarkTime = cycleEvents[i + 3].Timestamp;
                        endRedEvent = cycleEvents[i + 4].Timestamp;

                        if (GetEventType(cycleEvents[i + 3].EventCode) != RedToRedCycle.EventType.OverLapDark)
                        {
                            endRedEvent = cycleEvents[i + 3].Timestamp;
                        }

                        cycles.Add(new TimingAndActuationCycle(cycleEvents[i].Timestamp, dummyTime,
                            cycleEvents[i + 1].Timestamp,
                            dummyTime, cycleEvents[i + 2].Timestamp, endRedEvent, overlapDarkTime));
                    }
                }
            }
            return cycles;
        }

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
            if (cycleEvents.Any() && (GetEventType(cycleEvents.Last().EventCode) !=
                RedToRedCycle.EventType.ChangeToRed || cycleEvents.LastOrDefault().Timestamp < endDate))
                GetEventsToCompleteCycle(getPermissivePhase, endDate, detector.Approach, cycleEvents);
            if (cycleEvents.Any() && (GetEventType(cycleEvents.First().EventCode) !=
                RedToRedCycle.EventType.ChangeToRed || cycleEvents.LastOrDefault().Timestamp > startDate))
                GetEventsToStartCycle(getPermissivePhase, startDate, detector.Approach, cycleEvents);
            var cycles = new List<CycleSpeed>();
            if (cycleEvents != null)
                for (var i = 0; i < cycleEvents.Count; i++)
                    if (i < cycleEvents.Count - 3
                        && GetEventType(cycleEvents[i].EventCode) == RedToRedCycle.EventType.ChangeToRed
                        && GetEventType(cycleEvents[i + 1].EventCode) == RedToRedCycle.EventType.ChangeToGreen
                        && GetEventType(cycleEvents[i + 2].EventCode) == RedToRedCycle.EventType.ChangeToYellow
                        && GetEventType(cycleEvents[i + 3].EventCode) == RedToRedCycle.EventType.ChangeToRed)
                        cycles.Add(new CycleSpeed(cycleEvents[i].Timestamp, cycleEvents[i + 1].Timestamp,
                            cycleEvents[i + 2].Timestamp, cycleEvents[i + 3].Timestamp));


            return cycles;
        }

        /// <summary>
        /// Needs event codes 1,8,9,61,63,64,66
        /// </summary>
        /// <param name="getPermissivePhase"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="approach"></param>
        /// <returns></returns>
        //private List<ControllerEventLog> GetCycleEvents(bool getPermissivePhase, DateTime startDate,
        //    DateTime endDate, Approach approach, List<ControllerEventLog> cycleEvents)
        //{
        //    List<ControllerEventLog> cycleEvents;
        //    if (getPermissivePhase)
        //    {
        //        var cycleEventNumbers = approach.IsPermissivePhaseOverlap
        //            ? new List<int> { 61, 63, 64, 66 }
        //            : new List<int> { 1, 8, 9 };
        //        cycleEvents = controllerEventLogRepository.GetEventsByEventCodesParam(approach.SignalId, startDate,
        //            endDate, cycleEventNumbers, approach.PermissivePhaseNumber.Value).ToList();
        //    }
        //    else
        //    {
        //        var cycleEventNumbers = approach.IsProtectedPhaseOverlap
        //            ? new List<int> { 61, 63, 64, 66 }
        //            : new List<int> { 1, 8, 9 };
        //        cycleEvents = controllerEventLogRepository.GetEventsByEventCodesParam(approach.SignalId, startDate,
        //            endDate, cycleEventNumbers, approach.ProtectedPhaseNumber).ToList();
        //    }

        //    return cycleEvents;
        //}

        /// <summary>
        /// Needs event codes 1,3,8,9,11,61,63,64,66
        /// </summary>
        /// <param name="getPermissivePhase"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="approach"></param>
        /// <returns></returns>
        private List<ControllerEventLog> GetDetailedCycleEvents(bool getPermissivePhase, DateTime startDate,
            DateTime endDate, Approach approach)
        {
            List<ControllerEventLog> cycleEvents;


            if (getPermissivePhase)
            {
                var cycleEventNumbers = approach.IsPermissivePhaseOverlap
                    ? new List<int> { 61, 63, 64, 66 }
                    : new List<int> { 1, 3, 8, 9, 11 };
                cycleEvents = controllerEventLogRepository.GetEventsByEventCodesParam(approach.SignalId, startDate,
                    endDate, cycleEventNumbers, approach.PermissivePhaseNumber.Value).ToList();
            }
            else
            {
                var cycleEventNumbers = approach.IsProtectedPhaseOverlap
                    ? new List<int> { 61, 63, 64, 66 }
                    : new List<int> { 1, 3, 8, 9, 11 };
                cycleEvents = controllerEventLogRepository.GetEventsByEventCodesParam(approach.SignalId, startDate,
                    endDate, cycleEventNumbers, approach.ProtectedPhaseNumber).ToList();
            }

            return cycleEvents;
        }

        /// <summary>
        /// Needs event codes 1,3,8,9,11,61,63,64,65
        /// </summary>
        /// <param name="getPermissivePhase"></param>
        /// <param name="endDate"></param>
        /// <param name="approach"></param>
        /// <param name="cycleEvents"></param>
        public void GetEventsToCompleteCycle(bool getPermissivePhase, DateTime endDate, Approach approach,
            List<ControllerEventLog> cycleEvents)
        {
            if (getPermissivePhase)
            {
                var cycleEventNumbers = approach.IsPermissivePhaseOverlap
                    ? new List<int> { 61, 63, 64, 65 }
                    : new List<int> { 1, 8, 9, 11 };
                var eventsAfterEndDate = controllerEventLogRepository.GetTopEventsAfterDateByEventCodesParam(approach.SignalId,
                    endDate, cycleEventNumbers, approach.PermissivePhaseNumber.Value, 3);
                if (eventsAfterEndDate != null)
                    cycleEvents.AddRange(eventsAfterEndDate);
            }
            else
            {
                var cycleEventNumbers = approach.IsProtectedPhaseOverlap
                    ? new List<int> { 61, 63, 64, 65 }
                    : new List<int> { 1, 8, 9, 11 };
                var eventsAfterEndDate = controllerEventLogRepository.GetTopEventsAfterDateByEventCodesParam(approach.SignalId,
                    endDate, cycleEventNumbers, approach.ProtectedPhaseNumber, 3);
                if (eventsAfterEndDate != null)
                    cycleEvents.AddRange(eventsAfterEndDate);
            }
        }

        /// <summary>
        /// Needs event codes 1,3,8,9,11,63,64,65
        /// </summary>
        /// <param name="getPermissivePhase"></param>
        /// <param name="startDate"></param>
        /// <param name="approach"></param>
        /// <param name="cycleEvents"></param>
        public void GetEventsToStartCycle(bool getPermissivePhase, DateTime startDate, Approach approach,
            List<ControllerEventLog> cycleEvents)
        {
            if (getPermissivePhase)
            {
                var cycleEventNumbers = approach.IsPermissivePhaseOverlap
                    ? new List<int> { 63, 64, 65 }
                    : new List<int> { 1, 8, 9, 11 };
                var eventsBeforeStartDate = controllerEventLogRepository.GetTopEventsBeforeDateByEventCodesParam(approach.SignalId,
                    startDate, cycleEventNumbers, approach.PermissivePhaseNumber.Value, 3);
                if (eventsBeforeStartDate != null)
                    cycleEvents.InsertRange(0, eventsBeforeStartDate.OrderBy(e => e.Timestamp));
            }
            else
            {
                var cycleEventNumbers = approach.IsProtectedPhaseOverlap
                    ? new List<int> { 63, 64, 65 }
                    : new List<int> { 1, 8, 9, 11 };
                var eventsBeforeStartDate = controllerEventLogRepository.GetTopEventsBeforeDateByEventCodesParam(approach.SignalId,
                    startDate, cycleEventNumbers, approach.ProtectedPhaseNumber, 3);
                if (eventsBeforeStartDate != null)
                    cycleEvents.InsertRange(0, eventsBeforeStartDate.OrderBy(e => e.Timestamp));
            }
        }

        /// <summary>
        /// Needs event codes 1,8,9,61,63,64,66
        /// </summary>
        /// <param name="options"></param>
        /// <param name="approach"></param>
        /// <param name="getPermissivePhase"></param>
        /// <returns></returns>
        public List<CycleSplitFail> GetSplitFailCycles(SplitFailOptions options, Approach approach,
            bool getPermissivePhase, List<ControllerEventLog> cycleEvents)
        {
            //var cycleEvents = GetCycleEvents(getPermissivePhase, options.StartDate.AddSeconds(-900), options.EndDate.AddSeconds(900), approach);
            var terminationEvents =
                GetTerminationEvents(getPermissivePhase, options.StartDate, options.EndDate, approach);
            var cycles = new List<CycleSplitFail>();
            for (var i = 0; i < cycleEvents.Count - 3; i++)
                if (GetEventType(cycleEvents[i].EventCode) == RedToRedCycle.EventType.ChangeToGreen
                    && GetEventType(cycleEvents[i + 1].EventCode) == RedToRedCycle.EventType.ChangeToYellow
                    && GetEventType(cycleEvents[i + 2].EventCode) == RedToRedCycle.EventType.ChangeToRed
                    && (GetEventType(cycleEvents[i + 3].EventCode) == RedToRedCycle.EventType.ChangeToGreen ||
                        cycleEvents[i + 3].EventCode == 66))
                {
                    var termEvent = GetTerminationEventBetweenStartAndEnd(cycleEvents[i].Timestamp,
                        cycleEvents[i + 3].Timestamp, terminationEvents);
                    cycles.Add(new CycleSplitFail(cycleEvents[i].Timestamp, cycleEvents[i + 2].Timestamp,
                        cycleEvents[i + 1].Timestamp, cycleEvents[i + 3].Timestamp, termEvent,
                        options.FirstSecondsOfRed));
                    //i = i + 2;
                }

            return cycles.Where(c =>
                c.EndTime >= options.StartDate && c.EndTime <= options.EndDate || c.StartTime <= options.EndDate && c.StartTime >= options.StartDate).ToList();
        }

        private CycleSplitFail.TerminationType GetTerminationEventBetweenStartAndEnd(DateTime start,
            DateTime end, List<ControllerEventLog> terminationEvents)
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

        /// <summary>
        /// Needs event codes 4,5,6
        /// </summary>
        /// <param name="getPermissivePhase"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="approach"></param>
        /// <returns></returns>
        private List<ControllerEventLog> GetTerminationEvents(bool getPermissivePhase, DateTime startDate,
            DateTime endDate,
            Approach approach)
        {
            var cycleEvents = controllerEventLogRepository.GetEventsByEventCodesParam(approach.SignalId, startDate,
                endDate, new List<int> { 4, 5, 6 },
                getPermissivePhase ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber).ToList();
            return cycleEvents;
        }

        public List<RLMCycle> GetYellowToRedCycles(DateTime startDate, DateTime endDate, string signalId,
            int phaseNumber)
        {
            return new List<RLMCycle>();
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

        public enum NextEventResponse
        {
            GroupOK,
            GroupMissingData,
            GroupComplete
        }

    }
}