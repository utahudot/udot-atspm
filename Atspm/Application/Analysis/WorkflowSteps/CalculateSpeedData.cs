//#region license
//// Copyright 2024 Utah Departement of Transportation
//// for ApplicationCore - ATSPM.Application.Analysis.WorkflowSteps/CalculateTimingPlans.cs
//// 
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
//// 
//// http://www.apache.org/licenses/LICENSE-2.
//// 
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.
//#endregion

//using System.Threading.Tasks.Dataflow;
//using Utah.Udot.Atspm.Analysis.Plans;
//using Utah.Udot.Atspm.Data.Models.EventLogModels;

//namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
//{
//    public class CalculateSpeedData<T> : TransformProcessStepBase<Tuple<Tuple<Detector, string, IEnumerable<SpeedEvent>>, Tuple<Detector, string, IEnumerable<IndianaEvent>>>, IEnumerable<DetectorSpeedAggregation>> where T : IPlan, new()
//    {
//        public CalculateSpeedData(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

//        protected override Task<IEnumerable<DetectorSpeedAggregation>> Process(Tuple<Tuple<Detector, string, IEnumerable<SpeedEvent>>, Tuple<Detector, string, IEnumerable<IndianaEvent>>> input, CancellationToken cancelToken = default)
//        {
//            var speedTuple = input.Item1;
//            var speedDetectorIdentifier = speedTuple.Item2;
//            var speedEvents = speedTuple.Item3;
//            var indianaTuple = input.Item2;
//            var indianaDetectorIdentifier = indianaTuple.Item2;
//            var indianaEvents = indianaTuple.Item3;
//            var detector = speedTuple.Item1 as Detector;


//            if (!(speedDetectorIdentifier.Equals(indianaDetectorIdentifier)))
//            {
//                return Task.FromResult<IEnumerable<DetectorSpeedAggregation>>(new List<DetectorSpeedAggregation>());
//            }

//            //TODO Ask Which time extension I was suppose to use.....
//            var speedEventsHourlyBin = speedEvents
//                .GroupBy(speed => speed.Timestamp)
//                .ToList();

//            var indianaEventsHourlyBin = indianaEvents
//                .GroupBy(indiana => new DateTime(indiana.Timestamp.Year, indiana.Timestamp.Month, indiana.Timestamp.Day, indiana.Timestamp.Hour, 0, 0))
//                .ToList();

//            // Extract keys from both hourly bins
//            var speedHourlyKeys = speedEventsHourlyBin.Select(group => group.Key);
//            var indianaHourlyKeys = indianaEventsHourlyBin.Select(group => group.Key);

//            // Combine and remove duplicates
//            var combinedHourlyKeys = speedHourlyKeys
//                .Union(indianaHourlyKeys)  // Union removes duplicates by default
//                .OrderBy(key => key)       // Optional: sort the keys chronologically
//                .ToList();

//            var detectorSpeedAgg = new List<DetectorSpeedAggregation>();
//            // Loop through the combined hourly keys
//            combinedHourlyKeys.ForEach(timeStamp =>
//            {
//                // Get the matching speedEvents group for this timeStamp
//                var matchingSpeedGroup = speedEventsHourlyBin.Where(se => se.Key == timeStamp);

//                // Get the matching indianaEvents group for this timeStamp
//                var matchingIndianaGroup = indianaEventsHourlyBin.Where(ie => ie.Key == timeStamp);

//                var speedLimit = detector.Approach.Mph;

//                double averageSpeed = 0;
//                double? fifteenthPercentile = null;
//                double? eightyFifthPercentile = null;
//                double? ninetyFifthPercentile = null;
//                double? ninetyNinthPercentile = null;
//                double? minSpeed = null;
//                double? maxSpeed = null;
//                long? violations = null;
//                long? extremeViolations = null;
//                long? flowCount = null; //AC
//                long? speedFlow = null; //AS
//                double? percentObserved = null;
//                bool sourceDataAnalyzed = false;

//                // Perform actions with the matching groups
//                if (matchingSpeedGroup != null)
//                {
//                    var flatSpeedEvents = matchingSpeedGroup
//                        .SelectMany(group => group)
//                        .ToList();

//                    var combinedSpeeds = flatSpeedEvents
//                        .Select(s => (double)s.Mph)
//                    .ToList();

//                    averageSpeed = combinedSpeeds.Count > 0 ? Math.Round(combinedSpeeds.Average(), 1) : 0;
//                    fifteenthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 15), 1) : null;
//                    eightyFifthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 85), 1) : null;
//                    ninetyFifthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 95), 1) : null;
//                    ninetyNinthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 99), 1) : null;
//                    minSpeed = combinedSpeeds.Count > 0 ? combinedSpeeds.Min() : null;
//                    maxSpeed = combinedSpeeds.Count > 0 ? combinedSpeeds.Max() : null;

//                    violations = combinedSpeeds.Select(speed => (speed - speedLimit) > 2).Count();
//                    extremeViolations = combinedSpeeds.Select(speed => (speed - speedLimit) > 7).Count();
//                    speedFlow = combinedSpeeds.Count();
//                }

//                if (matchingIndianaGroup != null)
//                {
//                    var flatIndianaEvents = matchingIndianaGroup
//                        .SelectMany(group => group)
//                        .ToList();
//                    flowCount = flatIndianaEvents.Count();
//                }

//                percentObserved = flowCount != null ? ((speedFlow ?? 0) / flowCount) : 0;

//                if (speedFlow != null && flowCount != null)
//                {
//                    sourceDataAnalyzed = true;
//                }

//                detectorSpeedAgg.Add(new DetectorSpeedAggregation
//                {
//                    EventCount = 0,
//                    DetectorIdentifier = speedDetectorIdentifier,
//                    Average = averageSpeed,
//                    FifteenthSpeed = fifteenthPercentile,
//                    EightyFifthSpeed = eightyFifthPercentile,
//                    NinetyFifthSpeed = ninetyFifthPercentile,
//                    NinetyNinthSpeed = ninetyNinthPercentile,
//                    MinSpeed = minSpeed,
//                    MaxSpeed = maxSpeed,
//                    Violations = violations,
//                    ExtremeViolations = extremeViolations,
//                    FlowCount = flowCount,
//                    SpeedFlow = speedFlow,
//                    PercentObserved = percentObserved,
//                    SourceDataAnalyzed = sourceDataAnalyzed
//                });
//            });

//            return Task.FromResult<IEnumerable<DetectorSpeedAggregation>>(detectorSpeedAgg);
//        }

//        //var speedEventsMonthlyBin = speedEvents
//        //    .GroupBy(speed => new DateTime(speed.Timestamp.Year, speed.Timestamp.Month, 1))
//        //    .ToList();

//        //var indianaEventsMonthlyBin = indianaEvents
//        //    .GroupBy(indiana => new DateTime(indiana.Timestamp.Year, indiana.Timestamp.Month, 1))
//        //    .ToList();

//        //// Extract keys from both monthly bins
//        //var speedMonthlyKeys = speedEventsMonthlyBin.Select(group => group.Key);
//        //var indianaMonthlyKeys = indianaEventsMonthlyBin.Select(group => group.Key);

//        //// Combine and remove duplicates
//        //var combinedMonthlyKeys = speedMonthlyKeys
//        //    .Union(indianaMonthlyKeys)  // Union removes duplicates
//        //    .OrderBy(key => key)        // Optional: sort the keys chronologically
//        //    .ToList();


//    }
//}
