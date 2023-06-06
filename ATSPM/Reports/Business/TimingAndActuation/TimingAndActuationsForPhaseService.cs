using ATSPM.Application.Extensions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Business.TimingAndActuation
{
    public class TimingAndActuationsForPhaseService
    {

        public TimingAndActuationsForPhaseResult GetChartData(
            TimingAndActuationsOptions options,
            Approach approach,
            List<ControllerEventLog> controllerEventLogs
            )
        {
            var stopBarEvents = new Dictionary<string, List<ControllerEventLog>>();
            var pedestrianEvents = new Dictionary<string, List<ControllerEventLog>>();
            var laneByLanes = new Dictionary<string, List<ControllerEventLog>>();
            var advancePresenceEvents = new Dictionary<string, List<ControllerEventLog>>();
            var advanceCountEvents = new Dictionary<string, List<ControllerEventLog>>();
            var phaseCustomEvents = new Dictionary<string, List<ControllerEventLog>>();
            var pedestrianIntervals = new List<ControllerEventLog>();


            if (options.ShowStopBarPresence)
            {
                stopBarEvents = GetDetectionEvents(approach, options, controllerEventLogs, DetectionTypes.SBP);
            }
            if (options.ShowPedestrianActuation && !options.GetPermissivePhase)
            {
                pedestrianEvents = GetPedestrianEvents(approach, controllerEventLogs);
            }
            if (options.ShowPedestrianIntervals && !options.GetPermissivePhase)
            {
                pedestrianIntervals = GetPedestrianIntervals(approach, controllerEventLogs);
            }
            if (options.ShowLaneByLaneCount)
            {
                laneByLanes = GetDetectionEvents(approach, options, controllerEventLogs, DetectionTypes.LLC);
            }
            if (options.ShowAdvancedDilemmaZone)
            {
                advancePresenceEvents = GetDetectionEvents(approach, options, controllerEventLogs, DetectionTypes.AP);
            }
            if (options.ShowAdvancedCount)
            {
                advanceCountEvents = GetDetectionEvents(approach, options, controllerEventLogs, DetectionTypes.AC);
            }
            if (options.PhaseEventCodesList != null)
            {
                phaseCustomEvents = GetPhaseCustomEvents(approach, options, controllerEventLogs);
            }
            var cycleAllEvents = GetCycleEvents(options, approach, controllerEventLogs);
            var phaseNumberSort = GetPhaseSort(options, approach);
            var timingAndActuationsForPhaseData = new TimingAndActuationsForPhaseResult(
                options.ApproachId,
                approach.Signal.SignalId,
                options.Start,
                options.End,
                options.PhaseNumber,
                options.PhaseOrOverlap,
                phaseNumberSort,
                options.GetPermissivePhase,
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

        private string GetPhaseSort(TimingAndActuationsOptions options, Approach approach)
        {
            return options.GetPermissivePhase?  // Check if the 'GetPermissivePhase' property of 'options' is true
                approach.IsPermissivePhaseOverlap ?  // If true, check if the 'IsPermissivePhaseOverlap' property of 'approach' is true
                    "zOverlap - " + approach.PermissivePhaseNumber.Value.ToString("D2")  // If true, concatenate "zOverlap - " with 'PermissivePhaseNumber' formatted as a two-digit string
                    : "Phase - " + approach.PermissivePhaseNumber.Value.ToString("D2")  // If false, concatenate "Phase - " with 'PermissivePhaseNumber' formatted as a two-digit string
                :  // If 'GetPermissivePhase' is false
                approach.IsProtectedPhaseOverlap ?  // Check if the 'IsProtectedPhaseOverlap' property of 'approach' is true
                    "zOverlap - " + approach.ProtectedPhaseNumber.ToString("D2")  // If true, concatenate "zOverlap - " with 'ProtectedPhaseNumber' formatted as a two-digit string
                    : "Phase = " + approach.ProtectedPhaseNumber.ToString("D2");  // If false, concatenate "Phase = " with 'ProtectedPhaseNumber' formatted as a two-digit string
        }

        public Dictionary<string, List<ControllerEventLog>> GetCycleEvents(
            TimingAndActuationsOptions options,
            Approach approach,
            List<ControllerEventLog> controllerEventLogs)
        {
            var getOverlapCodes = (options.GetPermissivePhase && approach.IsPermissivePhaseOverlap) ||
                (!options.GetPermissivePhase && approach.IsPermissivePhaseOverlap);

            List<int> cycleEventCodes = GetCycleCodes(getOverlapCodes);
            var overlapLabel = getOverlapCodes == true ? "Overlap" : "";
            string keyLabel = "Cycles Intervals " + options.PhaseNumber + " " + overlapLabel;
            var events = new Dictionary<string, List<ControllerEventLog>>();
            if (controllerEventLogs.Any())
            {
                events.Add(keyLabel, controllerEventLogs.Where(c => cycleEventCodes.Contains(c.EventCode)).ToList());
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


        public Dictionary<string, List<ControllerEventLog>> GetPhaseCustomEvents(
            Approach approach,
            TimingAndActuationsOptions options,
            List<ControllerEventLog> controllerEventLogs)
        {
            var phaseCustomEvents = new Dictionary<string, List<ControllerEventLog>>();
            if (options.PhaseEventCodesList != null && options.PhaseEventCodesList.Any())
            {
                foreach (var phaseEventCode in options.PhaseEventCodesList)
                {

                    var phaseEvents = controllerEventLogs.Where(c => c.EventCode == phaseEventCode).ToList();
                    if (phaseEvents.Count > 0)
                    {
                        phaseCustomEvents.Add(
                            "Phase Events: " + phaseEventCode, phaseEvents);
                    }

                    if (phaseCustomEvents.Count == 0 && options.ShowAllLanesInfo)
                    {
                        var forceEventsForAllLanes = new List<ControllerEventLog>();
                        var tempEvent1 = new ControllerEventLog()
                        {
                            SignalId = approach.Signal.SignalId,
                            EventCode = phaseEventCode,
                            EventParam = options.PhaseNumber,
                            Timestamp = options.Start.AddSeconds(-10)
                        };
                        forceEventsForAllLanes.Add(tempEvent1);
                        var tempEvent2 = new ControllerEventLog()
                        {
                            SignalId = approach.Signal.SignalId,
                            EventCode = phaseEventCode,
                            EventParam = options.PhaseNumber,
                            Timestamp = options.Start.AddSeconds(-9)
                        };
                        forceEventsForAllLanes.Add(tempEvent2);
                        phaseCustomEvents.Add(
                            "Phase Events: " + phaseEventCode, forceEventsForAllLanes);
                    }
                }
            }
            return phaseCustomEvents;
        }




        public Dictionary<string, List<ControllerEventLog>> GetDetectionEvents(
            Approach approach,
            TimingAndActuationsOptions options,
            List<ControllerEventLog> controllerEventLogs,
            DetectionTypes detectionType
            )
        {
            var stopBarEvents = new Dictionary<string, List<ControllerEventLog>>();
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
                                            stopEvents);
                    }

                    if (stopEvents.Count == 0 && options.ShowAllLanesInfo)
                    {
                        var forceEventsForAllLanes = new List<ControllerEventLog>();
                        var event1 = new ControllerEventLog()
                        {
                            SignalId = approach.Signal.SignalId,
                            EventCode = 82,
                            EventParam = detector.DetChannel,
                            Timestamp = options.Start.AddSeconds(-10)
                        };
                        forceEventsForAllLanes.Add(event1);
                        var event2 = new ControllerEventLog()
                        {
                            SignalId = approach.Signal.SignalId,
                            EventParam = detector.DetChannel,
                            EventCode = 81,
                            Timestamp = options.Start.AddSeconds(-9)
                        };
                        forceEventsForAllLanes.Add(event2);
                        stopBarEvents.Add(detectionType.GetDisplayName() + ", ch " + detector.DetChannel + " " +
                                          detector.MovementType.Abbreviation + " " +
                                          laneNumber, forceEventsForAllLanes);
                    }
                }
            }
            return stopBarEvents;
        }


        public Dictionary<string, List<ControllerEventLog>> GetPedestrianEvents(
            Approach approach,
            List<ControllerEventLog> controllerEventLogs)
        {
            var pedestrianEvents = new Dictionary<string, List<ControllerEventLog>>();
            if (string.IsNullOrEmpty(approach.PedestrianDetectors) && (approach.Signal.Pedsare1to1 && approach.IsProtectedPhaseOverlap)
                || (!approach.Signal.Pedsare1to1 && approach.PedestrianPhaseNumber.HasValue))
                return pedestrianEvents;
            var pedEventCodes = new List<int> { 89, 90 };
            var pedDetectors = approach.GetPedDetectorsFromApproach();
            foreach (var pedDetector in pedDetectors)
            {
                pedestrianEvents.Add(pedDetector.ToString(), controllerEventLogs.Where(c => pedEventCodes.Contains(c.EventCode) && c.EventParam == pedDetector).ToList());
            }
            return pedestrianEvents;
        }

        public List<ControllerEventLog> GetPedestrianIntervals(
            Approach approach,
            List<ControllerEventLog> controllerEventLogs)
        {
            List<int> overlapCodes = GetPedestrianIntervalEventCodes(approach.IsPedestrianPhaseOverlap);
            var pedPhase = approach.PedestrianPhaseNumber ?? approach.ProtectedPhaseNumber;
            return controllerEventLogs.Where(c => overlapCodes.Contains(c.EventCode) && c.EventParam == pedPhase).ToList();
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

