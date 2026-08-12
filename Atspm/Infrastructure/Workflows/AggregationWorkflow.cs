#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.Workflows/AggregationWorkflow.cs
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

using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Analysis.Workflows;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.WorkflowSteps;

namespace Utah.Udot.ATSPM.Infrastructure.Workflows
{
    /// <summary>
    /// Workflow for aggregating event logs into various traffic metrics and analysis results.
    /// </summary>
    /// <remarks>
    /// This workflow coordinates multiple sub-workflows and processing steps to run in parallel,
    /// including detector event counts, pedestrian phases, phase cycles, split monitoring,
    /// preemption, and priority metrics, and saves the archived results.
    /// </remarks>
    public class AggregationWorkflow : WorkflowBase<Tuple<Location, IEnumerable<CompressedEventLogBase>>, CompressedAggregationBase>
    {
        private readonly IServiceScopeFactory _services;
        private readonly Timeline<StartEndRange> _timeline;
        private readonly int _parallelProcesses;
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregationWorkflow"/> class.
        /// </summary>
        /// <param name="services">The service scope factory to resolve dependencies.</param>
        /// <param name="timeline">The timeline specifying the start and end ranges for aggregation.</param>
        /// <param name="parallelProcesses">The maximum degree of parallel processes to run.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        public AggregationWorkflow(IServiceScopeFactory services, Timeline<StartEndRange> timeline, int parallelProcesses = 1, CancellationToken cancellationToken = default)
        {
            _services = services;
            _timeline = timeline;
            _parallelProcesses = parallelProcesses;
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets the process that restores archived events if necessary.
        /// </summary>
        public RestorArchivedEventsProcess RestorArchivedEventsProcess { get; private set; }

        /// <summary>
        /// Gets the broadcast block that distributes location and event log data to all sub-workflows.
        /// </summary>
        public BroadcastBlock<Tuple<Location, IEnumerable<EventLogModelBase>>> BroadcastEvents { get; private set; }


        /// <summary>
        /// Gets the sub-workflow that aggregates detector event counts.
        /// </summary>
        public AggregateDetectorEventCountWorkflow AggregateDetectorEventCountWorkflow { get; private set; }

        /// <summary>
        /// Gets the sub-workflow that aggregates pedestrian phases.
        /// </summary>
        public AggregatePedestrianPhasesWorkflow AggregatePedestrianPhasesWorkflow { get; private set; }

        /// <summary>
        /// Gets the sub-workflow that aggregates phase cycles.
        /// </summary>
        public AggregatePhaseCyclesWorkflow AggregatePhaseCyclesWorkflow { get; private set; }

        /// <summary>
        /// Gets the sub-workflow that aggregates phase split monitor metrics.
        /// </summary>
        public AggregatePhaseSplitMonitorWorkflow AggregatePhaseSplitMonitorWorkflow { get; private set; }

        /// <summary>
        /// Gets the sub-workflow that aggregates preemption events.
        /// </summary>
        public AggregatePreemptionWorkflow AggregatePreemptionWorkflow { get; private set; }

        /// <summary>
        /// Gets the sub-workflow that aggregates priority events.
        /// </summary>
        public AggregatePriorityWorkflow AggregatePriorityWorkflow { get; private set; }


        /// <summary>
        /// Gets the process that handles archiving of all aggregated metrics.
        /// </summary>
        public ArchiveAggregationsProcess ArchiveAggregationsProcess { get; private set; }

        /// <summary>
        /// Gets the process that saves the archived aggregations to storage.
        /// </summary>
        public SaveArchivedAggregationsProcess SaveArchivedAggregationsProcess { get; private set; }

        /// <inheritdoc/>
        public override async Task Initialize()
        {
            //Steps = new();
            Input = new(null, blockOptions);
            Output = new(blockOptions);

            InstantiateSteps();

            await Task.WhenAll(
                AggregateDetectorEventCountWorkflow.WhenInitialized(),
                AggregatePedestrianPhasesWorkflow.WhenInitialized(),
                AggregatePhaseCyclesWorkflow.WhenInitialized(),
                AggregatePhaseSplitMonitorWorkflow.WhenInitialized(),
                AggregatePreemptionWorkflow.WhenInitialized(),
                AggregatePriorityWorkflow.WhenInitialized()
            );


            Steps.Add(Input);
            AddStepsToTracker();
            LinkSteps();
        }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(RestorArchivedEventsProcess);
            Steps.Add(BroadcastEvents);

            Steps.Add(AggregateDetectorEventCountWorkflow.Output);
            Steps.Add(AggregatePedestrianPhasesWorkflow.Output);
            Steps.Add(AggregatePhaseCyclesWorkflow.Output);
            Steps.Add(AggregatePhaseSplitMonitorWorkflow.Output);
            Steps.Add(AggregatePreemptionWorkflow.Output);
            Steps.Add(AggregatePriorityWorkflow.Output);

            Steps.Add(ArchiveAggregationsProcess);
            Steps.Add(SaveArchivedAggregationsProcess);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            var aggregationOptions = new AggregationWorkflowOptions()
            {
                Timeline = _timeline,
                MaxDegreeOfParallelism = _parallelProcesses,
                CancellationToken = _cancellationToken
            };

            RestorArchivedEventsProcess = new(new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
            BroadcastEvents = new(null, new DataflowBlockOptions() { CancellationToken = _cancellationToken });

            AggregateDetectorEventCountWorkflow = new(aggregationOptions);
            AggregatePedestrianPhasesWorkflow = new(aggregationOptions);
            AggregatePhaseCyclesWorkflow = new(aggregationOptions);
            AggregatePhaseSplitMonitorWorkflow = new(aggregationOptions);
            AggregatePreemptionWorkflow = new(aggregationOptions);
            AggregatePriorityWorkflow = new(aggregationOptions);

            ArchiveAggregationsProcess = new ArchiveAggregationsProcess(new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
            SaveArchivedAggregationsProcess = new(_services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(RestorArchivedEventsProcess, new DataflowLinkOptions() { PropagateCompletion = true });
            RestorArchivedEventsProcess.LinkTo(BroadcastEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            BroadcastEvents.LinkTo(AggregateDetectorEventCountWorkflow.Input, new DataflowLinkOptions() { PropagateCompletion = true });
            BroadcastEvents.LinkTo(AggregatePedestrianPhasesWorkflow.Input, new DataflowLinkOptions() { PropagateCompletion = true });
            BroadcastEvents.LinkTo(AggregatePhaseCyclesWorkflow.Input, new DataflowLinkOptions() { PropagateCompletion = true });
            BroadcastEvents.LinkTo(AggregatePhaseSplitMonitorWorkflow.Input, new DataflowLinkOptions() { PropagateCompletion = true });
            BroadcastEvents.LinkTo(AggregatePreemptionWorkflow.Input, new DataflowLinkOptions() { PropagateCompletion = true });
            BroadcastEvents.LinkTo(AggregatePriorityWorkflow.Input, new DataflowLinkOptions() { PropagateCompletion = true });

            AggregateDetectorEventCountWorkflow.Output.LinkTo(ArchiveAggregationsProcess, new DataflowLinkOptions { PropagateCompletion = false });
            AggregatePedestrianPhasesWorkflow.Output.LinkTo(ArchiveAggregationsProcess, new DataflowLinkOptions { PropagateCompletion = false });
            AggregatePhaseCyclesWorkflow.Output.LinkTo(ArchiveAggregationsProcess, new DataflowLinkOptions { PropagateCompletion = false });
            AggregatePhaseSplitMonitorWorkflow.Output.LinkTo(ArchiveAggregationsProcess, new DataflowLinkOptions { PropagateCompletion = false });
            AggregatePreemptionWorkflow.Output.LinkTo(ArchiveAggregationsProcess, new DataflowLinkOptions { PropagateCompletion = false });
            AggregatePriorityWorkflow.Output.LinkTo(ArchiveAggregationsProcess, new DataflowLinkOptions { PropagateCompletion = false });

            Task.WhenAll(
                AggregateDetectorEventCountWorkflow.Output.Completion,
                AggregatePedestrianPhasesWorkflow.Output.Completion,
                AggregatePhaseCyclesWorkflow.Output.Completion,
                AggregatePhaseSplitMonitorWorkflow.Output.Completion,
                AggregatePreemptionWorkflow.Output.Completion,
                AggregatePriorityWorkflow.Output.Completion)
                .ContinueWith(_ =>
                {
                    ArchiveAggregationsProcess.Complete();
                }, _cancellationToken);

            ArchiveAggregationsProcess.LinkTo(SaveArchivedAggregationsProcess, new DataflowLinkOptions() { PropagateCompletion = true });
            SaveArchivedAggregationsProcess.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
