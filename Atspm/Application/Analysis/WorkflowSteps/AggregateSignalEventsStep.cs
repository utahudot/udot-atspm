#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.WorkflowSteps/AggregateSignalEventsStep.cs
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
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Analysis.WorkflowSteps
{
    /// <summary>
    /// A workflow step that aggregates signal-level event counts from <see cref="IndianaEvent"/> logs
    /// into <see cref="SignalEventCountAggregation"/> results.
    /// </summary>
    /// <remarks>
    /// This step processes raw event logs for a given location, filters events to ensure they belong
    /// to the specified signal, and computes the total count of controller events binned according
    /// to the provided <see cref="Timeline{T}"/>.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AggregateSignalEventsStep"/> class
    /// with the specified timeline and dataflow block options.
    /// </remarks>
    /// <param name="timeline">
    /// The timeline used to segment aggregation results into defined start and end ranges.
    /// </param>
    /// <param name="dataflowBlockOptions">
    /// Options that configure execution behavior of the dataflow block, such as cancellation and parallelism.
    /// Defaults to <c>null</c> if not provided.
    /// </param>
    public class AggregateSignalEventsStep(Timeline<StartEndRange> timeline, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformProcessStepBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<SignalEventCountAggregation>>(dataflowBlockOptions)
    {
        private readonly Timeline<StartEndRange> _timeline = timeline;

        /// <inheritdoc/>
        protected override Task<IEnumerable<SignalEventCountAggregation>> Process(Tuple<Location, IEnumerable<IndianaEvent>> input, CancellationToken cancelToken = default)
        {
            var (location, rawEvents) = input;

            var locationEvents = rawEvents
                .FromSpecification(new EventLogSpecification(location))
                .Cast<IndianaEvent>()
                .ToList();

            var result = _timeline.Segments.Select(segment => new SignalEventCountAggregation
            {
                LocationIdentifier = location.LocationIdentifier,
                Start = segment.Start,
                End = segment.End,
                EventCount = locationEvents.Count(e => segment.InRange(e))
            })
            .ToList()
            .AsEnumerable();

            return Task.FromResult(result);
        }
    }
}
