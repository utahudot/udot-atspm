using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Bcpg;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Services.Receivers;
using static MailKit.Telemetry;

namespace Utah.Udot.Atspm.Infrastructure.Services.Listeners
{
    public class UDPSpeedBatchListener : IDisposable
    {
        private readonly System.Net.Sockets.Socket _socket;
        private readonly IUdpReceiver _receiver;
        private readonly ILogger<UDPSpeedBatchListener> _logger;
        private readonly HttpClient _http;
        private readonly List<SpeedEvent> _batch = new();
        private readonly int _batchSize;
        private readonly Timer _timer;
        private readonly object _lock = new();
        private readonly EventListenerConfiguration _config;

        public UDPSpeedBatchListener(
       IUdpReceiver receiver,
       IHttpClientFactory httpFactory,
       IOptions<EventListenerConfiguration> opts,
       ILogger<UDPSpeedBatchListener> logger)
        {
            var config = opts.Value;
            _receiver = receiver;
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
        /// Starts receiving UDP datagrams.
        /// </summary>
        public Task StartListeningAsync(CancellationToken ct)
        {
            return _receiver.ReceiveAsync(async (buffer, endpoint) =>
            {
                var speedEvent = SpeedEventParser.Parse(buffer, endpoint.ToString());
                Enqueue(speedEvent);
            }, ct);
        }

        /// <summary>
        /// Adds an event to the batch; triggers send when batch size reached.
        /// </summary>
        public void Enqueue(SpeedEvent msg)
        {
            List<SpeedEvent>? toSend = null;

            lock (_lock)
            {
                _batch.Add(msg);
                if (_batch.Count >= _batchSize)
                {
                    toSend = new List<SpeedEvent>(_batch);
                    _batch.Clear();
                }
            }

            if (toSend != null)
                _ = SendBatchAsync(toSend);
        }

        private async Task FlushAsync()
        {
            List<SpeedEvent>? toSend = null;

            lock (_lock)
            {
                if (_batch.Count > 0)
                {
                    toSend = new List<SpeedEvent>(_batch);
                    _batch.Clear();
                }
            }

            if (toSend != null)
                await SendBatchAsync(toSend);
        }

        private async Task SendBatchAsync(List<SpeedEvent> batch)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/v1/SpeedEvent", batch);
                resp.EnsureSuccessStatusCode();
                _logger.LogInformation("Posted {Count} UDP events successfully.", batch.Count);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when posting UDP batch of {Count} events.", batch.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in UDPSpeedBatchListener.SendBatchAsync");
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
