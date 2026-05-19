#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/CalculateTimingPlans.cs
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

using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    public class CalculateHourlyDetectorAggregation : TransformProcessStepBase<List<Tuple<Guid, Detector, List<SpeedEvent>, List<IndianaEvent>>>, IEnumerable<HourlySpeed>>
    {
        public CalculateHourlyDetectorAggregation(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }
        protected override async Task<IEnumerable<HourlySpeed>> Process(List<Tuple<Guid, Detector, List<SpeedEvent>, List<IndianaEvent>>> input, CancellationToken cancelToken = default)
        {
            var detectorSpeedAgg = new List<HourlySpeed>();
            foreach (var segmentTuple in input)
            {
                var segmentId = segmentTuple.Item1;
                Detector detector = segmentTuple.Item2;
                var speedEvents = segmentTuple.Item3;
                var indianaEvents = segmentTuple.Item4;
                var speedLimit = detector.Approach.Mph;

                //We already should be getting it filtered
                //var filteredSpeedEvents = speedEvents.Where(i => i.DetectorId.Equals(detector.DectectorIdentifier));
                //var filteredIndianaEvents = indianaEvents.Where(log => log.LocationIdentifier.Equals(locationIdentifier) && log.EventCode == 82 && log.EventParam == detector.DetectorChannel);

                var speedEventsHourlyBin = speedEvents
                    .GroupBy(speed => new DateTime(speed.Timestamp.Year, speed.Timestamp.Month, speed.Timestamp.Day, speed.Timestamp.Hour, 0, 0))
                    .ToList();

                var indianaEventsHourlyBin = indianaEvents
                    .GroupBy(indiana => new DateTime(indiana.Timestamp.Year, indiana.Timestamp.Month, indiana.Timestamp.Day, indiana.Timestamp.Hour, 0, 0))
                    .ToList();

                // Extract keys from both hourly bins
                var speedHourlyKeys = speedEventsHourlyBin.Select(group => group.Key);
                var indianaHourlyKeys = indianaEventsHourlyBin.Select(group => group.Key);

                // Combine and remove duplicates
                var combinedHourlyKeys = speedHourlyKeys
                    .Union(indianaHourlyKeys)  // Union removes duplicates by default
                    .OrderBy(key => key)       // Sort the keys chronologically
                    .ToHashSet();


                // Loop through the combined hourly keys
                foreach (var timeStamp in combinedHourlyKeys)
                {
                    // Get the matching speedEvents group for this timeStamp
                    var matchingSpeedGroup = speedEventsHourlyBin.Where(se => se.Key == timeStamp).ToList();

                    // Get the matching indianaEvents group for this timeStamp
                    var matchingIndianaGroup = indianaEventsHourlyBin.Where(ie => ie.Key == timeStamp).ToList();

                    double averageSpeed = 0.0;
                    double? fifteenthPercentile = null;
                    double? eightyFifthPercentile = null;
                    double? ninetyFifthPercentile = null;
                    double? ninetyNinthPercentile = null;
                    double? minSpeed = null;
                    double? maxSpeed = null;
                    long? violations = null;
                    long? extremeViolations = null;
                    long? flowCount = null; //AC
                    long? speedFlow = null; //AS
                    double? percentObserved = null;
                    bool sourceDataAnalyzed = false;

                    // Perform actions with the matching groups
                    if (matchingSpeedGroup != null)
                    {
                        var flatSpeedEvents = matchingSpeedGroup
                            .SelectMany(group => group)
                            .ToList();

                        var combinedSpeeds = flatSpeedEvents
                            .Select(s => (double)s.Mph)
                        .ToList();

                        averageSpeed = combinedSpeeds.Count > 0 ? Math.Round(combinedSpeeds.Average(), 1) : 0.0;
                        fifteenthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 15), 1) : null;
                        eightyFifthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 85), 1) : null;
                        ninetyFifthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 95), 1) : null;
                        ninetyNinthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 99), 1) : null;
                        minSpeed = combinedSpeeds.Count > 0 ? combinedSpeeds.Min() : null;
                        maxSpeed = combinedSpeeds.Count > 0 ? combinedSpeeds.Max() : null;

                        violations = combinedSpeeds.Where(speed => (speed - speedLimit) > 2).Count();
                        extremeViolations = combinedSpeeds.Where(speed => (speed - speedLimit) > 7).Count();
                        speedFlow = combinedSpeeds.Count();
                    }

                    if (matchingIndianaGroup != null)
                    {
                        var flatIndianaEvents = matchingIndianaGroup
                            .SelectMany(group => group)
                            .ToList();
                        flowCount = flatIndianaEvents.Count();
                    }

                    percentObserved = (flowCount != null && flowCount > 0) ? (((double)(speedFlow ?? 0) / flowCount) * 100) : 0;

                    if (speedFlow != null && flowCount != null)
                    {
                        sourceDataAnalyzed = true;
                    }

                    detectorSpeedAgg.Add(new HourlySpeed
                    {
                        Date = timeStamp.Date,
                        BinStartTime = timeStamp,
                        SegmentId = segmentId,
                        SourceId = 1,
                        PercentObserved = percentObserved,
                        Average = averageSpeed,
                        FifteenthSpeed = fifteenthPercentile,
                        EightyFifthSpeed = eightyFifthPercentile,
                        NinetyFifthSpeed = ninetyFifthPercentile,
                        NinetyNinthSpeed = ninetyNinthPercentile,
                        Violation = (long?)violations,
                        ExtremeViolation = (long?)extremeViolations,
                        Flow = flowCount,
                        MinSpeed = minSpeed,
                        MaxSpeed = maxSpeed,
                        SourceDataAnalyzed = sourceDataAnalyzed
                    });

                    //new HourlySpeed
                    //{
                    //    Date = timeStamp.Date,
                    //    BinStartTime = timeStamp,
                    //    SegmentId = segmentId,
                    //    SourceId = 1,  // Ensure sourceId is set correctly in the broader scope
                    //    PercentObserved = percentObserved,
                    //    Flow = (long)flowCount,
                    //    Violation = (long?)violations,
                    //    ExtremeViolation = (long?)extremeViolations,
                    //    Average = averageSpeed,
                    //    FifteenthSpeed = fifteenthPercentile,
                    //    EightyFifthSpeed = eightyFifthPercentile,
                    //    NinetyFifthSpeed = ninetyFifthPercentile,
                    //    NinetyNinthSpeed = ninetyNinthPercentile,
                    //    MinSpeed = minSpeed,
                    //    MaxSpeed = maxSpeed,
                    //    SourceDataAnalyzed = sourceDataAnalyzed
                    //}
                }
            }
            return await Task.FromResult<IEnumerable<HourlySpeed>>(detectorSpeedAgg);
        }
    }
}

