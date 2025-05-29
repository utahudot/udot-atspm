using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.DependencyInjection;
using Utah.Udot.NetStandardToolkit.Workflows;
using Utah.Udot.ATSPM.Infrastructure.WorkflowSteps;
using Utah.Udot.Atspm.Infrastructure.Messaging;

namespace Utah.Udot.Atspm.Infrastructure.Workflows
{
    public class EventBatchEnvelopeWorkflow
        : WorkflowBase<EventBatchEnvelope, CompressedEventLogBase>
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly int _parallelProcesses;
        private readonly CancellationToken _cancellationToken;
        private bool _initialized = false;
        private readonly object _initLock = new();

        public EventBatchEnvelopeWorkflow(IServiceScopeFactory scopes, int parallelProcesses = 50, CancellationToken cancellationToken = default) 
        {
            _parallelProcesses = parallelProcesses;
            _cancellationToken = cancellationToken;
            _scopes = scopes;
        }

        public override Task Initialize()
        {
            lock (_initLock)
            {
                if (_initialized) return Task.CompletedTask;

                // Re-implement base.Initialize() logic
                Steps = new List<IDataflowBlock>();

                Input = new BroadcastBlock<EventBatchEnvelope>(null, blockOptions);
                Output = new BufferBlock<CompressedEventLogBase>(blockOptions);

                InstantiateSteps();
                Steps.Add(Input);
                AddStepsToTracker();
                LinkSteps();

                _initialized = true;
                return Task.CompletedTask;
            }
        }

        public ArchiveEnvelopeDataEvents Archive { get; private set; }
        public SaveArchivedEventLogs Save { get; private set; }

        /// <inheritdoc/>
        protected override void AddStepsToTracker()
        {
            Steps.Add(Archive);
            Steps.Add(Save);
        }

        /// <inheritdoc/>
        protected override void InstantiateSteps()
        {
            // 1) Compress directly from the envelope (no Device object)
            Archive = new ArchiveEnvelopeDataEvents(
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _parallelProcesses, CancellationToken = _cancellationToken }
            );

            // 2) Persist each CompressedEventLogBase
            Save = new SaveArchivedEventLogs(
                _scopes,     new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1, CancellationToken = _cancellationToken }
            );
        }


        /// <inheritdoc/>
        protected override void LinkSteps()
        {
            // Input<EventBatchEnvelope> → ArchiveEnvelopeDataEvents
            Input.LinkTo(Archive, new DataflowLinkOptions { PropagateCompletion = true });

            // ArchiveEnvelopeDataEvents → SaveArchivedEventLogs
            Archive.LinkTo(Save, new DataflowLinkOptions { PropagateCompletion = true });

            // SaveArchivedEventLogs → Output<CompressedEventLogBase>
            Save.LinkTo(Output, new DataflowLinkOptions { PropagateCompletion = true });
        }
    }
}
