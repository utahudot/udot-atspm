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
using Utah.Udot.Atspm.Infrastructure.WorkflowSteps;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.ATSPM.Infrastructure.Workflows
{
    /// <summary>
    /// A workflow responsible for processing Indiana events into validated and saved signal timing plans.
    /// </summary>
    public class SignalTimingPlansWorkflow : WorkflowBase<IndianaEvent, SignalTimingPlan>
    {
        private readonly IServiceScopeFactory _services;
        private readonly int _batchSize;
        private readonly int _parallelProcesses;
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalTimingPlansWorkflow"/> class.
        /// </summary>
        /// <param name="services">The service scope factory for dependency resolution within steps.</param>
        /// <param name="batchSize">The number of events to batch for plan generation.</param>
        /// <param name="parallelProcesses">The maximum degree of parallelism for processing blocks.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public SignalTimingPlansWorkflow(IServiceScopeFactory services, int batchSize = 50000, int parallelProcesses = 50, CancellationToken cancellationToken = default)
        {
            _services = services;
            _batchSize = batchSize;
            _parallelProcesses = parallelProcesses;
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets the block that groups incoming Indiana events into batches.
        /// </summary>
        public BatchBlock<IndianaEvent> BatchEventLogs { get; private set; }

        /// <inheritdoc/>
        public GenerateSignalPlansStep GenerateSignalPlansStep { get; private set; }

        /// <inheritdoc/>
        public MergeExistingSignalPlansStep MergeExistingSignalPlansStep { get; private set; }

        /// <inheritdoc/>
        public ReconcileSignalPlansStep ReconcileSignalPlansStep { get; private set; }

        /// <inheritdoc/>
        public SaveSignalTimingPlansStep SaveSignalTimingPlans { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(BatchEventLogs);
            Steps.Add(GenerateSignalPlansStep);
            Steps.Add(MergeExistingSignalPlansStep);
            Steps.Add(ReconcileSignalPlansStep);
            Steps.Add(SaveSignalTimingPlans);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            BatchEventLogs = new(_batchSize, new GroupingDataflowBlockOptions() { CancellationToken = _cancellationToken });
            GenerateSignalPlansStep = new(new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
            MergeExistingSignalPlansStep = new(_services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
            ReconcileSignalPlansStep = new(new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
            SaveSignalTimingPlans = new(_services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(BatchEventLogs, new DataflowLinkOptions() { PropagateCompletion = true });
            BatchEventLogs.LinkTo(GenerateSignalPlansStep, new DataflowLinkOptions() { PropagateCompletion = true });
            GenerateSignalPlansStep.LinkTo(MergeExistingSignalPlansStep, new DataflowLinkOptions() { PropagateCompletion = true });
            MergeExistingSignalPlansStep.LinkTo(ReconcileSignalPlansStep, new DataflowLinkOptions() { PropagateCompletion = true });
            ReconcileSignalPlansStep.LinkTo(SaveSignalTimingPlans, new DataflowLinkOptions() { PropagateCompletion = true });
            SaveSignalTimingPlans.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}


