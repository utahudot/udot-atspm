#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.Workflows/AggregateDetectorEventCountWorkflow.cs
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

namespace Utah.Udot.Atspm.Analysis.Workflows
{
    /// <summary>
    /// A workflow that aggregates detector event counts from event logs.
    /// </summary>
    /// <remarks>
    /// This workflow orchestrates multiple processing steps to filter, transform, and aggregate
    /// detector-related event data. It uses <see cref="FilterEventsByTypeStep{T}"/> to select
    /// relevant Indiana events, <see cref="FilterDetectorDataProcessStep"/> to refine detector data,
    /// and <see cref="AggregateDetectorEventsStep"/> to produce aggregated
    /// <see cref="DetectorEventCountAggregation"/> results.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AggregateDetectorEventCountWorkflow"/> class
    /// with the specified workflow options.
    /// </remarks>
    /// <param name="options">
    /// The workflow options used to configure execution, such as timeline, parallelism, and cancellation.
    /// Defaults to <c>null</c> if not provided.
    /// </param>
    public class AggregateDetectorEventCountWorkflow(AggregationWorkflowOptions options = default) : AggregationWorkflowBase<DetectorEventCountAggregation>(options)
    {
        /// <summary>
        /// Gets the step that filters event logs to only include <see cref="IndianaEvent"/> instances.
        /// </summary>
        public FilterEventsByTypeStep<IndianaEvent> FilterIndianaEvents { get; private set; }

        /// <summary>
        /// Gets the step that processes and filters detector-specific event data.
        /// </summary>
        public FilterDetectorDataProcessStep FilterDetectorDataProcessStep { get; private set; }

        /// <summary>
        /// Gets the step that aggregates detector events into <see cref="DetectorEventCountAggregation"/> results.
        /// </summary>
        public AggregateDetectorEventsStep AggregateDetectorEvents { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(FilterDetectorDataProcessStep);
            Steps.Add(AggregateDetectorEvents);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterIndianaEvents = new(executionBlockOptions);
            FilterDetectorDataProcessStep = new(blockOptions);
            AggregateDetectorEvents = new(workflowOptions.Timeline, executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterIndianaEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            FilterIndianaEvents.LinkTo(FilterDetectorDataProcessStep, new DataflowLinkOptions() { PropagateCompletion = true });
            FilterDetectorDataProcessStep.LinkTo(AggregateDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregateDetectorEvents.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
