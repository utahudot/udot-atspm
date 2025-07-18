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
    public class AggregateSplitMonitor : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<PhaseSplitMonitorAggregation>>
    {
        private readonly TimeSpan _binSize;

        /// <inheritdoc/>
        public AggregateSplitMonitor(TimeSpan binSize, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions)
        {
            _binSize = binSize;
        }

        /// <inheritdoc/>
        protected override Task<IEnumerable<PhaseSplitMonitorAggregation>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var phaseSplitMonitor = new List<PhaseSplitMonitorAggregation>();
            var location = input.Item1;
            var indianaEvents = input.Item2;
            var groupedIndianaEvents = indianaEvents
                .GroupBy(i => i.EventParam)
                .ToDictionary(g => g.Key, g => g.ToList());
            var listPhaseInformation = new List<PhaseSplitMonitorDto>();
            foreach (var phaseWithIndianaEvents in groupedIndianaEvents)
            {
                var phase = phaseWithIndianaEvents.Key;
                var phaseIndianaEvents = phaseWithIndianaEvents.Value.OrderBy(i => i.Timestamp).ToList();
                //This must go Event Code 1, 8, 11 also split out by phase numbers in Event Param

                // What is the time between 1 and 11
                //Count the phases (how many complete cycles of 1, 8, and 11)
                int cycleCount = 0;
                List<TimeSpan> durations = new List<TimeSpan>();

                for (int i = 0; i < phaseIndianaEvents.Count - 2; i++)
                {
                    var first = phaseIndianaEvents[i];
                    var second = phaseIndianaEvents[i + 1];
                    var third = phaseIndianaEvents[i + 2];

                    // Check for sequence 1 -> 8 -> 11
                    if (first.EventCode == 1 &&
                        second.EventCode == 8 &&
                        third.EventCode == 11)
                    {
                        // Valid cycle found
                        cycleCount++;

                        TimeSpan timeBetween1And11 = third.Timestamp - first.Timestamp;
                        durations.Add(timeBetween1And11);

                        Console.WriteLine($"  Cycle {cycleCount}: Time between EventCode 1 and 11 = {timeBetween1And11}");

                        // Skip to next possible cycle (non-overlapping)
                        i += 2;
                    }
                }
                listPhaseInformation.Add(new PhaseSplitMonitorDto
                {
                    PhaseNumber = phase,
                    PhaseCount = cycleCount,
                    Durations = durations
                });
            }

            var maxPhaseNumberCycles = listPhaseInformation.Max(i => i.PhaseCount);
            foreach (var phase in listPhaseInformation)
            {
                double eightyfifthPercentileSplit = AtspmMath.Percentile(phase.Durations.Select(d => d.TotalSeconds).ToList(), 85);
                var skippedCount = maxPhaseNumberCycles - phase.PhaseCount;
                var splitMonitor = new PhaseSplitMonitorAggregation
                {
                    LocationIdentifier = location.LocationIdentifier,
                    BinStartTime = input.Item2.OrderBy(i => i.Timestamp).Select(j => j.Timestamp).FirstOrDefault(),
                    PhaseNumber = phase.PhaseNumber,
                    EightyFifthPercentileSplit = eightyfifthPercentileSplit,
                    SkippedCount = skippedCount
                };
                phaseSplitMonitor.Add(splitMonitor);
            }

            var enumerable = phaseSplitMonitor.AsEnumerable();
            return Task.FromResult(enumerable);
        }
    }

    internal class PhaseSplitMonitorDto
    {
        public int PhaseNumber { get; set; }
        public int PhaseCount { get; set; }
        public List<TimeSpan> Durations { get; set; }
    }
}
