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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Extensions;
using Utah.Udot.ATSPM.Infrastructure.WorkflowSteps;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.ATSPM.Infrastructure.Workflows
{
    public class ImportEventLogsWorkflow : WorkflowBase<Device, Tuple<Device, EventLogModelBase>>
    {
        private readonly IServiceScopeFactory _services;
        private readonly int _parallelProcesses;
        private readonly CancellationToken _cancellationToken;

        public ImportEventLogsWorkflow(IServiceScopeFactory services, int parallelProcesses = 50, CancellationToken cancellationToken = default)
        {
            _services = services;   
            _parallelProcesses = parallelProcesses;
            _cancellationToken = cancellationToken;
        }

        ///<inheritdoc cref="DownloadDeviceData"/>
        public DownloadDeviceData DownloadDeviceData { get; private set; }

        ///<inheritdoc cref="DecodeDeviceData"/>
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





    public class ArchiveEventLogsWorkflow : WorkflowBase<Tuple<Device, EventLogModelBase>, CompressedEventLogBase>
    {
        private readonly IServiceScopeFactory _services;
        private readonly int _batchSize;
        private readonly int _parallelProcesses;
        private readonly CancellationToken _cancellationToken;

        public ArchiveEventLogsWorkflow(IServiceScopeFactory services, int batchSize = 50000, int parallelProcesses = 50, CancellationToken cancellationToken = default)
        {
            _services = services;
            _batchSize = batchSize;
            _parallelProcesses = parallelProcesses;
            _cancellationToken = cancellationToken;
        }

        ///<inheritdoc cref="BatchBlock{T}"/>
        public BatchBlock<Tuple<Device, EventLogModelBase>> BatchEventLogs { get; private set; }

        ///<inheritdoc cref="ArchiveDataEvents"/>
        public ArchiveDataEvents ArchiveDeviceData { get; private set; }

        ///<inheritdoc cref="SaveArchivedEventLogs"/>
        public SaveArchivedEventLogs SaveEventsToRepo { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(BatchEventLogs);
            Steps.Add(ArchiveDeviceData);
            Steps.Add(SaveEventsToRepo);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            BatchEventLogs = new(_batchSize, new GroupingDataflowBlockOptions() { CancellationToken = _cancellationToken });
            ArchiveDeviceData = new(new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
            SaveEventsToRepo = new(_services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, CancellationToken = _cancellationToken });
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(BatchEventLogs, new DataflowLinkOptions() { PropagateCompletion = true });
            BatchEventLogs.LinkTo(ArchiveDeviceData, new DataflowLinkOptions() { PropagateCompletion = true });
            ArchiveDeviceData.LinkTo(SaveEventsToRepo, new DataflowLinkOptions() { PropagateCompletion = true });
            SaveEventsToRepo.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }











    /// <summary>
    /// Download, import, deocde and compresses <see cref="EventLogModelBase"/> objects from <see cref="Device"/>
    /// </summary>
    public class DeviceEventLogWorkflow : WorkflowBase<Device, CompressedEventLogBase>
    {
        private readonly IServiceScopeFactory _services;
        private readonly int _batchSize;
        private readonly int _parallelProcesses;
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Download, import, deocde and compresses <see cref="EventLogModelBase"/> objects from <see cref="Device"/>
        /// </summary>
        /// <param name="services">Used for getting scoped services</param>
        /// <param name="batchSize"><inheritdoc cref="DeviceEventLoggingConfiguration.BatchSize"/></param>
        /// <param name="parallelProcesses"><inheritdoc cref="DeviceEventLoggingConfiguration.ParallelProcesses"/></param>
        /// <param name="cancellationToken"></param>
        public DeviceEventLogWorkflow(IServiceScopeFactory services, int batchSize = 50000, int parallelProcesses = 50, CancellationToken cancellationToken = default)
        {
            _services = services;
            _batchSize = batchSize;
            _parallelProcesses = parallelProcesses;
            _cancellationToken = cancellationToken;
        }

        ///<inheritdoc/>
        public ImportEventLogsWorkflow ImportEventLogsWorkflow { get; private set; }

        public BroadcastBlock<Tuple<Device, EventLogModelBase>> BroadcastEvents { get; private set; }

        public TransformManyBlock<Tuple<Device, EventLogModelBase>, IndianaEvent> TranformToIndianaEvent { get; private set; }

        ///<inheritdoc/>
        public ArchiveEventLogsWorkflow ArchiveEventLogsWorkflow { get; private set; }

        ///<inheritdoc/>
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
            SignalTimingPlansWorkflow = new(_services, _batchSize, _parallelProcesses, _cancellationToken);        }

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











    public class SignalTimingPlansWorkflow : WorkflowBase<IndianaEvent, SignalTimingPlan>
    {
        private readonly IServiceScopeFactory _services;
        private readonly int _batchSize;
        private readonly int _parallelProcesses;
        private readonly CancellationToken _cancellationToken;

        public SignalTimingPlansWorkflow(IServiceScopeFactory services, int batchSize = 50000, int parallelProcesses = 50, CancellationToken cancellationToken = default)
        {
            _services = services;
            _batchSize = batchSize;
            _parallelProcesses = parallelProcesses;
            _cancellationToken = cancellationToken;
        }

        public BatchBlock<IndianaEvent> BatchEventLogs { get; private set; }

        ///<inheritdoc/>
        public GenerateSignalPlansStep GenerateSignalPlansStep { get; private set; }

        ///<inheritdoc/>
        public MergeExistingSignalPlansStep MergeExistingSignalPlansStep { get; private set; }

        ///<inheritdoc/>
        public ReconcileSignalPlansStep ReconcileSignalPlansStep { get; private set; }

        ///<inheritdoc/>
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
            SaveSignalTimingPlans = new(_services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, CancellationToken = _cancellationToken });
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













    public class GenerateSignalPlansStep(ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformManyProcessStepBaseAsync<IEnumerable<IndianaEvent>, IEnumerable<SignalTimingPlan>>(dataflowBlockOptions)
    {
        protected override async IAsyncEnumerable<IEnumerable<SignalTimingPlan>> Process(IEnumerable<IndianaEvent> input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var groups = input
                .FromSpecification(new IndianaPlanDataSpecification())
                .GroupBy(e => (e.LocationIdentifier, e.EventParam));

            foreach (var g in groups)
            {
                var unique = g.KeepFirstSequentialParam().ToList();

                if (unique.Count > 0)
                {
                    var chunk = unique.Select(s => new SignalTimingPlan
                    {
                        LocationIdentifier = s.LocationIdentifier,
                        PlanNumber = s.EventParam,
                        Start = s.Timestamp,
                        End = DateTime.MinValue
                    }).ToList();

                    yield return chunk;
                }
            }
        }
    }


    public class MergeExistingSignalPlansStep(IServiceScopeFactory services, ExecutionDataflowBlockOptions options) : TransformManyProcessStepBaseAsync<IEnumerable<SignalTimingPlan>, IEnumerable<SignalTimingPlan>>(options)
    {
        private readonly IServiceScopeFactory _services = services;

        protected override async IAsyncEnumerable<IEnumerable<SignalTimingPlan>> Process(IEnumerable<SignalTimingPlan> input, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var groups = input.GroupBy(g => (g.LocationIdentifier, g.PlanNumber));
            if (!groups.Any()) yield break;

            using var scope = _services.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetService<ISignalTimingPlanRepository>();

            foreach (var g in groups)
            {
                var minStart = g.Min(p => p.Start).AddHours(-12);
                var maxStart = g.Max(p => p.Start).AddHours(12);

                var existing = await repo.GetList()
                    .AsNoTracking()
                    .Where(w => w.LocationIdentifier == g.Key.LocationIdentifier
                    && w.PlanNumber == g.Key.PlanNumber
                    && w.Start >= minStart
                    && w.Start < maxStart)             
                    .ToListAsync(cancelToken);

                var combined = g.Concat(existing).DistinctBy(p => new { p.LocationIdentifier, p.PlanNumber, p.Start }).ToList();

                yield return combined;
            }
        }
    }



    public class ReconcileSignalPlansStep(ExecutionDataflowBlockOptions options) : TransformManyProcessStepBaseAsync<IEnumerable<SignalTimingPlan>, IEnumerable<SignalTimingPlan>>(options)
    {
        protected override async IAsyncEnumerable<IEnumerable<SignalTimingPlan>> Process(IEnumerable<SignalTimingPlan> input, CancellationToken cancelToken = default)
        {
            var groups = input.GroupBy(g => (g.LocationIdentifier, g.PlanNumber));

            foreach (var g in groups)
            {
                var ordered = g.OrderBy(p => p.Start).ToList();

                var finalized = ordered.Zip(ordered.Skip(1).Append(null), (current, next) =>
                {
                    current.End = next?.Start ?? DateTime.MinValue;
                    return current;
                });

                yield return finalized;
            }
        }
    }

    public class SaveSignalTimingPlansStep(IServiceScopeFactory services, ExecutionDataflowBlockOptions dataflowBlockOptions = default) : TransformManyProcessStepBaseAsync<IEnumerable<SignalTimingPlan>, SignalTimingPlan>(dataflowBlockOptions)
    {
        private readonly IServiceScopeFactory _services = services;

        protected override async IAsyncEnumerable<SignalTimingPlan> Process(IEnumerable<SignalTimingPlan> input, CancellationToken cancelToken = default)
        {
            if (!input.Any()) yield break;

            using var scope = _services.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<ISignalTimingPlanRepository>();

            foreach (var i in input)
            {
                var existing = await repo.LookupAsync(i);

                if (existing != null && existing.End != i.End)
                {
                    existing.End = i.End;
                    await repo.UpdateAsync(existing);
                }
                else
                {
                    await repo.AddAsync(i);
                }

                yield return i;
            }
        }
    }
}


