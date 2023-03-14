using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Legacy.Common.Business.WCFServiceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Legacy.Common.Business.TimingAndActuations
{
    public class TimingAndActuationsForPhaseData
    {
        public int PhaseNumber { get; set; }
        public bool PhaseOrOverlap { get; set; }
        public Approach Approach { get; set; }
        public List<Plan> Plans { get; set; }
        public string PhaseNumberSort { get; set; }
        public bool GetPermissivePhase { get; set; }
        public TimingAndActuationsOptions Options { get; set; }
        public List<TimingAndActuationCycle> Cycles { get; set; }
        public List<ControllerEventLog> CycleDataEventLogs { get; set; }
        public List<ControllerEventLog> PedestrianIntervals { get; set; }
        public List<ControllerEventLog> ForceEventsForAllLanes { get; set; }
        public Dictionary<string, List<ControllerEventLog>> PedestrianEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> CycleAllEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> PedestrianAllEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> PedestrianAllIntervals { get; set; }
        public Dictionary<string, List<ControllerEventLog>> AdvanceCountEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> AdvancePresenceEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> StopBarEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> LaneByLanes { get; set; }
        public Dictionary<string, List<ControllerEventLog>> PhaseCustomEvents { get; set; }
    }
    public class TimingAndActuationsForPhaseService
    {
        private readonly IControllerEventLogRepository controllerEventLogRepository;

        public TimingAndActuationsForPhaseService(IControllerEventLogRepository controllerEventLogRepository)
        {
            this.controllerEventLogRepository = controllerEventLogRepository;
        }

        public TimingAndActuationsForPhaseData  GetChartData(TimingAndActuationsOptions options)
        {
            var timingAndActuationsForPhaseData = new TimingAndActuationsForPhaseData();
            timingAndActuationsForPhaseData.Approach = approach;
            timingAndActuationsForPhaseData.PhaseNumber = phaseNumber;
            timingAndActuationsForPhaseData.Options = options;
            timingAndActuationsForPhaseData.PhaseOrOverlap =  phaseOrOverlap;
            GetAllRawCycleData(options.StartDate, options.EndDate, timingAndActuationsForPhaseData.PhaseOrOverlap, timingAndActuationsForPhaseData);
            if (timingAndActuationsForPhaseData.Options.ShowPedestrianIntervals)
            {
                GetPedestrianIntervals(timingAndActuationsForPhaseData.PhaseOrOverlap, timingAndActuationsForPhaseData);
            }
            if (timingAndActuationsForPhaseData.Options.ShowPedestrianActuation)
            {
                GetPedestrianEvents(timingAndActuationsForPhaseData);
            }
            if (timingAndActuationsForPhaseData.Options.PhaseEventCodesList != null)
            {
                var optionsSignalID = timingAndActuationsForPhaseData.Options.SignalId;
                GetRawCustomEvents(optionsSignalID, timingAndActuationsForPhaseData.PhaseNumber, options.StartDate, options.EndDate, timingAndActuationsForPhaseData);
            }
            return timingAndActuationsForPhaseData;
        }

        public TimingAndActuationsForPhaseService(Approach approach, TimingAndActuationsOptions options,
            bool getPermissivePhase, TimingAndActuationsForPhaseData timingAndActuationsForPhaseData)
        {
            timingAndActuationsForPhaseData.GetPermissivePhase = getPermissivePhase;
            timingAndActuationsForPhaseData.Approach = approach;
            timingAndActuationsForPhaseData.Options = options;
            timingAndActuationsForPhaseData.PhaseNumber = timingAndActuationsForPhaseData.GetPermissivePhase ? timingAndActuationsForPhaseData.Approach.PermissivePhaseNumber.Value : timingAndActuationsForPhaseData.Approach.ProtectedPhaseNumber;
            if (timingAndActuationsForPhaseData.Options.ShowVehicleSignalDisplay)
            {
                GetAllCycleData(timingAndActuationsForPhaseData.Options.StartDate,
                    timingAndActuationsForPhaseData.Options.EndDate, approach, getPermissivePhase, timingAndActuationsForPhaseData);
            }
            if (timingAndActuationsForPhaseData.Options.ShowStopBarPresence)
            {
                GetStopBarPresenceEvents(timingAndActuationsForPhaseData);
            }
            if (timingAndActuationsForPhaseData.Options.ShowPedestrianActuation && !timingAndActuationsForPhaseData.GetPermissivePhase)
            {
                GetPedestrianEvents(timingAndActuationsForPhaseData);
            }
            if (timingAndActuationsForPhaseData.Options.ShowPedestrianIntervals && !timingAndActuationsForPhaseData.GetPermissivePhase)
            {
                var getPhaseOrOverlapEvents = timingAndActuationsForPhaseData.Approach.IsPedestrianPhaseOverlap;
                GetPedestrianIntervals(getPhaseOrOverlapEvents, timingAndActuationsForPhaseData);
            }
            if (timingAndActuationsForPhaseData.Options.ShowLaneByLaneCount)
            {
                GetLaneByLaneEvents(timingAndActuationsForPhaseData);
            }
            if (timingAndActuationsForPhaseData.Options.ShowAdvancedDilemmaZone)
            {
                GetAdvancePresenceEvents(timingAndActuationsForPhaseData);
            }
            if (timingAndActuationsForPhaseData.Options.ShowAdvancedCount)
            {
                GetAdvanceCountEvents(timingAndActuationsForPhaseData);
            }
            if (timingAndActuationsForPhaseData.Options.PhaseEventCodesList != null)
            {
                GetPhaseCustomEvents(timingAndActuationsForPhaseData);
            }
        }

        private void GetRawCustomEvents(string signalID, int numberPhase, DateTime optionsStartDateTime,
            DateTime optionsEndDateTime, TimingAndActuationsForPhaseData timingAndActuationsForPhaseData)
        {
            timingAndActuationsForPhaseData.PhaseCustomEvents = new Dictionary<string, List<ControllerEventLog>>();
            if (timingAndActuationsForPhaseData.Options.PhaseEventCodesList != null && timingAndActuationsForPhaseData.Options.PhaseEventCodesList.Any() &&
                timingAndActuationsForPhaseData.Options.PhaseEventCodesList.Count > 0)
            {
                foreach (var phaseEventCode in timingAndActuationsForPhaseData.Options.PhaseEventCodesList)
                {
                    var extentStartStopSearch = timingAndActuationsForPhaseData.Options.ExtendStartStopSearch * 60;
                    var phaseEvents = controllerEventLogRepository.GetEventsByEventCodesParam(signalID,
                        optionsStartDateTime.AddSeconds(-extentStartStopSearch), optionsEndDateTime.AddSeconds(extentStartStopSearch),
                        new List<int> { phaseEventCode }, numberPhase).ToList();
                    if (phaseEvents.Count > 0)
                    {
                        var minTimeStamp = phaseEvents[0].Timestamp.ToString(" hh:mm:ss ");
                        var maxTimeStamp = phaseEvents[phaseEvents.Count - 1].Timestamp.ToString(" hh:mm:ss ");
                        var keyLabel = "Phase Event Code: " + phaseEventCode;
                        //+ minTimeStamp + " -> " + maxTimeStamp;
                        timingAndActuationsForPhaseData.PhaseCustomEvents.Add(keyLabel, phaseEvents);
                    }
                }
            }
        }

        private void GetAllRawCycleData(DateTime optionsStartDate, DateTime optionsEndDate, bool phasedata, TimingAndActuationsForPhaseData timingAndActuationsForPhaseData)
        {
            timingAndActuationsForPhaseData.CycleDataEventLogs = new List<ControllerEventLog>();
            timingAndActuationsForPhaseData.CycleAllEvents = new Dictionary<string, List<ControllerEventLog>>();
            var simpleEndDate = optionsEndDate;
            var phaseEventCodesForCycles = new List<int> { 1, 3, 8, 9, 11 };
            if (!phasedata)
            {
                phaseEventCodesForCycles = new List<int> { 61, 62, 63, 64, 65 };
            }

            string keyLabel = "Cycles Intervals " + timingAndActuationsForPhaseData.PhaseNumber + " " + phasedata;
            var extendLeftSearch = timingAndActuationsForPhaseData.Options.ExtendVsdSearch * 60;
            timingAndActuationsForPhaseData.CycleDataEventLogs = controllerEventLogRepository.GetEventsByEventCodesParam(timingAndActuationsForPhaseData.Options.SignalId,
                optionsStartDate.AddSeconds(-extendLeftSearch), simpleEndDate,
                phaseEventCodesForCycles, timingAndActuationsForPhaseData.PhaseNumber).ToList();
            if (timingAndActuationsForPhaseData.CycleDataEventLogs.Count > 0)
            {
                timingAndActuationsForPhaseData.CycleAllEvents.Add(keyLabel, timingAndActuationsForPhaseData.CycleDataEventLogs);
            }
        }

        private void GetAllCycleData(
            DateTime startDate,
            DateTime endDate,
            Approach approach,
            bool getPermissivePhase,
            TimingAndActuationsForPhaseData timingAndActuationsForPhaseData)
        {
            timingAndActuationsForPhaseData.CycleDataEventLogs = new List<ControllerEventLog>();
            timingAndActuationsForPhaseData.CycleAllEvents = new Dictionary<string, List<ControllerEventLog>>();
            var extendStartTime = timingAndActuationsForPhaseData.Options.ExtendVsdSearch * 60.0;
            var phaseEventCodesForCycles = new List<int> { 1, 3, 8, 9, 11 };
            if (approach.IsProtectedPhaseOverlap || approach.IsPermissivePhaseOverlap)
            {
                phaseEventCodesForCycles = new List<int> { 61, 62, 63, 64, 65 };
            }

            int phaseNumber = getPermissivePhase ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber;
            string keyLabel = "Cycles Intervals " + phaseNumber;
            timingAndActuationsForPhaseData.CycleDataEventLogs = controllerEventLogRepository.GetEventsByEventCodesParam(timingAndActuationsForPhaseData.Approach.SignalId,
                startDate.AddSeconds(-extendStartTime), endDate,
                phaseEventCodesForCycles, timingAndActuationsForPhaseData.PhaseNumber).ToList();
            if (timingAndActuationsForPhaseData.CycleDataEventLogs.Count > 0)
            {
                //var minTimeStamp = CycleDataEventLogs[0].Timestamp.ToString(" hh:mm:ss ");
                //var maxTimeStamp = CycleDataEventLogs[CycleDataEventLogs.Count - 1].Timestamp.ToString(" hh:mm:ss ");
                //keyLabel = keyLabel + " " + minTimeStamp + " -> " + maxTimeStamp;
                timingAndActuationsForPhaseData.CycleAllEvents.Add(keyLabel, timingAndActuationsForPhaseData.CycleDataEventLogs);
            }

        }

        private void GetPhaseCustomEvents(TimingAndActuationsForPhaseData timingAndActuationsForPhaseData)
        {

            var startDate = timingAndActuationsForPhaseData.Options.StartDate;
            var endDate = timingAndActuationsForPhaseData.Options.EndDate;
            timingAndActuationsForPhaseData.PhaseCustomEvents = new Dictionary<string, List<ControllerEventLog>>();
            if (timingAndActuationsForPhaseData.Options.PhaseEventCodesList != null && timingAndActuationsForPhaseData.Options.PhaseEventCodesList.Any() &&
                timingAndActuationsForPhaseData.Options.PhaseEventCodesList.Count > 0)
            {
                foreach (var phaseEventCode in timingAndActuationsForPhaseData.Options.PhaseEventCodesList)
                {
                    var phaseEvents = controllerEventLogRepository.GetEventsByEventCodesParam(timingAndActuationsForPhaseData.Approach.SignalId,
                        startDate, endDate, new List<int> { phaseEventCode }, timingAndActuationsForPhaseData.PhaseNumber).ToList();
                    if (phaseEvents.Count > 0)
                    {
                        timingAndActuationsForPhaseData.PhaseCustomEvents.Add(
                            "Phase Events: " + phaseEventCode, phaseEvents);
                    }

                    if (timingAndActuationsForPhaseData.PhaseCustomEvents.Count == 0 && timingAndActuationsForPhaseData.Options.ShowAllLanesInfo)
                    {
                        var forceEventsForAllLanes = new List<ControllerEventLog>();
                        var tempEvent1 = new ControllerEventLog()
                        {
                            SignalId = timingAndActuationsForPhaseData.Options.SignalId,
                            EventCode = phaseEventCode,
                            EventParam = timingAndActuationsForPhaseData.PhaseNumber,
                            Timestamp = timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-10)
                        };
                        forceEventsForAllLanes.Add(tempEvent1);
                        var tempEvent2 = new ControllerEventLog()
                        {
                            SignalId = timingAndActuationsForPhaseData.Options.SignalId,
                            EventCode = phaseEventCode,
                            EventParam = timingAndActuationsForPhaseData.PhaseNumber,
                            Timestamp = timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-9)
                        };
                        forceEventsForAllLanes.Add(tempEvent2);
                        timingAndActuationsForPhaseData.PhaseCustomEvents.Add(
                            "Phase Events: " + phaseEventCode, forceEventsForAllLanes);
                    }
                }
            }
        }

        private void GetLaneByLaneEvents(TimingAndActuationsForPhaseData timingAndActuationsForPhaseData)
        {
            timingAndActuationsForPhaseData.LaneByLanes = new Dictionary<string, List<ControllerEventLog>>();
            var localSortedDetectors = timingAndActuationsForPhaseData.Approach.Detectors.OrderByDescending(d => d.MovementType.DisplayOrder)
                .ThenByDescending(l => l.LaneNumber).ToList();
            //Parallel.ForEach(localSortedDetectors, detector =>
            foreach (var detector in localSortedDetectors)
            {
                if (detector.DetectionTypes.Any(d => d.Id == ATSPM.Data.Enums.DetectionTypes.LLC))
                {
                    var laneNumber = "";
                    if (detector.LaneNumber != null)
                    {
                        laneNumber = detector.LaneNumber.Value.ToString();
                    }

                    var extendBothStartStopSearch = timingAndActuationsForPhaseData.Options.ExtendStartStopSearch * 60;
                    var laneByLane = controllerEventLogRepository.GetEventsByEventCodesParam(timingAndActuationsForPhaseData.Approach.SignalId,
                        timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-extendBothStartStopSearch),
                        timingAndActuationsForPhaseData.Options.EndDate.AddSeconds(extendBothStartStopSearch),
                        new List<int> { 81, 82 }, detector.DetChannel).ToList();
                    if (laneByLane.Count > 0)
                    {
                        //var minTimeStamp = laneByLane[0].Timestamp.ToString(" hh:mm:ss ");
                        //var maxTimeStamp = laneByLane[CycleDataEventLogs.Count - 1].Timestamp.ToString(" hh:mm:ss ");
                        //var keyLabel = "Lane-by-lane Count, " + detector.MovementType.Abbreviation + " " +
                        //               laneNumber + ", ch " + detector.DetChannel + 
                        //               " " + minTimeStamp + " -> " + maxTimeStamp;

                        timingAndActuationsForPhaseData.LaneByLanes.Add("Lane-by-lane Count, " + detector.MovementType.Abbreviation + " " +
                                        laneNumber + ", ch " + detector.DetChannel, laneByLane);
                        //LaneByLanes.Add(keyLabel, laneByLane);
                    }

                    if (timingAndActuationsForPhaseData.LaneByLanes.Count == 0 && timingAndActuationsForPhaseData.Options.ShowAllLanesInfo)
                    {
                        var forceEventsForAllLanes = new List<ControllerEventLog>();
                        var tempEvent1 = new ControllerEventLog()
                        {
                            SignalId = timingAndActuationsForPhaseData.Options.SignalId,
                            EventCode = 82,
                            EventParam = detector.DetChannel,
                            Timestamp = timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-10)
                        };
                        forceEventsForAllLanes.Add(tempEvent1);
                        var tempEvent2 = new ControllerEventLog()
                        {
                            SignalId = timingAndActuationsForPhaseData.Options.SignalId,
                            EventCode = 81,
                            EventParam = detector.DetChannel,
                            Timestamp = timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-9)
                        };
                        forceEventsForAllLanes.Add(tempEvent2);
                        timingAndActuationsForPhaseData.LaneByLanes.Add("Lane-by-Lane Count, " + detector.MovementType.Abbreviation + " " + laneNumber +
                                        " ch " + detector.DetChannel + " ", forceEventsForAllLanes);
                    }
                }
            }
            //});
        }

        private void GetStopBarPresenceEvents(TimingAndActuationsForPhaseData timingAndActuationsForPhaseData)
        {
            timingAndActuationsForPhaseData.StopBarEvents = new Dictionary<string, List<ControllerEventLog>>();
            var localSortedDetectors = timingAndActuationsForPhaseData.Approach.Detectors.OrderByDescending(d => d.MovementType.DisplayOrder)
                .ThenByDescending(l => l.LaneNumber).ToList();
            //Parallel.ForEach(localSortedDetectors, detector =>
            foreach (var detector in localSortedDetectors)
            {
                if (detector.DetectionTypes.Any(d => d.Id == ATSPM.Data.Enums.DetectionTypes.SBP))
                {
                    var extendStartStopLine = timingAndActuationsForPhaseData.Options.ExtendStartStopSearch * 60.0;
                    var stopEvents = controllerEventLogRepository.GetEventsByEventCodesParam(timingAndActuationsForPhaseData.Approach.SignalId,
                        timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-extendStartStopLine),
                        timingAndActuationsForPhaseData.Options.EndDate.AddSeconds(extendStartStopLine),
                        new List<int> { 81, 82 }, detector.DetChannel).ToList();
                    var laneNumber = "";
                    if (detector.LaneNumber != null)
                    {
                        laneNumber = detector.LaneNumber.Value.ToString();
                    }

                    if (stopEvents.Count > 0)
                    {

                        timingAndActuationsForPhaseData.StopBarEvents.Add("Stop Bar Presence, " + detector.MovementType.Abbreviation + " " +
                                          laneNumber + ", ch " + detector.DetChannel, stopEvents);
                    }

                    if (stopEvents.Count == 0 && timingAndActuationsForPhaseData.Options.ShowAllLanesInfo)
                    {
                        var forceEventsForAllLanes = new List<ControllerEventLog>();
                        var event1 = new ControllerEventLog()
                        {
                            SignalId = timingAndActuationsForPhaseData.Options.SignalId,
                            EventCode = 82,
                            EventParam = detector.DetChannel,
                            Timestamp = timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-10)
                        };
                        forceEventsForAllLanes.Add(event1);
                        var event2 = new ControllerEventLog()
                        {
                            SignalId = timingAndActuationsForPhaseData.Options.SignalId,
                            EventParam = detector.DetChannel,
                            EventCode = 81,
                            Timestamp = timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-9)
                        };
                        forceEventsForAllLanes.Add(event2);
                        timingAndActuationsForPhaseData.StopBarEvents.Add("Stop Bar Presence, ch " + detector.DetChannel + " " +
                                          detector.MovementType.Abbreviation + " " +
                                          laneNumber, forceEventsForAllLanes);
                    }
                }

            }
            //});
        }

        private void GetAdvanceCountEvents(TimingAndActuationsForPhaseData timingAndActuationsForPhaseData)
        {
            var extendBothStartStopSearch = timingAndActuationsForPhaseData.Options.ExtendStartStopSearch * 60.0;
            timingAndActuationsForPhaseData.AdvanceCountEvents = new Dictionary<string, List<ControllerEventLog>>();
            var localSortedDetectors = timingAndActuationsForPhaseData.Approach.Detectors.OrderByDescending(d => d.MovementType.DisplayOrder)
                .ThenByDescending(l => l.LaneNumber).ToList();
            //Parallel.ForEach(localSortedDetectors, detector =>
            foreach (var detector in localSortedDetectors)
            {
                string movementType = detector.MovementType.Abbreviation == null
                    ? " " : detector.MovementType.Abbreviation;
                string laneNumber = " ";
                if (detector.LaneNumber != null)
                {
                    laneNumber = detector.LaneNumber.Value == 0 ? " " : detector.LaneNumber.Value.ToString();
                }

                if (detector.DetectionTypes.Any(d => d.Id == ATSPM.Data.Enums.DetectionTypes.AC))
                {
                    var advanceEvents = controllerEventLogRepository.GetEventsByEventCodesParam(timingAndActuationsForPhaseData.Approach.SignalId,
                        timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-extendBothStartStopSearch),
                        timingAndActuationsForPhaseData.Options.EndDate.AddSeconds(extendBothStartStopSearch),
                        new List<int> { 81, 82 }, detector.DetChannel).ToList();
                    if (advanceEvents.Count > 0)
                    {
                        var keyLabel = "Advanced Count (" +
                                       detector.DistanceFromStopBar + " ft) " +
                                       movementType + " " + laneNumber + ", ch " + detector.DetChannel;
                        timingAndActuationsForPhaseData.AdvanceCountEvents.Add(keyLabel, advanceEvents);
                    }
                    else if (timingAndActuationsForPhaseData.AdvanceCountEvents.Count == 0 && timingAndActuationsForPhaseData.Options.ShowAllLanesInfo)
                    {
                        var forceEventsForAllLanes = new List<ControllerEventLog>();
                        var tempEvent1 = new ControllerEventLog()
                        {
                            SignalId = timingAndActuationsForPhaseData.Options.SignalId,
                            EventCode = 82,
                            EventParam = detector.DetChannel,
                            Timestamp = timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-10)
                        };
                        forceEventsForAllLanes.Add(tempEvent1);
                        var tempEvent2 = new ControllerEventLog()
                        {
                            SignalId = timingAndActuationsForPhaseData.Options.SignalId,
                            EventCode = 81,
                            EventParam = detector.DetChannel,
                            Timestamp = timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-9)
                        };
                        forceEventsForAllLanes.Add(tempEvent2);
                        timingAndActuationsForPhaseData.AdvanceCountEvents.Add("Advanced Count (" + detector.DistanceFromStopBar + " ft), ch " +
                                               detector.DetChannel + " " + movementType + " " +
                                               laneNumber, forceEventsForAllLanes);
                    }
                }
            }
        }

        private void GetAdvancePresenceEvents(TimingAndActuationsForPhaseData timingAndActuationsForPhaseData)
        {
            var extendBothStartStopSearch = timingAndActuationsForPhaseData.Options.ExtendStartStopSearch * 60.0;
            timingAndActuationsForPhaseData.AdvancePresenceEvents = new Dictionary<string, List<ControllerEventLog>>();
            var localSortedDetectors = timingAndActuationsForPhaseData.Approach.Detectors.OrderByDescending(d => d.MovementType.DisplayOrder)
                .ThenByDescending(l => l.LaneNumber).ToList();
            //Parallel.ForEach(localSortedDetectors, detector =>
            foreach (var detector in localSortedDetectors)
            {
                if (detector.DetectionTypes.Any(d => d.Id == ATSPM.Data.Enums.DetectionTypes.AP))
                {
                    var advancePresence = controllerEventLogRepository.GetEventsByEventCodesParam(timingAndActuationsForPhaseData.Approach.SignalId,
                        timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-extendBothStartStopSearch),
                        timingAndActuationsForPhaseData.Options.EndDate.AddSeconds(extendBothStartStopSearch),
                        new List<int> { 81, 82 }, detector.DetChannel).ToList();
                    var laneNumber = "";
                    if (detector.LaneNumber != null)
                    {
                        laneNumber = detector.LaneNumber.Value.ToString();
                    }

                    if (advancePresence.Count > 0)
                    {
                        var keyLabel = "Advanced Presence, " +
                                       detector.MovementType.Abbreviation + " " + laneNumber + ", ch " +
                                       detector.DetChannel;
                        timingAndActuationsForPhaseData.AdvancePresenceEvents.Add(keyLabel, advancePresence);
                    }
                    else if (timingAndActuationsForPhaseData.AdvancePresenceEvents.Count == 0 && timingAndActuationsForPhaseData.Options.ShowAllLanesInfo)
                    {
                        var forceEventsForAllLanes = new List<ControllerEventLog>();
                        var tempEvent1 = new ControllerEventLog()
                        {
                            SignalId = timingAndActuationsForPhaseData.Options.SignalId,
                            EventCode = 82,
                            EventParam = detector.DetChannel,
                            Timestamp = timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-10)
                        };
                        forceEventsForAllLanes.Add(tempEvent1);
                        var tempEvent2 = new ControllerEventLog()
                        {
                            SignalId = timingAndActuationsForPhaseData.Options.SignalId,
                            EventCode = 81,
                            EventParam = detector.DetChannel,
                            Timestamp = timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-9)
                        };
                        forceEventsForAllLanes.Add(tempEvent2);
                        timingAndActuationsForPhaseData.AdvancePresenceEvents.Add("Advanced Presence, ch " + detector.DetChannel + " " +
                                                  detector.MovementType.Abbreviation + " " +
                                                  laneNumber, forceEventsForAllLanes);
                    }
                }
            }
        }


        private void GetPedestrianEvents(TimingAndActuationsForPhaseData timingAndActuationsForPhaseData)
        {
            timingAndActuationsForPhaseData.PedestrianEvents = new Dictionary<string, List<ControllerEventLog>>();

            if (timingAndActuationsForPhaseData.Approach.Signal.Pedsare1to1 && timingAndActuationsForPhaseData.Approach.IsProtectedPhaseOverlap
                || !timingAndActuationsForPhaseData.Approach.Signal.Pedsare1to1 && timingAndActuationsForPhaseData.Approach.PedestrianPhaseNumber.HasValue
                && String.IsNullOrEmpty(timingAndActuationsForPhaseData.Approach.PedestrianDetectors))
                return;

            var pedDetectors = timingAndActuationsForPhaseData.Approach.GetPedDetectorsFromApproach();
            var extendStartTime = timingAndActuationsForPhaseData.Options.ExtendVsdSearch * 60.0;
            foreach (var pedDetector in pedDetectors)
            {
                var pedDetectorEvents = new List<ControllerEventLog>();
                pedDetectorEvents.AddRange(controllerEventLogRepository.GetEventsByEventCodesParam(
                    timingAndActuationsForPhaseData.Options.SignalId,
                    timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-extendStartTime),
                    timingAndActuationsForPhaseData.Options.EndDate,
                    new List<int> { 89, 90 },
                    pedDetector));
                timingAndActuationsForPhaseData.PedestrianEvents.Add(pedDetector.ToString(), pedDetectorEvents);
            }
        }

        public void GetPedestrianIntervals(bool phaseOrOverlap, TimingAndActuationsForPhaseData timingAndActuationsForPhaseData)
        {
            var extendStartSearch = timingAndActuationsForPhaseData.Options.ExtendStartStopSearch * 60.0;
            var overlapCodes = new List<int> { 21, 22, 23 };
            if (phaseOrOverlap)
            {
                overlapCodes = new List<int> { 67, 68, 69 };
            }

            var pedPhase = timingAndActuationsForPhaseData.Approach.PedestrianPhaseNumber ?? timingAndActuationsForPhaseData.Approach.ProtectedPhaseNumber;
            if (pedPhase != null)
            {
                timingAndActuationsForPhaseData.PedestrianIntervals = new List<ControllerEventLog>();
                timingAndActuationsForPhaseData.PedestrianIntervals = controllerEventLogRepository.GetEventsByEventCodesParam(timingAndActuationsForPhaseData.Options.SignalId,
                    timingAndActuationsForPhaseData.Options.StartDate.AddSeconds(-extendStartSearch), timingAndActuationsForPhaseData.Options.EndDate.AddSeconds(extendStartSearch),
                    overlapCodes, pedPhase).ToList();
            }
        }
    }
}

