using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Extensions
{
    public static class ControllerEventLogExtensions
    {
        public static IReadOnlyList<ControllerEventLog> GetPlanEvents(
           this IEnumerable<ControllerEventLog> events,
           DateTime start,
           DateTime end)
        {
            var planEvents = events.Where(e => e.EventCode == 131)
                .OrderBy(e => e.Timestamp)
                .ToList();

            var uniqueEvents = new List<ControllerEventLog>();
            // Iterate over the original events list
            for (int i = 0; i < planEvents.Count; i++)
            {
                // Check if the current event has the same EventParam value as the previous event
                if (i == 0 || planEvents[i].EventParam != planEvents[i - 1].EventParam)
                {
                    // If not, add it to the uniqueEvents list
                    uniqueEvents.Add(planEvents[i]);
                }
            }
            UpdateEventsBeforeDateForPlans(uniqueEvents, start);
            UpdateEventsAfterDateForPlans(uniqueEvents, end);
            // Return the uniqueEvents list
            return uniqueEvents; ;
        }

        public static void UpdateEventsAfterDateForPlans(List<ControllerEventLog> events, DateTime date)
        {
            // Find the first event that occurred after the specified date
            var index = events.FindIndex(e => e.Timestamp > date);

            if (index >= 0)
            {
                // If an event was found, remove all events after it
                events.RemoveRange(index + 1, events.Count - (index + 1));

                // Change the timestamp of the found event to match the specified date
                events[index].Timestamp = date;
            }
            else
            {
                // If no event was found, create a new event with event param 0, event code 131, and the specified date as the timestamp
                var newEvent = new ControllerEventLog
                {
                    SignalIdentifier = "0",
                    Timestamp = date,
                    EventCode = 131,
                    EventParam = 0
                };

                // Add the new event to the end of the list
                events.Add(newEvent);
            }
        }


        public static void UpdateEventsBeforeDateForPlans(List<ControllerEventLog> events, DateTime date)
        {
            // Find the first event that occurred before the specified date
            var index = events.FindIndex(e => e.Timestamp < date);

            if (index >= 0)
            {
                // If an event was found, change its timestamp to match the specified date
                events[index].Timestamp = date;

                // Remove all events before the found event
                events.RemoveRange(0, index);
            }
            else
            {
                // If no event was found, create a new event with event param 0, event code 131, and the specified date as the timestamp
                var newEvent = new ControllerEventLog
                {
                    SignalIdentifier = "0",
                    Timestamp = date,
                    EventCode = 131,
                    EventParam = 0
                };

                // Add the new event to the beginning of the list
                events.Insert(0, newEvent);
            }
        }

        public static IReadOnlyList<ControllerEventLog> GetDetectorEvents(
            this IEnumerable<ControllerEventLog> events,
            int metricTypeId,
            Approach approach,
            DateTime start,
            DateTime end,
            bool detectorOn,
            bool detectorOff,
            DetectionType detectionType)
        {
            var eventCodes = new List<int>();
            if (detectorOn)
                eventCodes.Add(82);
            if (detectorOff)
                eventCodes.Add(81);
            if (!detectorOn && !detectorOff)
                throw new ArgumentException("At least one detector event code must be true (detectorOn or detectorOff");
            var detectorsForMetric = approach.GetDetectorsForMetricType(metricTypeId);
            if (detectionType != null)
                detectorsForMetric = detectorsForMetric.Where(d => d.DetectionTypes.Select(d => d.Id).Contains(detectionType.Id)).ToList();
            if (!detectorsForMetric.Any())
                return null;
            var detectorEvents = new List<ControllerEventLog>();
            foreach (var d in detectorsForMetric)
                detectorEvents.AddRange(events.GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(
                    start,
                    end,
                    eventCodes,
                    d.DetChannel,
                    d.GetOffset(),
                    d.LatencyCorrection));
            return detectorEvents.OrderBy(e => e.Timestamp).ToList();
        }

        public static IReadOnlyList<ControllerEventLog> GetDetectorEvents(
            this IEnumerable<ControllerEventLog> events,
            int metricTypeId,
            Approach approach,
            DateTime start,
            DateTime end,
            bool detectorOn,
            bool detectorOff)
        {
            var eventCodes = new List<int>();
            if (detectorOn)
                eventCodes.Add(82);
            if (detectorOff)
                eventCodes.Add(81);
            if (!detectorOn && !detectorOff)
                throw new ArgumentException("At least one detector event code must be true (detectorOn or detectorOff");
            var detectorsForMetric = approach.GetDetectorsForMetricType(metricTypeId);
            if (!detectorsForMetric.Any())
                return null;
            var detectorEvents = new List<ControllerEventLog>();
            foreach (var d in detectorsForMetric)
                detectorEvents.AddRange(events.GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(
                    start,
                    end,
                    eventCodes,
                    d.DetChannel,
                    d.GetOffset(),
                    d.LatencyCorrection));
            return detectorEvents.OrderBy(e => e.Timestamp).ToList();
        }

        public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(
            this IEnumerable<ControllerEventLog> events,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<int> eventCodes,
            int param,
            double offset,
            double latencyCorrection)
        {
            var result = events.Where(e =>
            eventCodes.Contains(e.EventCode)
            && e.EventParam == param
            && e.Timestamp >= startTime
            && e.Timestamp < endTime);

            foreach (var item in result)
            {
                item.Timestamp = item.Timestamp.AddMilliseconds(offset);
                item.Timestamp = item.Timestamp.AddSeconds(0 - latencyCorrection);
            }

            return result.ToList();
        }

        public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParam(
            this IEnumerable<ControllerEventLog> events,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<int> eventCodes,
            int param)
        {
            var result = events.Where(e =>
            eventCodes.Contains(e.EventCode)
            && e.EventParam == param
            && e.Timestamp >= startTime
            && e.Timestamp < endTime);

            return result.ToList();
        }

        public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodes(
            this IEnumerable<ControllerEventLog> events,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<int> eventCodes)
        {
            var result = events.Where(e =>
            eventCodes.Contains(e.EventCode)
            && e.Timestamp >= startTime
            && e.Timestamp < endTime);

            return result.ToList();
        }

        public static IReadOnlyList<ControllerEventLog> GetCycleEventsWithTimeExtension(
           this IEnumerable<ControllerEventLog> events,
           Approach approach,
           bool getPermissivePhase,
           DateTime start,
           DateTime end)
        {
            return events.GetEventsByEventCodesParam(
                start.AddSeconds(-900),
                end.AddSeconds(900),
                approach.GetCycleEventCodes(getPermissivePhase),
                getPermissivePhase ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber).OrderBy(e => e.Timestamp).ToList();
        }

        public static IReadOnlyList<ControllerEventLog> GetPedEvents(
            this IEnumerable<ControllerEventLog> events,
            DateTime startTime,
            DateTime endTime,
            Approach approach)
        {
            return events.GetEvents(
                approach.Signal.SignalIdentifier,
                startTime,
                endTime,
                approach.GetPedDetectorsFromApproach(),
                approach.GetPedestrianCycleEventCodes());
        }

        public static IReadOnlyList<ControllerEventLog> GetEvents(
            this IEnumerable<ControllerEventLog> events,
            string signalIdentifier,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<int> eventParameters,
            IEnumerable<int> eventCodes)
        {
            var result = events
                .Where(e => e.SignalIdentifier == signalIdentifier
                && e.Timestamp >= startTime
                && e.Timestamp < endTime
                && eventCodes.Contains(e.EventCode)
                && eventParameters.Contains(e.EventParam)
                )
                .OrderBy(o => o.Timestamp)
                .ToList();

            return result;
        }
    }
}
