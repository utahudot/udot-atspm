#region license
// Copyright 2025 Utah Departement of Transportation
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
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.ATSPM.Infrastructure.Workflows
{
    public class AggregationWorkflow : WorkflowBase<Tuple<Location, IEnumerable<CompressedEventLogBase>>, CompressedAggregationBase>
    {
        private readonly IServiceScopeFactory _services;
        private readonly Timeline<StartEndRange> _timeline;
        private readonly int _parallelProcesses;
        private readonly CancellationToken _cancellationToken;

        public AggregationWorkflow(IServiceScopeFactory services, Timeline<StartEndRange> timeline, int parallelProcesses = 1, CancellationToken cancellationToken = default)
        {
            _services = services;
            _timeline = timeline;
            _parallelProcesses = parallelProcesses;
            _cancellationToken = cancellationToken;
        }

        ///<inheritdoc cref="RestorArchivedEventsProcess"/>
        public RestorArchivedEventsProcess RestorArchivedEventsProcess { get; private set; }

        public BroadcastBlock<Tuple<Location, IEnumerable<EventLogModelBase>>> BroadcastEvents { get; private set; }


        public AggregateDetectorEventCountWorkflow AggregateDetectorEventCountWorkflow { get; private set; }
        public AggregatePedestrianPhasesWorkflow AggregatePedestrianPhasesWorkflow { get; private set; }
        public AggregatePhaseCyclesWorkflow AggregatePhaseCyclesWorkflow { get; private set; }


        public ArchiveAggregationsProcess ArchiveAggregationsProcess { get; private set; }

        public SaveArchivedAggregationsProcess SaveArchivedAggregationsProcess { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(RestorArchivedEventsProcess);
            Steps.Add(BroadcastEvents);

            Steps.Add(AggregateDetectorEventCountWorkflow.Output);
            Steps.Add(AggregatePedestrianPhasesWorkflow.Output);
            Steps.Add(AggregatePhaseCyclesWorkflow.Output);

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

            AggregateDetectorEventCountWorkflow.Output.LinkTo(ArchiveAggregationsProcess, new DataflowLinkOptions { PropagateCompletion = false });
            AggregatePedestrianPhasesWorkflow.Output.LinkTo(ArchiveAggregationsProcess, new DataflowLinkOptions { PropagateCompletion = false });
            AggregatePhaseCyclesWorkflow.Output.LinkTo(ArchiveAggregationsProcess, new DataflowLinkOptions { PropagateCompletion = false });

            Task.WhenAll(
                AggregateDetectorEventCountWorkflow.Output.Completion,
                AggregatePedestrianPhasesWorkflow.Output.Completion,
                AggregatePhaseCyclesWorkflow.Output.Completion)
                .ContinueWith(_ =>
                {
                    ArchiveAggregationsProcess.Complete();
                }, _cancellationToken);

            ArchiveAggregationsProcess.LinkTo(SaveArchivedAggregationsProcess, new DataflowLinkOptions() { PropagateCompletion = true });
            SaveArchivedAggregationsProcess.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
