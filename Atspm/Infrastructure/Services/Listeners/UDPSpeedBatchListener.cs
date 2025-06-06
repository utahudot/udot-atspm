using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Utah.Udot.Atspm.Infrastructure.Messaging;
using Utah.Udot.Atspm.Infrastructure.Services.Listeners;
using Utah.Udot.Atspm.Infrastructure.Services.Receivers;

public class UDPSpeedBatchListener : SpeedBatchListenerBase
{
    private readonly IUdpReceiver _receiver;

    public UDPSpeedBatchListener(
        IUdpReceiver receiver,
        IOptions<EventListenerConfiguration> opts,
        ILoggerFactory loggerFactory,
        IDeviceRepository deviceRepository,
        IEventPublisher<EventBatchEnvelope> publisher)
        : base(opts.Value, loggerFactory, deviceRepository, publisher)
    {
        _receiver = receiver;
    }


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
                _logger.LogError(ex, "Failed to parse incoming UDP packet from {Endpoint}", endpoint);
            }
        }, ct);
    }

    protected override void DisposeInternal() => _receiver.Dispose();
}
