using Utah.Udot.Atspm.Business.TimingAndActuation;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Business.Common
{

    public class DetectionService
    {

        public DetectionService()
        {
        }

        public List<DetectorEventDto> GetDetectionEvents(
            Approach approach,
            DateTime start,
            DateTime end,
            List<IndianaEvent> controllerEventLogs,
            DetectionTypes detectionType
            )
        {
            var DetEvents = new List<DetectorEventDto>();
            var localSortedDetectors = approach.Detectors
                .OrderByDescending(d => d.MovementType.GetDisplayAttribute()?.Order)
                .ThenByDescending(l => l.LaneNumber).ToList();
            var detectorActivationCodes = new List<short> { 81, 82 };
            foreach (var detector in localSortedDetectors)
            {
                if (detector.DetectionTypes.Any(d => d.Id == detectionType))
                {
                    var filteredEvents = controllerEventLogs.Where(c => detectorActivationCodes.Contains(c.EventCode)
                                                                        && c.EventParam == detector.DetectorChannel
                                                                        && c.Timestamp >= start
                                                                        && c.Timestamp <= end).ToList();
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
                            if (i == 0 && filteredEvents[i].EventCode == 81)
                            {
                                detectorEvents.Add(new DetectorEventBase(null, filteredEvents[i].Timestamp));
                            }
                            else if (i + 1 == filteredEvents.Count && filteredEvents[i].EventCode == 81)
                            {
                                detectorEvents.Add(new DetectorEventBase(null, filteredEvents[i].Timestamp));
                            }
                            else if (i + 1 == filteredEvents.Count && filteredEvents[i].EventCode == 82)
                            {
                                detectorEvents.Add(new DetectorEventBase(filteredEvents[i].Timestamp, null));
                            }
                            else if (filteredEvents[i].EventCode == 82 && filteredEvents[i + 1].EventCode == 81)
                            {
                                detectorEvents.Add(new DetectorEventBase(filteredEvents[i].Timestamp, filteredEvents[i + 1].Timestamp));
                                i++;
                            }
                            else if (filteredEvents[i].EventCode == 81 && filteredEvents[i + 1].EventCode == 81)
                            {
                                detectorEvents.Add(new DetectorEventBase(null, filteredEvents[i + 1].Timestamp));
                            }
                            else if (filteredEvents[i].EventCode == 82 && filteredEvents[i + 1].EventCode == 82)
                            {
                                detectorEvents.Add(new DetectorEventBase(filteredEvents[i + 1].Timestamp, null));
                            }
                        }
                        DetEvents.Add(new DetectorEventDto(lableName, detectorEvents));
                    }

                    else if (filteredEvents.Count == 0)
                    {
                        var e = new DetectorEventBase(start.AddSeconds(-10), start.AddSeconds(-9));

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
    }
}
