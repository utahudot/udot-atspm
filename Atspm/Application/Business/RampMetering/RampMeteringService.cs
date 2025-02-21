#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.RampMetering/RampMeteringService.cs
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

using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.TimeSpaceDiagram;
using Utah.Udot.Atspm.Business.TimingAndActuation;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Business.RampMetering
{
    public class RampMeteringService
    {
        private readonly double speedMultiplier = 0.621371;
        public RampMeteringService() { }

        public RampMeteringResult GetChartData(Location location, RampMeteringOptions options, IReadOnlyList<IndianaEvent> events)
        {
            var activeRateCodes = new List<int>() { 1058, 1059, 1060, 1061 };
            var baseRateCodes = new List<int>() { 1042, 1043, 1044, 1045 };
            var startupAndShutdownCodes = new List<int>() { 1004, 1014 };
            var queueCodes = new List<RampMeteringQueueDto>()
            {
                new RampMeteringQueueDto { Lane = 1, OnCode = 1171, OffCode = 1170 },
                new RampMeteringQueueDto { Lane = 2, OnCode = 1173, OffCode = 1172 },
                new RampMeteringQueueDto { Lane = 3, OnCode = 1175, OffCode = 1174 }
            };
            var queue1Codes = new List<int>() { 1170, 1171 };
            var queue2Codes = new List<int>() { 1172, 1173 };
            var queue3Codes = new List<int>() { 1174, 1175 };
            //codes for active rate 1058-1073
            //codes for base rate 1042-1057
            // codes for start up warning 1004
            // codes for shut down warning 1014
            // codes for L1 Queue On 1171
            // codes for L2 Queue On 1173
            // codes for L3 Queue On 1175
            // codes for L1 Queue Off 1170
            // codes for L2 Queue Off 1172
            // codes for L3 Queue Off 1174

            var activeRateEvents = events.Where(e => activeRateCodes.Contains(e.EventCode) && e.Timestamp >= options.Start && e.Timestamp <= options.End);
            var baseRateEvents = events.Where(e => baseRateCodes.Contains(e.EventCode) && e.Timestamp >= options.Start && e.Timestamp <= options.End);

            var mainlineAvgSpeedEvents = events.Where(e => e.EventCode == 1373 && e.Timestamp >= options.Start && e.Timestamp <= options.End);
            var mainlineAvgOccurrenceEvents = events.Where(e => e.EventCode == 1372 && e.Timestamp >= options.Start && e.Timestamp <= options.End);
            var mainlineAvgFlowEvents = events.Where(e => e.EventCode == 1371 && e.Timestamp >= options.Start && e.Timestamp <= options.End);
            var startUpAndShutdownWarningEvents = events.Where(e => startupAndShutdownCodes.Contains(e.EventCode) && e.Timestamp >= options.Start && e.Timestamp <= options.End);

            var (startup, shutdown) = GetStartUpAndShutdownEvents(startUpAndShutdownWarningEvents, options);

            var mainlineAvgSpeedsList = mainlineAvgSpeedEvents.Select(e => new DataPointForDouble(e.Timestamp, (e.EventParam * speedMultiplier))).ToList();
            var mainlineAvgOccurrenceList = mainlineAvgOccurrenceEvents.Select(e => new DataPointForDouble(e.Timestamp, (e.EventParam / 10))).ToList();
            var mainlineAvgFlowList = mainlineAvgFlowEvents.Select(e => new DataPointForDouble(e.Timestamp, e.EventParam)).ToList();

            var lanesActiveRateList = GetDescriptionWithDataPoints(activeRateEvents, options);
            var lanesBaseRateList = GetDescriptionWithDataPoints(baseRateEvents, options);
            var queueList = GetQueueEvents(events, options, queueCodes);

            return new RampMeteringResult(location.LocationIdentifier, options.Start, options.End)
            {
                MainlineAvgFlow = mainlineAvgFlowList,
                MainlineAvgOcc = mainlineAvgOccurrenceList,
                MainlineAvgSpeed = mainlineAvgSpeedsList,
                StartUpWarning = startup,
                ShutdownWarning = shutdown,
                LanesActiveRate = lanesActiveRateList,
                LanesBaseRate = lanesBaseRateList,
                LanesQueueOnAndOffEvents = queueList,
            };
        }

        private static List<DataPointForDetectorEvent> GetQueueEvents(IEnumerable<IndianaEvent> events, RampMeteringOptions options, List<RampMeteringQueueDto> queueCodes)
        {
            var dataPoints = new List<DataPointForDetectorEvent>();
            var filteredEvents = events.Where(e => e.Timestamp >= options.Start && e.Timestamp <= options.End);

            foreach (var lane in queueCodes)
            {
                var laneEvents = filteredEvents
                    .Where(e => e.EventCode == lane.OnCode || e.EventCode == lane.OffCode)
                    .OrderBy(e => e.Timestamp)
                    .ToList();

                // Process "on" and "off" events for this lane
                for (int i = 0; i < laneEvents.Count; i++)
                {
                    var currentEvent = laneEvents[i];

                    if (currentEvent.EventCode == lane.OnCode)
                    {
                        var offEvent = laneEvents.Skip(i + 1)
                            .FirstOrDefault(e => e.EventCode == lane.OffCode && e.Timestamp > currentEvent.Timestamp);

                        if (offEvent != null)
                        {
                            var dataPoint = new DataPointForDetectorEvent(
                                value: lane.Lane,
                                start: currentEvent.Timestamp,
                                stop: offEvent.Timestamp
                            );

                            dataPoints.Add(dataPoint);
                        }
                    }
                }
            }

            return dataPoints;
        }

        private static List<DescriptionWithDataPoints> GetDescriptionWithDataPoints(IEnumerable<IndianaEvent> events, RampMeteringOptions options)
        {
            var descriptWithDataPointsEvents = new List<DescriptionWithDataPoints>();

            var groupedEvents = events.GroupBy(e => e.EventCode);
            int laneNumber = 1;

            foreach (var group in groupedEvents)
            {
                var codeEvents = group.Select(e => new DataPointForDouble(e.Timestamp, e.EventParam)).ToList();
                descriptWithDataPointsEvents.Add(new DescriptionWithDataPoints()
                {
                    Description = laneNumber.ToString(),
                    Value = codeEvents
                });
                laneNumber++;
            }

            return options.CombineLanes ? descriptWithDataPointsEvents.Where(d => d.Description == "1").ToList() : descriptWithDataPointsEvents;
        }

        private static (List<TimeSpaceEventBase>, List<TimeSpaceEventBase>) GetStartUpAndShutdownEvents(IEnumerable<IndianaEvent> events, RampMeteringOptions options)
        {
            var startUpList = new List<TimeSpaceEventBase>();
            var shutDownList = new List<TimeSpaceEventBase>();

            // Filter events within the time range
            var filteredEvents = events
                .Where(e => e.Timestamp >= options.Start && e.Timestamp <= options.End)
                .ToList();

            // Find the closest events before the startTime and after the endTime with EventCode 1004 or 1014
            var closestBeforeStart = events
                .Where(e => e.Timestamp < options.Start && (e.EventCode == 1004 || e.EventCode == 1014))
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefault();

            var closestAfterEnd = events
                .Where(e => e.Timestamp > options.End && (e.EventCode == 1004 || e.EventCode == 1014))
                .OrderBy(e => e.Timestamp)
                .FirstOrDefault();

            // Adjust closest events' timestamps if they exist
            if (closestBeforeStart != null)
            {
                closestBeforeStart = new IndianaEvent
                {
                    EventCode = closestBeforeStart.EventCode,
                    EventParam = closestBeforeStart.EventParam,
                    Timestamp = options.Start
                    // Other properties from closestBeforeStart if necessary
                };
                filteredEvents.Insert(0, closestBeforeStart);
            }

            if (closestAfterEnd != null)
            {
                closestAfterEnd = new IndianaEvent
                {
                    EventCode = closestAfterEnd.EventCode,
                    EventParam = closestAfterEnd.EventParam,
                    Timestamp = options.End
                    // Other properties from closestAfterEnd if necessary
                };
                filteredEvents.Add(closestAfterEnd);
            }

            // Group by EventParam and process the events
            var groupingByParam = filteredEvents.GroupBy(e => e.EventParam).FirstOrDefault();
            if (groupingByParam != null)
            {
                for (var i = 0; i < groupingByParam.Count() - 1; i++)
                {
                    var currentEvent = groupingByParam.ElementAt(i);
                    var nextEvent = groupingByParam.ElementAt(i + 1);

                    if (currentEvent.EventCode == 1004 && nextEvent.EventCode == 1014)
                    {
                        var start = currentEvent.Timestamp;
                        var stop = nextEvent.Timestamp;
                        var startUp = new TimeSpaceEventBase(start, stop, null);
                        startUpList.Add(startUp);
                    }
                    if (currentEvent.EventCode == 1014 && nextEvent.EventCode == 1004)
                    {
                        var start = currentEvent.Timestamp;
                        var stop = nextEvent.Timestamp;
                        var shutDown = new TimeSpaceEventBase(start, stop, null);
                        shutDownList.Add(shutDown);
                    }
                }
            }

            return (startUpList, shutDownList);
        }
    }
}
