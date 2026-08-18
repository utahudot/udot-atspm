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
    /// An aggregation workflow that coordinates the processing and binning of raw controller events 
    /// into phase-specific termination counts (Gap Outs, Max Outs, Force Offs, and Unknowns) 
    /// segmented across a specified timeline.
    /// </summary>
    /// <remarks>
    /// This workflow filters events by type and termination criteria, and aggregates them using
    /// the consolidated and optimized <see cref="AggregatePhaseTerminationStep"/>.
    /// </remarks>
    public class AggregatePhaseTerminationsWorkflow(AggregationWorkflowOptions options = default) : AggregationWorkflowBase<PhaseTerminationAggregation>(options)
    {
        /// <inheritdoc/>
        public FilterEventsByTypeStep<IndianaEvent> FilterEventsByTypeStep { get; set; }

        /// <inheritdoc/>
        public FilterTerminationsProcessStep FilteredTerminationsStep { get; private set; }

        /// <inheritdoc/>
        public AggregatePhaseTerminationStep AggregatePhaseTerminationStep { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterEventsByTypeStep);
            Steps.Add(FilteredTerminationsStep);
            Steps.Add(AggregatePhaseTerminationStep);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterEventsByTypeStep = new(executionBlockOptions);
            FilteredTerminationsStep = new(blockOptions);
            AggregatePhaseTerminationStep = new(workflowOptions.Timeline, executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterEventsByTypeStep, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredTerminationsStep.LinkTo(AggregatePhaseTerminationStep, new DataflowLinkOptions() { PropagateCompletion = true });
            AggregatePhaseTerminationStep.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
