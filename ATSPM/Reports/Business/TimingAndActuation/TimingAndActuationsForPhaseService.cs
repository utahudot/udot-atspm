using ATSPM.Application.Extensions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Reports.Business.Common;
using Reports.Business.TimingAndActuation;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.TimingAndActuation
{
    public class TimingAndActuationsForPhaseService
    {

        public TimingAndActuationsForPhaseResult GetChartData(
            TimingAndActuationsOptions options,
            PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs,
            bool usePermissivePhase
            )
        {
            var stopBarEvents = new List<DetectorEventDto>();
            var pedestrianEvents = new List<DetectorEventDto>();
            var laneByLanes = new List<DetectorEventDto>();
            var advancePresenceEvents = new List<DetectorEventDto>();
            var advanceCountEvents = new List<DetectorEventDto>();
            var phaseCustomEvents = new Dictionary<string, List<DataPointForInt>>();
            var pedestrianIntervals = new List<CycleEventsDto>();


            if (options.ShowStopBarPresence)
            {
                stopBarEvents = GetDetectionEventsNew(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.SBP);
            }
            if (options.ShowPedestrianActuation && !usePermissivePhase)
            {
                pedestrianEvents = GetPedestrianEventsNew(phaseDetail.Approach, options, controllerEventLogs );
            }
            if (options.ShowPedestrianIntervals && !usePermissivePhase)
            {
                pedestrianIntervals = GetPedestrianIntervals(phaseDetail.Approach, controllerEventLogs);
            }
            if (options.ShowLaneByLaneCount)
            {
                laneByLanes = GetDetectionEventsNew(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.LLC);
            }
            if (options.ShowAdvancedDilemmaZone)
            {
                advancePresenceEvents = GetDetectionEventsNew(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.AP);
            }
            if (options.ShowAdvancedCount)
            {
                advanceCountEvents = GetDetectionEventsNew(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.AC);
            }
            if (options.PhaseEventCodesList != null)
            {
                phaseCustomEvents = GetPhaseCustomEvents(phaseDetail.Approach.Signal.SignalIdentifier, phaseDetail.PhaseNumber, options, controllerEventLogs);
            }
            var cycleAllEvents = GetCycleEvents(phaseDetail, controllerEventLogs);
            var phaseNumberSort = GetPhaseSort(phaseDetail);
            var timingAndActuationsForPhaseData = new TimingAndActuationsForPhaseResult(
                phaseDetail.Approach.Id,
                phaseDetail.Approach.Signal.SignalIdentifier,
                options.Start,
                options.End,
                phaseDetail.PhaseNumber,
                phaseDetail.UseOverlap,
                phaseNumberSort,
                usePermissivePhase,
                pedestrianIntervals,
                pedestrianEvents,
                cycleAllEvents,
                advanceCountEvents,
                advancePresenceEvents,
                stopBarEvents,
                laneByLanes,
                phaseCustomEvents
                );
            return timingAndActuationsForPhaseData;
        }

        private string GetPhaseSort(PhaseDetail phaseDetail)
        {
            return phaseDetail.IsPermissivePhase ?  // Check if the 'GetPermissivePhase' property of 'options' is true
                phaseDetail.Approach.IsPermissivePhaseOverlap ?  // If true, check if the 'IsPermissivePhaseOverlap' property of 'approach' is true
                    "zOverlap - " + phaseDetail.Approach.PermissivePhaseNumber.Value.ToString("D2")  // If true, concatenate "zOverlap - " with 'PermissivePhaseNumber' formatted as a two-digit string
                    : "Phase - " + phaseDetail.Approach.PermissivePhaseNumber.Value.ToString("D2")  // If false, concatenate "Phase - " with 'PermissivePhaseNumber' formatted as a two-digit string
                :  // If 'GetPermissivePhase' is false
                phaseDetail.Approach.IsProtectedPhaseOverlap ?  // Check if the 'IsProtectedPhaseOverlap' property of 'approach' is true
                    "zOverlap - " + phaseDetail.Approach.ProtectedPhaseNumber.ToString("D2")  // If true, concatenate "zOverlap - " with 'ProtectedPhaseNumber' formatted as a two-digit string
                    : "Phase = " + phaseDetail.Approach.ProtectedPhaseNumber.ToString("D2");  // If false, concatenate "Phase = " with 'ProtectedPhaseNumber' formatted as a two-digit string
        }

        public List<CycleEventsDto> GetCycleEvents(
            PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs)
        {

            List<int> cycleEventCodes = GetCycleCodes(phaseDetail.UseOverlap);
            var overlapLabel = phaseDetail.UseOverlap == true ? "Overlap" : "";
            string keyLabel = $"Cycles Intervals {phaseDetail.PhaseNumber} {overlapLabel}";
            var events = new List<CycleEventsDto>();
            if (controllerEventLogs.Any())
            {
                events = controllerEventLogs.Where(c => cycleEventCodes.Contains(c.EventCode)).Select(e => new CycleEventsDto(e.Timestamp, e.EventCode)).ToList();
            }
            return events;
        }

        public List<int> GetCycleCodes(bool getOverlapCodes)
        {
            var phaseEventCodesForCycles = new List<int> { 1, 3, 8, 9, 11 };
            if (getOverlapCodes)
            {
                phaseEventCodesForCycles = new List<int> { 61, 62, 63, 64, 65 };
            }

            return phaseEventCodesForCycles;
        }


        public Dictionary<string, List<DataPointForInt>> GetPhaseCustomEvents(
            string signalIdentifier,
            int phaseNumber,
            TimingAndActuationsOptions options,
            List<ControllerEventLog> controllerEventLogs)
        {
            var phaseCustomEvents = new Dictionary<string, List<DataPointForInt>>();
            if (options.PhaseEventCodesList != null && options.PhaseEventCodesList.Any())
            {
                foreach (var phaseEventCode in options.PhaseEventCodesList)
                {

                    var phaseEvents = controllerEventLogs.Where(c => c.EventCode == phaseEventCode).ToList();
                    if (phaseEvents.Count > 0)
                    {
                        phaseCustomEvents.Add(
                            "Phase Events: " + phaseEventCode, phaseEvents.Select(s => new DataPointForInt(s.Timestamp, s.EventCode)).ToList());
                    }

                    if (phaseCustomEvents.Count == 0 && options.ShowAllLanesInfo)
                    {
                        var forceEventsForAllLanes = new List<ControllerEventLog>();
                        var tempEvent1 = new ControllerEventLog()
                        {
                            SignalIdentifier = signalIdentifier,
                            EventCode = phaseEventCode,
                            EventParam = phaseNumber,
                            Timestamp = options.Start.AddSeconds(-10)
                        };
                        forceEventsForAllLanes.Add(tempEvent1);
                        var tempEvent2 = new ControllerEventLog()
                        {
                            SignalIdentifier = signalIdentifier,
                            EventCode = phaseEventCode,
                            EventParam = phaseNumber,
                            Timestamp = options.Start.AddSeconds(-9)
                        };
                        forceEventsForAllLanes.Add(tempEvent2);
                        phaseCustomEvents.Add(
                            "Phase Events: " + phaseEventCode, forceEventsForAllLanes.Select(s => new DataPointForInt(s.Timestamp, s.EventCode)).ToList());
                    }
                }
            }
            return phaseCustomEvents;
        }


        public Dictionary<string, List<DataPointForInt>> GetDetectionEvents(
           Approach approach,
           TimingAndActuationsOptions options,
           List<ControllerEventLog> controllerEventLogs,
           DetectionTypes detectionType
           )
        {
            var stopBarEvents = new Dictionary<string, List<DataPointForInt>>();
            var localSortedDetectors = approach.Detectors.Where(d => d.DetectionTypes.Any(d => d.Id == detectionType))
                .OrderByDescending(d => d.MovementType.DisplayOrder)
                .ThenByDescending(l => l.LaneNumber).ToList();
            var detectorActivationCodes = new List<int> { 81, 82 };
            foreach (var detector in localSortedDetectors)
            {
                if (detector.DetectionTypes.Any(d => d.Id == detectionType))
                {
                    var extendStartStopLine = options.ExtendStartStopSearch * 60.0;
                    var stopEvents = controllerEventLogs.Where(c => detectorActivationCodes.Contains(c.EventCode) && c.EventParam == detector.DetChannel).ToList();
                    var laneNumber = "";
                    if (detector.LaneNumber != null)
                    {
                        laneNumber = detector.LaneNumber.Value.ToString();
                    }

                    if (stopEvents.Count > 0)
                    {
                        var distanceFromStopBarLable = detector.DistanceFromStopBar.HasValue ? $"({detector.DistanceFromStopBar} ft)" : "";
                        stopBarEvents.Add($"{detectionType.GetDisplayName()} {distanceFromStopBarLable}, {detector.MovementType.Abbreviation} {laneNumber}, ch {detector.DetChannel}",
                                            stopEvents.Select(s => new DataPointForInt(s.Timestamp, s.EventCode)).ToList());
                    }

                    if (stopEvents.Count == 0 && options.ShowAllLanesInfo)
                    {
                        var forceEventsForAllLanes = new List<ControllerEventLog>();
                        var event1 = new ControllerEventLog()
                        {
                            SignalIdentifier = approach.Signal.SignalIdentifier,
                            EventCode = 82,
                            EventParam = detector.DetChannel,
                            Timestamp = options.Start.AddSeconds(-10)
                        };
                        forceEventsForAllLanes.Add(event1);
                        var event2 = new ControllerEventLog()
                        {
                            SignalIdentifier = approach.Signal.SignalIdentifier,
                            EventParam = detector.DetChannel,
                            EventCode = 81,
                            Timestamp = options.Start.AddSeconds(-9)
                        };
                        forceEventsForAllLanes.Add(event2);
                        var labelName = detectionType.GetDisplayName() + ", " + detector.MovementType.Abbreviation + " " +
                                         laneNumber + ", ch " + detector.DetChannel + " ";
                        stopBarEvents.Add(labelName, forceEventsForAllLanes.Select(s => new DataPointForInt(s.Timestamp, s.EventCode)).ToList());
                    }
                }
            }
            return stopBarEvents;
        }

        public List<DetectorEventDto> GetDetectionEventsNew(
            Approach approach,
            TimingAndActuationsOptions options,
            List<ControllerEventLog> controllerEventLogs,
            DetectionTypes detectionType
            )
        {
            var stopBarEvents = new List<DetectorEventDto>();
            var localSortedDetectors = approach.Detectors.Where(d => d.DetectionTypes.Any(d => d.Id == detectionType))
                .OrderByDescending(d => d.MovementType.DisplayOrder)
                .ThenByDescending(l => l.LaneNumber).ToList();
            var detectorActivationCodes = new List<int> { 81, 82 };
            foreach (var detector in localSortedDetectors)
            {
                if (detector.DetectionTypes.Any(d => d.Id == detectionType))
                {
                    var extendStartStopLine = options.ExtendStartStopSearch * 60.0;
                    var stopEvents = controllerEventLogs.Where(c => detectorActivationCodes.Contains(c.EventCode) && c.EventParam == detector.DetChannel).ToList();
                    var laneNumber = "";
                    if (detector.LaneNumber != null)
                    {
                        laneNumber = detector.LaneNumber.Value.ToString();
                    }
                    var distanceFromStopBarLable = detector.DistanceFromStopBar.HasValue ? $"({detector.DistanceFromStopBar} ft)" : "";
                    var lableName = $"{detectionType.GetDisplayName()} {distanceFromStopBarLable}, {detector.MovementType.Abbreviation} {laneNumber}, ch {detector.DetChannel}";

                    if (stopEvents.Count > 0)
                    {
                        var detectorEvents = new List<DetectorEventBase>();
                        for ( var i = 0; i < stopEvents.Count; i+= 2)
                        {
                            if(i+1 == stopEvents.Count)
                            {
                                detectorEvents.Add(new DetectorEventBase(stopEvents[i].Timestamp, stopEvents[i].Timestamp));
                            }else
                            {
                                detectorEvents.Add(new DetectorEventBase(stopEvents[i].Timestamp, stopEvents[i + 1].Timestamp));
                            }
                        }
                        
                        
                        stopBarEvents.Add(new DetectorEventDto(lableName, detectorEvents));
                        //stopBarEvents.Add(stopEvents.Select(s => new DataPointForInt(s.Timestamp, s.EventCode)).ToList());
                        // name , movementType, lane number, channel
                    }

                    if (stopEvents.Count == 0 && options.ShowAllLanesInfo)
                    {
                        //var forceEventsForAllLanes = new List<ControllerEventLog>();
                        //var event1 = new ControllerEventLog()
                        //{
                        //    SignalIdentifier = approach.Signal.SignalIdentifier,
                        //    EventCode = 82,
                        //    EventParam = detector.DetChannel,
                        //    Timestamp = options.Start.AddSeconds(-10)
                        //};
                        //forceEventsForAllLanes.Add(event1);
                        //var event2 = new ControllerEventLog()
                        //{
                        //    SignalIdentifier = approach.Signal.SignalIdentifier,
                        //    EventParam = detector.DetChannel,
                        //    EventCode = 81,
                        //    Timestamp = options.Start.AddSeconds(-9)
                        //};
                        //forceEventsForAllLanes.Add(event2);
                        var e = new DetectorEventBase(options.Start.AddSeconds(-10), options.Start.AddSeconds(-9));

                        var list = new List<DetectorEventBase>
                        {
                            e
                        };
                        stopBarEvents.Add(new DetectorEventDto(lableName, list));
                    }
                }
            }
            return stopBarEvents;
        }

        public List<DetectorEventDto> GetPedestrianEventsNew(
            Approach approach,
            TimingAndActuationsOptions options,
            List<ControllerEventLog> controllerEventLogs)
        {
            var pedestrianEvents = new List<DetectorEventDto>();
            if (string.IsNullOrEmpty(approach.PedestrianDetectors) && (approach.Signal.Pedsare1to1 && approach.IsProtectedPhaseOverlap)
                || (!approach.Signal.Pedsare1to1 && approach.PedestrianPhaseNumber.HasValue))
                return pedestrianEvents;
            var pedEventCodes = new List<int> { 89, 90 };
            //var pedDetectors = approach.GetPedDetectorsFromApproach();
            foreach (var pedDetector in approach.Detectors)
            {
                // name, phase number, 
                var lableName = $"Ped Det. Actuations, ph {approach.ProtectedPhaseNumber}, ch {pedDetector.DetChannel}";
                var pedEvents = controllerEventLogs.Where(c => pedEventCodes.Contains(c.EventCode) && c.EventParam == pedDetector.DetChannel).ToList();
                if (pedEvents.Count > 0)
                {
                    var detectorEvents = new List<DetectorEventBase>();
                    for (var i = 0; i < pedEvents.Count; i += 2)
                    {
                        if (i + 1 == pedEvents.Count)
                        {
                            detectorEvents.Add(new DetectorEventBase(pedEvents[i].Timestamp, pedEvents[i].Timestamp));
                        }
                        else
                        {
                            detectorEvents.Add(new DetectorEventBase(pedEvents[i].Timestamp, pedEvents[i + 1].Timestamp));
                        }
                    }


                    pedestrianEvents.Add(new DetectorEventDto(lableName, detectorEvents));
                    //stopBarEvents.Add(stopEvents.Select(s => new DataPointForInt(s.Timestamp, s.EventCode)).ToList());
                    // name , movementType, lane number, channel
                }
                //pedestrianEvents.Add(pedDetector.ToString(), .Select(s => new DataPointForInt(s.Timestamp, s.EventCode)).ToList());
            }
            return pedestrianEvents;
        }

        public Dictionary<string, List<DataPointForInt>> GetPedestrianEvents(
            Approach approach,
            List<ControllerEventLog> controllerEventLogs)
        {
            var pedestrianEvents = new Dictionary<string, List<DataPointForInt>>();
            if (string.IsNullOrEmpty(approach.PedestrianDetectors) && (approach.Signal.Pedsare1to1 && approach.IsProtectedPhaseOverlap)
                || (!approach.Signal.Pedsare1to1 && approach.PedestrianPhaseNumber.HasValue))
                return pedestrianEvents;
            var pedEventCodes = new List<int> { 89, 90 };
            var pedDetectors = approach.GetPedDetectorsFromApproach();
            foreach (var pedDetector in pedDetectors)
            {
                pedestrianEvents.Add(pedDetector.ToString(), controllerEventLogs.Where(c => pedEventCodes.Contains(c.EventCode) && c.EventParam == pedDetector).ToList().Select(s => new DataPointForInt(s.Timestamp, s.EventCode)).ToList());
            }
            return pedestrianEvents;
        }

        public List<CycleEventsDto> GetPedestrianIntervals(
            Approach approach,
            List<ControllerEventLog> controllerEventLogs)
        {
            List<int> overlapCodes = GetPedestrianIntervalEventCodes(approach.IsPedestrianPhaseOverlap);
            var pedPhase = approach.PedestrianPhaseNumber ?? approach.ProtectedPhaseNumber;
            return controllerEventLogs.Where(c => overlapCodes.Contains(c.EventCode) && c.EventParam == pedPhase).Select(s => new CycleEventsDto(s.Timestamp, s.EventCode)).ToList();
        }

        public List<int> GetPedestrianIntervalEventCodes(bool isPhaseOrOverlap)
        {
            var overlapCodes = new List<int> { 21, 22, 23 };
            if (isPhaseOrOverlap)
            {
                overlapCodes = new List<int> { 67, 68, 69 };
            }

            return overlapCodes;
        }
    }
}

