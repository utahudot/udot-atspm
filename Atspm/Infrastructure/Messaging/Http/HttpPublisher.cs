using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Utah.Udot.Atspm.Infrastructure.Messaging;          // for EventBatchEnvelope

public class HttpEnvelopePublisher
    : IEventPublisher<EventBatchEnvelope>, IDisposable
{
    private readonly HttpClient _client;
    private readonly int _batchSize;
    private readonly TimeSpan _flushInterval;
    private readonly string _endpoint;
    private readonly List<EventBatchEnvelope> _buffer = new();
    private readonly Timer _timer;
    private readonly object _lock = new();

    public HttpEnvelopePublisher(
        HttpClient client,
        IOptions<EventListenerConfiguration> opts
    )
    {
        _client = client;
        _batchSize = opts.Value.BatchSize;
        _flushInterval = TimeSpan.FromSeconds(opts.Value.IntervalSeconds);
        _endpoint = opts.Value.ApiEndPoint;    // e.g. "api/v1.0/EventLog"

        // start periodic flush
        _timer = new Timer(_ => FlushAsync().GetAwaiter().GetResult(),
                           null,
                           _flushInterval,
                           _flushInterval);
    }

    public async Task PublishAsync(
       IReadOnlyList<EventBatchEnvelope> batch,
       CancellationToken ct = default
   )
    {
        // you’ll want to chunk if batch.Count is huge:
        const int MaxChunk = 500;
        for (var i = 0; i < batch.Count; i += MaxChunk)
        {
            var slice = batch.Skip(i).Take(MaxChunk);
            var resp = await _client.PostAsJsonAsync(_endpoint, slice, ct);
            resp.EnsureSuccessStatusCode();
        }
    }

    public async Task PublishAsync(
        EventBatchEnvelope envelope,
        CancellationToken ct = default
    )
    {
        List<EventBatchEnvelope>? toSend = null;
        lock (_lock)
        {
            _buffer.Add(envelope);
            if (_buffer.Count >= _batchSize)
            {
                toSend = new List<EventBatchEnvelope>(_buffer);
                _buffer.Clear();
            }
        }
        if (toSend != null)
            await SendBatchAsync(toSend, ct);
    }

    private async Task FlushAsync()
    {
        List<EventBatchEnvelope>? toSend = null;
        lock (_lock)
        {
            if (_buffer.Count > 0)
            {
                toSend = new List<EventBatchEnvelope>(_buffer);
                _buffer.Clear();
            }
        }
        if (toSend != null)
            await SendBatchAsync(toSend, CancellationToken.None);
    }

    private async Task SendBatchAsync(
        List<EventBatchEnvelope> batch,
        CancellationToken ct
    )
    {
        var resp = await _client.PostAsJsonAsync(_endpoint, batch, ct);
        resp.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        _timer.Dispose();
        FlushAsync().GetAwaiter().GetResult();
    }
}
