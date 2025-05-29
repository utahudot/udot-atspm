using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using System.Text;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Messaging;
using Utah.Udot.Atspm.Infrastructure.Services.Receivers;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Services;

namespace Utah.Udot.Atspm.Infrastructure.Services.Listeners
{
    /// <summary>
    /// Batches incoming UDP speed events and posts to the DataApi.
    /// </summary>
    public class UDPSpeedBatchListener : IDisposable
    {
        private EventListenerConfiguration _config;
        private readonly IUdpReceiver _receiver;
        private readonly ILogger<UDPSpeedBatchListener> _logger;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IEventPublisher<EventBatchEnvelope> _eventPublisher;
        private readonly List<SpeedEvent> _batch = new();
        private readonly int _batchSize;
        private readonly Timer _timer;
        private readonly object _lock = new();

        public UDPSpeedBatchListener(
            IUdpReceiver udpReceiver,
            IOptions<EventListenerConfiguration> opts,
            ILogger<UDPSpeedBatchListener> logger,
            IDeviceRepository deviceRepository,
            IEventPublisher<EventBatchEnvelope> eventPublisher)
        {
            _config = opts.Value;
            _receiver = udpReceiver;
            _logger = logger;
            _deviceRepository = deviceRepository;
            _eventPublisher = eventPublisher;
            _batchSize = _config.BatchSize;

            // schedule periodic flush
            _timer = new Timer(
                _ => FlushAsync().GetAwaiter().GetResult(),
                null,
                TimeSpan.FromSeconds(_config.IntervalSeconds),
                TimeSpan.FromSeconds(_config.IntervalSeconds)
            );
        }

        /// <summary>
        /// Starts receiving UDP messages.
        /// </summary>
        public Task StartListeningAsync(CancellationToken ct)
        {
            return _receiver.ReceiveAsync(async (buffer, endpoint) =>
            {
                try
                {
                    var speedEvent = RawSpeedPacketParser.Parse(buffer, endpoint.ToString());
                    Enqueue(speedEvent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse incoming packet from {Endpoint}", endpoint);
                }
            }, ct);
        }


        /// <summary>
        /// Adds an event to the batch and sends when batch size is reached.
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
            if (batch == null || batch.Count == 0)
            {
                _logger.LogWarning("SendBatchAsync called with empty batch — skipping.");
                return;
            }

            // 1) Group by SensorId
            var groups = batch.GroupBy(p => p.DetectorId).ToList();
            var sensorIds = groups.Select(g => g.Key).ToList();

            // 2) Load mappings
            IDictionary<string, (string LocationIdentifier, int DeviceId)> mappings;
            try
            {
                mappings = _deviceRepository
                    .GetList()
                    .Where(d => d.DeviceType == DeviceTypes.SpeedSensor)
                    .ToDictionary(
                        d => d.DeviceIdentifier,
                        d => (d.Location.LocationIdentifier, d.Id)
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to load device mappings for sensors {SensorIds}",
                    sensorIds);
                return;
            }

            // 3) Build one envelope per sensor
            var envelopes = groups
                .Where(g => mappings.ContainsKey(g.Key))
                .Select(g =>
                {
                    var (location, deviceId) = mappings[g.Key];
                    return new EventBatchEnvelope
                    {
                        DataType = nameof(SpeedEvent),
                        Start = g.Min(p => p.Timestamp),
                        End = g.Max(p => p.Timestamp),
                        LocationIdentifier = location,
                        DeviceId = deviceId,
                        Items = JToken.FromObject(g.ToList())
                    };
                })
                .ToList();

            if (!envelopes.Any()) return;

            // 4) Bulk-publish all envelopes in one call
            try
            {
                await _eventPublisher.PublishAsync(envelopes, CancellationToken.None);
                _logger.LogInformation("Published {Count} UDP envelopes", envelopes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk publish failed for {Count} UDP envelopes", envelopes.Count);
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
