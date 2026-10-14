#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.Workflows/AggregateSignalEventCountWorkflow.cs
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
    /// A workflow that aggregates total signal event counts from event logs.
    /// </summary>
    /// <remarks>
    /// This workflow orchestrates multiple processing steps to filter and aggregate
    /// total signal-level controller event counts into <see cref="SignalEventCountAggregation"/> results.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AggregateSignalEventCountWorkflow"/> class
    /// with the specified workflow options.
    /// </remarks>
    /// <param name="options">
    /// The workflow options used to configure execution, such as timeline, parallelism, and cancellation.
    /// Defaults to <c>null</c> if not provided.
    /// </param>
    public class AggregateSignalEventCountWorkflow(AggregationWorkflowOptions options = default) : AggregationWorkflowBase<SignalEventCountAggregation>(options)
    {
        /// <inheritdoc/>
        public FilterEventsByTypeStep<IndianaEvent> FilterIndianaEvents { get; private set; }

        /// <inheritdoc/>
        public AggregateSignalEventsStep AggregateSignalEventsStep { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(AggregateSignalEventsStep);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterIndianaEvents = new(executionBlockOptions);
            AggregateSignalEventsStep = new(workflowOptions.Timeline, executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterIndianaEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            FilterIndianaEvents.LinkTo(AggregateSignalEventsStep, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregateSignalEventsStep.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
