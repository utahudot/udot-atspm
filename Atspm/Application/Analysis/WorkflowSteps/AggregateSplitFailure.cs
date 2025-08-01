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

using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

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
            var phaseSplitMonitor = new List<ApproachSplitFailAggregation>();
            var location = input.Item1;
            var approaches = location.Approaches;
            var indianaEvents = input.Item2;
            var detectionEvents = indianaEvents
                .Where(i => i.EventCode == 81 || i.EventCode == 82)
                .ToList();
            var groupedIndianaEvents = indianaEvents
                .Where(i => i.EventCode == 1 || i.EventCode == 8 || i.EventCode == 9)
                .GroupBy(i => i.EventParam)
                .ToDictionary(g => g.Key, g => g.ToList());

            var listPhaseInformation = new List<PhaseSplitFailDto>();
            foreach (var phaseWithIndianaEvents in groupedIndianaEvents)
            {
                var phase = phaseWithIndianaEvents.Key;
                var phaseIndianaEvents = phaseWithIndianaEvents.Value.OrderBy(i => i.Timestamp).ToList();
                int cycleCount = 0;
                List<Tuple<DateTime, DateTime>> greenTimes = new List<Tuple<DateTime, DateTime>>();
                List<Tuple<DateTime, DateTime>> redTimes = new List<Tuple<DateTime, DateTime>>();
                int greenTimeSum = 0;

                for (int i = 0; i < phaseIndianaEvents.Count - 2; i++)
                {
                    var first = phaseIndianaEvents[i];
                    var second = phaseIndianaEvents[i + 1];
                    var third = phaseIndianaEvents[i + 2];

                    // Check for sequence 1 -> 8 -> 9
                    if (first.EventCode == 1 &&
                        second.EventCode == 8 &&
                        third.EventCode == 9)
                    {
                        // Valid cycle found
                        cycleCount++;
                        var greentimes = new Tuple<DateTime, DateTime>(first.Timestamp, second.Timestamp);
                        var redtimes = new Tuple<DateTime, DateTime>(third.Timestamp.AddSeconds(5), third.Timestamp.AddSeconds(10));
                        greenTimes.Add(greentimes);
                        redTimes.Add(redtimes);
                        greenTimeSum += (int)(second.Timestamp - first.Timestamp).TotalSeconds;

                        // Skip to next possible cycle (non-overlapping)
                        i += 2;
                    }
                }

                listPhaseInformation.Add(new PhaseSplitFailDto
                {
                    PhaseNumber = phase,
                    GreenTime = greenTimes,
                    RedTime = redTimes,
                    GreenTimeSum = greenTimeSum,
                    RedTimeSum = cycleCount * 5, // always 5 times the amount of cycles (only looking at 5 seconds after EC 9)
                    Cycles = cycleCount
                });
            }

            List<ApproachSplitFailAggregation> approachSplitFailure = new List<ApproachSplitFailAggregation>();
            // For each approach go through each detector and pull out the speed logs
            foreach (var approach in approaches)
            {
                var protectedPhaseNumber = approach.ProtectedPhaseNumber;
                var permissivePhaseNumber = approach.ProtectedPhaseNumber;
                var detectors = approach.Detectors.Select(i => i.DetectorChannel).ToList();
                var detectionEventsInApproach = detectionEvents.Where(detEvent => detectors.Contains(detEvent.EventParam)).ToList();

                if (protectedPhaseNumber != 0 && protectedPhaseNumber != null)
                {
                    ApproachSplitFailAggregation splitFailProtected = SplitFailHelper(input, location, listPhaseInformation, approach, protectedPhaseNumber, detectionEventsInApproach, true);

                    approachSplitFailure.Add(splitFailProtected);
                }
                if (permissivePhaseNumber != 0 && permissivePhaseNumber != null)
                {
                    ApproachSplitFailAggregation splitFailPermissive = SplitFailHelper(input, location, listPhaseInformation, approach, protectedPhaseNumber, detectionEventsInApproach, false);

                    approachSplitFailure.Add(splitFailPermissive);
                }
            }

            var enumerable = approachSplitFailure.AsEnumerable();
            return Task.FromResult(enumerable);
        }

        private static ApproachSplitFailAggregation SplitFailHelper(Tuple<Location, IEnumerable<IndianaEvent>> input, Location location, List<PhaseSplitFailDto> listPhaseInformation, Approach approach, int protectedPhaseNumber, List<IndianaEvent> detectionEventsInApproach, bool IsProtected)
        {
            var phaseInfo = listPhaseInformation
                                    .Where(i => i.PhaseNumber == protectedPhaseNumber)
                                    .FirstOrDefault();
            var greenOccupancySum = 0;
            var redOccupancySum = 0;
            var splitFailures = 0;
            for (int i = 0; i < phaseInfo.GreenTime.Count - 1; i++)
            {
                var greenTime = phaseInfo.GreenTime[i];
                var greenTimeStart = greenTime.Item1;
                var greenTimeEnd = greenTime.Item2;
                var greenDuration = (int)(greenTimeEnd - greenTimeStart).TotalSeconds;

                var redTime = phaseInfo.RedTime[i];
                var redTimeStart = redTime.Item1;
                var redTimeEnd = redTime.Item2;
                var redDuration = (int)(redTimeEnd - redTimeStart).TotalSeconds;

                var greenOccupancyEvents = detectionEventsInApproach.Where(i => i.Timestamp >= greenTimeStart && i.Timestamp <= greenTimeEnd).ToList();
                var redOccupancyEvents = detectionEventsInApproach.Where(i => i.Timestamp >= redTimeStart && i.Timestamp <= redTimeEnd).ToList();

                bool eightyFivePercentGreen = false;
                bool eightyFivePercentRed = false;

                var startTime = greenTimeStart;
                foreach (var greenEvent in greenOccupancyEvents)
                {
                    if (greenEvent.EventCode == 82) // Detector On
                    {
                        startTime = greenEvent.Timestamp;
                    }
                    else if (greenEvent.EventCode == 81) // Detector Off
                    {
                        var duration = (int)(greenEvent.Timestamp - startTime).TotalSeconds;
                        greenOccupancySum += duration;

                        if (duration >= greenDuration * 0.85)
                        {
                            eightyFivePercentGreen = true;
                        }
                    }
                }
                startTime = greenTimeStart;
                foreach (var redEvent in redOccupancyEvents)
                {
                    if (redEvent.EventCode == 82) // Detector On
                    {
                        startTime = redEvent.Timestamp;
                    }
                    else if (redEvent.EventCode == 81) // Detector Off
                    {
                        var duration = (int)(redEvent.Timestamp - startTime).TotalSeconds;
                        redOccupancySum += duration;

                        if (duration >= redDuration * 0.85)
                        {
                            eightyFivePercentRed = true;
                        }
                    }
                }

                if (eightyFivePercentGreen && eightyFivePercentRed)
                {
                    splitFailures++;
                }
            }

            var splitFailProtected = new ApproachSplitFailAggregation
            {
                LocationIdentifier = location.LocationIdentifier,
                BinStartTime = input.Item2.OrderBy(i => i.Timestamp).Select(j => j.Timestamp).FirstOrDefault(),
                PhaseNumber = protectedPhaseNumber,
                IsProtectedPhase = IsProtected,
                ApproachId = approach.Id,
                SplitFailures = splitFailures, // how many times the 85% was hit for both green and red
                GreenOccupancySum = greenOccupancySum, //how long the detector was on (EC 81on, 82 off)
                RedOccupancySum = redOccupancySum, // how long the detector was on those 5 sec. (after 9)
                GreenTimeSum = phaseInfo.GreenTimeSum,
                RedTimeSum = phaseInfo.RedTimeSum,
                Cycles = phaseInfo.Cycles
            };
            return splitFailProtected;
        }
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
}
