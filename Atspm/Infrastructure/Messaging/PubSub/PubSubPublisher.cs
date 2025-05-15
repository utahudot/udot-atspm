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
 
        public class PubSubPublisher : IEventBusPublisher<RawSpeedPacket>
        {
            private readonly PublisherClient _client;

            public PubSubPublisher(IOptions<PubSubConfiguration> opts)
            {
                var topicName = TopicName.FromProjectTopic(opts.Value.ProjectId, opts.Value.TopicId);
                _client = PublisherClient.Create(topicName);
            }

            public async Task PublishAsync(RawSpeedPacket msg, CancellationToken ct = default)
            {
                string json = JsonSerializer.Serialize(msg);
                // PubSub requires UTF8 bytes
                await _client.PublishAsync(json);
            }
        }

}
