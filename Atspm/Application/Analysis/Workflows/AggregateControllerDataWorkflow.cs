#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.Workflows/AggregateControllerDataWorkflow.cs
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
    public class AggregationWorkflowOptions
    {
        public TimeSpan BinSize { get; set; } = TimeSpan.FromMinutes(15);
        public int MaxDegreeOfParallelism { get; set; } = 1;
        public CancellationToken CancellationToken { get; set; }
    }

    public abstract class AggregationWorkflowBase<T> : WorkflowBase<Tuple<Location, IEnumerable<IndianaEvent>>, IEnumerable<T>> where T : AggregationModelBase
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

    //public abstract class AggregationSpeedWorkflowBase<T> : WorkflowBase<Tuple<Location, IEnumerable<CompressedEventLogBase>, IEnumerable<T>> where T : AggregationModelBase
    //{
    //    protected AggregationWorkflowOptions workflowOptions;
    //    protected ExecutionDataflowBlockOptions executionBlockOptions;

    //    /// <inheritdoc/>
    //    public AggregationSpeedWorkflowBase(AggregationWorkflowOptions options = default) : base(new DataflowBlockOptions() { CancellationToken = options.CancellationToken })
    //    {
    //        workflowOptions = options;
    //        executionBlockOptions = new ExecutionDataflowBlockOptions()
    //        {
    //            CancellationToken = options.CancellationToken,
    //            MaxDegreeOfParallelism = options.MaxDegreeOfParallelism,
    //        };
    //    }
    //}

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

            AggregatePhaseTerminationEvents.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }

    public class LocationPlansAggregationWorkflow : AggregationWorkflowBase<SignalPlanAggregation>
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

    //public class DetectorSpeedAggregationWorkflow : WorkflowBase<Tuple<Location, IEnumerable<CompressedEventLogBase>>, DetectorSpeedAggregation>
    //{
    //    /// <inheritdoc/>
    //    public DetectorSpeedAggregationWorkflow(DataflowBlockOptions options = default) : base(options)
    //    {
    //    }

    //    //BreakoutCompressedEvents
    //    //FilterIndianaAndSpeedEvents
    //    public FilterSpeedDetectorData FilterSpeedDetectorData { get; private set; }

    //    public CalculateSpeedData CalculateSpeedData { get; private set; }

    //    //Compress Step
    //    //SaveArchiveEvents (
    //    //ArchiveDataEvents
    //    ////////////public UploadSpeedData UploadSpeedData { get; private set; }

    //    /// <inheritdoc/>
    //    protected override void AddStepsToTracker()
    //    {
    //        Steps.Add(FilterSpeedDetectorData);
    //        Steps.Add(BreakOutIndianaEvent);
    //        Steps.Add(BreakOutSpeedEvent);
    //        Steps.Add(GroupDetectorSpeedData);
    //        Steps.Add(GroupDetectorIndianaSpeedData);
    //        Steps.Add(DetectorJoin);
    //        //Steps.Add(CalculateSpeedData);
    //        //Steps.Add(UploadSpeedData);
    //    }

    //    /// <inheritdoc/>
    //    protected override void InstantiateSteps()
    //    {
    //        //FilterSpeedDetectorData = new(blockOptions);
    //        //BreakOutIndianaEvent = new(blockOptions);
    //        //BreakOutSpeedEvent = new(blockOptions);
    //        //GroupDetectorSpeedData = new(blockOptions);
    //        //GroupDetectorIndianaSpeedData = new(blockOptions);

    //        //DetectorJoin = new();
    //        //CalculateSpeedData = new(executionBlockOptions);
    //        //UploadSpeedData = new(executionBlockOptions);
    //    }

    //    /// <inheritdoc/>
    //    protected override void LinkSteps()
    //    {
    //        Input.LinkTo(FilterSpeedDetectorData, new DataflowLinkOptions() { PropagateCompletion = true }, p => p.Any(i => i.DataType == typeof(IndianaEvent)));
    //        FilterSpeedDetectorData.LinkTo(BreakOutIndianaEvent, new DataflowLinkOptions() { PropagateCompletion = true }, p => p.Any(i => i.DataType == typeof(IndianaEvent)));
    //        FilterSpeedDetectorData.LinkTo(BreakOutSpeedEvent, new DataflowLinkOptions() { PropagateCompletion = true });//, p => p.Any(i => i.DataType == typeof(SpeedEvent)));

    //        BreakOutSpeedEvent.LinkTo(GroupDetectorSpeedData, new DataflowLinkOptions() { PropagateCompletion = true });
    //        BreakOutIndianaEvent.LinkTo(GroupDetectorIndianaSpeedData, new DataflowLinkOptions() { PropagateCompletion = true });

    //        GroupDetectorSpeedData.LinkTo(DetectorJoin.Target1);
    //        GroupDetectorIndianaSpeedData.LinkTo(DetectorJoin.Target2);

    //        //GroupPriorityNumber.LinkTo(AggregatePriorityCodes, new DataflowLinkOptions() { PropagateCompletion = true });

    //        //AggregatePriorityCodes.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
    //    }
    //}


    ///// <summary>
    ///// returns a <see cref="CompressedEventLogs{T}"/> object to store to a repository that is grouped by:
    ///// <list type="bullet">
    ///// <item><see cref="ILocationLayer.LocationIdentifier"/></item>
    ///// <item><see cref="ITimestamp.Timestamp"/></item>
    ///// <item><see cref="Device"/></item>
    ///// <item>Event type derived from <see cref="AggregationModelBase"/></item>
    ///// </list>
    ///// </summary>
    //public class ArchiveAggregations : TransformManyProcessStepBaseAsync<IEnumerable<AggregationModelBase>, CompressedAggregationBase>
    //{
    //    /// <summary>
    //    /// returns a <see cref="CompressedEventLogs{T}"/> object to store to a repository that is grouped by:
    //    /// <list type="bullet">
    //    /// <item><see cref="ILocationLayer.LocationIdentifier"/></item>
    //    /// <item><see cref="ITimestamp.Timestamp"/></item>
    //    /// <item><see cref="Device"/></item>
    //    /// <item>Event type derived from <see cref="AggregationModelBase"/></item>
    //    /// </list>
    //    /// </summary>
    //    /// <param name="dataflowBlockOptions"></param>
    //    public ArchiveAggregations(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : base(dataflowBlockOptions) { }

    //    /// <inheritdoc/>
    //    protected override async IAsyncEnumerable<CompressedAggregationBase> Process(IEnumerable<AggregationModelBase> input, [EnumeratorCancellation] CancellationToken cancelToken = default)
    //    {
    //        var result = input.GroupBy(g => (g.LocationIdentifier, g.Start.Date, g.GetType()))
    //            .Select(s =>
    //            {
    //                dynamic list = Activator.CreateInstance(typeof(List<>).MakeGenericType(s.Key.Item3));

    //                foreach (var i in s.Select(s => s))
    //                {
    //                    if (list is IList l)
    //                    {
    //                        l.Add(i);
    //                    }
    //                }

    //                dynamic comp = Activator.CreateInstance(typeof(CompressedAggregations<>).MakeGenericType(s.Key.Item3));

    //                comp.LocationIdentifier = s.Key.LocationIdentifier;
    //                comp.ArchiveDate = DateOnly.FromDateTime(s.Key.Date);
    //                comp.DataType = s.Key.Item3;
    //                //comp.DeviceId = s.Key.Id;
    //                comp.Data = list;

    //                return comp;
    //            });

    //        foreach (var r in result)
    //        {
    //            yield return r;
    //        }
    //    }
    //}

    public class AggregateControllerDataWorkflow : WorkflowBase<Tuple<Location, IEnumerable<IndianaEvent>, IEnumerable<SpeedEvent>>, IEnumerable<AggregationModelBase>>
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
