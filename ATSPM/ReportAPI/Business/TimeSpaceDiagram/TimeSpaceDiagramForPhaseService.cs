using ATSPM.Application.Analysis.Common;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using ATSPM.ReportApi.Business.Common;
using ATSPM.ReportApi.Business.TimingAndActuation;

namespace ATSPM.ReportApi.Business.TimeSpaceDiagram
{
    public class TimeSpaceDiagramForPhaseService
    {
        private static readonly double FeetPerMile = 5280;
        private static readonly double SecondsInHour = 3600;

        public TimeSpaceDiagramResult GetChartData(
           TimeSpaceDiagramOption options,
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
            var cycleAllEvents = GetCycleEvents(phaseDetail, controllerEventLogs, options);
           

            if (isFirstElement)
            {
                countEvents = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.LLC);
                countEventsTimeSpaceResult = CalculateTimeSpaceResult(countEvents, options, distanceToNextLocation);

                stopBarPresenceEvents = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.SBP);
                stopBarPresenceEventsTimeSpaceResult = CalculateTimeSpaceResult(stopBarPresenceEvents, options, distanceToNextLocation, true);

                greenTimeEventsResult = GetGreenTimeEvents(phaseDetail, controllerEventLogs, options, distanceToNextLocation, phaseDetail.Approach.Mph ?? 0);
            }
            else if(isLastElement)
            {
                advanceCountEvents = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.AC);
                advanceCountEventsTimeSpaceResult = CalculateTimeSpaceResultForAdvanceCount(advanceCountEvents, options, distanceToNextLocation);
            }
            else
            {
                countEvents = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.LLC);
                countEventsTimeSpaceResult = CalculateTimeSpaceResult(countEvents, options, distanceToNextLocation);

                stopBarPresenceEvents = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.SBP);
                stopBarPresenceEventsTimeSpaceResult = CalculateTimeSpaceResult(stopBarPresenceEvents, options, distanceToNextLocation, true);

                advanceCountEvents = GetDetectionEvents(phaseDetail.Approach, options, controllerEventLogs, DetectionTypes.AC);
                advanceCountEventsTimeSpaceResult = CalculateTimeSpaceResultForAdvanceCount(advanceCountEvents, options, distanceToNextLocation);

                greenTimeEventsResult = GetGreenTimeEvents(phaseDetail, controllerEventLogs, options, distanceToNextLocation, phaseDetail.Approach.Mph ?? 0);
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
                cycleAllEvents,
                countEventsTimeSpaceResult,
                advanceCountEventsTimeSpaceResult,
                stopBarPresenceEventsTimeSpaceResult,
                greenTimeEventsResult
                );
            return timeSpaceDiagramResult;
        }

        private List<TimeSpaceEventBase> GetGreenTimeEvents(PhaseDetail phaseDetail,
            List<ControllerEventLog> controllerEventLogs,
            TimeSpaceDiagramOption options,
            double distanceToNextLocation,
            int speedLimit)
        {
            List<int> cycleGreenStartEndCodes = new List<int>() { 1, 8 };
            var events = new List<CycleEventsDto>();
            var greenTimeEvents = new List<TimeSpaceEventBase>();
            if (controllerEventLogs.Any())
            {
                var tempEvents = controllerEventLogs.Where(c => cycleGreenStartEndCodes.Contains(c.EventCode) && c.EventParam == phaseDetail.PhaseNumber)
                    .Select(e => new CycleEventsDto(e.Timestamp, e.EventCode)).ToList();
                events.AddRange(tempEvents.Where(e => e.Start >= options.Start
                                                        && e.Start <= options.End));
                var firstEvent = tempEvents.Where(e => e.Start < options.Start).OrderByDescending(e => e.Start).FirstOrDefault();
                if (firstEvent != null)
                {
                    firstEvent.Start = options.Start;
                    events.Insert(0, firstEvent);
                }
            }
            var uniqueEvents = new HashSet<CycleEventsDto>(events);
            foreach (var gEvent in uniqueEvents) {
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
            TimeSpaceDiagramOption options,
            double distanceToNextLocation,
            bool includeDetectorOff = false
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
                DateTime currentDetectorOn = detectorEvent.DetectorOn;
                DateTime currentDetectorOff = detectorEvent.DetectorOff;
                GetArrivalTime(distanceToNextLocation, speedLimit, detectorEvent.DetectorOn, out _, out DateTime arrivalTimeOn);

                TimeSpaceEventBase resultOn = new TimeSpaceEventBase(
                    currentDetectorOn,
                    arrivalTimeOn,
                    includeDetectorOff ? (bool?)true : null);

                results.Add(resultOn);

                if (includeDetectorOff)
                {
                    GetArrivalTime(distanceToNextLocation, speedLimit, detectorEvent.DetectorOff, out _, out DateTime arrivalTimeOff);
                    TimeSpaceEventBase resultOff = new TimeSpaceEventBase(currentDetectorOff, arrivalTimeOff, false);
                    results.Add(resultOff);
                }
            }

            return results;
        }

        private static double GetSpeedInFeetPerSecond(double speedLimit)
        {
            return speedLimit * FeetPerMile / SecondsInHour;
        }

        private List<TimeSpaceEventBase> CalculateTimeSpaceResultForAdvanceCount(
            List<TimeSpaceDetectorEventDto> events,
            TimeSpaceDiagramOption options,
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
            TimeSpaceDiagramOption options)
        {

            List<int> cycleEventCodes = GetCycleCodes(phaseDetail.UseOverlap);
            var overlapLabel = phaseDetail.UseOverlap == true ? "Overlap" : "";
            string keyLabel = $"Cycles Intervals {phaseDetail.PhaseNumber} {overlapLabel}";
            var events = new List<CycleEventsDto>();
            if (controllerEventLogs.Any())
            {
                var tempEvents = controllerEventLogs.Where(c => cycleEventCodes.Contains(c.EventCode) && c.EventParam == phaseDetail.PhaseNumber)
                    .Select(e => new CycleEventsDto(e.Timestamp, e.EventCode)).ToList();
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
            TimeSpaceDiagramOption options,
            List<ControllerEventLog> controllerEventLogs,
            DetectionTypes detectionType
            )
        {
            var DetEvents = new List<TimeSpaceDetectorEventDto>();
            var localSortedDetectors = approach.Detectors.Where(d => d.DetectionTypes.Any(d => d.Id == detectionType))
                .OrderByDescending(d => d.MovementType.GetDisplayAttribute()?.Order)
                .ThenByDescending(l => l.LaneNumber).ToList();
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
                        for (var i = 0; i < filteredEvents.Count; i += 2)
                        {
                            if (i + 1 == filteredEvents.Count)
                            {
                                detectorEvents.Add(new TimeSpaceDetectorEventDto(filteredEvents[i].Timestamp,
                                    filteredEvents[i].Timestamp,
                                    approach.Mph ?? 0,
                                    detector.DistanceFromStopBar ?? 0));
                            }
                            else
                            {
                                detectorEvents.Add(new TimeSpaceDetectorEventDto(filteredEvents[i].Timestamp,
                                    filteredEvents[i + 1].Timestamp,
                                    approach.Mph ?? 0,
                                    detector.DistanceFromStopBar ?? 0));
                            }
                        }
                        DetEvents.AddRange(detectorEvents);
                    }

                    else if (filteredEvents.Count == 0 && options.ShowAllLanesInfo)
                    {
                        var e = new TimeSpaceDetectorEventDto(options.Start.AddSeconds(-10),
                            options.Start.AddSeconds(-9),
                            approach.Mph ?? 0,
                            detector.DistanceFromStopBar ?? 0);

                        var list = new List<TimeSpaceDetectorEventDto>
                        {
                            e
                        };
                        DetEvents.AddRange(list);
                    }
                }
            }
            return DetEvents;
        }
    }
}

