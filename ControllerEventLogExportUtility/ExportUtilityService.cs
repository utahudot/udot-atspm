using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATSPM.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ControllerEventLogExportUtility
{
    internal class ExportUtilityService : BackgroundService
    {
        private readonly ILogger _log;
        private IServiceProvider _serviceProvider;
        private IOptions<ExtractConsoleConfiguration> _options;

        public ExportUtilityService(ILogger<ExportUtilityService> log, IServiceProvider serviceProvider, IOptions<ExtractConsoleConfiguration> options) =>
            (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var eventRepository = scope.ServiceProvider.GetService<IControllerEventLogRepository>();

                    int skip = 0;
                    int take = 20;
                    //bool loop = true;


                    //while (skip < 100)
                    //{
                        var archives = eventRepository.GetList().Where(i => i.ArchiveDate == DateTime.Parse("1/1/2020")).Select(s => new ControllerLogArchive() {SignalId = s.SignalId, ArchiveDate = s.ArchiveDate }).ToList();

                    Console.WriteLine($"archives time: {sw.Elapsed}");

                    //var logs = eventRepository.GetList().Where(i => i.ArchiveDate == DateTime.Parse("1/1/2020")).Take(100).ToList();
                    //var logs = eventRepository.GetList().Where(i => i.ArchiveDate == DateTime.Parse("1/1/2020")).Skip(skip).Take(take).ToList();
                    //var logs = await eventRepository.GetListAsync(g => g.SignalId == "1001");

                    //if (logs.Count == 0)
                    //    loop = false;

                    

                    foreach (var archive in archives)
                    {
                        Console.Write($"Writing... {archive.SignalId} {archives.IndexOf(archive)} of {archives.Count}");

                        var logs = await eventRepository.LookupAsync(archive);

                        DirectoryInfo dir = new DirectoryInfo("C:\\temp\\exports\\1-1-2020");

                        dir.Create();

                        var path = Path.Combine(dir.FullName, $"{archive.SignalId}.csv");

                        await File.WriteAllLinesAsync(path, new string[] { "SignalId, Timestamp, EventCode, EventParam" });

                        var csv = logs.LogData.Select(x => $"{archive.SignalId},{x.Timestamp:s},{x.EventCode},{x.EventParam}");

                        await File.AppendAllLinesAsync(path, csv);

                        Console.WriteLine($"Completed {archive.SignalId} {logs.LogData.Count}");
                    }


                    //foreach (var log in logs)
                    //{
                    //    Console.WriteLine($"log: {log}");
                    //}

                    //

                    //

                    //Console.WriteLine($"complete?: {logs.Count}");
                    //Console.WriteLine($"complete?: skip {skip} take {take}");

                    //skip = skip + take;
                    //skip = 100;
                    //}
                }
            }
            catch (Exception e)
            {

                _log.LogError("Exception: {e}", e);
            }
            finally
            {
                Console.WriteLine($"total time: {sw.Elapsed}");
                sw.Stop();
            }

            //_serviceProvider?.GetService<IHostApplicationLifetime>()?.StopApplication();
        }
    }
}
