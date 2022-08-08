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

namespace ATSPM.SignalControllerLogger
{


    public class TPLDataflowService : BackgroundService
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<SignalControllerLoggerConfiguration> _options;

        //private IReadOnlyList<Signal> _signalList;

        public TPLDataflowService(ILogger<TPLDataflowService> log, IServiceProvider serviceProvider, IOptions<SignalControllerLoggerConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);






        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //stoppingToken.Register(() => Console.WriteLine($"-------------------------------------------------------------------------------------------stoppingToken"));


            //while(!stoppingToken.IsCancellationRequested)
            //{
            //var testSignals = new List<string>() { "9704", "9705", "9721", "9741", "9709" };

            IReadOnlyList<Signal> _signalList;
            //List<ControllerLogArchive> finalList;

            using (var scope = _serviceProvider.CreateScope())
            {
                _signalList = scope.ServiceProvider.GetService<ISignalRepository>().GetLatestVersionOfAllSignals().Where(w => w.Enabled).Take(50).ToList();
                //_signalList = scope.ServiceProvider.GetService<ISignalRepository>().GetLatestVersionOfAllSignals().Where(w => w.Enabled && w.SignalId == "1091").ToList();
                //_signalList = scope.ServiceProvider.GetService<ISignalRepository>().GetLatestVersionOfAllSignals().Where(w => w.Enabled && testSignals.Contains(w.SignalId)).ToList();
            }

            using (var process = new SignalControllerDataFlow(_serviceProvider.GetService<ILogger<SignalControllerDataFlow>>(), _serviceProvider))
            {
                await process.ExecuteAsync(_signalList).ContinueWith(t => Console.WriteLine($"-----------------------it's done!!! {t.Status} --- {System.Diagnostics.Process.GetCurrentProcess().WorkingSet64}"));
            }


            //var process = new SignalControllerDataFlow(_serviceProvider.GetService<ILogger<SignalControllerDataFlow>>(), _serviceProvider);
            //await process.ExecuteAsync(_signalList);

            //Console.WriteLine($"PRE: {System.Diagnostics.Process.GetCurrentProcess().WorkingSet64} - {System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64} - {GC.GetTotalMemory(false)}");

            //GC.Collect();

            //Console.WriteLine($"POST: {System.Diagnostics.Process.GetCurrentProcess().WorkingSet64} - {System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64} - {GC.GetTotalMemory(false)}");

            //await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            //}
        }
    }
}
