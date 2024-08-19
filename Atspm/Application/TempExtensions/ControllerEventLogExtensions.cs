﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.TempExtensions/ControllerEventLogExtensions.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;

namespace Utah.Udot.Atspm.TempExtensions
{
    //public static class ControllerEventLogExtensions
    //{
    //    public static IReadOnlyList<ControllerEventLog> GetPlanEvents(
    //       this IEnumerable<ControllerEventLog> events,
    //       DateTime start,
    //       DateTime end)
    //    {
    //        var planEvents = events.Where(e => e.EventCode == 131)
    //            .OrderBy(e => e.Timestamp)
    //            .ToList();

    //        var uniqueEvents = new List<IndianaEvent>();
    //        // Iterate over the original events list
    //        for (int i = 0; i < planEvents.Count; i++)
    //        {
    //            // Check if the current event has the same EventParam value as the previous event
    //            if (i == 0 || planEvents[i].EventParam != planEvents[i - 1].EventParam)
    //            {
    //                // If not, add it to the uniqueEvents list
    //                uniqueEvents.Add(planEvents[i]);
    //            }
    //        }
    //        UpdateEventsBeforeDateForPlans(uniqueEvents, start);
    //        UpdateEventsAfterDateForPlans(uniqueEvents, end);
    //        // Return the uniqueEvents list
    //        return uniqueEvents; ;
    //    }

    //    public static void UpdateEventsAfterDateForPlans(List<ControllerEventLog> events, DateTime date)
    //    {
    //        // Find the first event that occurred after the specified date
    //        var index = events.FindIndex(e => e.Timestamp > date);

    //        if (index >= 0)
    //        {
    //            // If an event was found, remove all events after it
    //            events.RemoveRange(index + 1, events.Count - (index + 1));

    //            // Change the timestamp of the found event to match the specified date
    //            events[index].Timestamp = date;
    //        }
    //        else
    //        {
    //            // If no event was found, create a new event with event param 0, event code 131, and the specified date as the timestamp
    //            var newEvent = new ControllerEventLog
    //            {
    //                LocationIdentifier = "0",
    //                Timestamp = date,
    //                EventCode = 131,
    //                EventParam = 0
    //            };

    //            // Add the new event to the end of the list
    //            events.Add(newEvent);
    //        }
    //    }


    //    public static void UpdateEventsBeforeDateForPlans(List<ControllerEventLog> events, DateTime date)
    //    {
    //        // Find the first event that occurred before the specified date
    //        var index = events.FindIndex(e => e.Timestamp < date);

    //        if (index >= 0)
    //        {
    //            // If an event was found, change its timestamp to match the specified date
    //            events[index].Timestamp = date;

    //            // Remove all events before the found event
    //            events.RemoveRange(0, index);
    //        }
    //        else
    //        {
    //            // If no event was found, create a new event with event param 0, event code 131, and the specified date as the timestamp
    //            var newEvent = new ControllerEventLog
    //            {
    //                SignalIdentifier = "0",
    //                Timestamp = date,
    //                EventCode = 131,
    //                EventParam = 0
    //            };

    //            // Add the new event to the beginning of the list
    //            events.Insert(0, newEvent);
    //        }
    //    }

    //    public static IReadOnlyList<ControllerEventLog> GetDetectorEvents(
    //        this IEnumerable<ControllerEventLog> events,
    //        int metricTypeId,
    //        Approach approach,
    //        DateTime start,
    //        DateTime end,
    //        bool detectorOn,
    //        bool detectorOff,
    //        DetectionType detectionType)
    //    {
    //        var eventCodes = new List<int>();
    //        if (detectorOn)
    //            eventCodes.Add(82);
    //        if (detectorOff)
    //            eventCodes.Add(81);
    //        if (!detectorOn && !detectorOff)
    //            throw new ArgumentException("At least one detector event code must be true (detectorOn or detectorOff");
    //        var detectorsForMetric = approach.GetDetectorsForMetricType(metricTypeId);
    //        if (detectionType != null)
    //            detectorsForMetric = detectorsForMetric.Where(d => d.DetectionTypes.Select(d => d.Id).Contains(detectionType.Id)).ToList();
    //        if (!detectorsForMetric.Any())
    //            return null;
    //        var detectorEvents = new List<ControllerEventLog>();
    //        foreach (var d in detectorsForMetric)
    //            detectorEvents.AddRange(events.GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(
    //                start,
    //                end,
    //                eventCodes,
    //                d.DetectorChannel,
    //                d.GetOffset(),
    //                d.LatencyCorrection));
    //        return detectorEvents.OrderBy(e => e.Timestamp).ToList();
    //    }

    //    public static IReadOnlyList<ControllerEventLog> GetDetectorEvents(
    //        this IEnumerable<ControllerEventLog> events,
    //        int metricTypeId,
    //        Approach approach,
    //        DateTime start,
    //        DateTime end,
    //        bool detectorOn,
    //        bool detectorOff)
    //    {
    //        var eventCodes = new List<int>();
    //        if (detectorOn)
    //            eventCodes.Add(82);
    //        if (detectorOff)
    //            eventCodes.Add(81);
    //        if (!detectorOn && !detectorOff)
    //            throw new ArgumentException("At least one detector event code must be true (detectorOn or detectorOff");
    //        var detectorsForMetric = approach.GetDetectorsForMetricType(metricTypeId);
    //        if (!detectorsForMetric.Any())
    //            return null;
    //        var detectorEvents = new List<ControllerEventLog>();
    //        foreach (var d in detectorsForMetric)
    //            detectorEvents.AddRange(events.GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(
    //                start,
    //                end,
    //                eventCodes,
    //                d.DetectorChannel,
    //                d.GetOffset(),
    //                d.LatencyCorrection));
    //        return detectorEvents.OrderBy(e => e.Timestamp).ToList();
    //    }

    //    public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(
    //        this IEnumerable<ControllerEventLog> events,
    //        DateTime startTime,
    //        DateTime endTime,
    //        IEnumerable<int> eventCodes,
    //        int param,
    //        double offset,
    //        double latencyCorrection)
    //    {
    //        var result = events.Where(e =>
    //        eventCodes.Contains(e.EventCode)
    //        && e.EventParam == param
    //        && e.Timestamp >= startTime
    //        && e.Timestamp < endTime);

    //        foreach (var item in result)
    //        {
    //            item.Timestamp = item.Timestamp.AddMilliseconds(offset);
    //            item.Timestamp = item.Timestamp.AddSeconds(0 - latencyCorrection);
    //        }

    //        return result.ToList();
    //    }

    //    public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodes(
    //        this IEnumerable<ControllerEventLog> events,
    //        DateTime startTime,
    //        DateTime endTime,
    //        IEnumerable<int> eventCodes,
    //        int param)
    //    {
    //        var result = events.Where(e =>
    //        eventCodes.Contains(e.EventCode)
    //        && e.EventParam == param
    //        && e.Timestamp >= startTime
    //        && e.Timestamp < endTime);

    //        return result.ToList();
    //    }

    //    public static IReadOnlyList<ControllerEventLog> GetEventsByEventCodes(
    //        this IEnumerable<ControllerEventLog> events,
    //        DateTime startTime,
    //        DateTime endTime,
    //        IEnumerable<IndianaEnumerations> eventCodes)
    //    {
    //        var result = events.Where(e =>
    //        eventCodes.Contains(e.EventCode)
    //        && e.Timestamp >= startTime
    //        && e.Timestamp < endTime);

    //        return result.ToList();
    //    }

    //    public static IReadOnlyList<ControllerEventLog> GetCycleEventsWithTimeExtension(
    //       this IEnumerable<ControllerEventLog> events,
    //       int phaseNumber,
    //       bool useOverlap,
    //       DateTime start,
    //       DateTime end)
    //    {
    //        return events.GetEventsByEventCodes(
    //            start.AddSeconds(-900),
    //            end.AddSeconds(900),
    //            GetCycleEventCodes(useOverlap),
    //            phaseNumber).OrderBy(e => e.Timestamp).ToList();
    //    }

    //    public static List<int> GetCycleEventCodes(bool useOvelap)
    //    {
    //        return useOvelap
    //            ? new List<int> { 61, 63, 64, 66 }
    //            : new List<int> { 1, 8, 9 };
    //    }

    //    public static IReadOnlyList<ControllerEventLog> GetPedEvents(
    //        this IEnumerable<ControllerEventLog> events,
    //        DateTime startTime,
    //        DateTime endTime,
    //        Approach approach)
    //    {
    //        return events.GetEvents(
    //            approach.Location.LocationIdentifier,
    //            startTime,
    //            endTime,
    //            approach.GetPedDetectorsFromApproach(),
    //            approach.GetPedestrianCycleEventCodes());
    //    }

    //    public static IReadOnlyList<ControllerEventLog> GetEvents(
    //        this IEnumerable<ControllerEventLog> events,
    //        string LocationIdentifier,
    //        DateTime startTime,
    //        DateTime endTime,
    //        IEnumerable<int> eventParameters,
    //        IEnumerable<int> eventCodes)
    //    {
    //        var result = events
    //            .Where(e => e.SignalIdentifier == LocationIdentifier
    //            && e.Timestamp >= startTime
    //            && e.Timestamp < endTime
    //            && eventCodes.Contains(e.EventCode)
    //            && eventParameters.Contains(e.EventParam)
    //            )
    //            .OrderBy(o => o.Timestamp)
    //            .ToList();

    //        return result;
    //    }
    //}

    public static class IndianaEventExtensions
    {
        public static IReadOnlyList<IndianaEvent> GetPlanEvents(
           this IEnumerable<IndianaEvent> events,
           DateTime start,
           DateTime end)
        {
            var planEvents = events.Where(e => e.EventCode == 131)
                .OrderBy(e => e.Timestamp)
                .ToList();

            var uniqueEvents = new List<IndianaEvent>();
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

        public static void UpdateEventsAfterDateForPlans(List<IndianaEvent> events, DateTime date)
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
                var newEvent = new IndianaEvent
                {
                    LocationIdentifier = "0",
                    Timestamp = date,
                    EventCode = 131,
                    EventParam = 0
                };

                // Add the new event to the end of the list
                events.Add(newEvent);
            }
        }


        public static void UpdateEventsBeforeDateForPlans(List<IndianaEvent> events, DateTime date)
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
                var newEvent = new IndianaEvent
                {
                    LocationIdentifier = "0",
                    Timestamp = date,
                    EventCode = 131,
                    EventParam = 0
                };

                // Add the new event to the beginning of the list
                events.Insert(0, newEvent);
            }
        }

        public static IReadOnlyList<IndianaEvent> GetDetectorEvents(
            this IEnumerable<IndianaEvent> events,
            int metricTypeId,
            Approach approach,
            DateTime start,
            DateTime end,
            bool detectorOn,
            bool detectorOff,
            DetectionType detectionType)
        {
            var eventCodes = new List<short>();
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
            var detectorEvents = new List<IndianaEvent>();
            foreach (var d in detectorsForMetric)
                detectorEvents.AddRange(events.GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(
                    start,
                    end,
                    eventCodes,
                    d.DetectorChannel,
                    d.GetOffset(),
                    d.LatencyCorrection));
            return detectorEvents.OrderBy(e => e.Timestamp).ToList();
        }

        public static IReadOnlyList<IndianaEvent> GetDetectorEvents(
            this IEnumerable<IndianaEvent> events,
            int metricTypeId,
            Approach approach,
            DateTime start,
            DateTime end,
            bool detectorOn,
            bool detectorOff)
        {
            var eventCodes = new List<short>();
            if (detectorOn)
                eventCodes.Add(82);
            if (detectorOff)
                eventCodes.Add(81);
            if (!detectorOn && !detectorOff)
                throw new ArgumentException("At least one detector event code must be true (detectorOn or detectorOff");
            var detectorsForMetric = approach.GetDetectorsForMetricType(metricTypeId);
            if (!detectorsForMetric.Any())
                return null;
            var detectorEvents = new List<IndianaEvent>();
            foreach (var d in detectorsForMetric)
                detectorEvents.AddRange(events.GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(
                    start,
                    end,
                    eventCodes,
                    d.DetectorChannel,
                    d.GetOffset(),
                    d.LatencyCorrection));
            return detectorEvents.OrderBy(e => e.Timestamp).ToList();
        }

        public static IReadOnlyList<IndianaEvent> GetEventsByEventCodesParamWithOffsetAndLatencyCorrection(
            this IEnumerable<IndianaEvent> events,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<short> eventCodes,
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

        public static IReadOnlyList<IndianaEvent> GetEventsByEventCodes(
            this IEnumerable<IndianaEvent> events,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<short> eventCodes,
            int param)
        {
            var result = events.Where(e =>
            eventCodes.Contains(e.EventCode)
            && e.EventParam == param
            && e.Timestamp >= startTime
            && e.Timestamp < endTime);

            return result.ToList();
        }

        public static IReadOnlyList<IndianaEvent> GetEventsByEventCodes(
            this IEnumerable<IndianaEvent> events,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<short> eventCodes)
        {
            var result = events.Where(e =>
            eventCodes.Contains(e.EventCode)
            && e.Timestamp >= startTime
            && e.Timestamp < endTime);

            return result.ToList();
        }

        public static IReadOnlyList<IndianaEvent> GetCycleEventsWithTimeExtension(
           this IEnumerable<IndianaEvent> events,
           int phaseNumber,
           bool useOverlap,
           DateTime start,
           DateTime end)
        {
            return events.GetEventsByEventCodes(
                start.AddSeconds(-900),
                end.AddSeconds(900),
                GetCycleEventCodes(useOverlap),
                phaseNumber).OrderBy(e => e.Timestamp).ToList();
        }

        public static List<short> GetCycleEventCodes(bool useOvelap)
        {
            return useOvelap
                ? new List<short>
                {
                    61,
                    63,
                    64,
                    66
                }
                : new List<short>
                {
                    1,
                    8,
                    9
                };
        }

        public static IReadOnlyList<IndianaEvent> GetPedEvents(
            this IEnumerable<IndianaEvent> events,
            DateTime startTime,
            DateTime endTime,
            Approach approach)
        {
            return events.GetEvents(
                approach.Location.LocationIdentifier,
                startTime,
                endTime,
                approach.GetPedDetectorsFromApproach(),
                approach.GetPedestrianCycleEventCodes());
        }

        public static IReadOnlyList<IndianaEvent> GetEvents(
            this IEnumerable<IndianaEvent> events,
            string locationIdentifier,
            DateTime startTime,
            DateTime endTime,
            IEnumerable<int> eventParameters,
            IEnumerable<short> eventCodes)
        {
            var result = events
                .Where(e => e.LocationIdentifier == locationIdentifier
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
