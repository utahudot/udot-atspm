using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Infrastructure.Services.DownloaderClients;
using Utah.Udot.Atspm.Infrastructure.Services.Listeners;

public class EventListenerWorker : BackgroundService
{
    private readonly UDPSpeedBatchListener _udp;
    private readonly TCPSpeedBatchListener _tcp;
    public EventListenerWorker(
        UDPSpeedBatchListener udp,
        TCPSpeedBatchListener tcp)
    {
        _udp = udp;
        _tcp = tcp;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int udpPort = 10088;
        const int tcpPort = 10088;  // or distinct

        var udpTask = _udp.StartListeningAsync(stoppingToken);
        var tcpTask = _tcp.StartListeningAsync(stoppingToken);

        return Task.WhenAll(udpTask, tcpTask);
    }
}
