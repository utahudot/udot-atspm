#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.Workflows/AggregatePhaseCyclesWorkflow.cs
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
    /// A workflow that aggregates phase split monitor data from event logs.
    /// </summary>
    /// <remarks>
    /// This workflow orchestrates multiple processing steps to filter, transform, and aggregate
    /// phase split information. It uses <see cref="FilterEventsByTypeStep{T}"/> to select relevant
    /// Indiana events and <see cref="AggregatePhaseSplitMonitorStep"/> to produce aggregated
    /// <see cref="PhaseSplitMonitorAggregation"/> results.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AggregatePhaseSplitMonitorWorkflow"/> class
    /// with the specified workflow options.
    /// </remarks>
    /// <param name="options">
    /// The workflow options used to configure execution, such as timeline, parallelism, and cancellation.
    /// Defaults to <c>null</c> if not provided.
    /// </param>
    public class AggregatePhaseSplitMonitorWorkflow(AggregationWorkflowOptions options = default): AggregationWorkflowBase<PhaseSplitMonitorAggregation>(options)
    {
        /// <summary>
        /// Gets the step that filters event logs to only include <see cref="IndianaEvent"/> instances.
        /// </summary>
        public FilterEventsByTypeStep<IndianaEvent> FilterIndianaEvents { get; private set; }

        /// <summary>
        /// Gets the step that processes and filters phase interval change date data.
        /// </summary>
        public FilterPhaseIntervalChangeDateProcessStep FilterPhaseIntervalChangeDateProcessStep { get; private set; }

        /// <summary>
        /// Gets the step that aggregates phase split monitor data into 
        /// <see cref="PhaseSplitMonitorAggregation"/> results.
        /// </summary>
        public AggregatePhaseSplitMonitorStep AggregatePhaseSplitMonitorStep { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(FilterPhaseIntervalChangeDateProcessStep);
            Steps.Add(AggregatePhaseSplitMonitorStep);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterIndianaEvents = new(executionBlockOptions);
            FilterPhaseIntervalChangeDateProcessStep = new(blockOptions);
            AggregatePhaseSplitMonitorStep = new(workflowOptions.Timeline, executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterIndianaEvents, new DataflowLinkOptions { PropagateCompletion = true });

            FilterIndianaEvents.LinkTo(FilterPhaseIntervalChangeDateProcessStep, new DataflowLinkOptions() { PropagateCompletion = true });
            FilterPhaseIntervalChangeDateProcessStep.LinkTo(AggregatePhaseSplitMonitorStep, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregatePhaseSplitMonitorStep.LinkTo(Output, new DataflowLinkOptions { PropagateCompletion = true });
        }
    }
}
