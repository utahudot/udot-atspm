using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Infrastructure.Services.Listeners;

public class EventListenerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventListenerWorker> _logger;

    public EventListenerWorker(IServiceScopeFactory scopeFactory, ILogger<EventListenerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EventListenerWorker starting up.");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var udp = scope.ServiceProvider.GetRequiredService<UDPSpeedBatchListener>();
            var tcp = scope.ServiceProvider.GetRequiredService<TCPSpeedBatchListener>();

            var udpTask = udp.StartListeningAsync(stoppingToken);
            var tcpTask = tcp.StartListeningAsync(stoppingToken);

            await Task.WhenAll(udpTask, tcpTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in EventListenerWorker");
            throw;
        }

        _logger.LogInformation("EventListenerWorker shutting down.");
    }

}
