#region license
// Copyright 2025 Utah Departement of Transportation
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
using Utah.Udot.Atspm.Analysis.Plans;
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

    public class LocationPlansAggregationWorkflow : AggregationWorkflowBase<SignalPlanAggregation>
    {
        /// <inheritdoc/>
        public LocationPlansAggregationWorkflow(AggregationWorkflowOptions options = default) : base(options)
        {
        }

        public FilterEventsByTypeStep<IndianaEvent> FilterIndianaEvents { get; set; }
        public FilterPlanDataProcessStep FilteredPlanData { get; private set; }
        public GroupLocationByParameter GroupLocationPlans { get; private set; }
        public CalculateTimingPlans<Plan> CalculateTimingPlans { get; private set; }
        public AggregateLocationPlans AggregateLocationPlans { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(FilteredPlanData);
            Steps.Add(GroupLocationPlans);
            Steps.Add(CalculateTimingPlans);
            Steps.Add(AggregateLocationPlans);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterIndianaEvents = new(executionBlockOptions);
            FilteredPlanData = new(blockOptions);
            GroupLocationPlans = new(executionBlockOptions);
            CalculateTimingPlans = new(executionBlockOptions);
            AggregateLocationPlans = new(executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterIndianaEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            FilterIndianaEvents.LinkTo(FilteredPlanData, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPlanData.LinkTo(GroupLocationPlans, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupLocationPlans.LinkTo(CalculateTimingPlans, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTimingPlans.LinkTo(AggregateLocationPlans, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregateLocationPlans.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    public class PreemptCodesAggregationWorkflow : AggregationWorkflowBase<PreemptionAggregation>
    {
        /// <inheritdoc/>
        public PreemptCodesAggregationWorkflow(AggregationWorkflowOptions options = default) : base(options)
        {
        }

        public FilterEventsByTypeStep<IndianaEvent> FilterIndianaEvents { get; set; }
        public FilterPreemptionDataProcessStep FilteredPreemptionData { get; private set; }
        public GroupLocationByParameter GroupPreemptNumber { get; private set; }
        public AggregatePreemptCodes AggregatePreemptCodes { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(FilteredPreemptionData);
            Steps.Add(GroupPreemptNumber);
            Steps.Add(AggregatePreemptCodes);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterIndianaEvents = new(executionBlockOptions);
            FilteredPreemptionData = new(blockOptions);
            GroupPreemptNumber = new(executionBlockOptions);
            AggregatePreemptCodes = new(executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterIndianaEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            FilterIndianaEvents.LinkTo(FilteredPreemptionData, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredPreemptionData.LinkTo(GroupPreemptNumber, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupPreemptNumber.LinkTo(AggregatePreemptCodes, new DataflowLinkOptions() { PropagateCompletion = true });

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
        public GroupLocationByParameter GroupPriorityNumber { get; private set; }
        public AggregatePriorityCodes AggregatePriorityCodes { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(FilterPriorityData);
            Steps.Add(GroupPriorityNumber);
            Steps.Add(AggregatePriorityCodes);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterIndianaEvents = new(executionBlockOptions);
            FilterPriorityData = new(blockOptions);
            GroupPriorityNumber = new(executionBlockOptions);
            AggregatePriorityCodes = new(executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterIndianaEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            FilterIndianaEvents.LinkTo(FilterPriorityData, new DataflowLinkOptions() { PropagateCompletion = true });
            FilterPriorityData.LinkTo(GroupPriorityNumber, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupPriorityNumber.LinkTo(AggregatePriorityCodes, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregatePriorityCodes.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
