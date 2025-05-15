using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Infrastructure.Services.DownloaderClients;
using Utah.Udot.Atspm.Infrastructure.Services.Listeners;

public class EventListenerWorker : BackgroundService
{
    private readonly UDPSpeedBatchListener _udp;
    private readonly TCPSpeedBatchListener _tcp;
    private readonly ILogger<EventListenerWorker> _logger;

    public EventListenerWorker(
        UDPSpeedBatchListener udp,
        TCPSpeedBatchListener tcp,
        ILogger<EventListenerWorker> logger)
    {
        _udp = udp;
        _tcp = tcp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EventListenerWorker starting up.");
        var udpTask = _udp.StartListeningAsync(stoppingToken);
        var tcpTask = _tcp.StartListeningAsync(stoppingToken);

        await Task.WhenAll(udpTask, tcpTask);
        _logger.LogInformation("EventListenerWorker shutting down.");
    }
}
