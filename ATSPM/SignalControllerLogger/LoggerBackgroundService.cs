using ATSPM.Application;
using ATSPM.Application.Configuration;
using ATSPM.Data.Models;
using ATSPM.Application.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ATSPM.Application.Services;

namespace ATSPM.SignalControllerLogger
{
    public class LoggerBackgroundService : BackgroundService
    {
        private readonly ILogger _log;
        private IServiceProvider _serviceProvider;

        public LoggerBackgroundService(ILogger<LoggerBackgroundService> log,IServiceProvider serviceProvider) =>
            (_log, _serviceProvider) = (log, serviceProvider);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{

            List<Signal> signals;

            try
            {
                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var signalRepository = scope.ServiceProvider.GetService<ISignalRepository>();
                    var controllerLoggingService = scope.ServiceProvider.GetService<ISignalControllerLoggerService>();

                    signals = signalRepository.GetLatestVersionOfAllSignals().Where(w => w.Enabled && w.ControllerTypeId == 4).ToList();
                    await controllerLoggingService.ExecuteAsync(signals, stoppingToken);
                }
            }
            catch (Exception e)
            {

                _log.LogError("Exception: {e}", e);
            }

            _serviceProvider.GetService<IHostApplicationLifetime>().StopApplication();

            //    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            //}
        }
    }
}
