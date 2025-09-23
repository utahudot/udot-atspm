using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Infrastructure.Messaging;
using Utah.Udot.Atspm.Infrastructure.Workflows;
using Utah.Udot.ATSPM.Infrastructure.Messaging;    // for EventBatchEnvelope
using Utah.Udot.ATSPM.Infrastructure.Workflows;    // for EventBatchEnvelopeWorkflow
using Utah.Udot.NetStandardToolkit.Workflows;     // for WorkflowBase<TIn,TOut>

namespace Utah.Udot.ATSPM.Infrastructure.Messaging.Database
{
    public class DatabaseEventPublisher : IEventPublisher<EventBatchEnvelope>
    {
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<DatabaseEventPublisher> _logger;

        public DatabaseEventPublisher(
            IServiceScopeFactory scopes,
            ILogger<DatabaseEventPublisher> logger
        )
        {
            _scopes = scopes;
            _logger = logger;
        }

        /// <summary>
        /// One-by-one envelope path.
        /// </summary>
        public async Task PublishAsync(EventBatchEnvelope envelope, CancellationToken ct = default)
        {
            var workflow = new EventBatchEnvelopeWorkflow(_scopes, 1, ct);

            // Send single envelope
            await workflow.Input.SendAsync(envelope, ct);

            // Complete + await compression → save
            workflow.Input.Complete();
            await Task.WhenAll(workflow.Steps.Select(s => s.Completion));

            _logger.LogInformation(
                "Archived & saved compressed logs for device {DeviceId}@{Location}",
                envelope.DeviceId,
                envelope.LocationIdentifier
            );
        }

        /// <summary>
        /// Bulk path: n envelopes in one workflow instance.
        /// </summary>
        public async Task PublishAsync(IReadOnlyList<EventBatchEnvelope> batch, int threads, CancellationToken ct = default)
        {
            try
            {
                var workflow = new EventBatchEnvelopeWorkflow(_scopes, threads, ct);
                await workflow.Initialize();

                foreach (var env in batch)
                {
                    await workflow.Input.SendAsync(env, ct);
                }

                workflow.Input.Complete();
                await Task.WhenAll(workflow.Steps.Select(s => s.Completion));

                _logger.LogInformation("Archived & saved {Count} compressed log batches", batch.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish batch of {Count} envelopes", batch.Count);
            }
        }
    }
}
