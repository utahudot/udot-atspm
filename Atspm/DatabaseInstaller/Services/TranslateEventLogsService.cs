#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Services/TranslateEventLogsService.cs
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
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using System.Data;
using System.IO.Compression;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Extensions;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace DatabaseInstaller.Services
{
    public class TranslateEventLogsService : IHostedService
    {
        private readonly ILogger<TranslateEventLogsService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILocationRepository _locationRepository;
        private readonly IIndianaEventLogRepository _indianaEventLogEFRepository;
        private readonly TransferCommandConfiguration _config;
        private readonly AsyncRetryPolicy _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                sleepDurations: Backoff.DecorrelatedJitterBackoffV2(
                    medianFirstRetryDelay: TimeSpan.FromSeconds(10), // Initial 5-second delay
                    retryCount: 5 // First 5 retries at 5s
                )
            .Concat(new[]
                {
                    TimeSpan.FromMinutes(5),
                    TimeSpan.FromMinutes(30)
                })
            .Concat(Enumerable.Repeat(TimeSpan.FromHours(1), 24)), // Next 24 retries every 1 hour
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds} seconds due to {exception.Message}");
                }
            );

        public TranslateEventLogsService(
            ILogger<TranslateEventLogsService> logger,
            IServiceProvider serviceProvider,
            ILocationRepository locationRepository,
            IOptions<TransferCommandConfiguration> config,
            IIndianaEventLogRepository indianaEventLogEFRepository
            )
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _locationRepository = locationRepository;
            _indianaEventLogEFRepository = indianaEventLogEFRepository;
            _config = config.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            for (var currentDay = _config.Start.Date; currentDay <= _config.End.Date; currentDay = currentDay.AddDays(1))
            {
                _logger.LogInformation($"Processing data for {currentDay:yyyy-MM-dd}");

                var locationsQuery = _locationRepository.GetList()
                    .Include(s => s.Devices)
                    .AsQueryable();

                if (_config.Device != null)
                {
                    locationsQuery = locationsQuery
                        .Where(l => l.Devices.Any(d => d.DeviceType == (DeviceTypes)_config.Device));
                }

                if (!string.IsNullOrEmpty(_config.Locations))
                {
                    var locationIdentifiers = _config.Locations.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    locationsQuery = locationsQuery
                        .Where(l => locationIdentifiers.Contains(l.LocationIdentifier));
                }

                var locations = locationsQuery
                    .FromSpecification(new ActiveLocationSpecification())
                    .GroupBy(r => r.LocationIdentifier)
                    .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                    .ToList();

                int batchCount = _config.Batch ?? 100;

                foreach (var batch in locations.Batch(batchCount))
                {
                    await ProcessBatchAsync(batch, currentDay, cancellationToken);
                }
            }
        }



        private async Task ProcessBatchAsync(IEnumerable<Location> locations, DateTime currentDay, CancellationToken cancellationToken)
        {
            await Parallel.ForEachAsync(locations, async (location, token) =>
            {
                List<ControllerEventLog> dailyEvents = null;

                await _retryPolicy.ExecuteAsync(async () =>
                {
                    dailyEvents = await GetDailyEvents(location, currentDay);
                });

                if (dailyEvents != null && dailyEvents.Any())
                {
                    var hourlyGroups = dailyEvents
                        .GroupBy(evt => evt.Timestamp.Hour)
                        .OrderBy(g => g.Key);

                    foreach (var hourlyGroup in hourlyGroups)
                    {
                        var hourStart = currentDay.AddHours(hourlyGroup.Key);
                        var hourEnd = hourStart.AddHours(1);

                        var hourlyCompressedEvents = ConvertToCompressedEvents(
                            hourlyGroup.ToList(), location, DateOnly.FromDateTime(currentDay), hourStart, hourEnd);

                        await _retryPolicy.ExecuteAsync(async () =>
                        {
                            await InsertWithRetryAsync(hourlyCompressedEvents, location.LocationIdentifier, hourStart, cancellationToken);
                        });
                    }
                }
            });
        }



        private async Task<List<ControllerEventLog>> GetDailyEvents(Location location, DateTime date)
        {
            string selectQuery = @$"
        SELECT LogData FROM [dbo].[ControllerLogArchives]
        WHERE SignalId = {location.LocationIdentifier}
        AND ArchiveDate = '{date.Date:yyyy-MM-dd}'";

            var jsonObject = new List<ControllerEventLog>();

            await _retryPolicy.ExecuteAsync(async () =>
            {
                using (var connection = new SqlConnection(_config.Source))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(selectQuery, connection))
                    {
                        _logger.LogInformation($"Reading data from table for {location.LocationIdentifier} on {date:yyyy-MM-dd}");
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                byte[] compressedData = (byte[])reader["LogData"];
                                byte[] decompressedData;

                                using (var memoryStream = new MemoryStream(compressedData))
                                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                                using (var decompressedStream = new MemoryStream())
                                {
                                    await gzipStream.CopyToAsync(decompressedStream);
                                    decompressedData = decompressedStream.ToArray();
                                }

                                string json = System.Text.Encoding.UTF8.GetString(decompressedData);
                                jsonObject = JsonConvert.DeserializeObject<List<ControllerEventLog>>(json);
                                jsonObject.ForEach(x => x.SignalIdentifier = location.LocationIdentifier);
                            }
                            else
                            {
                                _logger.LogWarning("No data found for Location: {LocationId} on Date: {Date}.", location.LocationIdentifier, date);
                            }
                        }
                    }
                }
            });

            return jsonObject;
        }



        private async Task InsertWithRetryAsync(CompressedEventLogs<IndianaEvent> indianaEvents, string locationId, DateTime date, CancellationToken token)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetService<EventLogContext>();

                    if (context != null && indianaEvents != null)
                    {
                        context.CompressedEvents.Add(indianaEvents);
                        await context.SaveChangesAsync(token);
                    }
                }

                _logger.LogInformation("Successfully inserted data for Location: {LocationId} on Date: {Date}.", locationId, date);
                return; // Exit the retry loop on success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting data for Location: {LocationId} on Date: {Date}.", locationId, date);
                throw;
            }
        }

        private CompressedEventLogs<IndianaEvent> ConvertToCompressedEvents(
    List<ControllerEventLog> events, Location location, DateOnly archiveDate, DateTime hourStart, DateTime hourEnd)
        {
            var indianaEvents = events
                .Where(item => item.EventParam < 32000)
                .Select(item => new IndianaEvent
                {
                    LocationIdentifier = item.SignalIdentifier,
                    Timestamp = item.Timestamp,
                    EventCode = (short)item.EventCode,
                    EventParam = (short)item.EventParam
                })
                .ToList();

            var deviceId = location.Devices
                .FirstOrDefault(x => x.DeviceType == DeviceTypes.SignalController)?.Id;

            if (deviceId == null)
            {
                _logger.LogError("No device found for Location: {LocationId} on {HourStart} to {HourEnd}.",
                    location.LocationIdentifier, hourStart, hourEnd);
                return null;
            }

            return new CompressedEventLogs<IndianaEvent>
            {
                LocationIdentifier = location.LocationIdentifier,
                //ArchiveDate = archiveDate,
                Start = hourStart,
                End = hourEnd,
                Data = indianaEvents,
                DeviceId = deviceId.Value
            };
        }


        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
