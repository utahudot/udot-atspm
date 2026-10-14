#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Analysis.Workflows/AggregateApproachSplitFailWorkflow.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
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
    /// Linear TPL Dataflow sub-workflow coordinating Approach Split Fail aggregations.
    /// </summary>
    public class AggregateApproachSplitFailWorkflow(AggregationWorkflowOptions options = default) : AggregationWorkflowBase<ApproachSplitFailAggregation>(options)
    {
        /// <inheritdoc/>
        public FilterEventsByTypeStep<IndianaEvent> FilterIndianaEvents { get; private set; }

        /// <inheritdoc/>
        public FilterSplitFailProcessStep FilterSplitFailProcessStep { get; private set; }

        /// <inheritdoc/>
        public CreateSplitFailCyclesStep CreateCyclesStep { get; private set; }

        /// <inheritdoc/>
        public CreateSplitFailDetectorActivationsStep CreateDetectorActivationsStep { get; private set; }

        /// <inheritdoc/>
        public CalculateSplitFailOccupancyStep CalculateOccupancyStep { get; private set; }

        /// <inheritdoc/>
        public AggregateApproachSplitFailStep AggregateStep { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(FilterSplitFailProcessStep);
            Steps.Add(CreateCyclesStep);
            Steps.Add(CreateDetectorActivationsStep);
            Steps.Add(CalculateOccupancyStep);
            Steps.Add(AggregateStep);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            var flowOptions = executionBlockOptions;

            FilterIndianaEvents = new(flowOptions);
            FilterSplitFailProcessStep = new(flowOptions);
            CreateCyclesStep = new(flowOptions);
            CreateDetectorActivationsStep = new(flowOptions);
            CalculateOccupancyStep = new(5.0, flowOptions);
            AggregateStep = new(workflowOptions.Timeline, flowOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            var propagateOptions = new DataflowLinkOptions { PropagateCompletion = true };

            Input.LinkTo(FilterIndianaEvents, propagateOptions);
            FilterIndianaEvents.LinkTo(FilterSplitFailProcessStep, propagateOptions);
            FilterSplitFailProcessStep.LinkTo(CreateCyclesStep, propagateOptions);
            CreateCyclesStep.LinkTo(CreateDetectorActivationsStep, propagateOptions);
            CreateDetectorActivationsStep.LinkTo(CalculateOccupancyStep, propagateOptions);
            CalculateOccupancyStep.LinkTo(AggregateStep, propagateOptions);
            AggregateStep.LinkTo(Output, propagateOptions);
        }
    }
}
