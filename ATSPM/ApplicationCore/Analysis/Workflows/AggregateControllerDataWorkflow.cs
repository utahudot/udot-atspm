using ATSPM.Application.Analysis.ApproachVolume;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Models;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.Workflows
{
    public class AggregateControllerDataWorkflow : WorkflowBase<Tuple<Signal, IEnumerable<ControllerEventLog>>, IEnumerable<ATSPMAggregationBase>>
    {
        //public GroupSignalsByApproaches GroupSignalsByApproaches { get; private set; }

        //aggregate detector events
        public FilteredDetectorData FilteredDetectorData { get; private set; }
        public GroupSignalsByApproaches GroupSignalsByApproaches1 { get; private set; }
        public GroupDetectorsByDetectorEvent GroupDetectorsByDetectorEvent { get; private set; }
        public AggregateDetectorEvents AggregateDetectorEvents { get; private set; }

        //aggregate termination events
        public FilteredTerminations FilteredTerminations { get; private set; }
        public GroupSignalsByApproaches GroupSignalsByApproaches2 { get; private set; }
        public GroupApproachesByPhase GroupApproachesByPhase { get; private set; }
        public IdentifyTerminationTypesAndTimes IdentifyTerminationTypesAndTimes { get; private set; }
        public AggregatePhaseTerminationEvents AggregatePhaseTerminationEvents { get; private set; }

        //AggregateSignalPlans
        public FilteredPlanData FilteredPlanData { get; private set; }
        public GroupSignalByParameter GroupSignalPlans { get; private set; }
        public CalculateTimingPlans<Plan> CalculateTimingPlans { get; private set; }
        public AggregateSignalPlans AggregateSignalPlans { get; private set; }

        /// <inheritdoc/>
        public override void InstantiateSteps()
        {
            //aggregate detector events
            FilteredDetectorData = new();
            GroupSignalsByApproaches1 = new();
            GroupDetectorsByDetectorEvent = new();
            AggregateDetectorEvents = new();

            //aggregate termination events
            FilteredTerminations = new();
            GroupSignalsByApproaches2 = new();
            GroupApproachesByPhase = new();
            IdentifyTerminationTypesAndTimes = new();
            AggregatePhaseTerminationEvents = new();

            //AggregateSignalPlans
            FilteredPlanData = new();
            GroupSignalPlans = new();
            CalculateTimingPlans = new();
            AggregateSignalPlans = new();

        }

        /// <inheritdoc/>
        public override void AddStepsToTracker()
        {
            //aggregate detector events
            Steps.Add(FilteredDetectorData);
            Steps.Add(GroupSignalsByApproaches1);
            Steps.Add(GroupDetectorsByDetectorEvent);
            Steps.Add(AggregateDetectorEvents);

            //aggregate termination events
            Steps.Add(FilteredTerminations);
            Steps.Add(GroupSignalsByApproaches2);
            Steps.Add(GroupApproachesByPhase);
            Steps.Add(IdentifyTerminationTypesAndTimes);
            Steps.Add(AggregatePhaseTerminationEvents);

            //AggregateSignalPlans
            Steps.Add(FilteredPlanData);
            Steps.Add(GroupSignalPlans);
            Steps.Add(CalculateTimingPlans);
            Steps.Add(AggregateSignalPlans);
        }

        /// <inheritdoc/>
        public override void LinkSteps()
        {
            //link input to event filters
            Input.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });
            Input.LinkTo(FilteredTerminations, new DataflowLinkOptions() { PropagateCompletion = true });
            Input.LinkTo(FilteredPlanData, new DataflowLinkOptions() { PropagateCompletion = true });

            //aggregate detector events
            FilteredDetectorData.LinkTo(GroupSignalsByApproaches1, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupSignalsByApproaches1.LinkTo(GroupDetectorsByDetectorEvent, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupDetectorsByDetectorEvent.LinkTo(AggregateDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            //aggregate termination events
            FilteredTerminations.LinkTo(GroupSignalsByApproaches2, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupSignalsByApproaches2.LinkTo(GroupApproachesByPhase, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupApproachesByPhase.LinkTo(IdentifyTerminationTypesAndTimes, new DataflowLinkOptions() { PropagateCompletion = true });
            IdentifyTerminationTypesAndTimes.LinkTo(AggregatePhaseTerminationEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            //AggregateSignalPlans
            FilteredPlanData.LinkTo(GroupSignalPlans, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupSignalPlans.LinkTo(CalculateTimingPlans, new DataflowLinkOptions() { PropagateCompletion = true });
            CalculateTimingPlans.LinkTo(AggregateSignalPlans, new DataflowLinkOptions() { PropagateCompletion = true });

            //link output to aggregation results
            AggregateDetectorEvents.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
            AggregatePhaseTerminationEvents.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
            AggregateSignalPlans.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
