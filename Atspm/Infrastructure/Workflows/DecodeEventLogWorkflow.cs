#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.ATSPM.Infrastructure.Workflows/DecodeEventLogWorkflow.cs
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
using Utah.Udot.ATSPM.Infrastructure.WorkflowSteps;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.ATSPM.Infrastructure.Workflows
{
    public class DecodeEventLogWorkflow : WorkflowBase<Tuple<Device, FileInfo>, CompressedEventLogBase>
    {
        private readonly IServiceScopeFactory _services;
        private readonly int _batchSize;
        private readonly CancellationToken _cancellationToken;

        public DecodeEventLogWorkflow(IServiceScopeFactory services, int batchSize = 50000, CancellationToken cancellationToken = default)
        {
            _services = services;
            _batchSize = batchSize;
            _cancellationToken = cancellationToken;
        }

        ///<inheritdoc cref="DecodeDeviceData"/>
        public DecodeDeviceData DecodeDeviceData { get; private set; }

        ///<inheritdoc cref="BatchBlock{T}"/>
        public BatchBlock<Tuple<Device, EventLogModelBase>> BatchEventLogs { get; private set; }

        ///<inheritdoc cref="ArchiveDataEvents"/>
        public ArchiveDataEvents ArchiveDeviceData { get; private set; }

        ///<inheritdoc cref="SaveArchivedEventLogs"/>
        public SaveArchivedEventLogs SaveEventsToRepo { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(DecodeDeviceData);
            Steps.Add(BatchEventLogs);
            Steps.Add(ArchiveDeviceData);
            Steps.Add(SaveEventsToRepo);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            DecodeDeviceData = new(_services, new ExecutionDataflowBlockOptions() { CancellationToken = _cancellationToken });
            BatchEventLogs = new(_batchSize);
            ArchiveDeviceData = new(new ExecutionDataflowBlockOptions() { CancellationToken = _cancellationToken });
            SaveEventsToRepo = new(_services, new ExecutionDataflowBlockOptions() { CancellationToken = _cancellationToken });
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(DecodeDeviceData, new DataflowLinkOptions() { PropagateCompletion = true });
            DecodeDeviceData.LinkTo(BatchEventLogs, new DataflowLinkOptions() { PropagateCompletion = true });
            BatchEventLogs.LinkTo(ArchiveDeviceData, new DataflowLinkOptions() { PropagateCompletion = true });
            ArchiveDeviceData.LinkTo(SaveEventsToRepo, new DataflowLinkOptions() { PropagateCompletion = true });
            SaveEventsToRepo.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
