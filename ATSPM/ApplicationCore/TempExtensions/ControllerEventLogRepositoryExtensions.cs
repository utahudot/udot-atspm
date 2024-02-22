using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.TempExtensions
{
    public static class ControllerEventLogRepositoryExtensions
    {
        public static IReadOnlyList<ControllerEventLog> GetDetectorEvents(
            this IControllerEventLogRepository repo,
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
            var events = new List<ControllerEventLog>();
            var detectorsForMetric = approach.GetDetectorsForMetricType(metricTypeId);
            if (!detectorsForMetric.Any())
                return new List<ControllerEventLog>();
            foreach (var d in detectorsForMetric)
                events.AddRange(repo.GetEventsByEventCodesParam(
                    approach.Location.LocationIdentifier,
                    start,
                    end,
                    eventCodes,
                    d.DetectorChannel,
                    d.GetOffset(),
                    d.LatencyCorrection));
            return events.OrderBy(e => e.Timestamp).ToList();
        }

        public static IReadOnlyList<ControllerEventLog> GetPlanEvents(
           this IControllerEventLogRepository repo,
           string locationId,
           DateTime start,
           DateTime end)
        {
            var events = repo.GetLocationEventsByEventCode(
                locationId,
                start.AddHours(-12),
                end.AddHours(12),
                131)
                .OrderBy(e => e.Timestamp)
                .ToList();

            var uniqueEvents = new List<ControllerEventLog>();
            // Iterate over the original events list
            for (int i = 0; i < events.Count; i++)
            {
                // Check if the current event has the same EventParam value as the previous event
                if (i == 0 || events[i].EventParam != events[i - 1].EventParam)
                {
                    // If not, add it to the uniqueEvents list
                    uniqueEvents.Add(events[i]);
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
    }
}
