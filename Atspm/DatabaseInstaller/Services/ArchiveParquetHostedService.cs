#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Services/ArchiveParquetHostedService.cs
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

using DatabaseInstaller.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parquet;
using Parquet.Serialization;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using System.Collections.Concurrent;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace DatabaseInstaller.Services
{
    public class ArchiveParquetHostedService : IHostedService
    {
        private readonly ILogger<ArchiveParquetHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILocationRepository _locationRepository;
        private readonly ArchiveParquetCommandConfiguration _config;

        private readonly AsyncRetryPolicy _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                sleepDurations: Backoff.DecorrelatedJitterBackoffV2(
                        TimeSpan.FromSeconds(10),
                        retryCount: 5
                    )
                    .Concat(new[] { TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(30) }),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine(
                        $"Retry {retryCount} after {timeSpan.TotalSeconds} seconds due to {exception.Message}");
                });

        public ArchiveParquetHostedService(
            ILogger<ArchiveParquetHostedService> logger,
            IServiceProvider serviceProvider,
            ILocationRepository locationRepository,
            IOptions<ArchiveParquetCommandConfiguration> config)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _locationRepository = locationRepository;
            _config = config.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Discover date folders from filesystem if no date range provided
            var dateFolders = GetDateFolders();

            if (!dateFolders.Any())
            {
                _logger.LogWarning("No archive folders found in {InputPath} with prefix '{Prefix}'",
                    _config.InputPath, _config.FolderPrefix);
                return;
            }

            _logger.LogInformation("Found {Count} date folders to process", dateFolders.Count);

            foreach (var (date, folderPath) in dateFolders)
            {
                if (cancellationToken.IsCancellationRequested) break;

                _logger.LogInformation("Processing parquet files for {Date}", date.ToString("yyyy-MM-dd"));

                // If locations filter provided, use DB query; otherwise discover from filesystem
                List<Location> locations;
                if (!string.IsNullOrEmpty(_config.Locations) || _config.Device != null)
                {
                    locations = GetFilteredLocations();
                }
                else
                {
                    locations = GetLocationsFromFolder(folderPath, date);
                }

                if (!locations.Any())
                {
                    _logger.LogWarning("No locations found for {Date}, skipping", date);
                    continue;
                }

                var dayLogs = new ConcurrentBag<CompressedEventLogs<IndianaEvent>>();

                await Task.WhenAll(locations.Select(async location =>
                {
                    try
                    {
                        var filePath = Path.Combine(
                            folderPath,
                            $"{location.LocationIdentifier}_{date:yyyy-MM-dd}.parquet");

                        var log = await ReadParquetLogsAsync(filePath, date, location, cancellationToken);
                        if (log != null)
                            dayLogs.Add(log);
                        else
                            _logger.LogWarning("No parquet logs found for location {Location} on {Date}",
                                location.LocationIdentifier, date);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to read parquet for location {Location} on {Date}: {Message}",
                            location.LocationIdentifier, date, ex.Message);
                    }
                }));

                await InsertLogsWithRetryAsync(dayLogs.ToList(), date);
            }

            _logger.LogInformation("Parquet import completed.");
        }

        /// <summary>
        /// Returns all date folders in InputPath that match the prefix pattern,
        /// filtered by Start/End if provided.
        /// </summary>
        private List<(DateTime Date, string FolderPath)> GetDateFolders()
        {
            return Directory.EnumerateDirectories(_config.InputPath)
                .Select(folder =>
                {
                    var name = Path.GetFileName(folder); // e.g. "date=2026-03-08"
                    var parts = name.Split('=');
                    if (parts.Length == 2
                        && parts[0] == _config.FolderPrefix
                        && DateTime.TryParseExact(parts[1], "yyyy-MM-dd", null,
                            System.Globalization.DateTimeStyles.None, out var date))
                    {
                        return (Matched: true, Date: date, FolderPath: folder);
                    }

                    return (Matched: false, Date: default, FolderPath: folder);
                })
                .Where(x => x.Matched)
                .Where(x => (_config.Start == null || x.Date >= _config.Start.Value.Date)
                            && (_config.End == null || x.Date <= _config.End.Value.Date))
                .OrderBy(x => x.Date)
                .Select(x => (x.Date, x.FolderPath))
                .ToList();
        }

        /// <summary>
        /// When --locations or --device is specified, filter from the DB as before.
        /// </summary>
        private List<Location> GetFilteredLocations()
        {
            var query = _locationRepository.GetList()
                .Include(s => s.Devices)
                .AsQueryable();

            if (_config.Device != null)
                query = query.Where(l => l.Devices.Any(d => d.DeviceType == (DeviceTypes)_config.Device));

            if (!string.IsNullOrEmpty(_config.Locations))
            {
                var identifiers = _config.Locations.Split(',', StringSplitOptions.RemoveEmptyEntries);
                query = query.Where(l => identifiers.Contains(l.LocationIdentifier));
            }

            return query
                .FromSpecification(new ActiveLocationSpecification())
                .GroupBy(r => r.LocationIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();
        }

        /// <summary>
        /// When no filters are provided, discover locations directly from parquet filenames
        /// in the folder (e.g. 300030_2026-03-08.parquet -> LocationIdentifier "300030").
        /// Falls back to DB lookup to get Device info.
        /// </summary>
        private List<Location> GetLocationsFromFolder(string folderPath, DateTime date)
        {
            var dateStr = date.ToString("yyyy-MM-dd");
            var identifiers = Directory
                .EnumerateFiles(folderPath, $"*_{dateStr}.parquet")
                .Select(f => Path.GetFileName(f).Replace($"_{dateStr}.parquet", ""))
                .ToHashSet();

            _logger.LogInformation("Discovered {Count} locations from filesystem for {Date}",
                identifiers.Count, dateStr);

            return _locationRepository.GetList()
                .Include(s => s.Devices)
                .Where(l => identifiers.Contains(l.LocationIdentifier))
                .FromSpecification(new ActiveLocationSpecification())
                .GroupBy(r => r.LocationIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();
        }

        private async Task<CompressedEventLogs<IndianaEvent>?> ReadParquetLogsAsync(
            string filePath,
            DateTime date,
            Location location,
            CancellationToken cancellationToken)
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Parquet file not found: {FilePath}", filePath);
                return null;
            }

            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    using var stream = File.OpenRead(filePath);
                    var records = await ParquetSerializer.DeserializeAsync<ParquetEventRecord>(
                        stream, cancellationToken: cancellationToken);

                    var eventLogs = records
                        .Where(r => r.SignalID == location.LocationIdentifier && r.Date != null)
                        .Select(r => new IndianaEvent
                        {
                            Timestamp = DateTime.ParseExact(r.Date, "M/d/yyyy", null)
                                .AddMilliseconds(r.TimestampMs),
                            EventCode = (short)r.EventCode,
                            EventParam = (short)r.EventParam
                        })
                        .ToList();

                    _logger.LogInformation("Found {Count} events for location {Location} on {Date}",
                        eventLogs.Count, location.LocationIdentifier, date);

                    if (!eventLogs.Any())
                        return null;

                    var device = location.Devices
                        .FirstOrDefault(d => d.DeviceType == DeviceTypes.SignalController);

                    if (device == null)
                    {
                        _logger.LogWarning("No SignalController device found for location {Location}",
                            location.LocationIdentifier);
                        return null;
                    }

                    return new CompressedEventLogs<IndianaEvent>
                    {
                        LocationIdentifier = location.LocationIdentifier,
                        DeviceId = device.Id,
                        Start = date,
                        End = date.AddDays(1),
                        DataType = typeof(IndianaEvent),
                        Data = eventLogs
                    };
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to read {FilePath} after multiple attempts: {Message}",
                    filePath, ex.Message);
                return null;
            }
        }

        private async Task InsertLogsWithRetryAsync(
            List<CompressedEventLogs<IndianaEvent>> archiveLogs,
            DateTime date)
        {
            var batchNum = 1;
            foreach (var logs in archiveLogs.Batch(500))
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetService<EventLogContext>();
                    if (context == null)
                    {
                        _logger.LogError("EventLogContext is not available.");
                        return;
                    }

                    // Filter out records that already exist by DeviceId + Start date
                    var incomingKeys = logs.Select(l => (l.DeviceId, l.Start)).ToHashSet();
                    var existingKeys = context.CompressedEvents
                        .Where(e => incomingKeys.Select(k => k.DeviceId).Contains(e.DeviceId)
                                    && incomingKeys.Select(k => k.Start).Contains(e.Start))
                        .Select(e => new { e.DeviceId, e.Start })
                        .AsEnumerable()
                        .Select(e => (e.DeviceId, e.Start))
                        .ToHashSet();

                    var newLogs = logs.Where(l => !existingKeys.Contains((l.DeviceId, l.Start))).ToList();

                    if (!newLogs.Any())
                    {
                        _logger.LogInformation("Batch {BatchNum} for {Date} already exists, skipping",
                            batchNum, date);
                        batchNum++;
                        return;
                    }

                    context.CompressedEvents.AddRange(newLogs);
                    await context.SaveChangesAsync();

                    _logger.LogInformation(
                        "Inserted batch {BatchNum} of size {Size} for {Date} (skipped {Skipped} duplicates)",
                        batchNum, newLogs.Count, date, logs.Count() - newLogs.Count);
                    batchNum++;
                });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    // TODO: Confirm these column names match the actual parquet schema
    // Updated ParquetEventRecord - matches actual parquet schema exactly
    public class ParquetEventRecord
    {
        public string SignalID { get; set; }
        public string Date { get; set; }
        public double TimestampMs { get; set; }   // not null -> non-nullable
        public int EventCode { get; set; }        // int32 not null -> int, non-nullable
        public int EventParam { get; set; }       // int32 not null -> int, non-nullable
    }
}