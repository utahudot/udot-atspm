using ATSPM.Application.Analysis.ApproachVolume;
using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Application.Analysis.WorkflowFilters;
using ATSPM.Application.Analysis.WorkflowSteps;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using ATSPM.Domain.Common;
using ATSPM.Domain.Workflows;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ATSPM.Application.Analysis.Workflows
{
    public class AggregationWorkflowOptions
    {
        public TimeSpan BinSize { get; set; } = TimeSpan.FromMinutes(15);
        public int MaxDegreeOfParallelism { get; set; } = 1;
        public CancellationToken CancellationToken { get; set; }
    }
    
    public abstract class AggregationWorkflowBase<T> : WorkflowBase<Tuple<Location, IEnumerable<ControllerEventLog>>, IEnumerable<T>> where T : ATSPMAggregationBase
    {
        protected AggregationWorkflowOptions workflowOptions;
        protected ExecutionDataflowBlockOptions executionBlockOptions;

        /// <inheritdoc/>
        public AggregationWorkflowBase(AggregationWorkflowOptions options = default) : base(new DataflowBlockOptions() { CancellationToken = options.CancellationToken})
        {
            workflowOptions = options;
            executionBlockOptions = new ExecutionDataflowBlockOptions()
            { 
                CancellationToken = options.CancellationToken,
                MaxDegreeOfParallelism = options.MaxDegreeOfParallelism,
            };
        }
    }

    public class DetectorEventCountAggregationWorkflow : AggregationWorkflowBase<DetectorEventCountAggregation>
    {
        /// <inheritdoc/>
        public DetectorEventCountAggregationWorkflow(AggregationWorkflowOptions options = default) : base(options)
        {   
        }

        public FilteredDetectorData FilteredDetectorData { get; private set; }
        public GroupLocationsByApproaches GroupApproachesForDetectors { get; private set; }
        public GroupDetectorsByDetectorEvent GroupDetectorsByDetectorEvent { get; private set; }
        public AggregateDetectorEvents AggregateDetectorEvents { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilteredDetectorData);
            Steps.Add(GroupApproachesForDetectors);
            Steps.Add(GroupDetectorsByDetectorEvent);
            Steps.Add(AggregateDetectorEvents);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilteredDetectorData = new(blockOptions);
            GroupApproachesForDetectors = new(executionBlockOptions);
            GroupDetectorsByDetectorEvent = new(executionBlockOptions);
            AggregateDetectorEvents = new(executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });

            FilteredDetectorData.LinkTo(GroupApproachesForDetectors, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupApproachesForDetectors.LinkTo(GroupDetectorsByDetectorEvent, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupDetectorsByDetectorEvent.LinkTo(AggregateDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregateDetectorEvents.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    public class PhaseTerminationAggregationWorkflow : AggregationWorkflowBase<PhaseTerminationAggregation>
    {
        private readonly int _consecutiveCount;

        /// <inheritdoc/>
        public PhaseTerminationAggregationWorkflow(int consecutiveCount = 3, AggregationWorkflowOptions options = default) : base(options)
        {
            _consecutiveCount = consecutiveCount;
        }

        public FilteredTerminations FilteredTerminations { get; private set; }
        public GroupLocationsByApproaches GroupApproachesForTerminations { get; private set; }
        public GroupApproachesByPhase GroupApproachesByPhase { get; private set; }
        public IdentifyTerminationTypesAndTimes IdentifyTerminationTypesAndTimes { get; private set; }
        public AggregatePhaseTerminationEvents AggregatePhaseTerminationEvents { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilteredTerminations);
            Steps.Add(GroupApproachesForTerminations);
            Steps.Add(GroupApproachesByPhase);
            Steps.Add(IdentifyTerminationTypesAndTimes);
            Steps.Add(AggregatePhaseTerminationEvents);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilteredTerminations = new(blockOptions);
            GroupApproachesForTerminations = new(executionBlockOptions);
            GroupApproachesByPhase = new(executionBlockOptions);
            IdentifyTerminationTypesAndTimes = new(_consecutiveCount, executionBlockOptions);
            AggregatePhaseTerminationEvents = new(executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilteredTerminations, new DataflowLinkOptions() { PropagateCompletion = true });

            FilteredTerminations.LinkTo(GroupApproachesForTerminations, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupApproachesForTerminations.LinkTo(GroupApproachesByPhase, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupApproachesByPhase.LinkTo(IdentifyTerminationTypesAndTimes, new DataflowLinkOptions() { PropagateCompletion = true });
            IdentifyTerminationTypesAndTimes.LinkTo(AggregatePhaseTerminationEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregatePhaseTerminationEvents.LinkTo(this.Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    public class LocationPlansAggregationWorkflow : AggregationWorkflowBase<LocationPlanAggregation>
    {
        /// <inheritdoc/>
        public LocationPlansAggregationWorkflow(AggregationWorkflowOptions options = default) : base(options)
        {
        }

        public FilteredPlanData FilteredPlanData { get; private set; }
        public GroupLocationByParameter GroupLocationPlans { get; private set; }
        public CalculateTimingPlans<Plan> CalculateTimingPlans { get; private set; }
        public AggregateLocationPlans AggregateLocationPlans { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilteredPlanData);
            Steps.Add(GroupLocationPlans);
            Steps.Add(CalculateTimingPlans);
            Steps.Add(AggregateLocationPlans);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilteredPlanData = new(blockOptions);
            GroupLocationPlans = new(executionBlockOptions);
            CalculateTimingPlans = new(executionBlockOptions);
            AggregateLocationPlans = new(executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilteredPlanData, new DataflowLinkOptions() { PropagateCompletion = true });

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

        public FilteredPreemptionData FilteredPreemptionData { get; private set; }
        public GroupLocationByParameter GroupPreemptNumber { get; private set; }
        public AggregatePreemptCodes AggregatePreemptCodes { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilteredPreemptionData);
            Steps.Add(GroupPreemptNumber);
            Steps.Add(AggregatePreemptCodes);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilteredPreemptionData = new(blockOptions);
            GroupPreemptNumber = new(executionBlockOptions);
            AggregatePreemptCodes = new(executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilteredPreemptionData, new DataflowLinkOptions() { PropagateCompletion = true });

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

        public FilterPriorityData FilterPriorityData { get; private set; }
        public GroupLocationByParameter GroupPriorityNumber { get; private set; }
        public AggregatePriorityCodes AggregatePriorityCodes { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterPriorityData);
            Steps.Add(GroupPriorityNumber);
            Steps.Add(AggregatePriorityCodes);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterPriorityData = new(blockOptions);
            GroupPriorityNumber = new(executionBlockOptions);
            AggregatePriorityCodes = new(executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterPriorityData, new DataflowLinkOptions() { PropagateCompletion = true });

            FilterPriorityData.LinkTo(GroupPriorityNumber, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupPriorityNumber.LinkTo(AggregatePriorityCodes, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregatePriorityCodes.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    public class AggregateControllerDataWorkflow : WorkflowBase<Tuple<Location, IEnumerable<ControllerEventLog>>, IEnumerable<ATSPMAggregationBase>>
    {
        //aggregate detector events
        //public FilteredDetectorData FilteredDetectorData { get; private set; }
        //public GroupLocationsByApproaches GroupApproachesForDetectors { get; private set; }
        //public GroupDetectorsByDetectorEvent GroupDetectorsByDetectorEvent { get; private set; }
        //public AggregateDetectorEvents AggregateDetectorEvents { get; private set; }

        //aggregate termination events
        //public FilteredTerminations FilteredTerminations { get; private set; }
        //public GroupLocationsByApproaches GroupApproachesForTerminations { get; private set; }
        //public GroupApproachesByPhase GroupApproachesByPhase { get; private set; }
        //public IdentifyTerminationTypesAndTimes IdentifyTerminationTypesAndTimes { get; private set; }
        //public AggregatePhaseTerminationEvents AggregatePhaseTerminationEvents { get; private set; }

        //aggregate Location plans
        //public FilteredPlanData FilteredPlanData { get; private set; }
        //public GroupLocationByParameter GroupLocationPlans { get; private set; }
        //public CalculateTimingPlans<Plan> CalculateTimingPlans { get; private set; }
        //public AggregateLocationPlans AggregateLocationPlans { get; private set; }

        //aggregate preempt codes
        //public FilteredPreemptionData FilteredPreemptionData { get; private set; }
        //public GroupLocationByParameter GroupPreemptNumber { get; private set; }
        //public AggregatePreemptCodes AggregatePreemptCodes { get; private set; }

        //aggregate priority codes
        //public FilterPriorityData FilterPriorityData { get; private set; }
        //public GroupLocationByParameter GroupPriorityNumber { get; private set; }
        //public AggregatePriorityCodes AggregatePriorityCodes { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            //aggregate detector events
            //Steps.Add(FilteredDetectorData);
            //Steps.Add(GroupApproachesForDetectors);
            //Steps.Add(GroupDetectorsByDetectorEvent);
            //Steps.Add(AggregateDetectorEvents);

            //aggregate termination events
            //Steps.Add(FilteredTerminations);
            //Steps.Add(GroupApproachesForTerminations);
            //Steps.Add(GroupApproachesByPhase);
            //Steps.Add(IdentifyTerminationTypesAndTimes);
            //Steps.Add(AggregatePhaseTerminationEvents);

            //AggregateLocationPlans
            //Steps.Add(FilteredPlanData);
            //Steps.Add(GroupLocationPlans);
            //Steps.Add(CalculateTimingPlans);
            //Steps.Add(AggregateLocationPlans);

            //aggregate preempt codes
            //Steps.Add(FilteredPreemptionData);
            //Steps.Add(GroupPreemptNumber);
            //Steps.Add(AggregatePreemptCodes);

            //aggregate priority codes
            //Steps.Add(FilterPriorityData);
            //Steps.Add(GroupPriorityNumber);
            //Steps.Add(AggregatePriorityCodes);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            //aggregate detector events
            //FilteredDetectorData = new();
            //GroupApproachesForDetectors = new();
            //GroupDetectorsByDetectorEvent = new();
            //AggregateDetectorEvents = new();

            //aggregate termination events
            //FilteredTerminations = new();
            //GroupApproachesForTerminations = new();
            //GroupApproachesByPhase = new();
            //IdentifyTerminationTypesAndTimes = new();
            //AggregatePhaseTerminationEvents = new();

            //AggregateLocationPlans
            //FilteredPlanData = new();
            //GroupLocationPlans = new();
            //CalculateTimingPlans = new();
            //AggregateLocationPlans = new();

            //aggregate preempt codes
            //FilteredPreemptionData = new();
            //GroupPreemptNumber = new();
            //AggregatePreemptCodes = new();

            //aggregate priority codes
            //FilterPriorityData = new();
            //GroupPriorityNumber = new();
            //AggregatePriorityCodes = new();
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            //link input to event filters
            //Input.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });
            //Input.LinkTo(FilteredTerminations, new DataflowLinkOptions() { PropagateCompletion = true });
            //Input.LinkTo(FilteredPlanData, new DataflowLinkOptions() { PropagateCompletion = true });
            //Input.LinkTo(FilteredPreemptionData, new DataflowLinkOptions() { PropagateCompletion = true });
            //Input.LinkTo(FilterPriorityData, new DataflowLinkOptions() { PropagateCompletion = true });

            //aggregate detector events
            //FilteredDetectorData.LinkTo(GroupApproachesForDetectors, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupApproachesForDetectors.LinkTo(GroupDetectorsByDetectorEvent, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupDetectorsByDetectorEvent.LinkTo(AggregateDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            //aggregate termination events
            //FilteredTerminations.LinkTo(GroupApproachesForTerminations, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupApproachesForTerminations.LinkTo(GroupApproachesByPhase, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupApproachesByPhase.LinkTo(IdentifyTerminationTypesAndTimes, new DataflowLinkOptions() { PropagateCompletion = true });
            //IdentifyTerminationTypesAndTimes.LinkTo(AggregatePhaseTerminationEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            //AggregateLocationPlans
            //FilteredPlanData.LinkTo(GroupLocationPlans, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupLocationPlans.LinkTo(CalculateTimingPlans, new DataflowLinkOptions() { PropagateCompletion = true });
            //CalculateTimingPlans.LinkTo(AggregateLocationPlans, new DataflowLinkOptions() { PropagateCompletion = true });

            //aggregate preempt codes
            //FilteredPreemptionData.LinkTo(GroupPreemptNumber, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupPreemptNumber.LinkTo(AggregatePreemptCodes, new DataflowLinkOptions() { PropagateCompletion = true });

            //aggregate priority codes
            //FilterPriorityData.LinkTo(GroupPriorityNumber, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupPriorityNumber.LinkTo(AggregatePriorityCodes, new DataflowLinkOptions() { PropagateCompletion = true });

            //link output to aggregation results
            //AggregateDetectorEvents.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
            //AggregatePhaseTerminationEvents.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
            //AggregateLocationPlans.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
            //AggregatePreemptCodes.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
            //AggregatePriorityCodes.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
