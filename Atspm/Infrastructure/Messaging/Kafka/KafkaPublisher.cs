using Confluent.Kafka;
using Lextm.SharpSnmpLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Messaging.Kafka
{
    public class KafkaPublisher : IEventBusPublisher<SpeedEvent>, IDisposable
    {
        private readonly IProducer<Confluent.Kafka.Null, string> _producer;
        private readonly string _topic;

        public KafkaPublisher(IOptions<KafkaConfiguration> opts)
        {
            var cfg = new ProducerConfig { BootstrapServers = opts.Value.BootstrapServers };
            _producer = new ProducerBuilder<Confluent.Kafka.Null, string>(cfg).Build();
            _topic = opts.Value.Topic;
        }

        public Task PublishAsync(SpeedEvent msg, CancellationToken ct = default)
       => _producer.ProduceAsync(_topic, new Message<Confluent.Kafka.Null, string> { Value = JsonSerializer.Serialize(msg) }, ct);

        public void Dispose() => _producer.Dispose();
    }
}
