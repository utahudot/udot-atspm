using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Messaging;

namespace Utah.Udot.Atspm.Infrastructure.Services.Listeners
{
    public abstract class SpeedBatchListenerBase : IDisposable
    {
        protected readonly EventListenerConfiguration _config;
        protected readonly ILogger _logger;
        protected readonly IDeviceRepository _deviceRepository;
        protected readonly IEventPublisher<EventBatchEnvelope> _eventPublisher;
        protected readonly int _batchSize;

        private readonly List<SpeedEvent> _batch = new();
        private readonly object _lock = new();

        protected SpeedBatchListenerBase(
            EventListenerConfiguration config,
            ILoggerFactory loggerFactory,
            IDeviceRepository deviceRepository,
            IEventPublisher<EventBatchEnvelope> eventPublisher)
        {
            _config = config;
            _logger = loggerFactory.CreateLogger(GetType()); // dynamically uses the subclass name
            _deviceRepository = deviceRepository;
            _eventPublisher = eventPublisher;
            _batchSize = _config.BatchSize;
        }


        protected void Enqueue(SpeedEvent msg)
        {
            List<SpeedEvent>? toSend = null;

            lock (_lock)
            {
                _batch.Add(msg);
                _logger.LogDebug("Enqueued SpeedEvent [{Sensor}, {Mph}mph] — current batch size: {Size}", msg.DetectorId, msg.Mph, _batch.Count);

                if (_batch.Count >= _batchSize)
                {
                    _logger.LogInformation("Batch size threshold reached. Sending batch of {Count} events.", _batch.Count);
                    toSend = new List<SpeedEvent>(_batch);
                    _batch.Clear();
                }
            }

            if (toSend != null)
                _ = SendBatchAsync(toSend);
        }

        protected async Task FlushAsync()
        {
            List<SpeedEvent>? toSend = null;

            lock (_lock)
            {
                if (_batch.Count > 0)
                {
                    _logger.LogInformation("Flushing remaining batch of {Count} SpeedEvents.", _batch.Count);
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

            var groups = batch.GroupBy(p => p.DetectorId).ToList();
            var sensorIds = groups.Select(g => g.Key).ToList();

            IDictionary<string, (string LocationIdentifier, int DeviceId)> mappings;
            try
            {
                mappings = _deviceRepository
                    .GetList()
                    .Where(d => d.DeviceType == DeviceTypes.SpeedSensor)
                    .GroupBy(d => d.DeviceIdentifier)
                    .ToDictionary(g => g.Key, g => (g.First().Location.LocationIdentifier, g.First().Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load device mappings for sensor IDs: {SensorIds}", sensorIds);
                return;
            }

            var envelopes = new List<EventBatchEnvelope>();

            foreach (var group in groups)
            {
                if (!mappings.TryGetValue(group.Key, out var map))
                {
                    _logger.LogWarning("No mapping found for sensor ID {SensorId} — skipping this group", group.Key);
                    continue;
                }

                try
                {
                    //debug log the envelope info
                    _logger.LogDebug($"Creating envelope for Location Identifier:{map.LocationIdentifier} Device:{map.DeviceId}");
                    var envelope = new EventBatchEnvelope
                    {
                        DataType = nameof(SpeedEvent),
                        Start = group.Min(p => p.Timestamp),
                        End = group.Max(p => p.Timestamp),
                        LocationIdentifier = map.LocationIdentifier,
                        DeviceId = map.DeviceId,
                        Items = JToken.FromObject(group.ToList())
                    };

                    envelopes.Add(envelope);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to build envelope for sensor {SensorId}", group.Key);
                }
            }

            if (!envelopes.Any())
            {
                _logger.LogWarning("No valid envelopes created from batch — skipping publish.");
                return;
            }

            try
            {
                await _eventPublisher.PublishAsync(envelopes, CancellationToken.None);
                _logger.LogInformation("Published {Count} envelopes for {SensorCount} sensors", envelopes.Count, envelopes.Select(e => e.DeviceId).Distinct().Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk publish failed for {Count} envelopes", envelopes.Count);
            }
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing listener. Flushing batch and releasing resources.");
            FlushAsync().GetAwaiter().GetResult();
            DisposeInternal();
        }

        protected abstract void DisposeInternal();
    }

}
