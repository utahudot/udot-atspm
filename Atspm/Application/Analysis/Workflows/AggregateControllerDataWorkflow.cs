﻿#region license
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
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace Utah.Udot.Atspm.Analysis.Workflows
{
    public class AggregationWorkflowOptions
    {
        public TimeSpan BinSize { get; set; } = TimeSpan.FromMinutes(15);
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












    public class UnboxArchivedEvents : TransformProcessStepBase<Tuple<Location, IEnumerable<CompressedEventLogBase>>, Tuple<Location, IEnumerable<EventLogModelBase>>>
    {
        /// <inheritdoc/>
        public UnboxArchivedEvents(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
        protected override Task<Tuple<Location, IEnumerable<EventLogModelBase>>> Process(Tuple<Location, IEnumerable<CompressedEventLogBase>> input, CancellationToken cancelToken = default)
        {
            var location = input.Item1;
            var events = input.Item2
                .SelectMany(m => m.Data)
                .FromSpecification(new EventLogSpecification(location))
                .ToList()
                .AsEnumerable();

            return Task.FromResult(Tuple.Create(location, events));
        }
    }

    public class FilterEventsByType<T> : TransformProcessStepBase<Tuple<Location, IEnumerable<EventLogModelBase>>, Tuple<Location, IEnumerable<T>>> where T : EventLogModelBase
    {
        /// <inheritdoc/>
        public FilterEventsByType(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

        /// <inheritdoc/>
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







    public class DetectorEventCountAggregationWorkflow : AggregationWorkflowBase<DetectorEventCountAggregation>
    {
        /// <inheritdoc/>
        public DetectorEventCountAggregationWorkflow(AggregationWorkflowOptions options = default) : base(options)
        {
        }

        public FilterEventsByType<IndianaEvent> FilterIndianaEvents { get; set; }
        public FilteredDetectorData FilteredDetectorData { get; private set; }
        public GroupLocationsByApproaches GroupApproachesForDetectors { get; private set; }
        public GroupDetectorsByDetectorEvent GroupDetectorsByDetectorEvent { get; private set; }
        public AggregateDetectorEvents AggregateDetectorEvents { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(FilteredDetectorData);
            Steps.Add(GroupApproachesForDetectors);
            Steps.Add(GroupDetectorsByDetectorEvent);
            Steps.Add(AggregateDetectorEvents);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterIndianaEvents = new(executionBlockOptions);
            FilteredDetectorData = new(blockOptions);
            GroupApproachesForDetectors = new(executionBlockOptions);
            GroupDetectorsByDetectorEvent = new(executionBlockOptions);
            AggregateDetectorEvents = new(TimeSpan.FromMinutes(15), executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterIndianaEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            FilterIndianaEvents.LinkTo(FilteredDetectorData, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredDetectorData.LinkTo(GroupApproachesForDetectors, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupApproachesForDetectors.LinkTo(GroupDetectorsByDetectorEvent, new DataflowLinkOptions() { PropagateCompletion = true });
            GroupDetectorsByDetectorEvent.LinkTo(AggregateDetectorEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregateDetectorEvents.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    public class SpeedAggregationWorkflow : AggregationWorkflowBase<ApproachSpeedAggregation>
    {
        /// <inheritdoc/>
        public SpeedAggregationWorkflow(AggregationWorkflowOptions options = default) : base(options)
        {
        }
        public FilterEventsByType<IndianaEvent> FilterIndianaEvents { get; set; }
        public FilterEventsByType<SpeedEvent> FilterSpeedEvents { get; set; }
        public FilteredIndianaAndSpeedData FilterIndianaAndSpeedEvents { get; set; }
        public GroupLocationApproachByParameter GroupPriorityNumber { get; private set; }
        public AggregateSpeedItemEvents AggregateSpeedItemEvents { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(FilterSpeedEvents);
            Steps.Add(FilterIndianaAndSpeedEvents);
            Steps.Add(GroupPriorityNumber);
            Steps.Add(AggregateSpeedItemEvents);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterIndianaEvents = new(executionBlockOptions);
            FilterSpeedEvents = new(executionBlockOptions);
            FilterIndianaAndSpeedEvents = new(executionBlockOptions);
            GroupPriorityNumber = new(executionBlockOptions);
            AggregateSpeedItemEvents = new(TimeSpan.FromMinutes(15), executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            //TODO - This needs help because it is indiana and speed events
            //Input.LinkTo(FilterIndianaEvents, new DataflowLinkOptions() { PropagateCompletion = true });
            //FilterIndianaEvents.LinkTo(FilterSpeedEvents, new DataflowLinkOptions() { PropagateCompletion = true });
            //FilterSpeedEvents.LinkTo(FilterIndianaAndSpeedEvents, new DataflowLinkOptions() { PropagateCompletion = true });
            //FilterIndianaAndSpeedEvents.LinkTo(GroupPriorityNumber, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupPriorityNumber.LinkTo(AggregateSpeedItemEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            //AggregateSpeedItemEvents.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    public class SplitMonitorAggregationWorkflow : AggregationWorkflowBase<PhaseSplitMonitorAggregation>
    {
        /// <inheritdoc/>
        public SplitMonitorAggregationWorkflow(AggregationWorkflowOptions options = default) : base(options)
        {
        }

        public FilterEventsByType<IndianaEvent> FilterIndianaEvents { get; set; }
        public FilteredSplitMonitor FilteredSplitMonitor { get; set; }
        public AggregateSplitMonitor AggregateSplitMonitor { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(FilteredSplitMonitor);
            Steps.Add(AggregateSplitMonitor);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterIndianaEvents = new(executionBlockOptions);
            FilteredSplitMonitor = new(executionBlockOptions);
            AggregateSplitMonitor = new(TimeSpan.FromMinutes(15), executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterIndianaEvents, new DataflowLinkOptions() { PropagateCompletion = true });
            FilterIndianaEvents.LinkTo(FilteredSplitMonitor, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredSplitMonitor.LinkTo(AggregateSplitMonitor, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregateSplitMonitor.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    public class SplitFailureAggregationWorkflow : AggregationWorkflowBase<ApproachSplitFailAggregation>
    {
        /// <inheritdoc/>
        public SplitFailureAggregationWorkflow(AggregationWorkflowOptions options = default) : base(options)
        {
        }

        public FilterEventsByType<IndianaEvent> FilterIndianaEvents { get; set; }
        public FilteredSplitFail FilteredSplitFail { get; set; }
        public AggregateSplitFailure AggregateSplitFailure { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(FilterIndianaEvents);
            Steps.Add(FilteredSplitFail);
            Steps.Add(AggregateSplitFailure);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            FilterIndianaEvents = new(executionBlockOptions);
            FilteredSplitFail = new(executionBlockOptions);
            AggregateSplitFailure = new(TimeSpan.FromMinutes(15), executionBlockOptions);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(FilterIndianaEvents, new DataflowLinkOptions() { PropagateCompletion = true });
            FilterIndianaEvents.LinkTo(FilteredSplitFail, new DataflowLinkOptions() { PropagateCompletion = true });
            FilteredSplitFail.LinkTo(AggregateSplitFailure, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregateSplitFailure.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
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
        public FilteredTerminations FilteredTerminations { get; private set; }
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
        public FilteredPlanData FilteredPlanData { get; private set; }
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
        public FilteredPreemptionData FilteredPreemptionData { get; private set; }
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
        public FilterPriorityData FilterPriorityData { get; private set; }
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
        //public FilteredDetectorData FilteredDetectorData { get; private set; }
        //public GroupLocationsByApproaches GroupApproachesForDetectors { get; private set; }
        //public GroupDetectorsByDetectorEvent GroupDetectorsByDetectorEvent { get; private set; }
        //public AggregateDetectorEvents AggregateDetectorEvents { get; private set; }

        //aggregate termination events
        //public FilteredTerminations FilteredTerminations { get; private set; }
        //public GroupLocationsByApproaches GroupApproachesForTerminations { get; private set; }
        //public GroupPhaseTerminationsByApproaches GroupPhaseTerminationsByApproaches { get; private set; }
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
            //Steps.Add(GroupPhaseTerminationsByApproaches);
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
            //GroupPhaseTerminationsByApproaches = new();
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
            //GroupApproachesForTerminations.LinkTo(GroupPhaseTerminationsByApproaches, new DataflowLinkOptions() { PropagateCompletion = true });
            //GroupPhaseTerminationsByApproaches.LinkTo(IdentifyTerminationTypesAndTimes, new DataflowLinkOptions() { PropagateCompletion = true });
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
