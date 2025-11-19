#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregatePedestrianPhasesStep.cs
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
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// Aggregates pedestrian phase data for a given <see cref="Location"/> and its approaches from a collection of <see cref="IndianaEvent"/> objects.
    /// This class processes event data into time-binned <see cref="PhasePedAggregation"/> results, including counts and delay metrics
    /// for pedestrian cycles, requests, and detections. Designed for use in ATSPM workflow analysis pipelines.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AggregatePedestrianPhasesStep"/> class.
    /// </remarks>
    /// <param name="timeline">The timeline used to segment and aggregate pedestrian phase data by time intervals.</param>
    /// <param name="dataflowBlockOptions">Options for configuring the dataflow block execution. Optional.</param>
    public class AggregatePedestrianPhasesStep(Timeline<StartEndRange> timeline, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<PhasePedAggregation>>(dataflowBlockOptions)
    {
        private readonly Timeline<StartEndRange> _timeline = timeline;

        /// <inheritdoc/>
        protected override Task<IEnumerable<PhasePedAggregation>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var (location, rawEvents) = input;

            var eventsByParam = rawEvents
                .FromSpecification(new IndianaLogLocationFilterSpecification(location))
                .FromSpecification(new IndianaPedDataSpecification())
                .ToParamLookup();

            var result = location.Approaches
                .AsParallel()
                .SelectMany(approach =>
                {
                    var matchingEvents = eventsByParam[(short)approach.ProtectedPhaseNumber];

                    var pedCycles = matchingEvents.IdentifyPedCycles(approach.IsPedestrianPhaseOverlap);
                    var pedDelayCycles = matchingEvents.IdentifyPedDelayCycles(approach.IsPedestrianPhaseOverlap);

                    return _timeline.Segments.Select(segment =>
                    {
                        var inRangePedCycle = pedCycles.Where(c => segment.InRange(c.PedestrianBeginWalk)).ToList();
                        var inRangePedDelayCycle = pedDelayCycles.Where(c => segment.InRange(c.BeginWalk)).ToList();

                        var agg = new PhasePedAggregation
                        {
                            Start = segment.Start,
                            End = segment.End,
                            LocationIdentifier = approach.Location.LocationIdentifier,
                            PhaseNumber = approach.ProtectedPhaseNumber,
                            PedBeginWalkCount = inRangePedCycle.Count,
                            PedRequests = inRangePedCycle.Sum(c => c.PedRequests.Count),
                            UniquePedDetections = inRangePedCycle.Sum(c => c.UniquePedDetections),
                            ImputedPedCallsRegistered = inRangePedCycle.Sum(c => c.ImputedCalls),
                            PedCallsRegisteredCount = inRangePedCycle.Sum(c => c.PedCallsRegistered.Count),
                            PedCycles = inRangePedDelayCycle.Count,
                            PedDelay = Math.Round(inRangePedDelayCycle.Sum(c => c.PedDelay), 1),
                            MinPedDelay = inRangePedDelayCycle.Where(c => c.PedDelay > 0).Select(c => c.PedDelay).DefaultIfEmpty(0).Min(),
                            MaxPedDelay = inRangePedDelayCycle.Select(c => c.PedDelay).DefaultIfEmpty(0).Max()
                        };

                        return agg;
                    });
                })
                .ToList()
                .AsEnumerable();

            return Task.FromResult(result);
        }

    }
}