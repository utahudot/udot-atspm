using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Messaging.Kafka
{
    public class KafkaPublisher : IEventPublisher<EventBatchEnvelope>, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;

        public KafkaPublisher(IOptions<KafkaConfiguration> opts)
        {
            var cfg = new ProducerConfig
            {
                BootstrapServers = opts.Value.BootstrapServers,
                // wait up to 50 ms for more messages before sending a request
                LingerMs = 50,
                // accumulate up to 128 KB of messages
                BatchSize = 128 * 1024,
                // shrink your JSON by ~5×–10×
                CompressionType = CompressionType.Snappy,
                Partitioner = Partitioner.Consistent
            };
            _producer = new ProducerBuilder<string, string>(cfg).Build();

            _topic = opts.Value.Topic;
        }

        public Task PublishAsync(EventBatchEnvelope msg, CancellationToken ct = default)
       => _producer.ProduceAsync(
            _topic,
            new Message<string, string>
            {
                Key = msg.LocationIdentifier,
                Value = JsonSerializer.Serialize(msg)
            },
            ct);

        public void Dispose() => _producer.Dispose();

        Task IEventPublisher<EventBatchEnvelope>.PublishAsync(IReadOnlyList<EventBatchEnvelope> batch, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
