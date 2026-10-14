#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.Workflows/AggregateControllerDataWorkflow.cs
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
    /// A workflow that aggregates priority event data from event logs.
    /// </summary>
    /// <remarks>
    /// This workflow filters, processes, and aggregates signal priority events
    /// into summarized <see cref="PriorityAggregation"/> data.
    /// </remarks>
    public class AggregatePriorityWorkflow : AggregationWorkflowBase<PriorityAggregation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregatePriorityWorkflow"/> class
        /// with the specified workflow options.
        /// </summary>
        /// <param name="options">
        /// The workflow options used to configure execution, such as timeline, parallelism, and cancellation.
        /// Defaults to <c>null</c> if not provided.
        /// </param>
        public AggregatePriorityWorkflow(AggregationWorkflowOptions options = default) : base(options)
        {
        }

        /// <summary>
        /// Gets the step that filters events by type to select <see cref="IndianaEvent"/> instances.
        /// </summary>
        public FilterEventsByTypeStep<IndianaEvent> FilterIndianaEvents { get; set; }

        /// <summary>
        /// Gets the step that filters and refines priority-specific data.
        /// </summary>
        public FilterPriorityDataProcessStep FilterPriorityData { get; private set; }

        /// <summary>
        /// Gets the step that aggregates processed priority events into <see cref="PriorityAggregation"/> results.
        /// </summary>
        public AggregatePriorityStep AggregatePriorityCodes { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(FilterPriorityData);
            Steps.Add(AggregatePriorityCodes);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterIndianaEvents = new(executionBlockOptions);
            FilterPriorityData = new(blockOptions);
            AggregatePriorityCodes = new(workflowOptions.Timeline, executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterIndianaEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            FilterIndianaEvents.LinkTo(FilterPriorityData, new DataflowLinkOptions() { PropagateCompletion = true });
            FilterPriorityData.LinkTo(AggregatePriorityCodes, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregatePriorityCodes.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
