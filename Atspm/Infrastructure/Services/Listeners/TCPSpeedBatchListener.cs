using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Messaging;
using Utah.Udot.Atspm.Infrastructure.Services.Receivers;

namespace Utah.Udot.Atspm.Infrastructure.Services.Listeners
{
    /// <summary>
    /// Batches incoming TCP speed events and posts to the DataApi.
    /// </summary>
    public class TCPSpeedBatchListener : IDisposable
    {
        private readonly ITcpReceiver _receiver;
        private readonly ILogger<TCPSpeedBatchListener> _logger;
        private readonly HttpClient _http;
        private readonly List<RawSpeedPacket> _batch = new();
        private readonly int _batchSize;
        private readonly Timer _timer;
        private readonly object _lock = new();

        public TCPSpeedBatchListener(
            ITcpReceiver tcpReceiver,
            IHttpClientFactory httpFactory,
            IOptions<EventListenerConfiguration> opts,
            ILogger<TCPSpeedBatchListener> logger)
        {
            var config = opts.Value;
            _receiver = tcpReceiver;
            _logger = logger;
            _http = httpFactory.CreateClient("IngestApi");
            _batchSize = config.BatchSize;

            // schedule periodic flush
            _timer = new Timer(
                _ => FlushAsync().GetAwaiter().GetResult(),
                null,
                TimeSpan.FromSeconds(config.IntervalSeconds),
                TimeSpan.FromSeconds(config.IntervalSeconds)
            );
        }

        /// <summary>
        /// Starts receiving TCP messages.
        /// </summary>
        public Task StartListeningAsync(CancellationToken ct)
        {
            return _receiver.ReceiveAsync(async (buffer, endpoint) =>
            {
                var speedEvent = RawSpeedPacketParser.Parse(buffer, endpoint.ToString());
                Enqueue(speedEvent);
            }, ct);
        }

        /// <summary>
        /// Adds an event to the batch and sends when batch size is reached.
        /// </summary>
        public void Enqueue(RawSpeedPacket msg)
        {
            List<RawSpeedPacket>? toSend = null;

            lock (_lock)
            {
                _batch.Add(msg);
                if (_batch.Count >= _batchSize)
                {
                    toSend = new List<RawSpeedPacket>(_batch);
                    _batch.Clear();
                }
            }

            if (toSend != null)
                _ = SendBatchAsync(toSend);
        }

        private async Task FlushAsync()
        {
            List<RawSpeedPacket>? toSend = null;
            lock (_lock)
            {
                if (_batch.Count > 0)
                {
                    toSend = new List<RawSpeedPacket>(_batch);
                    _batch.Clear();
                }
            }

            if (toSend != null)
                await SendBatchAsync(toSend);
        }

        private async Task SendBatchAsync(List<RawSpeedPacket> batch)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/v1/SpeedEvent", batch);
                resp.EnsureSuccessStatusCode();
                _logger.LogInformation("Posted {Count} TCP events successfully.", batch.Count);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when posting TCP batch of {Count} events.", batch.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in TCPSpeedBatchListener.SendBatchAsync");
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
            FlushAsync().GetAwaiter().GetResult();
            _receiver.Dispose();
        }
    }
}
