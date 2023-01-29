using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Legacy.Common.Business
{
    public class RedLLightMonitorService
    {
        private int _detChannel;
        private bool _showVolume;
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public RedLLightMonitorService(IControllerEventLogRepository controllerEventLogRepository)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
        }


        public RLMSignalPhaseData GetRedLLightMonitorSignalPhaseData(DateTime startDate, DateTime endDate, int binSize, double severeRedLightViolationsSeconds,
            Approach approach, bool usePermissivePhase)
        {
            var rlmSignalPhase = new RLMSignalPhaseData();
            rlmSignalPhase.SevereRedLightViolationSeconds = severeRedLightViolationsSeconds;
            rlmSignalPhase.Approach = approach;
            rlmSignalPhase.GetPermissivePhase = usePermissivePhase;
                if (usePermissivePhase)
                {
                    if (rlmSignalPhase.Approach.IsPermissivePhaseOverlap)
                    {
                        GetSignalOverlapData(startDate, endDate, _showVolume, binSize, usePermissivePhase,rlmSignalPhase);
                    }
                    else
                    {
                        GetSignalPhaseData(startDate, endDate, usePermissivePhase, rlmSignalPhase);
                    }
                }
                else
                {
                    if (rlmSignalPhase.Approach.IsProtectedPhaseOverlap)
                    {
                        GetSignalOverlapData(startDate, endDate, _showVolume, binSize, usePermissivePhase, rlmSignalPhase);
                    }
                    else
                    {
                        GetSignalPhaseData(startDate, endDate, usePermissivePhase, rlmSignalPhase);
                    }
                }
            return rlmSignalPhase;
        }



        private void GetSignalPhaseData(DateTime startDate, DateTime endDate, bool usePermissivePhase, RLMSignalPhaseData rlmSignalPhase)
        {
            if (usePermissivePhase)
                rlmSignalPhase.PhaseNumber = rlmSignalPhase.Approach.PermissivePhaseNumber ?? 0;
            else
                rlmSignalPhase.PhaseNumber = rlmSignalPhase.Approach.ProtectedPhaseNumber;
            List<int> li = new List<int> { 1, 8, 9, 10, 11 };
            var cycleEvents = controllerEventLogRepository.GetEventsByEventCodesParam(rlmSignalPhase.Approach.SignalId,
                startDate.AddSeconds(-900), endDate.AddSeconds(900), li, rlmSignalPhase.PhaseNumber).ToList();
            GetRedCycle(startDate, endDate, cycleEvents, rlmSignalPhase);
            rlmSignalPhase.Plans = GetPlanCollection(
                rlmSignalPhase.Cycles.Any() ? rlmSignalPhase.Cycles.First().StartTime : startDate,
                rlmSignalPhase.Cycles.Any() ? rlmSignalPhase.Cycles.Last().EndTime : endDate,
                rlmSignalPhase.Cycles,
                rlmSignalPhase.Approach,
                rlmSignalPhase.SevereRedLightViolationSeconds);
            if (rlmSignalPhase.Plans.PlanList.Count == 0)
                rlmSignalPhase.Plans.PlanList.Add(new RLMPlan(
                    rlmSignalPhase.Cycles.Any() ? rlmSignalPhase.Cycles.First().StartTime : startDate,
                    rlmSignalPhase.Cycles.Any() ? rlmSignalPhase.Cycles.Last().EndTime : endDate,
                    0,
                    rlmSignalPhase.Cycles,
                    rlmSignalPhase.SevereRedLightViolationSeconds,
                    rlmSignalPhase.Approach));

        }


        private void GetSignalOverlapData(DateTime startDate, DateTime endDate, bool showVolume, int binSize, bool usePermissive, RLMSignalPhaseData rlmSignalPhase)
        {
            var li = new List<int> { 62, 63, 64 };
            var cycleEvents = controllerEventLogRepository.GetEventsByEventCodesParam(rlmSignalPhase.Approach.SignalId,
                startDate.AddSeconds(-900), endDate.AddSeconds(900), li, rlmSignalPhase.Approach.ProtectedPhaseNumber).ToList();
            
            GetRedCycle(startDate, endDate, cycleEvents, rlmSignalPhase);
            rlmSignalPhase.Plans = GetPlanCollection(startDate, endDate, rlmSignalPhase.Cycles, rlmSignalPhase.Approach, rlmSignalPhase.SevereRedLightViolationSeconds);
            if (rlmSignalPhase.Plans.PlanList.Count == 0)
                rlmSignalPhase.Plans.PlanList.Add(new RLMPlan(startDate, endDate, 0, rlmSignalPhase.Cycles, rlmSignalPhase.SevereRedLightViolationSeconds,
                    rlmSignalPhase.Approach));
        }
        
        public List<ControllerEventLog> GetEventsToCompleteCycle(bool getPermissivePhase, DateTime endDate, Approach approach)
        {
            if (getPermissivePhase)
            {
                var cycleEventNumbers = approach.IsPermissivePhaseOverlap
                    ? new List<int> { 61, 63, 64, 65 }
                    : new List<int> { 1, 8, 9 };
                return controllerEventLogRepository.GetTopEventsAfterDateByEventCodesParam(approach.SignalId,
                    endDate, cycleEventNumbers, approach.PermissivePhaseNumber.Value, 3).OrderByDescending(e => e.Timestamp).ToList();
            }
            else
            {
                var cycleEventNumbers = approach.IsProtectedPhaseOverlap
                    ? new List<int> { 61, 63, 64, 65 }
                    : new List<int> { 1, 8, 9 };
                return controllerEventLogRepository.GetTopEventsAfterDateByEventCodesParam(approach.SignalId,
                    endDate, cycleEventNumbers, approach.ProtectedPhaseNumber, 3).OrderByDescending(e => e.Timestamp).ToList();
            }
        }

        public List<ControllerEventLog> GetEventsToStartCycle(bool getPermissivePhase, DateTime startDate, Approach approach)
        {
            if (getPermissivePhase)
            {
                var cycleEventNumbers = approach.IsPermissivePhaseOverlap
                    ? new List<int> { 63, 64, 65 }
                    : new List<int> { 8, 9, 11 };
                return controllerEventLogRepository.GetTopEventsBeforeDateByEventCodesParam(approach.SignalId,
                    startDate, cycleEventNumbers, approach.PermissivePhaseNumber.Value, 3).ToList();
            }
            else
            {
                var cycleEventNumbers = approach.IsProtectedPhaseOverlap
                    ? new List<int> { 63, 64, 65 }
                    : new List<int> { 8, 9, 11 };
                return controllerEventLogRepository.GetTopEventsBeforeDateByEventCodesParam(approach.SignalId,
                    startDate, cycleEventNumbers, approach.ProtectedPhaseNumber, 3).ToList();
            }
        }



        private void GetRedCycle(
            DateTime startTime,
            DateTime endTime,
            List<ControllerEventLog> cycleEvents,
            RLMSignalPhaseData rlmSignalPhase)
        {
            if (rlmSignalPhase.Cycles == null)
                rlmSignalPhase.Cycles = new List<RLMCycle>();
            RLMCycle cycle = null;
            //use a counter to help determine when we are on the last row
            var counter = 0;

            foreach (var row in cycleEvents)
            {
                //use a counter to help determine when we are on the last row
                counter++;
                //if (row.Timestamp >= startTime && row.Timestamp <= endTime)
                if (cycle == null && GetEventType(row.EventCode) == RLMCycle.EventType.BeginYellowClearance)
                {
                    cycle = new RLMCycle(row.Timestamp, rlmSignalPhase.SevereRedLightViolationSeconds);
                    cycle.NextEvent(GetEventType(row.EventCode), row.Timestamp);
                    if (cycle.Status == RLMCycle.NextEventResponse.GroupMissingData)
                        cycle = null;
                }
                else if (cycle != null)
                {
                    cycle.NextEvent(GetEventType(row.EventCode), row.Timestamp);
                    if (cycle.Status == RLMCycle.NextEventResponse.GroupComplete) //&&((cycle.StartTime <= endTime && cycle.StartTime >= startTime)|| (cycle.EndTime >= startTime && cycle.EndTime <= endTime)))
                    {
                        rlmSignalPhase.Cycles.Add(cycle);
                        cycle = null;
                    }
                    else if (cycle.Status == RLMCycle.NextEventResponse.GroupMissingData)
                    {
                        cycle = null;
                    }
                }
            }
            rlmSignalPhase.Cycles = rlmSignalPhase.Cycles.Where(c => (c.EndTime >= startTime && c.EndTime <= endTime) || (c.StartTime <= endTime && c.StartTime >= startTime)).ToList();
            AddDetectorData(startTime, endTime, rlmSignalPhase);
        }

        private RLMCycle.EventType GetEventType(int EventCode)
        {
            switch (EventCode)
            {
                case 8:
                    return RLMCycle.EventType.BeginYellowClearance;
                // overlap yellow
                case 63:
                    return RLMCycle.EventType.BeginYellowClearance;

                case 9:
                    return RLMCycle.EventType.BeginRedClearance;
                // overlap red
                case 64:
                    return RLMCycle.EventType.BeginRedClearance;

                case 65:
                    return RLMCycle.EventType.BeginRed;
                case 11:
                    return RLMCycle.EventType.BeginRed;

                case 1:
                    return RLMCycle.EventType.EndRed;
                // overlap green
                case 61:
                    return RLMCycle.EventType.EndRed;

                default:
                    return RLMCycle.EventType.Unknown;
            }
        }


        private void AddDetectorData(DateTime startTime, DateTime endTime, RLMSignalPhaseData rlmSignalPhase)
        {
            var detectors = rlmSignalPhase.Approach.GetDetectorsForMetricType(11);
            var detectorActivations = new List<ControllerEventLog>();
            foreach (var d in detectors)
                detectorActivations.AddRange(controllerEventLogRepository.GetEventsByEventCodesParam(rlmSignalPhase.Approach.SignalId,
                    startTime, endTime,
                    new List<int> { 82 }, d.DetChannel, 0, d.LatencyCorrection));
            rlmSignalPhase.TotalVolume = detectorActivations.Count;
            foreach (var cycle in rlmSignalPhase.Cycles)
            {
                var events =
                    detectorActivations.Where(d => d.Timestamp >= cycle.StartTime && d.Timestamp < cycle.EndTime);
                foreach (var cve in events)
                {
                    var ddp = new RLMDetectorDataPoint(cycle.StartTime, cve.Timestamp);
                    cycle.AddDetector(ddp);
                }
            }
        }

        public RedLightMonitorPlanCollectionData GetPlanCollection(
            DateTime startDate,
            DateTime endDate,
            List<RLMCycle> cycles,
            Approach approach,
            double severeRedLightViolationSeconds)
        {
            var planCollection = new RedLightMonitorPlanCollectionData();
            var ds =controllerEventLogRepository.GetSignalEventsByEventCodes(approach.SignalId, startDate, endDate, new List<int> { 131 }).ToList();
            var row = new ControllerEventLog();
            row.Timestamp = startDate;
            row.SignalId = approach.SignalId;
            row.EventCode = 131;
            try
            {
                row.EventParam = GetPreviousPlan(approach.SignalId, startDate);

                ds.Insert(0, row);
            }
            catch
            {
                row.EventParam = 0;
                ds.Insert(0, row);
            }
            // remove duplicate plan entries
            MergeEvents(ds,ds,startDate, endDate);
            for (var i = 0; i < ds.Count(); i++)
                //if this is the last plan then we want the end of the plan
                //to cooincide with the end of the graph
                if (ds.Count() - 1 == i)
                {
                    if (ds[i].Timestamp != endDate)
                    {
                        var plan = new RLMPlan(ds[i].Timestamp, endDate, ds[i].EventParam,
                            cycles, severeRedLightViolationSeconds, approach);
                        planCollection.PlanList.Add(plan);
                    }
                }
                //else we add the plan with the next plans' time stamp as the end of the plan
                else
                {
                    if (ds[i].Timestamp != ds[i + 1].Timestamp)
                    {
                        var plan = new RLMPlan(ds[i].Timestamp,
                            ds[i + 1].Timestamp, ds[i].EventParam, cycles, severeRedLightViolationSeconds, approach);
                        planCollection.PlanList.Add(plan);
                    }
                }
            return planCollection;
        }

        public int GetPreviousPlan(string signalID, DateTime startDate)
        {
            var endDate = startDate.AddHours(-12);
            var planRecord = controllerEventLogRepository.GetSignalEventsByEventCodes(signalID, startDate, endDate, new List<int> { 131 });
            if (planRecord.Count() > 0)
                return planRecord.OrderByDescending(s => s.Timestamp).FirstOrDefault().EventParam;
            return 0;
        }

        public void MergeEvents(
            List<ControllerEventLog> eventLogs,
            List<ControllerEventLog> newEvents,
            DateTime startTime,
            DateTime endTime)
        {
            var eventCodes = eventLogs.Select(n => n.EventCode).Distinct().ToList();
            var incomingEventCodes = newEvents.Select(n => n.EventCode).Distinct().ToList();

            foreach (var i in incomingEventCodes)
                if (!eventCodes.Contains(i))
                    eventCodes.Add(i);

            eventLogs.AddRange(newEvents);
            eventLogs = eventLogs.Distinct().ToList();
            eventLogs.Sort((x, y) => DateTime.Compare(x.Timestamp, y.Timestamp));
        }
    }
}