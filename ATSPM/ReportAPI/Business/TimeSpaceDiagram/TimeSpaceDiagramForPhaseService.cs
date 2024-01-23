using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.TimingAndActuation;
using System.Linq;

namespace ATSPM.ReportApi.Business.TimeSpaceDiagram
{
    public class TimeSpaceDiagramForPhaseService
    {
        private static readonly double FeetPerMile = 5280;
        private static readonly double SecondsInHour = 3600;
        private readonly CycleService _cycleService;

        public TimeSpaceDiagramForPhaseService(CycleService cycleService)
        {
            _cycleService = cycleService;
        }
        
        public TimeSpaceDiagramResult GetChartData(
           TimeSpaceDiagramOptions options,
           PhaseDetail phaseDetail,
           List<ControllerEventLog> controllerEventLogs,
           double distanceToNextLocation,
           bool isFirstElement,
           bool isLastElement
           )
        {
            var countEvents = new List<TimeSpaceDetectorEventDto>();
            var stopBarPresenceEvents = new List<TimeSpaceDetectorEventDto>();
            var advanceCountEvents = new List<TimeSpaceDetectorEventDto>();
            var greenTimeEventsResult = new List<TimeSpaceEventBase>();
            var countEventsTimeSpaceResult = new List<TimeSpaceEventBase>();
            var stopBarPresenceEventsTimeSpaceResult = new List<TimeSpaceEventBase>();
            var advanceCountEventsTimeSpaceResult = new List<TimeSpaceEventBase>();
            var cycleAllEvents = GetCycleEvents(phaseDetail, controllerEventLogs, options, out List<GreenToGreenCycle> resultCycles);
            var speed = options.SpeedLimit ?? phaseDetail.Approach.Mph;

            if(!speed.HasValue)
            {
                throw new ArgumentNullException($"No speed available for {phaseDetail.PhaseNumber}");
            }

            if (isFirstElement)
            {
                greenTimeEventsResult = GetGreenTimeEvents(phaseDetail, cycleAllEvents, options, distanceToNextLocation, phaseDetail.Approach.Mph ?? 0);

                countEvents = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.LLC);
                countEventsTimeSpaceResult = CalculateTimeSpaceResult(countEvents, options, distanceToNextLocation);

                stopBarPresenceEvents = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.SBP);
                stopBarPresenceEventsTimeSpaceResult = CalculateTimeSpaceResultForStopBar(stopBarPresenceEvents, options, distanceToNextLocation, resultCycles);
            }
            else if(isLastElement)
            {
                advanceCountEvents = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.AC);
                advanceCountEventsTimeSpaceResult = CalculateTimeSpaceResultForAdvanceCount(advanceCountEvents, options, distanceToNextLocation);
            }
            else
            {
                greenTimeEventsResult = GetGreenTimeEvents(phaseDetail, cycleAllEvents, options, distanceToNextLocation, phaseDetail.Approach.Mph ?? 0);

                countEvents = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.LLC);
                countEventsTimeSpaceResult = CalculateTimeSpaceResult(countEvents, options, distanceToNextLocation);

                stopBarPresenceEvents = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.SBP);
                stopBarPresenceEventsTimeSpaceResult = CalculateTimeSpaceResultForStopBar(stopBarPresenceEvents, options, distanceToNextLocation, resultCycles);

                advanceCountEvents = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.AC);
                advanceCountEventsTimeSpaceResult = CalculateTimeSpaceResultForAdvanceCount(advanceCountEvents, options, distanceToNextLocation);
            }

            var phaseNumberSort = GetPhaseSort(phaseDetail);
            var timeSpaceDiagramResult = new TimeSpaceDiagramResult(
                phaseDetail.Approach.Id,
                phaseDetail.Approach.Location.LocationIdentifier,
                options.Start,
                options.End,
                phaseDetail.PhaseNumber,
                phaseNumberSort,
                distanceToNextLocation,
                (int)speed,
                cycleAllEvents,
                countEventsTimeSpaceResult,
                advanceCountEventsTimeSpaceResult,
                stopBarPresenceEventsTimeSpaceResult,
                greenTimeEventsResult
                );
            return timeSpaceDiagramResult;
        }

        private List<TimeSpaceEventBase> CalculateTimeSpaceResultForStopBar(
            List<TimeSpaceDetectorEventDto> stopBarPresenceEvents,
            TimeSpaceDiagramOptions options,
            double distanceToNextLocation,
            List<GreenToGreenCycle> cycles)
        {
            List<TimeSpaceEventBase> results = new List<TimeSpaceEventBase>();

            if (stopBarPresenceEvents == null || stopBarPresenceEvents.Count < 1)
            {
                return results;
            }
              
            foreach (var detectorEvent in stopBarPresenceEvents)
            {
                double speedLimit = options.SpeedLimit ?? detectorEvent.SpeedLimit;
                DateTime currentDetectorOn = detectorEvent.DetectorOn;
                DateTime currentDetectorOff = detectorEvent.DetectorOff;

                //Only add events that exist over the green time
                GreenToGreenCycle isEventOnGreenTime = cycles.Find(c => currentDetectorOn >= c.StartTime && currentDetectorOn <= c.YellowEvent);
                if (isEventOnGreenTime == null)
                {
                    continue;
                }

                //If overlaps with yellow event, we want the result off to use yellow time
                GreenToGreenCycle overlappingYellowEvent = cycles.Find(e => currentDetectorOn <= e.YellowEvent && currentDetectorOff > e.YellowEvent);

                GetArrivalTime(
                    distanceToNextLocation,
                    speedLimit,
                    currentDetectorOn,
                    out _,
                    out DateTime arrivalTimeOn);

                TimeSpaceEventBase resultOn = new TimeSpaceEventBase(
                    currentDetectorOn,
                    arrivalTimeOn,
                    true);

                results.Add(resultOn);

                GetArrivalTime(
                    distanceToNextLocation,
                    speedLimit,
                    overlappingYellowEvent == null ? currentDetectorOff : overlappingYellowEvent.YellowEvent,
                    out _,
                    out DateTime arrivalTimeOff);
                TimeSpaceEventBase resultOff = new TimeSpaceEventBase(
                    overlappingYellowEvent == null ? currentDetectorOff : overlappingYellowEvent.YellowEvent,
                    arrivalTimeOff,
                    false);
                results.Add(resultOff);
            }

            return results;
        }

        private List<TimeSpaceEventBase> GetGreenTimeEvents(PhaseDetail phaseDetail,
            List<CycleEventsDto> cycleEvents,
            TimeSpaceDiagramOptions options,
            double distanceToNextLocation,
            int speedLimit)
        {
            List<int> cycleGreenStartEndCodes = new List<int>() { 1, 8 };
            var events = new List<CycleEventsDto>();
            var greenTimeEvents = new List<TimeSpaceEventBase>();
            var tempEvents =  cycleEvents.Where(c => cycleGreenStartEndCodes.Contains(c.Value)).ToList();

            foreach (var gEvent in tempEvents) {
                double speed = options.SpeedLimit ?? speedLimit;
                DateTime start = gEvent.Start;
                GetArrivalTime(distanceToNextLocation, speedLimit, start, out _, out DateTime arrivalTime);
                TimeSpaceEventBase resultOn = new TimeSpaceEventBase(
                    start,
                    arrivalTime,
                    gEvent.Value == 1 ? true : false);
                greenTimeEvents.Add(resultOn);
            }
            return greenTimeEvents;
        }

        private List<TimeSpaceEventBase> CalculateTimeSpaceResult(
            List<TimeSpaceDetectorEventDto> events,
            TimeSpaceDiagramOptions options,
            double distanceToNextLocation)
        {
            List<TimeSpaceEventBase> results = new List<TimeSpaceEventBase>();

            if (events == null || events.Count < 1)
            {
                return results;
            }

            foreach (var detectorEvent in events)
            {
                double speedLimit = options.SpeedLimit ?? detectorEvent.SpeedLimit;
                DateTime currentDetectorOn = detectorEvent.DetectorOn;
                GetArrivalTime(distanceToNextLocation, speedLimit, detectorEvent.DetectorOn, out _, out DateTime arrivalTimeOn);

                TimeSpaceEventBase resultOn = new TimeSpaceEventBase(
                    currentDetectorOn,
                    arrivalTimeOn,
                    null);

                results.Add(resultOn);
            }

            return results;
        }

        private static double GetSpeedInFeetPerSecond(double speedLimit)
        {
            return speedLimit * FeetPerMile / SecondsInHour;
        }

        private List<TimeSpaceEventBase> CalculateTimeSpaceResultForAdvanceCount(
            List<TimeSpaceDetectorEventDto> events,
            TimeSpaceDiagramOptions options,
            double distanceToNextLocation
            )
        {
            List<TimeSpaceEventBase> results = new List<TimeSpaceEventBase>();

            if (events == null || events.Count < 1)
            {
                return results;
            }


            foreach (var detectorEvent in events)
            {
                double speedLimit = options.SpeedLimit ?? detectorEvent.SpeedLimit;

                GetArrivalTime(detectorEvent.DistanceToStopBar, speedLimit, detectorEvent.DetectorOn, out double speedInFeetPerSecond, out DateTime arrivalTime);

                results.Add(new TimeSpaceEventBase(arrivalTime.AddSeconds(-distanceToNextLocation / speedInFeetPerSecond), arrivalTime, null));
            }

            return results;
        }

        private static void GetArrivalTime(double distanceToDetector, double speedLimit, DateTime InitialTime, out double speedInFeetPerSecond, out DateTime arrivalTime)
        {
            DateTime currentDetectorOn = InitialTime;

            speedInFeetPerSecond = GetSpeedInFeetPerSecond(speedLimit);
            double timeToTravel = distanceToDetector / speedInFeetPerSecond;

            arrivalTime = currentDetectorOn.AddSeconds(timeToTravel);
        }

        public List<int> GetCycleCodes(bool getOverlapCodes)
        {
            var phaseEventCodesForCycles = new List<int> { 1, 8, 9 };
            if (getOverlapCodes)
            {
                phaseEventCodesForCycles = new List<int> { 61, 62, 63, 64, 65 };
            }

            return phaseEventCodesForCycles;
        }

        public List<CycleEventsDto> GetCycleEvents(
            PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs,
            TimeSpaceDiagramOptions options,
            out List<GreenToGreenCycle> cycles)
        {

            List<int> cycleEventCodes = GetCycleCodes(phaseDetail.UseOverlap);
            var overlapLabel = phaseDetail.UseOverlap == true ? "Overlap" : "";
            string keyLabel = $"Cycles Intervals {phaseDetail.PhaseNumber} {overlapLabel}";
            var events = new List<CycleEventsDto>();
            cycles = new List<GreenToGreenCycle>();
            if (controllerEventLogs.Any())
            {
                var distinctTimeStamps = new HashSet<string>();
                var tempEvents = controllerEventLogs.Aggregate(new List<ControllerEventLog>(), (result, c) =>
                {
                    if (cycleEventCodes.Contains(c.EventCode) && c.EventParam == phaseDetail.PhaseNumber)
                    {
                        if (!distinctTimeStamps.Contains(c.ToString()))
                        {
                            result.Add(c);
                            distinctTimeStamps.Add(c.ToString());
                        }
                    }
                    return result;
                });
                cycles = _cycleService.GetGreenToGreenCycles(options.Start.AddMinutes(-2), options.End.AddMinutes(2), tempEvents).ToList();

                for ( int i = 0; i < cycles.Count; i++ )
                {
                    var cycle = cycles[i];
                    
                    events.Add(new CycleEventsDto(cycle.StartTime, 1));
                    events.Add(new CycleEventsDto(cycle.YellowEvent, 8));
                    events.Add(new CycleEventsDto(cycle.RedEvent, 9));
                }
            }
            return events;
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

        public List<TimeSpaceDetectorEventDto> GetDetectionEvents(
            Approach approach,
            TimeSpaceDiagramOptions options,
            List<ControllerEventLog> controllerEventLogs,
            DetectionTypes detectionType
            )
        {
            var DetEvents = new List<TimeSpaceDetectorEventDto>();
            var localSortedDetectors = approach.Detectors.Where(d => d.DetectionTypes.Any(d => d.Id == detectionType));
            //  82 is on, 81 is off
            var detectorActivationCodes = new List<int> { 81, 82 };
            foreach (var detector in localSortedDetectors)
            {
                if (detector.DetectionTypes.Any(d => d.Id == detectionType))
                {
                    var extendStartStopLine = options.ExtendStartStopSearch * 60.0;
                    var filteredEvents = controllerEventLogs.Where(c => detectorActivationCodes.Contains(c.EventCode)
                                                                        && c.EventParam == detector.DetectorChannel
                                                                        && c.Timestamp >= options.Start
                                                                        && c.Timestamp <= options.End).ToList();
                    if (filteredEvents.Count > 0)
                    {
                        var detectorEvents = new List<TimeSpaceDetectorEventDto>();
                        for (var i = 0; i < filteredEvents.Count; i++)
                        {
                            if(i == 0 && filteredEvents[i].EventCode == 81)
                            {
                                detectorEvents.Add(new TimeSpaceDetectorEventDto(filteredEvents[i].Timestamp,
                                   filteredEvents[i].Timestamp,
                                   approach.Mph ?? 0,
                                   detector.DistanceFromStopBar ?? 0));
                            }
                            else if (i + 1 == filteredEvents.Count && filteredEvents[i].EventCode != 81)
                            {
                                detectorEvents.Add(new TimeSpaceDetectorEventDto(filteredEvents[i].Timestamp,
                                    filteredEvents[i].Timestamp,
                                    approach.Mph ?? 0,
                                    detector.DistanceFromStopBar ?? 0));
                            }
                            else if (filteredEvents[i].EventCode == 82 && filteredEvents[i+1].EventCode == 81)
                            {
                                detectorEvents.Add(new TimeSpaceDetectorEventDto(filteredEvents[i].Timestamp,
                                    filteredEvents[i + 1].Timestamp,
                                    approach.Mph ?? 0,
                                    detector.DistanceFromStopBar ?? 0));
                            }
                        }
                        DetEvents.AddRange(detectorEvents);
                    }
                }
            }
            return DetEvents;
        }
    }
}

