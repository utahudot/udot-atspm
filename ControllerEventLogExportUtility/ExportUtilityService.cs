using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ControllerEventLogExportUtility
{
    internal class ExportUtilityService : IHostedService
    {
        private readonly ILogger _log;
        private IServiceProvider _serviceProvider;
        private IOptions<ExtractConsoleConfiguration> _options;
        public ExportUtilityService(ILogger<ExportUtilityService> log, IServiceProvider serviceProvider, IOptions<ExtractConsoleConfiguration> options) =>
                (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Extraction path {_options.Value.Path}");

            try
            {
                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var eventRepository = scope.ServiceProvider.GetService<IControllerEventLogRepository>();

                    foreach (var s in _options.Value.Dates)
                    {
                        Console.WriteLine($"Extracting event logs for {s:dd/MM/yyyy}");
                    }

                    var archiveQuery = eventRepository.GetList().Where(i => _options.Value.Dates.Any(d => i.ArchiveDate == d));

                    if (_options.Value.Included != null)
                    {
                        foreach (var s in _options.Value.Included)
                        {
                            Console.WriteLine($"Extracting event logs for signal {s}");
                        }

                        archiveQuery = archiveQuery.Where(i => _options.Value.Included.Any(d => i.SignalId == d));
                    }

                    if (_options.Value.Excluded != null)
                    {
                        foreach (var s in _options.Value.Excluded)
                        {
                            Console.WriteLine($"Excluding event logs for signal {s}");
                        }

                        archiveQuery = archiveQuery.Where(i => !_options.Value.Excluded.Contains(i.SignalId));
                    }

                    //int archiveCount = archiveQuery.Count();
                    int processedCount = 0;

                    //Console.WriteLine($"Starting to process {archiveCount} archives");


                    var archives = await archiveQuery.Select(s => new ControllerLogArchive() { SignalId = s.SignalId, ArchiveDate = s.ArchiveDate }).ToListAsync(cancellationToken);

                    //while (skip < count && !cancellationToken.IsCancellationRequested)
                    //{

                    foreach (var archive in archives)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        Console.Write($"Writing... {archive.SignalId} ({archives.IndexOf(archive) + 1} of {archives.Count})");

                        var log = await eventRepository.LookupAsync(archive);

                        var file = await WriteLog(log);

                        //Console.CursorLeft = 0;
                        do { Console.Write("\b \b"); } while (Console.CursorLeft > 0);
                        Console.WriteLine($"Completed {file.FullName} ({archives.IndexOf(archive) + 1} of {archives.Count})");

                        processedCount++;
                    }

                    //skip = skip + take;
                    //}

                    Console.WriteLine($"processedCount: {processedCount}");
                }
            }
            catch (Exception e)
            {

                _log.LogError("Exception: {e}", e);
            }


            //_serviceProvider?.GetService<IHostApplicationLifetime>()?.StopApplication();
            //return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine();
            Console.WriteLine($"Operation Cancelled...");

            return Task.CompletedTask;
        }

        public async Task<FileInfo> WriteLog(ControllerLogArchive archive)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(Path.Combine(_options.Value.Path.FullName, archive.ArchiveDate.ToString("MM-dd-yyyy")));

                dir.Create();

                var path = Path.Combine(dir.FullName, $"{archive.SignalId}-{archive.ArchiveDate:MM-dd-yyyy}.csv");

                await File.WriteAllLinesAsync(path, new string[] { "SignalId, Timestamp, EventCode, EventParam" });

                var csv = archive.LogData.Select(x => $"{archive.SignalId},{x.Timestamp:s},{x.EventCode},{x.EventParam}");

                await File.AppendAllLinesAsync(path, csv);

                return new FileInfo(path);
            }
            catch (Exception e)
            {
                _log.LogError("WriteLog Exception: {e}", e);
                return await Task.FromException<FileInfo>(e);
            }
        }
    }
}