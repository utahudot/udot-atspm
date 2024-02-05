using ATSPM.Application.Configuration;
using ATSPM.Application.Repositories;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ATSPM.Infrastructure.Services.HostedServices
{
    public class ExportUtilityService : IHostedService
    {
        private readonly ILogger _log;
        private IServiceProvider _serviceProvider;
        private IOptions<EventLogExtractConfiguration> _options;

        public ExportUtilityService(ILogger<ExportUtilityService> log, IServiceProvider serviceProvider, IOptions<EventLogExtractConfiguration> options) =>
                (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //_serviceProvider.PrintHostInformation();

            try
            {
                _log.LogInformation("Extraction Path: {path}", _options.Value.Path);
                _log.LogInformation("Extraction File Formate: {format}", _options.Value.FileFormat);

                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var eventRepository = scope.ServiceProvider.GetService<IControllerEventLogRepository>();

                    foreach (var s in _options.Value.Dates)
                    {
                        _log.LogInformation("Extracting Event Logs for Date(s): {date}", s.ToString("dd/MM/yyyy"));
                    }

                    var archiveQuery = eventRepository.GetList().Where(i => _options.Value.Dates.Any(d => i.ArchiveDate == d));

                    if (_options.Value.Included != null)
                    {
                        foreach (var s in _options.Value.Included)
                        {
                            _log.LogInformation("Including Event Logs for Location(s): {Location}", s);
                        }

                        archiveQuery = archiveQuery.Where(i => _options.Value.Included.Any(d => i.SignalIdentifier == d));
                    }

                    if (_options.Value.Excluded != null)
                    {
                        foreach (var s in _options.Value.Excluded)
                        {
                            _log.LogInformation("Excluding Event Logs for Location(s): {Location}", s);
                        }

                        archiveQuery = archiveQuery.Where(i => !_options.Value.Excluded.Contains(i.SignalIdentifier));
                    }

                    int processedCount = 0;

                    var archives = await archiveQuery.Select(s => new ControllerLogArchive() { SignalIdentifier = s.SignalIdentifier, ArchiveDate = s.ArchiveDate }).ToListAsync(cancellationToken);

                    _log.LogInformation("Number of Event Log Archives to Process: {count}", archives.Count);

                    foreach (var archive in archives)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        Console.Write($"Writing... {archive.SignalIdentifier} ({archives.IndexOf(archive) + 1} of {archives.Count})");

                        var log = await eventRepository.LookupAsync(archive);

                        var file = await WriteLog(log);

                        do { Console.Write("\b \b"); } while (Console.CursorLeft > 0);
                        Console.WriteLine($"Completed {file.FullName} ({archives.IndexOf(archive) + 1} of {archives.Count})");

                        processedCount++;
                    }

                    _log.LogInformation("Log Archives Processed: {count}", processedCount);
                }
            }
            catch (Exception e)
            {

                _log.LogError("Exception: {e}", e);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine();
            Console.WriteLine($"Operation Completed or Cancelled...");

            return Task.CompletedTask;
        }

        public async Task<FileInfo> WriteLog(ControllerLogArchive archive)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(Path.Combine(_options.Value.Path.FullName, archive.ArchiveDate.ToString("MM-dd-yyyy")));

                dir.Create();

                var path = Path.Combine(dir.FullName, $"{archive.SignalIdentifier}-{archive.ArchiveDate:MM-dd-yyyy}.csv");

                await File.WriteAllLinesAsync(path, new string[] { "locationId, Timestamp, EventCode, EventParam" });

                var csv = archive.LogData.Select(x => $"{archive.SignalIdentifier},{x.Timestamp.ToString(_options.Value.DateTimeFormat)},{x.EventCode},{x.EventParam}");

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