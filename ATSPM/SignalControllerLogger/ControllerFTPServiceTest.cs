using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Threading.Channels;
using ControllerLogger.Models;
using ControllerLogger.Data;
using System.Text.Json;
using ControllerLogger.Helpers;
using System.Diagnostics;
using ControllerLogger.Configuration;
namespace ControllerLogger.Services
{
    public class ControllerFTPServiceTest : BackgroundService
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        protected readonly IOptions<ControllerFTPSettings> _options;

        internal IReadOnlyList<Signal> signalList;

        internal Channel<DirectoryInfo> fromControllerPipe;

        public ControllerFTPServiceTest(ILogger<FileETLHostedService> log, IServiceProvider serviceProvider, IOptions<ControllerFTPSettings> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _log.LogInformation("object: {Object} method: {Method}", MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);

            await Work(stoppingToken);
        }

        private async Task Work(CancellationToken stoppingToken)
        {
            var options = new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait };
            fromControllerPipe = Channel.CreateBounded<DirectoryInfo>(options);

            _log.LogWarning($"Work: {DateTime.Now} - {_options.Value.PollTime} - {_options.Value.TimeOffset}");

           

            while (!stoppingToken.IsCancellationRequested)
            {
                TempReader();

                DirectoryInfo dir = new DirectoryInfo(_options.Value.RootPath);

                signalList = _serviceProvider.GetRequiredService<MOEContext>().Signals.ToList();

                foreach (DirectoryInfo d in dir.GetDirectories("", SearchOption.AllDirectories))
                {
                    //_log.LogInformation($"Getting Directory: {d.Name}");





                    if (signalList.Select(s => s.SignalId).Contains(d.Name))
                    {
                        //_log.LogInformation($"Directory is valid Signal: {d.Name}");

                        //var path = Path.Combine("C:\\ControlLogs", d.Name);
                        //Directory.CreateDirectory(path);

                        //foreach (FileInfo f in d.GetFiles("", SearchOption.AllDirectories))
                        //{
                        //    f.CopyTo(Path.Combine(path, f.Name));
                        //}

                        _log.LogInformation($"Writing Directory: {d.Name}");

                        await fromControllerPipe.Writer.WriteAsync(d);

                    }
                    else
                    {
                        _log.LogError($"Directory is NOT valid Signal: {d.Name}");
                    }






                    //await Task.Delay(TimeSpan.FromMilliseconds(_options.Value.TimeOffset), stoppingToken);
                }

                fromControllerPipe.Writer.Complete();

                await Task.Delay(TimeSpan.FromMinutes(_options.Value.PollTime), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _log.LogError("object: {Object} method: {Method}", MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name);

            await base.StopAsync(cancellationToken);
        }

        public void TempReader()
        {
            

            async Task Work()
            {
                await foreach (var item in fromControllerPipe.Reader.ReadAllAsync())
                {
                    _log.LogWarning($"Reading: {item.FullName}");
                    //await Task.Delay(TimeSpan.FromMilliseconds(500));
                }
            }

            Task.Run(async () =>
            {
                await Work();
                
                _log.LogInformation($"Step Complete: {nameof(TempReader)}");
            });
        }
    }
}
