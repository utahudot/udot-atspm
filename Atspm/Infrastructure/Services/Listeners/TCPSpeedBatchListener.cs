using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Infrastructure.Messaging;
using Utah.Udot.Atspm.Infrastructure.Services.Listeners;
using Utah.Udot.Atspm.Infrastructure.Services.Receivers;

public class TCPSpeedBatchListener : IDisposable
{
    private readonly EventListenerConfiguration _config;
    private readonly ITcpReceiver _receiver;
    private readonly ILogger<TCPSpeedBatchListener> _logger;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IEventPublisher<EventBatchEnvelope> _eventPublisher;
    private readonly List<SpeedEvent> _batch = new();
    private readonly int _batchSize;
    private readonly object _lock = new();

    public TCPSpeedBatchListener(
        ITcpReceiver tcpReceiver,
        IOptions<EventListenerConfiguration> opts,
        ILogger<TCPSpeedBatchListener> logger,
        IDeviceRepository deviceRepository,
        IEventPublisher<EventBatchEnvelope> eventPublisher)
    {
        _config = opts.Value;
        _receiver = tcpReceiver;
        _logger = logger;
        _deviceRepository = deviceRepository;
        _eventPublisher = eventPublisher;
        _batchSize = _config.BatchSize;
    }

    public Task StartListeningAsync(CancellationToken ct)
    {
        _logger.LogInformation("TCP listener starting on port {Port}", _config.TcpPort);
        _logger.LogInformation("Batch size from config: {BatchSize}", _batchSize);
        return _receiver.ReceiveAsync(async (buffer, endpoint) =>
        {
            try
            {
                var speedEvent = RawSpeedPacketParser.Parse(buffer, endpoint.ToString());

                _logger.LogDebug("Parsed packet from {Endpoint}: {Sensor}, {Mph}mph", endpoint, speedEvent.DetectorId, speedEvent.Mph);

                Enqueue(speedEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse incoming TCP packet from {Endpoint}", endpoint);
            }
        }, ct);
    }

    public void Enqueue(SpeedEvent msg)
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

    private async Task FlushAsync()
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
                .ToDictionary(
                    d => d.DeviceIdentifier,
                    d => (d.Location.LocationIdentifier, d.Id)
                );
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
            _logger.LogInformation("Published {Count} TCP envelopes for {SensorCount} sensors", envelopes.Count, envelopes.Select(e => e.DeviceId).Distinct().Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk publish failed for {Count} TCP envelopes", envelopes.Count);
        }
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing TCP listener. Flushing batch and releasing socket.");
        FlushAsync().GetAwaiter().GetResult();
        _receiver.Dispose();
    }
}
