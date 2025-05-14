using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Messaging.Http
{
    public class HttpBatchingPublisher : IEventBusPublisher<SpeedEvent>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly int _batchSize;
        private readonly TimeSpan _flushInterval;
        private readonly List<SpeedEvent> _buffer = new();
        private readonly Timer _timer;
        private readonly object _lock = new();

        public HttpBatchingPublisher(HttpClient client, IOptions<BatchOptions> opts)
        {
            _client = client;
            _batchSize = opts.Value.Size;
            _flushInterval = TimeSpan.FromSeconds(opts.Value.IntervalSeconds);

            // start periodic flush
            _timer = new Timer(_ => FlushAsync().GetAwaiter().GetResult(),
                               null,
                               _flushInterval,
                               _flushInterval);
        }

        public async Task PublishAsync(SpeedEvent msg, CancellationToken ct = default)
        {
            List<SpeedEvent>? toSend = null;
            lock (_lock)
            {
                _buffer.Add(msg);
                if (_buffer.Count >= _batchSize)
                {
                    toSend = new List<SpeedEvent>(_buffer);
                    _buffer.Clear();
                }
            }

            if (toSend != null)
                await SendBatchAsync(toSend, ct);
        }

        private async Task FlushAsync()
        {
            List<SpeedEvent>? toSend = null;
            lock (_lock)
            {
                if (_buffer.Count > 0)
                {
                    toSend = new List<SpeedEvent>(_buffer);
                    _buffer.Clear();
                }
            }

            if (toSend != null)
                await SendBatchAsync(toSend, CancellationToken.None);
        }

        private async Task SendBatchAsync(List<SpeedEvent> batch, CancellationToken ct)
        {
            // Assumes your API POST /SpeedEvent accepts a JSON array
            var response = await _client.PostAsJsonAsync("SpeedEvent", batch, ct);
            response.EnsureSuccessStatusCode();
        }

        public void Dispose()
        {
            _timer.Dispose();
            // final flush
            FlushAsync().GetAwaiter().GetResult();
        }
    }

    public class BatchOptions
    {
        public int Size { get; set; } = 50_000;
        public int IntervalSeconds { get; set; } = 30;
    }
}
