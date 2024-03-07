using ATSPM.Application.Business.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.EventLogModels;
using ATSPM.Domain.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.TimingAndActuation
{
    public class TimingAndActuationsForPhaseService
    {

        public TimingAndActuationsForPhaseResult GetChartData(
            TimingAndActuationsOptions options,
            PhaseDetail phaseDetail,
            List<IndianaEvent> controllerEventLogs,
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
                stopBarEvents = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.SBP);
            }
            if (options.ShowPedestrianActuation && !usePermissivePhase)
            {
                pedestrianEvents = GetPedestrianEventsNew(phaseDetail.Approach, options, controllerEventLogs);
            }
            if (options.ShowPedestrianIntervals && !usePermissivePhase)
            {
                pedestrianIntervals = GetPedestrianIntervals(phaseDetail.Approach, controllerEventLogs, options);
            }
            if (options.ShowLaneByLaneCount)
            {
                laneByLanes = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.LLC);
            }
            if (options.ShowAdvancedDilemmaZone)
            {
                advancePresenceEvents = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.AP);
            }
            if (options.ShowAdvancedCount)
            {
                advanceCountEvents = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.AC);
            }
            if (options.PhaseEventCodesList != null)
            {
                phaseCustomEvents = GetPhaseCustomEvents(phaseDetail.Approach.Location.LocationIdentifier, phaseDetail.PhaseNumber, options, controllerEventLogs);
            }
            var cycleAllEvents = GetCycleEvents(phaseDetail, controllerEventLogs, options);
            var phaseNumberSort = GetPhaseSort(phaseDetail);
            var timingAndActuationsForPhaseData = new TimingAndActuationsForPhaseResult(
                phaseDetail.Approach.Id,
                phaseDetail.Approach.Location.LocationIdentifier,
                options.Start,
                options.End,
                phaseDetail.PhaseNumber,
                phaseDetail.UseOverlap,
                phaseNumberSort,
                usePermissivePhase ? "Permissive" : "Protected",
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
            return phaseDetail.IsPermissivePhase ?  // Check if the 'PhaseType' property of 'options' is true
                phaseDetail.Approach.IsPermissivePhaseOverlap ?  // If true, check if the 'IsPermissivePhaseOverlap' property of 'approach' is true
                    "zOverlap - " + phaseDetail.Approach.PermissivePhaseNumber.Value.ToString("D2")  // If true, concatenate "zOverlap - " with 'PermissivePhaseNumber' formatted as a two-digit string
                    : "Phase - " + phaseDetail.Approach.PermissivePhaseNumber.Value.ToString("D2")  // If false, concatenate "Phase - " with 'PermissivePhaseNumber' formatted as a two-digit string
                :  // If 'PhaseType' is false
                phaseDetail.Approach.IsProtectedPhaseOverlap ?  // Check if the 'IsProtectedPhaseOverlap' property of 'approach' is true
                    "zOverlap - " + phaseDetail.Approach.ProtectedPhaseNumber.ToString("D2")  // If true, concatenate "zOverlap - " with 'ProtectedPhaseNumber' formatted as a two-digit string
                    : "Phase = " + phaseDetail.Approach.ProtectedPhaseNumber.ToString("D2");  // If false, concatenate "Phase = " with 'ProtectedPhaseNumber' formatted as a two-digit string
        }

        public List<CycleEventsDto> GetCycleEvents(
            PhaseDetail phaseDetail,
            List<IndianaEvent> controllerEventLogs,
            TimingAndActuationsOptions options)
        {

            List<DataLoggerEnum> cycleEventCodes = GetCycleCodes(phaseDetail.UseOverlap);
            var overlapLabel = phaseDetail.UseOverlap == true ? "Overlap" : "";
            string keyLabel = $"Cycles Intervals {phaseDetail.PhaseNumber} {overlapLabel}";
            var events = new List<CycleEventsDto>();
            if (controllerEventLogs.Any())
            {
                var tempEvents = controllerEventLogs.Where(c => cycleEventCodes.Contains(c.EventCode) && c.EventParam == phaseDetail.PhaseNumber)
                    .Select(e => new CycleEventsDto(e.Timestamp, (int)e.EventCode)).ToList();
                events.AddRange(tempEvents.Where(e => e.Start >= options.Start
                                                        && e.Start <= options.End));
                var firstEvent = tempEvents.Where(e => e.Start < options.Start).OrderByDescending(e => e.Start).FirstOrDefault();
                if (firstEvent != null)
                {
                    firstEvent.Start = options.Start;
                    events.Insert(0, firstEvent);
                }
            }
            return events;
        }

        public List<DataLoggerEnum> GetCycleCodes(bool getOverlapCodes)
        {
            var phaseEventCodesForCycles = new List<DataLoggerEnum>
            {
                DataLoggerEnum.PhaseBeginGreen,
                DataLoggerEnum.PhaseMinComplete,
                DataLoggerEnum.PhaseBeginYellowChange,
                DataLoggerEnum.PhaseEndYellowChange,
                DataLoggerEnum.PhaseEndRedClearance
            };
            if (getOverlapCodes)
            {
                phaseEventCodesForCycles = new List<DataLoggerEnum>
                {
                    DataLoggerEnum.OverlapBeginGreen,
                    DataLoggerEnum.OverlapBeginTrailingGreenExtension,
                    DataLoggerEnum.OverlapBeginYellow,
                    DataLoggerEnum.OverlapBeginRedClearance,
                    DataLoggerEnum.OverlapOffInactivewithredindication
                };
            }

            return phaseEventCodesForCycles;
        }


        public Dictionary<string, List<DataPointForInt>> GetPhaseCustomEvents(
            string locationIdentifier,
            int phaseNumber,
            TimingAndActuationsOptions options,
            List<IndianaEvent> controllerEventLogs)
        {
            var phaseCustomEvents = new Dictionary<string, List<DataPointForInt>>();
            if (options.PhaseEventCodesList != null && options.PhaseEventCodesList.Any())
            {
                foreach (var phaseEventCode in options.PhaseEventCodesList.Select(e => (DataLoggerEnum)e))
                {

                    var phaseEvents = controllerEventLogs.Where(c => c.EventCode == phaseEventCode
                                                                        && c.Timestamp >= options.Start
                                                                        && c.Timestamp <= options.End).ToList();
                    if (phaseEvents.Count > 0)
                    {
                        phaseCustomEvents.Add(
                            "Phase Events: " + phaseEventCode, phaseEvents.Select(s => new DataPointForInt(s.Timestamp, (int)s.EventCode)).ToList());
                    }

                    if (phaseCustomEvents.Count == 0 && options.ShowAllLanesInfo)
                    {
                        var forceEventsForAllLanes = new List<IndianaEvent>();
                        var tempEvent1 = new IndianaEvent()
                        {
                            LocationIdentifier = locationIdentifier,
                            EventCode = phaseEventCode,
                            EventParam = Convert.ToByte(phaseNumber),
                            Timestamp = options.Start.AddSeconds(-10)
                        };
                        forceEventsForAllLanes.Add(tempEvent1);
                        var tempEvent2 = new IndianaEvent()
                        {
                            LocationIdentifier = locationIdentifier,
                            EventCode = phaseEventCode,
                            EventParam = Convert.ToByte(phaseNumber),
                            Timestamp = options.Start.AddSeconds(-9)
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

        public List<DetectorEventDto> GetDetectionEvents(
            Approach approach,
            TimingAndActuationsOptions options,
            List<IndianaEvent> controllerEventLogs,
            DetectionTypes detectionType
            )
        {
            var DetEvents = new List<DetectorEventDto>();
            var localSortedDetectors = approach.Detectors.Where(d => d.DetectionTypes.Any(d => d.Id == detectionType))
                .OrderByDescending(d => d.MovementType.GetDisplayAttribute()?.Order)
                .ThenByDescending(l => l.LaneNumber).ToList();
            var detectorActivationCodes = new List<DataLoggerEnum> { DataLoggerEnum.DetectorOff, DataLoggerEnum.DetectorOn };
            foreach (var detector in localSortedDetectors)
            {
                if (detector.DetectionTypes.Any(d => d.Id == detectionType))
                {
                    var extendStartStopLine = options.ExtendStartStopSearch * 60.0;
                    var filteredEvents = controllerEventLogs.Where(c => detectorActivationCodes.Contains(c.EventCode)
                                                                        && c.EventParam == detector.DetectorChannel
                                                                        && c.Timestamp >= options.Start
                                                                        && c.Timestamp <= options.End).ToList();
                    var laneNumber = "";
                    if (detector.LaneNumber != null)
                    {
                        laneNumber = detector.LaneNumber.Value.ToString();
                    }
                    var distanceFromStopBarLable = detector.DistanceFromStopBar.HasValue ? $"({detector.DistanceFromStopBar} ft)" : "";
                    var lableName = $"{detectionType.GetDisplayAttribute()?.Name} {distanceFromStopBarLable}, {detector.MovementType} {laneNumber}, ch {detector.DetectorChannel}";

                    if (filteredEvents.Count > 0)
                    {
                        var detectorEvents = new List<DetectorEventBase>();
                        for (var i = 0; i < filteredEvents.Count; i++)
                        {
                            if (i == 0 && filteredEvents[i].EventCode == DataLoggerEnum.DetectorOff)
                            {
                                detectorEvents.Add(new DetectorEventBase(null, filteredEvents[i].Timestamp));
                            }
                            else if (i + 1 == filteredEvents.Count && filteredEvents[i].EventCode == DataLoggerEnum.DetectorOff)
                            {
                                detectorEvents.Add(new DetectorEventBase(null, filteredEvents[i].Timestamp));
                            }
                            else if (i + 1 == filteredEvents.Count && filteredEvents[i].EventCode == DataLoggerEnum.DetectorOn)
                            {
                                detectorEvents.Add(new DetectorEventBase(filteredEvents[i].Timestamp, null));
                            }
                            else if (filteredEvents[i].EventCode == DataLoggerEnum.DetectorOn && filteredEvents[i + 1].EventCode == DataLoggerEnum.DetectorOff)
                            {
                                detectorEvents.Add(new DetectorEventBase(filteredEvents[i].Timestamp, filteredEvents[i + 1].Timestamp));
                                i++;
                            }
                            else if (filteredEvents[i].EventCode == DataLoggerEnum.DetectorOff && filteredEvents[i + 1].EventCode == DataLoggerEnum.DetectorOff)
                            {
                                detectorEvents.Add(new DetectorEventBase(null, filteredEvents[i + 1].Timestamp));
                            }
                            else if (filteredEvents[i].EventCode == DataLoggerEnum.DetectorOn && filteredEvents[i + 1].EventCode == DataLoggerEnum.DetectorOn)
                            {
                                detectorEvents.Add(new DetectorEventBase(filteredEvents[i + 1].Timestamp, null));
                            }
                        }
                        DetEvents.Add(new DetectorEventDto(lableName, detectorEvents));
                    }

                    else if (filteredEvents.Count == 0 && options.ShowAllLanesInfo)
                    {
                        var e = new DetectorEventBase(options.Start.AddSeconds(-10), options.Start.AddSeconds(-9));

                        var list = new List<DetectorEventBase>
                        {
                            e
                        };
                        DetEvents.Add(new DetectorEventDto(lableName, list));
                    }
                }
            }
            return DetEvents;
        }

        public List<DetectorEventDto> GetPedestrianEventsNew(
            Approach approach,
            TimingAndActuationsOptions options,
            List<IndianaEvent> controllerEventLogs)
        {
            var pedestrianEvents = new List<DetectorEventDto>();
            if (string.IsNullOrEmpty(approach.PedestrianDetectors) && approach.Location.PedsAre1to1 && approach.IsProtectedPhaseOverlap
                || !approach.Location.PedsAre1to1 && approach.PedestrianPhaseNumber.HasValue)
                return pedestrianEvents;
            var pedEventCodes = new List<DataLoggerEnum> { DataLoggerEnum.PedDetectorOff, DataLoggerEnum.PedDetectorOn };
            foreach (var pedDetector in approach.Detectors)
            {
                var lableName = $"Ped Det. Actuations, ph {approach.ProtectedPhaseNumber}, ch {pedDetector.DetectorChannel}";
                var pedEvents = controllerEventLogs.Where(c => pedEventCodes.Contains(c.EventCode)
                                                                && c.EventParam == pedDetector.DetectorChannel
                                                                && c.Timestamp >= options.Start
                                                                && c.Timestamp <= options.End)
                                                   .ToList();
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
                }
            }
            return pedestrianEvents;
        }

        public List<CycleEventsDto> GetPedestrianIntervals(
            Approach approach,
            List<IndianaEvent> controllerEventLogs,
            TimingAndActuationsOptions options)
        {
            List<DataLoggerEnum> overlapCodes = GetPedestrianIntervalEventCodes(approach.IsPedestrianPhaseOverlap);
            var pedPhase = approach.PedestrianPhaseNumber ?? approach.ProtectedPhaseNumber;
            return controllerEventLogs.Where(c => overlapCodes.Contains(c.EventCode)
                                                    && c.EventParam == pedPhase
                                                    && c.Timestamp >= options.Start
                                                    && c.Timestamp <= options.End).Select(s => new CycleEventsDto(s.Timestamp, (int)s.EventCode)).ToList();
        }

        public List<DataLoggerEnum> GetPedestrianIntervalEventCodes(bool isPhaseOrOverlap)
        {
            var overlapCodes = new List<DataLoggerEnum>
            {
                DataLoggerEnum.PedestrianBeginWalk,
                DataLoggerEnum.PedestrianBeginChangeInterval,
                DataLoggerEnum.PedestrianBeginSolidDontWalk
            };
            if (isPhaseOrOverlap)
            {
                overlapCodes = new List<DataLoggerEnum> { DataLoggerEnum.PedestrianOverlapBeginWalk, DataLoggerEnum.PedestrianOverlapBeginClearance, DataLoggerEnum.PedestrianOverlapBeginSolidDontWalk };
            }

            return overlapCodes;
        }
    }
}

