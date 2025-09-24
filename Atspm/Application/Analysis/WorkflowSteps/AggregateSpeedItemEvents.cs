#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregateSpeedItemEvents.cs
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

using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Business.ApproachSpeed;
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.TempExtensions;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// Transforms <see cref="IndianaEvent"/> into <see cref="DetectorEventCountAggregation"/>
    /// where <see cref="IndianaEvent.EventCode"/> equals <see cref="IndianaEnumerations.VehicleDetectorOn"/>
    /// and <see cref="IndianaEvent.EventParam"/> equals <see cref="Detector.DetectorChannel"/>.
    /// </summary>
    public class AggregateSpeedItemEvents : TransformProcessStepBase<Tuple<Location, Tuple<IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>>, IEnumerable<ApproachSpeedAggregation>>
    {
        private readonly TimeSpan _binSize;

        /// <inheritdoc/>
        public AggregateSpeedItemEvents(TimeSpan binSize, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _binSize = binSize;
        }

        /// <inheritdoc/>
        protected override Task<IEnumerable<ApproachSpeedAggregation>> Process(Tuple<Location, Tuple<IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>> input, CancellationToken cancelToken = default)
        {
            return (Task<IEnumerable<ApproachSpeedAggregation>>)SpeedAgg(input);
        }

        private IEnumerable<ApproachSpeedAggregation> SpeedAgg(Tuple<Location, Tuple<IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>> input)
        {
            var phaseSplitMonitor = new List<ApproachSpeedAggregation>();
            var location = input.Item1;
            var approaches = location.Approaches;
            var indianaEvents = input.Item2.Item1;
            var speedaEvents = input.Item2.Item2;
            var locationIdentifier = location.LocationIdentifier;
            var phaseDetails = GetPhases(location);
            DateTime binStart = indianaEvents.Select(i => i.Timestamp).OrderBy(i => i).FirstOrDefault();
            DateTime binEnd = indianaEvents.Select(i => i.Timestamp).OrderBy(i => i).LastOrDefault();
            var results = new ConcurrentBag<ApproachSpeedAggregation>();


            foreach (var phase in phaseDetails)
            {
                var tuple = new Tuple<PhaseDetail, IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>, DateTime, DateTime>(phase, indianaEvents, speedaEvents, binStart, binEnd);
                var aggregatedEvents = SpeedAggregationCalculations(tuple);
                foreach (var result in aggregatedEvents)
                {
                    if (result == null) continue;
                    result.Start = binStart;
                    result.End = binEnd;
                    result.LocationIdentifier = location.LocationIdentifier;
                    results.Add(result);
                }
            }
            var enumerable = results.AsEnumerable();
            return enumerable;
        }

        private static List<PhaseDetail> GetPhases(Location Location)
        {
            if (Location.Approaches == null || Location.Approaches.Count == 0)
            {
                return new List<PhaseDetail>();
            }

            var phaseDetails = new List<PhaseDetail>();
            foreach (var approach in Location.Approaches)
            {
                if (approach.ProtectedPhaseNumber != 0)
                {
                    phaseDetails.Add(new PhaseDetail
                    {
                        PhaseNumber = approach.ProtectedPhaseNumber,
                        UseOverlap = approach.IsProtectedPhaseOverlap,
                        IsPermissivePhase = false,
                        Approach = approach
                    });
                }
                if (approach.PermissivePhaseNumber != null)
                {
                    phaseDetails.Add(new PhaseDetail
                    {
                        PhaseNumber = approach.PermissivePhaseNumber.Value,
                        UseOverlap = approach.IsPermissivePhaseOverlap,
                        IsPermissivePhase = true,
                        Approach = approach
                    });
                }
            }
            return phaseDetails;
        }

        public static IEnumerable<(DateTime Start, DateTime End)> GetDateChunks(DateTime start, DateTime end, int chunkSizeDays)
        {
            var current = DateTime.SpecifyKind(start.Date, DateTimeKind.Utc);
            var final = DateTime.SpecifyKind(end.Date, DateTimeKind.Utc);

            while (current < final)
            {
                var next = current.AddDays(chunkSizeDays);
                yield return (current, next < final ? next : final);
                current = next;
            }
        }

        private IEnumerable<ApproachSpeedAggregation> SpeedAggregationCalculations(Tuple<PhaseDetail, IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>, DateTime, DateTime> input)
        {
            var phaseDetail = input.Item1;
            var speedEvents = input.Item3;
            var indianaEvents = input.Item2;
            var start = input.Item4;
            var end = input.Item5;
            List<ApproachSpeedAggregation> approachSpeeds = new List<ApproachSpeedAggregation>();

            if (speedEvents == null || !speedEvents.Any())
            {
                return Enumerable.Empty<ApproachSpeedAggregation>();
            }

            var detectors = phaseDetail.Approach.GetDetectorsForMetricType(10);
            Detector detector;
            if (detectors.IsNullOrEmpty())
            {
                return approachSpeeds;
            }
            else
            {
                detector = detectors.First();
            }

            var cycleEvents = indianaEvents.GetCycleEventsWithTimeExtension(
                phaseDetail.PhaseNumber,
                phaseDetail.UseOverlap,
                start,
                end);

            var aggregatedSpeedEvents = GetAggregateSpeedEvents(input, detector);
            approachSpeeds.Add(aggregatedSpeedEvents);

            return approachSpeeds;
        }

        private ApproachSpeedAggregation GetAggregateSpeedEvents(Tuple<PhaseDetail, IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>, DateTime, DateTime> input, Detector detector)
        {
            var speedEvents = input.Item3;
            var cycleEvents = input.Item2;
            var speedEventsForDetector = speedEvents.Where(d => d.DetectorId == detector.DectectorIdentifier && d.Timestamp >= input.Item4 && d.Timestamp < input.Item5).ToList();
            var speedDetector = GetSpeedDetector(
                input.Item4,
                input.Item5,
                detector,
                cycleEvents.ToList(),
                speedEventsForDetector);

            var averageSpeeds = new List<int>();
            var eightyFifthSpeeds = new List<int>();
            var fifteenthSpeeds = new List<int>();
            if (speedDetector.AvgSpeedBucketCollection != null)
            {
                foreach (var bucket in speedDetector.AvgSpeedBucketCollection.AvgSpeedBuckets)
                {
                    try
                    {
                        averageSpeeds.Add(bucket.AvgSpeed);
                        eightyFifthSpeeds.Add(bucket.EightyFifth);
                        fifteenthSpeeds.Add(bucket.FifteenthPercentile);
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            var aggregatedSpeed = new ApproachSpeedAggregation
            {
                Start = input.Item4,
                End = input.Item5,
                ApproachId = input.Item1.Approach.Id,
                SummedSpeed = averageSpeeds.Sum(),
                SpeedVolume = averageSpeeds.Count,
                AverageSpeed = averageSpeeds.Average(),
                Speed85th = eightyFifthSpeeds.Average(),
                Speed15th = fifteenthSpeeds.Average()
            };

            return aggregatedSpeed;
        }

        public SpeedDetector GetSpeedDetector(
        DateTime start,
        DateTime end,
        Detector detector,
        List<IndianaEvent> cycleEvents,
        List<SpeedEvent> speedEventsForDetector)
        {
            var cycles = GetSpeedCycles(start, end, cycleEvents);
            if (cycles.Any())
            {
                foreach (var cycle in cycles)
                    cycle.FindSpeedEventsForCycle(speedEventsForDetector);
            }

            var totalDetectorHits = cycles.Sum(c => c.SpeedEvents.Count);
            var movementDelay = 0;
            if (detector.MovementDelay != null)
                movementDelay = detector.MovementDelay.Value;
            var avgSpeedBucketCollection = new AvgSpeedBucketCollection(start, end, 15, movementDelay, cycles, null);
            return new SpeedDetector(
            new List<SpeedPlan>(),
            totalDetectorHits,
            start,
            end,
            cycles,
            speedEventsForDetector,
            avgSpeedBucketCollection
            );
        }

        public List<CycleSpeed> GetSpeedCycles(DateTime startDate, DateTime endDate, List<IndianaEvent> cycleEvents)
        {
            var mainEvents = cycleEvents.Where(c => c.Timestamp <= endDate && c.Timestamp >= startDate).ToList();
            var previousEvents = cycleEvents.Where(c => c.Timestamp < startDate).ToList();
            var nextEvents = cycleEvents.Where(c => c.Timestamp > endDate).ToList();
            var cycles = new List<CycleSpeed>();
            if (!mainEvents.IsNullOrEmpty())
            {
                if (GetEventType(mainEvents[mainEvents.Count - 1].EventCode) != Business.Common.RedToRedCycle.EventType.ChangeToRed || mainEvents.LastOrDefault().Timestamp < endDate)
                    //Get events to complete cycles
                    mainEvents.AddRange(nextEvents.OrderBy(e => e.Timestamp).Take(3));
                if (GetEventType(mainEvents[0].EventCode) != Business.Common.RedToRedCycle.EventType.ChangeToRed || mainEvents[0].Timestamp > startDate)
                    //Get events to start cycles
                    mainEvents.InsertRange(0, previousEvents.OrderByDescending(e => e.Timestamp).Take(3).OrderBy(e => e.Timestamp));
                for (var i = 0; i < mainEvents.Count; i++)
                    if (i < mainEvents.Count - 3
                        && GetEventType(mainEvents[i].EventCode) == Business.Common.RedToRedCycle.EventType.ChangeToRed
                        && GetEventType(mainEvents[i + 1].EventCode) == Business.Common.RedToRedCycle.EventType.ChangeToGreen
                        && GetEventType(mainEvents[i + 2].EventCode) == Business.Common.RedToRedCycle.EventType.ChangeToYellow
                        && GetEventType(mainEvents[i + 3].EventCode) == Business.Common.RedToRedCycle.EventType.ChangeToRed)
                        cycles.Add(new CycleSpeed(mainEvents[i].Timestamp, mainEvents[i + 1].Timestamp,
                            mainEvents[i + 2].Timestamp, mainEvents[i + 3].Timestamp));
            }
            return cycles;
        }
        private static Business.Common.RedToRedCycle.EventType GetEventType(short eventCode)
        {
            return eventCode switch
            {
                1 => Business.Common.RedToRedCycle.EventType.ChangeToGreen,
                3 => Business.Common.RedToRedCycle.EventType.ChangeToEndMinGreen,
                61 => Business.Common.RedToRedCycle.EventType.ChangeToGreen,
                8 => Business.Common.RedToRedCycle.EventType.ChangeToYellow,
                63 => Business.Common.RedToRedCycle.EventType.ChangeToYellow,
                9 => Business.Common.RedToRedCycle.EventType.ChangeToRed,
                11 => Business.Common.RedToRedCycle.EventType.ChangeToEndOfRedClearance,
                64 => Business.Common.RedToRedCycle.EventType.ChangeToRed,
                66 => Business.Common.RedToRedCycle.EventType.OverLapDark,
                _ => Business.Common.RedToRedCycle.EventType.Unknown,
            };
        }

    }
}
