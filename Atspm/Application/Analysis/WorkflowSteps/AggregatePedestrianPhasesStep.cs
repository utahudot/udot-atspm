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
    /// A workflow step that aggregates pedestrian phase data from <see cref="IndianaEvent"/> logs
    /// into <see cref="PhasePedAggregation"/> results.
    /// </summary>
    /// <remarks>
    /// This step processes raw event logs for a given location, identifies pedestrian cycles and delay cycles,
    /// and computes aggregated statistics such as pedestrian requests, detections, calls, and delays.
    /// Results are grouped by approach and segmented according to the provided <see cref="Timeline{T}"/>.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AggregatePedestrianPhasesStep"/> class
    /// with the specified timeline and dataflow block options.
    /// </remarks>
    /// <param name="timeline">
    /// The timeline used to segment aggregation results into defined start and end ranges.
    /// </param>
    /// <param name="dataflowBlockOptions">
    /// Options that configure execution behavior of the dataflow block, such as cancellation and parallelism.
    /// Defaults to <c>null</c> if not provided.
    /// </param>
    public class AggregatePedestrianPhasesStep(Timeline<StartEndRange> timeline, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<PhasePedAggregation>>(dataflowBlockOptions)
    {
        private readonly Timeline<StartEndRange> _timeline = timeline;

        /// <summary>
        /// Processes the input tuple by aggregating pedestrian phase data for the given location.
        /// </summary>
        /// <param name="input">
        /// A tuple containing the <see cref="Location"/> and a collection of <see cref="IndianaEvent"/> instances.
        /// </param>
        /// <param name="cancelToken">
        /// A cancellation token used to cancel the operation if requested.
        /// </param>
        /// <returns>
        /// A task that produces a collection of <see cref="PhasePedAggregation"/> results,
        /// segmented by the provided timeline.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The method filters events using <see cref="EventLogSpecification"/> and 
        /// <see cref="IndianaPedDataSpecification"/>, then groups them by phase number.
        /// </para>
        /// <para>
        /// Pedestrian cycles and delay cycles are identified, and aggregated statistics include:
        /// <list type="bullet">
        /// <item><description>Counts of pedestrian begin-walk events</description></item>
        /// <item><description>Total pedestrian requests and registered calls</description></item>
        /// <item><description>Unique pedestrian detections</description></item>
        /// <item><description>Imputed pedestrian calls</description></item>
        /// <item><description>Cycle counts and delay metrics (total, min, max)</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        protected override Task<IEnumerable<PhasePedAggregation>> Process(
            Tuple<Location, IEnumerable<IndianaEvent>> input,
            CancellationToken cancelToken = default)
        {
            var (location, rawEvents) = input;

            var eventsByParam = rawEvents
                .FromSpecification(new EventLogSpecification(location))
                .Cast<IndianaEvent>()
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