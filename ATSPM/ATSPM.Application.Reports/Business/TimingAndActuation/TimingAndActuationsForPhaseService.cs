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
            var timingAndActuationsForPhaseData = new TimingAndActuationsForPhaseResult
            {
                GetPermissivePhase = options.GetPermissivePhase,
                ApproachId = options.ApproachId,
                PhaseNumber = options.PhaseNumber,
                PhaseOrOverlap = options.PhaseOrOverlap,
                CycleAllEvents = GetCycleEvents(options, approach, controllerEventLogs)
            };
            if (options.ShowStopBarPresence)
            {
                timingAndActuationsForPhaseData.StopBarEvents = GetDetectionEvents(approach, options, controllerEventLogs, DetectionTypes.SBP);
            }
            if (options.ShowPedestrianActuation && !options.GetPermissivePhase)
            {
                timingAndActuationsForPhaseData.PedestrianEvents = GetPedestrianEvents(approach, options, controllerEventLogs);
            }
            if (options.ShowPedestrianIntervals && !options.GetPermissivePhase)
            {
                timingAndActuationsForPhaseData.PedestrianIntervals = GetPedestrianIntervals(approach, options, controllerEventLogs);
            }
            if (options.ShowLaneByLaneCount)
            {
                timingAndActuationsForPhaseData.LaneByLanes = GetDetectionEvents(approach, options, controllerEventLogs, DetectionTypes.LLC);
            }
            if (options.ShowAdvancedDilemmaZone)
            {
                timingAndActuationsForPhaseData.AdvancePresenceEvents = GetDetectionEvents(approach, options, controllerEventLogs, DetectionTypes.AP);
            }
            if (options.ShowAdvancedCount)
            {
                timingAndActuationsForPhaseData.AdvanceCountEvents = GetDetectionEvents(approach, options, controllerEventLogs, DetectionTypes.AC);
            }
            if (options.PhaseEventCodesList != null)
            {
                timingAndActuationsForPhaseData.PhaseCustomEvents = GetPhaseCustomEvents(approach, options, controllerEventLogs);
            }
            return timingAndActuationsForPhaseData;
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
            Dictionary<string, List<ControllerEventLog>> events = new Dictionary<string, List<ControllerEventLog>>();
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
                            SignalId = approach.SignalId,
                            EventCode = phaseEventCode,
                            EventParam = options.PhaseNumber,
                            Timestamp = options.Start.AddSeconds(-10)
                        };
                        forceEventsForAllLanes.Add(tempEvent1);
                        var tempEvent2 = new ControllerEventLog()
                        {
                            SignalId = approach.SignalId,
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
            Dictionary<string, List<ControllerEventLog>> stopBarEvents = new Dictionary<string, List<ControllerEventLog>>();
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
                            SignalId = approach.SignalId,
                            EventCode = 82,
                            EventParam = detector.DetChannel,
                            Timestamp = options.Start.AddSeconds(-10)
                        };
                        forceEventsForAllLanes.Add(event1);
                        var event2 = new ControllerEventLog()
                        {
                            SignalId = approach.SignalId,
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
            TimingAndActuationsOptions options,
            List<ControllerEventLog> controllerEventLogs)
        {
            var pedestrianEvents = new Dictionary<string, List<ControllerEventLog>>();
            if (approach.PedestrianDetectors.IsNullOrEmpty() && (approach.Signal.Pedsare1to1 && approach.IsProtectedPhaseOverlap)
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
            TimingAndActuationsOptions options,
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

