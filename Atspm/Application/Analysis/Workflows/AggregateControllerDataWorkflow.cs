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

using System.Collections;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Analysis.Plans;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Analysis.Workflows
{
    public class AggregationWorkflowOptions
    {
        public Timeline<StartEndRange> Timeline { get; set; }
        public int MaxDegreeOfParallelism { get; set; } = 1;
        public CancellationToken CancellationToken { get; set; }
    }

    public abstract class AggregationWorkflowBase<T> : WorkflowBase<Tuple<Location, IEnumerable<EventLogModelBase>>, IEnumerable<T>> where T : AggregationModelBase
    {
        protected AggregationWorkflowOptions workflowOptions;
        protected ExecutionDataflowBlockOptions executionBlockOptions;

        /// <inheritdoc/>
        public AggregationWorkflowBase(AggregationWorkflowOptions options = default) : base(new DataflowBlockOptions() { CancellationToken = options.CancellationToken })
        {
            workflowOptions = options;
            executionBlockOptions = new ExecutionDataflowBlockOptions()
            {
                CancellationToken = options.CancellationToken,
                MaxDegreeOfParallelism = options.MaxDegreeOfParallelism,
            };
        }
    }




    






    


    public class FilterEventsByType<T>(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformProcessStepBase<Tuple<Location, IEnumerable<EventLogModelBase>>, Tuple<Location, IEnumerable<T>>>(dataflowBlockOptions) where T : EventLogModelBase
    {
        protected override Task<Tuple<Location, IEnumerable<T>>> Process(Tuple<Location, IEnumerable<EventLogModelBase>> input, CancellationToken cancelToken = default)
        {
            var location = input.Item1;
            var events = input.Item2
                .FromSpecification(new EventLogSpecification(location))
                .Where(w => w.GetType() == typeof(T))
                .Cast<T>()
                .ToList()
                .AsEnumerable();

            return Task.FromResult(Tuple.Create(location, events));
        }
    }


    public class AggregatePedestrianPhasesWorkflow(AggregationWorkflowOptions options = default) : AggregationWorkflowBase<PhasePedAggregation>(options)
    {
        public FilterEventsByType<IndianaEvent> FilterIndianaEvents { get; private set; }
        public FilterPedDataProcessStep FilterPedDataProcessStep { get; private set; }
        public AggregatePedestrianPhasesStep AggregatePedestrianPhases { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(FilterPedDataProcessStep);
            Steps.Add(AggregatePedestrianPhases);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterIndianaEvents = new(executionBlockOptions);
            FilterPedDataProcessStep = new(blockOptions);
            AggregatePedestrianPhases = new(workflowOptions.Timeline, executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterIndianaEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            FilterIndianaEvents.LinkTo(FilterPedDataProcessStep, new DataflowLinkOptions() { PropagateCompletion = true });
            FilterPedDataProcessStep.LinkTo(AggregatePedestrianPhases, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregatePedestrianPhases.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }




    /// <inheritdoc/>
    public class AggregateDetectorEventCountWorkflow(AggregationWorkflowOptions options = default) : AggregationWorkflowBase<DetectorEventCountAggregation>(options)
    {
        public FilterEventsByType<IndianaEvent> FilterIndianaEvents { get; private set; }
        public FilterDetectorDataProcessStep FilterDetectorDataProcessStep { get; private set; }
        public AggregateDetectorEventsStep AggregateDetectorEvents { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(FilterDetectorDataProcessStep);
            Steps.Add(AggregateDetectorEvents);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterIndianaEvents = new(executionBlockOptions);
            FilterDetectorDataProcessStep = new(blockOptions);
            AggregateDetectorEvents = new(workflowOptions.Timeline, executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterIndianaEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            FilterIndianaEvents.LinkTo(FilterDetectorDataProcessStep, new DataflowLinkOptions() { PropagateCompletion = true });
            FilterDetectorDataProcessStep.LinkTo(AggregateDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });

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

        public FilterEventsByType<IndianaEvent> FilterIndianaEvents { get; set; }
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

        public FilterEventsByType<IndianaEvent> FilterIndianaEvents { get; set; }
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

        public FilterEventsByType<IndianaEvent> FilterIndianaEvents { get; set; }
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

        public FilterEventsByType<IndianaEvent> FilterIndianaEvents { get; set; }
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

    public class AggregateControllerDataWorkflow : WorkflowBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<AggregationModelBase>>
    {
        //aggregate detector events
        //public FilterDetectorDataProcessStep FilterDetectorDataProcessStep { get; private set; }
        //public GroupLocationsByApproaches GroupApproachesForDetectors { get; private set; }
        //public GroupDetectorsByDetectorEvent GroupDetectorsByDetectorEvent { get; private set; }
        //public AggregateDetectorEventsStep AggregateDetectorEventsStep { get; private set; }

        //aggregate termination events
        //public FilterTerminationsProcessStep FilterTerminationsProcessStep { get; private set; }
        //public GroupLocationsByApproaches GroupApproachesForTerminations { get; private set; }
        //public GroupPhaseTerminationsByApproaches GroupPhaseTerminationsByApproaches { get; private set; }
        //public IdentifyTerminationTypesAndTimes IdentifyTerminationTypesAndTimes { get; private set; }
        //public AggregatePhaseTerminationEvents AggregatePhaseTerminationEvents { get; private set; }

        //aggregate Location plans
        //public FilterPlanDataProcessStep FilterPlanDataProcessStep { get; private set; }
        //public GroupLocationByParameter GroupLocationPlans { get; private set; }
        //public CalculateTimingPlans<Plan> CalculateTimingPlans { get; private set; }
        //public AggregateLocationPlans AggregateLocationPlans { get; private set; }

        //aggregate preempt codes
        //public FilterPreemptionDataProcessStep FilterPreemptionDataProcessStep { get; private set; }
        //public GroupLocationByParameter GroupPreemptNumber { get; private set; }
        //public AggregatePreemptCodes AggregatePreemptCodes { get; private set; }

        //aggregate priority codes
        //public FilterPriorityDataProcessStep FilterPriorityDataProcessStep { get; private set; }
        //public GroupLocationByParameter GroupPriorityNumber { get; private set; }
        //public AggregatePriorityCodes AggregatePriorityCodes { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            //aggregate detector events
            //Steps.Add(FilterDetectorDataProcessStep);
            //Steps.Add(GroupApproachesForDetectors);
            //Steps.Add(GroupDetectorsByDetectorEvent);
            //Steps.Add(AggregateDetectorEventsStep);

            //aggregate termination events
            //Steps.Add(FilterTerminationsProcessStep);
            //Steps.Add(GroupApproachesForTerminations);
            //Steps.Add(GroupPhaseTerminationsByApproaches);
            //Steps.Add(IdentifyTerminationTypesAndTimes);
            //Steps.Add(AggregatePhaseTerminationEvents);

            //AggregateLocationPlans
            //Steps.Add(FilterPlanDataProcessStep);
            //Steps.Add(GroupLocationPlans);
            //Steps.Add(CalculateTimingPlans);
            //Steps.Add(AggregateLocationPlans);

            //aggregate preempt codes
            //Steps.Add(FilterPreemptionDataProcessStep);
            //Steps.Add(GroupPreemptNumber);
            //Steps.Add(AggregatePreemptCodes);

            //aggregate priority codes
            //Steps.Add(FilterPriorityDataProcessStep);
            //Steps.Add(GroupPriorityNumber);
            //Steps.Add(AggregatePriorityCodes);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            //aggregate detector events
            //FilterDetectorDataProcessStep = new();
            //GroupApproachesForDetectors = new();
            //GroupDetectorsByDetectorEvent = new();
            //AggregateDetectorEventsStep = new();

            //aggregate termination events
            //FilterTerminationsProcessStep = new();
            //GroupApproachesForTerminations = new();
            //GroupPhaseTerminationsByApproaches = new();
            //IdentifyTerminationTypesAndTimes = new();
            //AggregatePhaseTerminationEvents = new();

            //AggregateLocationPlans
            //FilterPlanDataProcessStep = new();
            //GroupLocationPlans = new();
            //CalculateTimingPlans = new();
            //AggregateLocationPlans = new();

            //aggregate preempt codes
            //FilterPreemptionDataProcessStep = new();
            //GroupPreemptNumber = new();
            //AggregatePreemptCodes = new();

            //aggregate priority codes
            //FilterPriorityDataProcessStep = new();
            //GroupPriorityNumber = new();
            //AggregatePriorityCodes = new();
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            //link input to event filters
            //Input.LinkTo(FilterDetectorDataProcessStep, new DataflowLinkOptions() { PropagateCompletion = true });
            //Input.LinkTo(FilterTerminationsProcessStep, new DataflowLinkOptions() { PropagateCompletion = true });
            //Input.LinkTo(FilterPlanDataProcessStep, new DataflowLinkOptions() { PropagateCompletion = true });
            //Input.LinkTo(FilterPreemptionDataProcessStep, new DataflowLinkOptions() { PropagateCompletion = true });
            //Input.LinkTo(FilterPriorityDataProcessStep, new DataflowLinkOptions() { PropagateCompletion = true });

            //aggregate detector events
            //FilterDetectorDataProcessStep.LinkTo(GroupApproachesForDetectors, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupApproachesForDetectors.LinkTo(GroupDetectorsByDetectorEvent, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupDetectorsByDetectorEvent.LinkTo(AggregateDetectorEventsStep, new DataflowLinkOptions() { PropagateCompletion = true });

            //aggregate termination events
            //FilterTerminationsProcessStep.LinkTo(GroupApproachesForTerminations, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupApproachesForTerminations.LinkTo(GroupPhaseTerminationsByApproaches, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupPhaseTerminationsByApproaches.LinkTo(IdentifyTerminationTypesAndTimes, new DataflowLinkOptions() { PropagateCompletion = true });
            //IdentifyTerminationTypesAndTimes.LinkTo(AggregatePhaseTerminationEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            //AggregateLocationPlans
            //FilterPlanDataProcessStep.LinkTo(GroupLocationPlans, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupLocationPlans.LinkTo(CalculateTimingPlans, new DataflowLinkOptions() { PropagateCompletion = true });
            //CalculateTimingPlans.LinkTo(AggregateLocationPlans, new DataflowLinkOptions() { PropagateCompletion = true });

            //aggregate preempt codes
            //FilterPreemptionDataProcessStep.LinkTo(GroupPreemptNumber, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupPreemptNumber.LinkTo(AggregatePreemptCodes, new DataflowLinkOptions() { PropagateCompletion = true });

            //aggregate priority codes
            //FilterPriorityDataProcessStep.LinkTo(GroupPriorityNumber, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupPriorityNumber.LinkTo(AggregatePriorityCodes, new DataflowLinkOptions() { PropagateCompletion = true });

            //link output to aggregation results
            //AggregateDetectorEventsStep.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
            //AggregatePhaseTerminationEvents.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
            //AggregateLocationPlans.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
            //AggregatePreemptCodes.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
            //AggregatePriorityCodes.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
