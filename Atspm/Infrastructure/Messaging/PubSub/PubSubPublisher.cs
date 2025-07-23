using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Messaging.PubSub
{
 
        public class PubSubPublisher : IEventPublisher<EventBatchEnvelope>
        {
            private readonly PublisherClient _client;

            public PubSubPublisher(IOptions<PubSubConfiguration> opts)
            {
                var topicName = TopicName.FromProjectTopic(opts.Value.ProjectId, opts.Value.TopicId);
                _client = PublisherClient.Create(topicName);
            }

        public async Task PublishAsync(EventBatchEnvelope msg, CancellationToken ct = default)
        {
            string json = JsonSerializer.Serialize(msg);

            var pubsubMessage = new PubsubMessage
            {
                Data = Google.Protobuf.ByteString.CopyFromUtf8(json),
                // add your “key” here
                Attributes =
        {
            { "LocationIdentifier", msg.LocationIdentifier }
        }
            };

            // publishes and returns the server‐assigned message ID
            await _client.PublishAsync(pubsubMessage);
        }

        Task IEventPublisher<EventBatchEnvelope>.PublishAsync(IReadOnlyList<EventBatchEnvelope> batch, int threads, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }

}
