#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregateSplitFailureEvents.cs
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
using Utah.Udot.Atspm.Business.Common;
using Utah.Udot.Atspm.Business.SplitFail;
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
    public class AggregateSplitFailure : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<ApproachSplitFailAggregation>>
    {
        private readonly TimeSpan _binSize;

        /// <inheritdoc/>
        public AggregateSplitFailure(TimeSpan binSize, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _binSize = binSize;
        }

        /// <inheritdoc/>
        protected override Task<IEnumerable<ApproachSplitFailAggregation>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            IEnumerable<ApproachSplitFailAggregation> enumerable = splitFailOther(input);
            return Task.FromResult(enumerable);
        }


        private static IEnumerable<ApproachSplitFailAggregation> splitFailOther(Tuple<Location, IEnumerable<IndianaEvent>> input)
        {
            var phaseSplitMonitor = new List<ApproachSplitFailAggregation>();
            var location = input.Item1;
            var approaches = location.Approaches;
            var indianaEvents = input.Item2;
            var locationIdentifier = location.LocationIdentifier;
            var phaseDetails = GetPhases(location);
            DateTime binStart = indianaEvents.Select(i => i.Timestamp).OrderBy(i => i).FirstOrDefault();
            DateTime binEnd = indianaEvents.Select(i => i.Timestamp).OrderBy(i => i).LastOrDefault();
            var results = new ConcurrentBag<ApproachSplitFailAggregation>();

            foreach (var phase in phaseDetails)
            {
                var tuple = new Tuple<string, PhaseDetail, IEnumerable<IndianaEvent>, DateTime, DateTime>(location.LocationIdentifier, phase, indianaEvents, binStart, binEnd);
                var aggregatedEvents = SplitFailureAggregationCalculation(tuple);
                if (aggregatedEvents == null || !aggregatedEvents.Any()) continue;
                foreach (var result in aggregatedEvents)
                {
                    if (result == null) continue;
                    results.Add(result);
                }
            }

            var enumerable = results.AsEnumerable();
            return enumerable;
        }

        private static IEnumerable<ApproachSplitFailAggregation> SplitFailureAggregationCalculation(Tuple<string, PhaseDetail, IEnumerable<IndianaEvent>, DateTime, DateTime> input)
        {
            var phaseSplitMonitor = new List<ApproachSplitFailAggregationFinal>();
            var phaseDetail = input.Item2;
            var indianaEvents = input.Item3.ToList();
            var start = input.Item4;
            var end = input.Item5;

            //Trying to copy what the plans does
            var cycleEvents = indianaEvents.GetEventsByEventCodes(
                    start.AddSeconds(-900),
                    end.AddSeconds(900),
                    IndianaEventExtensions.GetCycleEventCodes(phaseDetail.UseOverlap),
                    phaseDetail.PhaseNumber).OrderBy(e => e.Timestamp).ToList();

            if (cycleEvents.IsNullOrEmpty())
                return null;
            var terminationEvents = indianaEvents.GetEventsByEventCodes(
                 start.AddSeconds(-900),
                 end.AddSeconds(900),
                 new List<short> { 4, 5, 6 },
                 phaseDetail.PhaseNumber);
            var detectors = phaseDetail.Approach.GetDetectorsForMetricType(12);

            var stopbarDetector = detectors
               .SelectMany(d => d.DetectionTypes)
               .FirstOrDefault(dt => dt.Id == DetectionTypes.SBP);

            List<ApproachSplitFailAggregation> approachSplitFailure = new List<ApproachSplitFailAggregation>();
            if (stopbarDetector != null)
            {
                var aggregatedDetections = GetAggregationDataByDetectionType(input, phaseDetail, indianaEvents, cycleEvents, terminationEvents, detectors, stopbarDetector);
                approachSplitFailure.Add(aggregatedDetections);
            }


            var enumerable = approachSplitFailure.AsEnumerable();
            return enumerable;
        }

        private static ApproachSplitFailAggregation GetAggregationDataByDetectionType(
            Tuple<string, PhaseDetail, IEnumerable<IndianaEvent>, DateTime, DateTime> input,
            PhaseDetail phaseDetail,
            List<IndianaEvent> indianaEvents,
            IReadOnlyList<IndianaEvent> cycleEvents,
            IReadOnlyList<IndianaEvent> terminationEvents,
            List<Detector> detectors,
            DetectionType stopbarDetector)
        {
            var options = new SplitFailOptions
            {
                Start = input.Item4,
                End = input.Item5,
                LocationIdentifier = input.Item1,
                FirstSecondsOfRed = 5
            };

            var tempDetectorEvents = indianaEvents.GetDetectorEvents(
                   12,
                   phaseDetail.Approach,
                   input.Item4.AddHours(-1),
                   input.Item5.AddHours(1),
                   true,
                   true,
                   stopbarDetector).Distinct().ToList();
            if (tempDetectorEvents == null)
            {
                return null;
            }
            var detectorEvents = tempDetectorEvents.ToList();
            AddBeginEndEventsByDetector(options, detectors, stopbarDetector, detectorEvents);
            var approach = phaseDetail.Approach;



            var splitFailPhaseData = new SplitFailPhaseData
            {
                Approach = approach,
                GetPermissivePhase = phaseDetail.IsPermissivePhase
            };

            if (cycleEvents.Count < 3) return null;

            splitFailPhaseData.Cycles = GetSplitFailCycles(
            options,
            cycleEvents,
            terminationEvents);

            var detectorActivations = new List<SplitFailDetectorActivation>();
            foreach (var detector in detectors)
            {
                var channelEvents = detectorEvents
                    .Where(e => e.EventParam == detector.DetectorChannel)
                    .OrderBy(e => e.Timestamp)
                    .ToList();

                AddDetectorOnToBeginningIfNecessary(options, detector, channelEvents);
                AddDetectorOffToEndIfNecessary(options, detector, channelEvents);
                AddDetectorActivationsFromList(channelEvents, splitFailPhaseData);
            }

            CombineDetectorActivations(splitFailPhaseData);

            foreach (var cycle in splitFailPhaseData.Cycles)
                cycle.SetDetectorActivations(splitFailPhaseData.DetectorActivations);

            var aggregation = new ApproachSplitFailAggregation
            {
                LocationIdentifier = input.Item1,
                BinStartTime = input.Item4,
                Start = input.Item4,
                End = input.Item5,
                PhaseNumber = phaseDetail.PhaseNumber,
                IsProtectedPhase = !phaseDetail.IsPermissivePhase,
                ApproachId = approach.Id,
                Cycles = splitFailPhaseData.Cycles?.Count ?? 0,
                SplitFailures = splitFailPhaseData?.Cycles.Count(c => c.IsSplitFail) ?? 0,
                GreenOccupancySum = (int)Math.Round(splitFailPhaseData?.Cycles.Sum(c => c.GreenOccupancyTimeInMilliseconds / 1000.0) ?? 0),
                RedOccupancySum = (int)Math.Round(splitFailPhaseData?.Cycles.Sum(c => c.RedOccupancyTimeInMilliseconds / 1000.0) ?? 0),
                GreenTimeSum = (int)Math.Round(splitFailPhaseData?.Cycles.Sum(c => c.TotalGreenTimeMilliseconds / 1000.0) ?? 0),
                RedTimeSum = (int)splitFailPhaseData?.Cycles.Sum(c => c.FirstSecondsOfRed)
            };

            return aggregation;
        }

        private static void AddBeginEndEventsByDetector(SplitFailOptions options, List<Detector> detectors, DetectionType detectionType, List<IndianaEvent> detectorEvents)
        {
            foreach (Detector channel in detectors.Where(d => d.DetectionTypes.Contains(detectionType)))
            {
                //add an EC 82 at the beginning if the first EC code is 81
                var firstEvent = detectorEvents.Where(d => d.EventParam == channel.DetectorChannel).FirstOrDefault();
                var lastEvent = detectorEvents.Where(d => d.EventParam == channel.DetectorChannel).LastOrDefault();

                if (firstEvent != null && firstEvent.EventCode == 81)
                {
                    var newDetectorOn = new IndianaEvent();
                    newDetectorOn.LocationIdentifier = options.LocationIdentifier;
                    newDetectorOn.Timestamp = options.Start;
                    newDetectorOn.EventCode = 82;
                    newDetectorOn.EventParam = Convert.ToByte(channel.DetectorChannel);
                    detectorEvents.Add(newDetectorOn);
                }

                //add an EC 81 at the end if the last EC code is 82
                if (lastEvent != null && lastEvent.EventCode == 82)
                {
                    var newDetectorOn = new IndianaEvent();
                    newDetectorOn.LocationIdentifier = options.LocationIdentifier;
                    newDetectorOn.Timestamp = options.End;
                    newDetectorOn.EventCode = 81;
                    newDetectorOn.EventParam = Convert.ToByte(channel.DetectorChannel);
                    detectorEvents.Add(newDetectorOn);
                }
            }
        }

        private static List<CycleSplitFail> GetSplitFailCycles(SplitFailOptions options, IReadOnlyList<IndianaEvent> cycleEvents, IReadOnlyList<IndianaEvent> terminationEvents)
        {
            var uniqueCycleEvents = cycleEvents.Distinct().OrderBy(c => c.Timestamp).ToList();
            var cycles = Enumerable.Range(0, uniqueCycleEvents.Count - 3)
                .Where(i => GetEventType(uniqueCycleEvents[i].EventCode) == Business.Common.RedToRedCycle.EventType.ChangeToGreen &&
                            GetEventType(uniqueCycleEvents[i + 1].EventCode) == Business.Common.RedToRedCycle.EventType.ChangeToYellow &&
                            GetEventType(uniqueCycleEvents[i + 2].EventCode) == Business.Common.RedToRedCycle.EventType.ChangeToRed &&
                            (GetEventType(uniqueCycleEvents[i + 3].EventCode) == Business.Common.RedToRedCycle.EventType.ChangeToGreen ||
                            uniqueCycleEvents[i + 3].EventCode == 66))
                .Select(i =>
                {
                    var termEvent = GetTerminationTypeBetweenStartAndEnd(uniqueCycleEvents[i].Timestamp, uniqueCycleEvents[i + 3].Timestamp, terminationEvents);
                    return new CycleSplitFail(uniqueCycleEvents[i].Timestamp, uniqueCycleEvents[i + 2].Timestamp, uniqueCycleEvents[i + 1].Timestamp,
                                              uniqueCycleEvents[i + 3].Timestamp, termEvent, options.FirstSecondsOfRed);
                })
                .Where(c => c.EndTime > options.Start && c.StartTime < options.End && c.StartTime >= options.Start)
                .ToList();

            return cycles;
        }

        private static void CombineDetectorActivations(SplitFailPhaseData splitFailPhaseData)
        {
            var tempDetectorActivations = new List<SplitFailDetectorActivation>();

            //look at every item in the original list
            foreach (var current in splitFailPhaseData.DetectorActivations)
                if (!current.ReviewedForOverlap)
                {
                    var overlapingActivations = splitFailPhaseData.DetectorActivations.Where(d => d.ReviewedForOverlap == false &&
                        (
                            //   if it starts after current starts  and    starts before current ends      and    end after current ends   
                            d.DetectorOn >=
                            current.DetectorOn &&
                            d.DetectorOn <=
                            current.DetectorOff &&
                            d.DetectorOff >= current.DetectorOff
                            //OR if it starts BEFORE curent starts  and ends AFTER current starts           and ends BEFORE current ends
                            || d.DetectorOn <=
                            current.DetectorOn &&
                            d.DetectorOff >=
                            current.DetectorOn &&
                            d.DetectorOff <= current.DetectorOff
                            //OR if it starts AFTER current starts   and it ends BEFORE current ends
                            || d.DetectorOn >=
                            current.DetectorOn &&
                            d.DetectorOff <= current.DetectorOff
                            //OR if it starts BEFORE current starts  and it ens AFTER current ends 
                            || d.DetectorOn <=
                            current.DetectorOn &&
                            d.DetectorOff >= current.DetectorOff
                        )
                    //then add it to the overlap list
                    ).ToList();

                    //if there are any in the list (and here should be at least one that matches current)
                    if (overlapingActivations.Any())
                    {
                        //Then make a new activation that starts witht eh earliest start and ends with the latest end
                        var tempDetectorActivation = new SplitFailDetectorActivation
                        {
                            DetectorOn = overlapingActivations.Min(o => o.DetectorOn),
                            DetectorOff = overlapingActivations.Max(o => o.DetectorOff)
                        };
                        //and add the new one to a temp list
                        tempDetectorActivations.Add(tempDetectorActivation);

                        //mark everything in the  overlap list as Reviewed
                        foreach (var splitFailDetectorActivation in overlapingActivations)
                            splitFailDetectorActivation.ReviewedForOverlap = true;
                    }
                }

            //since we went through every item in the original list, if there were no overlaps, it shoudl equal the temp list
            if (splitFailPhaseData.DetectorActivations.Count != tempDetectorActivations.Count)
            {
                //if the counts do not match, we have to set the original list to the temp list and try the combinaitons again
                splitFailPhaseData.DetectorActivations = tempDetectorActivations;
                CombineDetectorActivations(splitFailPhaseData);
            }
        }

        private static void AddDetectorActivationsFromList(List<IndianaEvent> events, SplitFailPhaseData splitFailPhaseData)
        {
            events = events.OrderBy(e => e.Timestamp).ToList();
            for (var i = 0; i < events.Count - 1; i++)
                if (events[i].EventCode == 82 && events[i + 1].EventCode == 81)
                    splitFailPhaseData.DetectorActivations.Add(new SplitFailDetectorActivation
                    {
                        DetectorOn = events[i].Timestamp,
                        DetectorOff = events[i + 1].Timestamp
                    });
        }

        private static void AddDetectorOffToEndIfNecessary(SplitFailOptions options, Detector detector,
            List<IndianaEvent> events)
        {
            if (events.LastOrDefault()?.EventCode == 82)
                events.Insert(events.Count, new IndianaEvent
                {
                    Timestamp = options.End,
                    EventCode = 81,
                    EventParam = Convert.ToByte(detector.DetectorChannel),
                    LocationIdentifier = options.LocationIdentifier
                });
        }

        private static void AddDetectorOnToBeginningIfNecessary(SplitFailOptions options, Detector detector,
            List<IndianaEvent> events)
        {
            if (events.FirstOrDefault()?.EventCode == 81)
                events.Insert(0, new IndianaEvent
                {
                    Timestamp = options.Start,
                    EventCode = 82,
                    EventParam = Convert.ToByte(detector.DetectorChannel),
                    LocationIdentifier = options.LocationIdentifier
                });
        }

        internal class PhaseSplitFailDto
        {
            public int PhaseNumber { get; set; }
            public List<Tuple<DateTime, DateTime>> GreenTime { get; set; } // start time of the green phase
            public List<Tuple<DateTime, DateTime>> RedTime { get; set; } // start time of the red phase
            public int GreenTimeSum { get; set; } //how long the light was green (EC 1 - 8)
            public int RedTimeSum { get; set; } // always 5 times the amount of cycles (only looking at 5 seconds after EC 9)
            public int Cycles { get; set; } //(1 , 8, 9)
        }

        internal class ApproachSplitFailAggregationFinal
        {
            public string LocationIdentifier { get; set; }
            public DateTime BinStartTime { get; set; }
            public int PhaseNumber { get; set; }

            ///<inheritdoc/>
            public int ApproachId { get; set; }

            public bool IsProtectedPhase { get; set; } // not zero get the calculations for each version protected phase and not :P use this as the event param
            public int SplitFailures { get; set; } // how many times the 85% was hit for both green and red
            public int GreenOccupancySum { get; set; } //how long the detector was on (EC 81on, 82 off)
            public int RedOccupancySum { get; set; } // how long the detector was on those 5 sec. (after 9)
            public int GreenTimeSum { get; set; } //how long the light was green (EC 1 - 8)
            public int RedTimeSum { get; set; } // always 5 times the amount of cycles (only looking at 5 seconds after EC 9)
            public int Cycles { get; set; } //(1 , 8, 9)
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

        private static CycleSplitFail.TerminationType GetTerminationTypeBetweenStartAndEnd(DateTime start,
            DateTime end, IReadOnlyList<IndianaEvent> terminationEvents)
        {
            var terminationType = CycleSplitFail.TerminationType.Unknown;
            var terminationEvent = terminationEvents.FirstOrDefault(t => t.Timestamp > start && t.Timestamp <= end);
            if (terminationEvent != null)
                terminationType = terminationEvent.EventCode switch
                {
                    4 => CycleSplitFail.TerminationType.GapOut,
                    5 => CycleSplitFail.TerminationType.MaxOut,
                    6 => CycleSplitFail.TerminationType.ForceOff,
                    _ => CycleSplitFail.TerminationType.Unknown,
                };
            return terminationType;
        }


    }
}
