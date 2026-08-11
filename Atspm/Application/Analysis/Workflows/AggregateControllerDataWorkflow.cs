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
    public class PhaseTerminationAggregationWorkflow : AggregationWorkflowBase<PhaseTerminationAggregation>
    {
        private readonly int _consecutiveCount;

        /// <inheritdoc/>
        public PhaseTerminationAggregationWorkflow(int consecutiveCount = 3, AggregationWorkflowOptions options = default) : base(options)
        {
            _consecutiveCount = consecutiveCount;
        }

        public FilterEventsByTypeStep<IndianaEvent> FilterIndianaEvents { get; set; }
        public FilterTerminationsProcessStep FilteredTerminations { get; private set; }
        public GroupLocationsByApproaches GroupApproachesForTerminations { get; private set; }
        public GroupPhaseTerminationsByApproaches GroupApproachesByPhase { get; private set; }
        public IdentifyTerminationTypesAndTimes IdentifyTerminationTypesAndTimes { get; private set; }
        public AggregatePhaseTerminationEvents AggregatePhaseTerminationEvents { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(FilteredTerminations);
            Steps.Add(GroupApproachesForTerminations);
            Steps.Add(GroupApproachesByPhase);
            Steps.Add(IdentifyTerminationTypesAndTimes);
            Steps.Add(AggregatePhaseTerminationEvents);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterIndianaEvents = new(executionBlockOptions);
            FilteredTerminations = new(blockOptions);
            GroupApproachesForTerminations = new(executionBlockOptions);
            GroupApproachesByPhase = new(executionBlockOptions);
            IdentifyTerminationTypesAndTimes = new(_consecutiveCount, executionBlockOptions);
            AggregatePhaseTerminationEvents = new(executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterIndianaEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            FilteredTerminations.LinkTo(GroupApproachesForTerminations, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupApproachesForTerminations.LinkTo(GroupApproachesByPhase, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupApproachesByPhase.LinkTo(IdentifyTerminationTypesAndTimes, new DataflowLinkOptions() { PropagateCompletion = true });
            IdentifyTerminationTypesAndTimes.LinkTo(AggregatePhaseTerminationEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregatePhaseTerminationEvents.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    /// <inheritdoc/>
    public class PreemptCodesAggregationWorkflow(AggregationWorkflowOptions options = default) : AggregationWorkflowBase<PreemptionAggregation>(options)
    {
        public FilterEventsByTypeStep<IndianaEvent> FilterIndianaEvents { get; set; }
        public FilterPreemptionDataProcessStep FilteredPreemptionData { get; private set; }
        public AggregatePreemptStep AggregatePreemptCodes { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(FilteredPreemptionData);
            Steps.Add(AggregatePreemptCodes);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterIndianaEvents = new(executionBlockOptions);
            FilteredPreemptionData = new(blockOptions);
            AggregatePreemptCodes = new(workflowOptions.Timeline, executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterIndianaEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            FilterIndianaEvents.LinkTo(FilteredPreemptionData, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPreemptionData.LinkTo(AggregatePreemptCodes, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregatePreemptCodes.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    public class PriorityCodesAggregationWorkflow : AggregationWorkflowBase<PriorityAggregation>
    {
        /// <inheritdoc/>
        public PriorityCodesAggregationWorkflow(AggregationWorkflowOptions options = default) : base(options)
        {
        }

        public FilterEventsByTypeStep<IndianaEvent> FilterIndianaEvents { get; set; }
        public FilterPriorityDataProcessStep FilterPriorityData { get; private set; }
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
