#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.Workflows/AggregatePedestrianPhasesWorkflow.cs
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
    /// A workflow that aggregates pedestrian phase data from event logs.
    /// </summary>
    /// <remarks>
    /// This workflow orchestrates multiple processing steps to filter, transform, and aggregate
    /// pedestrian-related event data. It uses <see cref="FilterEventsByTypeStep{T}"/> to select
    /// relevant events, <see cref="FilterPedDataProcessStep"/> to refine pedestrian data,
    /// and <see cref="AggregatePedestrianPhasesStep"/> to produce aggregated pedestrian phase results.
    /// </remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AggregatePedestrianPhasesWorkflow"/> class
    /// with the specified workflow options.
    /// </remarks>
    /// <param name="options">
    /// The workflow options used to configure execution, such as timeline, parallelism, and cancellation.
    /// Defaults to <c>null</c> if not provided.
    /// </param>
    public class AggregatePedestrianPhasesWorkflow(AggregationWorkflowOptions options = default) : AggregationWorkflowBase<PhasePedAggregation>(options)
    {
        /// <summary>
        /// Gets the step that filters events by type, specifically <see cref="IndianaEvent"/>.
        /// </summary>
        public FilterEventsByTypeStep<IndianaEvent> FilterEventsByTypeStep { get; private set; }

        /// <summary>
        /// Gets the step that processes and filters pedestrian-specific data.
        /// </summary>
        public FilterPedDataProcessStep FilterPedDataProcessStep { get; private set; }

        /// <summary>
        /// Gets the step that aggregates pedestrian phase data into <see cref="PhasePedAggregation"/> results.
        /// </summary>
        public AggregatePedestrianPhasesStep AggregatePedestrianPhasesStep { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterEventsByTypeStep);
            Steps.Add(FilterPedDataProcessStep);
            Steps.Add(AggregatePedestrianPhasesStep);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterEventsByTypeStep = new(executionBlockOptions);
            FilterPedDataProcessStep = new(blockOptions);
            AggregatePedestrianPhasesStep = new(workflowOptions.Timeline, executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterEventsByTypeStep, new DataflowLinkOptions() { PropagateCompletion = true });

            FilterEventsByTypeStep.LinkTo(FilterPedDataProcessStep, new DataflowLinkOptions() { PropagateCompletion = true });
            FilterPedDataProcessStep.LinkTo(AggregatePedestrianPhasesStep, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregatePedestrianPhasesStep.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
