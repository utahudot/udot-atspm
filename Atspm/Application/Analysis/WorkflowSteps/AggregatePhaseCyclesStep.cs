#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregatePhaseCyclesStep.cs
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
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// A workflow step that aggregates phase cycle data from <see cref="IndianaEvent"/> logs
    /// into <see cref="PhaseCycleAggregation"/> results.
    /// </summary>
    /// <remarks>
    /// This step processes raw event logs for a given location, identifies red-to-red and green-to-green cycles,
    /// and computes aggregated statistics such as cycle counts and total time spent in red, yellow, and green intervals.
    /// Results are grouped by approach and segmented according to the provided <see cref="Timeline{T}"/>.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AggregatePhaseCyclesStep"/> class
    /// with the specified timeline and dataflow block options.
    /// </remarks>
    /// <param name="timeline">
    /// The timeline used to segment aggregation results into defined start and end ranges.
    /// </param>
    /// <param name="dataflowBlockOptions">
    /// Options that configure execution behavior of the dataflow block, such as cancellation and parallelism.
    /// Defaults to <c>null</c> if not provided.
    /// </param>
    public class AggregatePhaseCyclesStep(Timeline<StartEndRange> timeline, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<PhaseCycleAggregation>>(dataflowBlockOptions)
    {
        private readonly Timeline<StartEndRange> _timeline = timeline;

        /// <summary>
        /// Processes the input tuple by aggregating phase cycle data for the given location.
        /// </summary>
        /// <param name="input">
        /// A tuple containing the <see cref="Location"/> and a collection of <see cref="IndianaEvent"/> instances.
        /// </param>
        /// <param name="cancelToken">
        /// A cancellation token used to cancel the operation if requested.
        /// </param>
        /// <returns>
        /// A task that produces a collection of <see cref="PhaseCycleAggregation"/> results,
        /// segmented by the provided timeline.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The method filters events using <see cref="EventLogSpecification"/> and 
        /// <see cref="IndianaPhaseIntervalChangesDataSpecification"/>, then groups them by phase number.
        /// </para>
        /// <para>
        /// Red-to-red and green-to-green cycles are identified, and distinct intervals for red, yellow,
        /// and green phases are computed. Aggregated statistics include cycle counts, phase begin counts,
        /// and total time spent in each interval type.
        /// </para>
        /// </remarks>
        protected override Task<IEnumerable<PhaseCycleAggregation>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var (location, rawEvents) = input;

            var eventsByParam = rawEvents
                .FromSpecification(new EventLogSpecification(location))
                .Cast<IndianaEvent>()
                .FromSpecification(new IndianaPhaseIntervalChangesDataSpecification())
                .ToParamLookup();

            var result = location.Approaches
                .AsParallel()
                .SelectMany(approach =>
                {
                    var matchingEvents = eventsByParam[(short)approach.ProtectedPhaseNumber];

                    var redCycles = matchingEvents.IdentifyRedToRedCycles();
                    var greenCycles = matchingEvents.IdentifyGreenToGreenCycles();
                    var cycles = redCycles.Concat<CycleBase>(greenCycles);

                    var compare = new LambdaEqualityComparer<StartEndRange>((a, b) => a.Start == b.Start && a.End == b.End);
                    var r = cycles.Select(s => s.RedInterval).Distinct(compare).ToList();
                    var y = cycles.Select(s => s.YellowInterval).Distinct(compare).ToList();
                    var g = cycles.Select(s => s.GreenInterval).Distinct(compare).ToList();

                    return _timeline.Segments.Select(segment =>
                    {
                        var agg = new PhaseCycleAggregation
                        {
                            Start = segment.Start,
                            End = segment.End,
                            LocationIdentifier = approach.Location.LocationIdentifier,
                            ApproachId = approach.Id,
                            PhaseNumber = approach.ProtectedPhaseNumber,
                            TotalRedToRedCycles = redCycles.Count(c => segment.InRange(c.Start)),
                            TotalGreenToGreenCycles = greenCycles.Count(c => segment.InRange(c.Start)),
                            PhaseBeginCount = matchingEvents.Count(c => c.EventCode == (short)IndianaEnumerations.PhaseOn && segment.InRange(c.Timestamp)),
                            RedTime = (int)Math.Round(r.Where(w => segment.InRange(w.Start)).Sum(s => s.Span().TotalSeconds), MidpointRounding.AwayFromZero),
                            YellowTime = (int)Math.Round(y.Where(w => segment.InRange(w.Start)).Sum(s => s.Span().TotalSeconds), MidpointRounding.AwayFromZero),
                            GreenTime = (int)Math.Round(g.Where(w => segment.InRange(w.Start)).Sum(s => s.Span().TotalSeconds), MidpointRounding.AwayFromZero),
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