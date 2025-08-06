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

            _logger.LogInformation("Calling StartListeningAsync...");
            var udpTask = udp.StartListeningAsync(stoppingToken);
            _logger.LogInformation("StartListeningAsync returned. Awaiting task...");
            await udpTask;
            _logger.LogInformation("UDP listener finished or was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in EventListenerWorker");
            throw;
        }

        _logger.LogInformation("EventListenerWorker shutting down.");
    }

}
