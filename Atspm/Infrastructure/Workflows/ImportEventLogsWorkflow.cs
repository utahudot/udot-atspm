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
using Utah.Udot.ATSPM.Infrastructure.WorkflowSteps;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.ATSPM.Infrastructure.Workflows
{
    /// <summary>
    /// Orchestrates the parallelized import of device event logs.
    /// </summary>
    public class ImportEventLogsWorkflow : WorkflowBase<Device, Tuple<Device, EventLogModelBase>>
    {
        private readonly IServiceScopeFactory _services;
        private readonly int _parallelProcesses;
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportEventLogsWorkflow"/> class.
        /// </summary>
        /// <param name="services">The service scope factory for step dependencies.</param>
        /// <param name="parallelProcesses">The maximum degree of parallelism.</param>
        /// <param name="cancellationToken">The cancellation token for the workflow.</param>
        public ImportEventLogsWorkflow(IServiceScopeFactory services, int parallelProcesses = 50, CancellationToken cancellationToken = default)
        {
            _services = services;
            _parallelProcesses = parallelProcesses;
            _cancellationToken = cancellationToken;
        }

        /// <inheritdoc/>
        public DownloadDeviceData DownloadDeviceData { get; private set; }

        /// <inheritdoc/>
        public DecodeDeviceData DecodeDeviceData { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(DownloadDeviceData);
            Steps.Add(DecodeDeviceData);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            Thread.MemoryBarrier();

            DownloadDeviceData = new(_services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
            DecodeDeviceData = new(_services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(DownloadDeviceData, new DataflowLinkOptions() { PropagateCompletion = true });
            DownloadDeviceData.LinkTo(DecodeDeviceData, new DataflowLinkOptions() { PropagateCompletion = true });
            DecodeDeviceData.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}


