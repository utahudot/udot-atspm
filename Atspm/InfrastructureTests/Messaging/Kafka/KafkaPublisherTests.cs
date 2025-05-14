using Xunit;
using Utah.Udot.Atspm.Infrastructure.Messaging.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using System.Threading;
using System.Text.Json;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Messaging.Kafka.Tests
{
    public class KafkaPublisherTests
    {
        public class KafkaIntegrationTests : IDisposable
        {
            private const string BootstrapServers = "localhost:9092";
            private const string Topic = "speed-events";
            private readonly IProducer<Null, string> _producer;
            private readonly string _groupId = $"test-group-{Guid.NewGuid()}";

            public KafkaIntegrationTests()
            {
                var producerConfig = new ProducerConfig { BootstrapServers = BootstrapServers };
                _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
            }

            [Fact(DisplayName = "Kafka cluster metadata can be fetched")]
            public void CanFetchClusterMetadata()
            {
                using var admin = new AdminClientBuilder(
                    new AdminClientConfig { BootstrapServers = BootstrapServers })
                    .Build();

                var meta = admin.GetMetadata(TimeSpan.FromSeconds(5));
                Assert.NotNull(meta.Brokers);
                Assert.True(meta.Brokers.Any(), "No brokers returned in metadata");
            }

            [Fact(DisplayName = "Produce and consume a test message")]
            public async Task ProduceAndConsumeRoundTrip()
            {
                // 1) Create a unique test payload
                var testEvent = new SpeedEvent
                {
                    DetectorId = "test-detector",
                    Mph = 42,
                    Kph = 67,
                    Timestamp = DateTime.UtcNow
                };
                var json = JsonSerializer.Serialize(testEvent);

                // 2) Produce it
                await _producer.ProduceAsync(Topic, new Message<Null, string> { Value = json });

                // 3) Spin up a consumer to read it back
                var consumerConfig = new ConsumerConfig
                {
                    BootstrapServers = BootstrapServers,
                    GroupId = _groupId,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };

                using var consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
                consumer.Subscribe(Topic);

                SpeedEvent? received = null;
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                try
                {
                    while (!cts.IsCancellationRequested)
                    {
                        var cr = consumer.Consume(cts.Token);
                        if (cr.Message.Value == json)
                        {
                            received = JsonSerializer.Deserialize<SpeedEvent>(cr.Message.Value);
                            break;
                        }
                    }
                }
                catch (OperationCanceledException) { }

                Assert.NotNull(received);
                Assert.Equal(testEvent.DetectorId, received!.DetectorId);
                Assert.Equal(testEvent.Mph, received.Mph);
                Assert.Equal(testEvent.Kph, received.Kph);
            }

            public void Dispose() => _producer.Dispose();
        }
    }
}