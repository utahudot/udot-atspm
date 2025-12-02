#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregateDetectorEventsStep.cs
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
    /// A workflow step that aggregates detector event counts from <see cref="IndianaEvent"/> logs
    /// into <see cref="DetectorEventCountAggregation"/> results.
    /// </summary>
    /// <remarks>
    /// This step processes raw event logs for a given location, filters for vehicle detector events,
    /// and computes aggregated counts of detector activations. Results are grouped by detector and
    /// segmented according to the provided <see cref="Timeline{T}"/>.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AggregateDetectorEventsStep"/> class
    /// with the specified timeline and dataflow block options.
    /// </remarks>
    /// <param name="timeline">
    /// The timeline used to segment aggregation results into defined start and end ranges.
    /// </param>
    /// <param name="dataflowBlockOptions">
    /// Options that configure execution behavior of the dataflow block, such as cancellation and parallelism.
    /// Defaults to <c>null</c> if not provided.
    /// </param>
    public class AggregateDetectorEventsStep(Timeline<StartEndRange> timeline, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<DetectorEventCountAggregation>>(dataflowBlockOptions)
    {
        private readonly Timeline<StartEndRange> _timeline = timeline;

        /// <summary>
        /// Processes the input tuple by aggregating detector event counts for the given location.
        /// </summary>
        /// <param name="input">
        /// A tuple containing the <see cref="Location"/> and a collection of <see cref="IndianaEvent"/> instances.
        /// </param>
        /// <param name="cancelToken">
        /// A cancellation token used to cancel the operation if requested.
        /// </param>
        /// <returns>
        /// A task that produces a collection of <see cref="DetectorEventCountAggregation"/> results,
        /// segmented by the provided timeline.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The method filters events using <see cref="EventLogSpecification"/> and 
        /// <see cref="IndianaDetectorDataSpecification"/>, then restricts results to
        /// <see cref="IndianaEnumerations.VehicleDetectorOn"/> events.
        /// </para>
        /// <para>
        /// Aggregated statistics include the total count of detector activations per detector channel,
        /// grouped by approach and segmented by timeline intervals.
        /// </para>
        /// </remarks>
        protected override Task<IEnumerable<DetectorEventCountAggregation>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var (location, rawEvents) = input;

            var eventsByParam = rawEvents
                .FromSpecification(new EventLogSpecification(location))
                .Cast<IndianaEvent>()
                .FromSpecification(new IndianaDetectorDataSpecification())
                .Where(e => e.EventCode == (short)IndianaEnumerations.VehicleDetectorOn)
                .ToParamLookup();

            var result = location.Approaches
                .AsParallel()
                .SelectMany(a => a.Detectors)
                .SelectMany(d => _timeline.Segments.Select(s => new DetectorEventCountAggregation
                {
                    LocationIdentifier = location.LocationIdentifier,
                    ApproachId = d.ApproachId,
                    DetectorPrimaryId = d.Id,
                    Start = s.Start,
                    End = s.End,
                    EventCount = eventsByParam[(short)d.DetectorChannel].Count(e => s.InRange(e))
                }))
                .ToList()
                .AsEnumerable();

            return Task.FromResult(result);
        }
    }
}
