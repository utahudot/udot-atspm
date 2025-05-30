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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EventListenerWorker starting up.");
        _ = Task.Run(() => _udp.StartListeningAsync(stoppingToken), stoppingToken);
        _ = Task.Run(() => _tcp.StartListeningAsync(stoppingToken), stoppingToken);

        return Task.CompletedTask;
    }
}
