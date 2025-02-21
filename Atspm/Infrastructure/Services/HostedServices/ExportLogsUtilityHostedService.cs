#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.HostedServices/ExportLogsUtilityHostedService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Utah.Udot.Atspm.Infrastructure.Services.HostedServices
{
    public class ExportUtilityService : IHostedService
    {
        private readonly ILogger _log;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<EventLogExtractConfiguration> _options;

        public ExportUtilityService(ILogger<ExportUtilityService> log, IServiceProvider serviceProvider, IOptions<EventLogExtractConfiguration> options) =>
                (_log, _serviceProvider, _options) = (log, serviceProvider, options);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //_services.PrintHostInformation();

            try
            {
                _log.LogInformation("Extraction Path: {path}", _options.Value.Path);
                _log.LogInformation("Extraction File Formate: {format}", _options.Value.FileFormat);

                //using (var scope = _services.CreateAsyncScope())
                //{
                //    var eventRepository = scope.ServiceProvider.GetService<IIndianaEventLogRepository>();

                //    foreach (var s in _options.Value.Dates)
                //    {
                //        _log.LogInformation("Extracting Event Logs for Date(s): {date}", s.ToString("dd/MM/yyyy"));
                //    }

                //    var archiveQuery = eventRepository.GetList().Where(i => _options.Value.Dates.Any(d => i.ArchiveDate == d));

                //    if (_options.Value.IncludedLocations != null)
                //    {
                //        foreach (var s in _options.Value.IncludedLocations)
                //        {
                //            _log.LogInformation("Including Event Logs for Location(s): {Location}", s);
                //        }

                //        archiveQuery = archiveQuery.Where(i => _options.Value.IncludedLocations.Any(d => i.SignalIdentifier == d));
                //    }

                //    if (_options.Value.ExcludedLocations != null)
                //    {
                //        foreach (var s in _options.Value.ExcludedLocations)
                //        {
                //            _log.LogInformation("Excluding Event Logs for Location(s): {Location}", s);
                //        }

                //        archiveQuery = archiveQuery.Where(i => !_options.Value.ExcludedLocations.Contains(i.SignalIdentifier));
                //    }

                //    int processedCount = 0;

                //    var archives = await archiveQuery.Select(s => new ControllerLogArchive() { SignalIdentifier = s.SignalIdentifier, ArchiveDate = s.ArchiveDate }).ToListAsync(cancellationToken);

                //    _log.LogInformation("Number of Event Log Archives to Process: {count}", archives.Count);

                //    foreach (var archive in archives)
                //    {
                //        if (cancellationToken.IsCancellationRequested) break;

                //        Console.Write($"Writing... {archive.SignalIdentifier} ({archives.IndexOf(archive) + 1} of {archives.Count})");

                //        var log = await eventRepository.LookupAsync(archive);

                //        var file = await WriteLog(log);

                //        do { Console.Write("\b \b"); } while (Console.CursorLeft > 0);
                //        Console.WriteLine($"Completed {file.FullName} ({archives.IndexOf(archive) + 1} of {archives.Count})");

                //        processedCount++;
                //    }

                //    _log.LogInformation("Log Archives Processed: {count}", processedCount);
                //}
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

        //public async Task<FileInfo> WriteLog(ControllerLogArchive archive)
        //{
        //    try
        //    {
        //        DirectoryInfo dir = new DirectoryInfo(Path.Combine(_options.Value.Path.FullName, archive.ArchiveDate.ToString("MM-dd-yyyy")));

        //        dir.Create();

        //        var path = Path.Combine(dir.FullName, $"{archive.SignalIdentifier}-{archive.ArchiveDate:MM-dd-yyyy}.csv");

        //        await File.WriteAllLinesAsync(path, new string[] { "LocationId, Timestamp, EventCode, EventParam" });

        //        var csv = archive.LogData.Select(x => $"{archive.SignalIdentifier},{x.Timestamp.ToString(_options.Value.DateTimeFormat)},{x.EventCode},{x.EventParam}");

        //        await File.AppendAllLinesAsync(path, csv);

        //        return new FileInfo(path);
        //    }
        //    catch (Exception e)
        //    {
        //        _log.LogError("WriteLog Exception: {e}", e);
        //        return await Task.FromException<FileInfo>(e);
        //    }
        //}
    }
}