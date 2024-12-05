using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.ATSPM.Infrastructure.WorkflowSteps;
using Utah.Udot.NetStandardToolkit.Workflows;

namespace Utah.Udot.ATSPM.Infrastructure.Workflows
{
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

        ///<inheritdoc cref="DownloadDeviceData"/>
        public DownloadDeviceData DownloadDeviceData { get; private set; }

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
            Steps.Add(DownloadDeviceData);
            Steps.Add(DecodeDeviceData);
            Steps.Add(BatchEventLogs);
            Steps.Add(ArchiveDeviceData);
            Steps.Add(SaveEventsToRepo);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            DownloadDeviceData = new(_services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
            DecodeDeviceData = new(_services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
            BatchEventLogs = new(_batchSize);
            ArchiveDeviceData = new(new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken });
            SaveEventsToRepo = new(_services, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, CancellationToken = _cancellationToken });
        }

        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            Input.LinkTo(DownloadDeviceData, new DataflowLinkOptions() { PropagateCompletion = true });
            DownloadDeviceData.LinkTo(DecodeDeviceData, new DataflowLinkOptions() { PropagateCompletion = true });
            DecodeDeviceData.LinkTo(BatchEventLogs, new DataflowLinkOptions() { PropagateCompletion = true });
            BatchEventLogs.LinkTo(ArchiveDeviceData, new DataflowLinkOptions() { PropagateCompletion = true });
            ArchiveDeviceData.LinkTo(SaveEventsToRepo, new DataflowLinkOptions() { PropagateCompletion = true });
            SaveEventsToRepo.LinkTo(Output, new DataflowLinkOptions() { PropagateCompletion = true });
        }
    }
}
