#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.Workflows/DeviceEventLogWorkflow.cs
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
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.ATSPM.Infrastructure.Workflows
{
    /// <summary>
    /// A high-level composite workflow that coordinates the import, broadcasting, transformation, and archival of device event logs.
    /// </summary>
    public class DeviceEventLogWorkflow(IServiceScopeFactory services, int batchSize = 50000, int parallelProcesses = 50, CancellationToken cancellationToken = default) : WorkflowBase<Device, CompressedEventLogBase>
    {
        private readonly IServiceScopeFactory _services = services;
        private readonly int _batchSize = batchSize;
        private readonly int _parallelProcesses = parallelProcesses;
        private readonly CancellationToken _cancellationToken = cancellationToken;

        /// <inheritdoc/>
        public ImportEventLogsWorkflow ImportEventLogsWorkflow { get; private set; }

        /// <summary>
        /// Gets the block that broadcasts imported events to multiple downstream processing paths.
        /// </summary>
        public BroadcastBlock<Tuple<Device, EventLogModelBase>> BroadcastEvents { get; private set; }

        /// <summary>
        /// Gets the block that filters and transforms event logs into Indiana-specific event types.
        /// </summary>
        public TransformManyBlock<Tuple<Device, EventLogModelBase>, IndianaEvent> TranformToIndianaEvent { get; private set; }

        /// <inheritdoc/>
        public ArchiveEventLogsWorkflow ArchiveEventLogsWorkflow { get; private set; }

        /// <inheritdoc/>
        public SignalTimingPlansWorkflow SignalTimingPlansWorkflow { get; private set; }

        /// <inheritdoc/>
        public override async Task Initialize()
        {
            Steps = new();
            Input = new(null, blockOptions);
            Output = new(blockOptions);

            InstantiateSteps();

            await Task.WhenAll(
                ImportEventLogsWorkflow.WhenInitialized(),
                ArchiveEventLogsWorkflow.WhenInitialized(),
                SignalTimingPlansWorkflow.WhenInitialized()
            );

            Steps.Add(Input);
            AddStepsToTracker();
            LinkSteps();
        }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(ImportEventLogsWorkflow.Output);
            Steps.Add(BroadcastEvents);
            Steps.Add(TranformToIndianaEvent);
            Steps.Add(ArchiveEventLogsWorkflow.Output);
            Steps.Add(SignalTimingPlansWorkflow.Output);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            ImportEventLogsWorkflow = new(_services, _parallelProcesses, _cancellationToken);
            BroadcastEvents = new(null, new DataflowBlockOptions() { CancellationToken = _cancellationToken });
            TranformToIndianaEvent = new(t => new[] { t.Item2 }.OfType<IndianaEvent>(), new ExecutionDataflowBlockOptions { CancellationToken = _cancellationToken });
            ArchiveEventLogsWorkflow = new(_services, _batchSize, _parallelProcesses, _cancellationToken);
            SignalTimingPlansWorkflow = new(_services, _batchSize, _parallelProcesses, _cancellationToken);
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(ImportEventLogsWorkflow.Input, new DataflowLinkOptions() { PropagateCompletion = true });
            ImportEventLogsWorkflow.Output.LinkTo(BroadcastEvents, new DataflowLinkOptions() { PropagateCompletion = true });

            BroadcastEvents.LinkTo(ArchiveEventLogsWorkflow.Input, new DataflowLinkOptions() { PropagateCompletion = true });
            BroadcastEvents.LinkTo(TranformToIndianaEvent, new DataflowLinkOptions() { PropagateCompletion = true });
            TranformToIndianaEvent.LinkTo(SignalTimingPlansWorkflow.Input, new DataflowLinkOptions() { PropagateCompletion = true });

            ArchiveEventLogsWorkflow.Output.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    } 
}


