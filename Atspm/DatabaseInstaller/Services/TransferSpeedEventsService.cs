#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Services/TransferSpeedEventsService.cs
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace DatabaseInstaller.Services
{
    public class TransferSpeedEventsHostedService : IHostedService
    {
        private readonly ILogger<TransferSpeedEventsHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILocationRepository _locationRepository;
        private readonly TransferCommandConfiguration _config;

        public TransferSpeedEventsHostedService(
            ILogger<TransferSpeedEventsHostedService> logger,
            IServiceProvider serviceProvider,
            ILocationRepository locationRepository,
            IOptions<TransferCommandConfiguration> config)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _locationRepository = locationRepository;
            _config = config.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            for (var date = _config.Start; date <= _config.End; date = date.AddDays(1))
            {
                for (int hour = 0; hour < 24; hour++)
                {
                    var periodStart = new DateTime(DateOnly.FromDateTime(date), new TimeOnly(hour, 0));
                    var periodEnd = periodStart.AddHours(1);

                    _logger.LogInformation(
                        $"Processing speed events from {periodStart:yyyy-MM-dd HH:mm} to {periodEnd:HH:mm}");

                    var locations = _locationRepository
                        .GetLatestVersionOfAllLocations(periodStart)
                        .Where(loc => loc.Devices.Any(d => d.DeviceType == Utah.Udot.Atspm.Data.Enums.DeviceTypes.SpeedSensor))
                        .ToList();

                    var archiveLogs = new ConcurrentBag<CompressedEventLogs<SpeedEvent>>();

                    var locationBatches = locations
                        .Select((location, index) => new { location, index })
                        .GroupBy(x => x.index / 10)
                        .Select(g => g.Select(x => x.location).ToList());

                    foreach (var batch in locationBatches)
                    {
                        var tasks = new List<Task>();

                        foreach (var location in batch)
                        {
                            tasks.Add(Task.Run(async () =>
                            {
                                int retryCount = 3;
                                for (int attempt = 1; attempt <= retryCount; attempt++)
                                {
                                    try
                                    {
                                        await ProcessLocationAsync(
                                            periodStart,
                                            periodEnd,
                                            location,
                                            archiveLogs,
                                            cancellationToken);
                                        break;
                                    }
                                    catch (Exception ex) when (attempt < retryCount)
                                    {
                                        _logger.LogWarning(
                                            $"Attempt {attempt} failed for {location.LocationIdentifier}: {ex.Message}. Retrying...");
                                        await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(
                                            $"Failed after {retryCount} attempts for {location.LocationIdentifier}: {ex.Message}");
                                        break;
                                    }
                                }
                            }, cancellationToken));
                        }

                        await Task.WhenAll(tasks);

                        if (archiveLogs.Count > 50)
                        {
                            await FlushLogsAsync(archiveLogs);
                        }
                    }

                    if (archiveLogs.Any())
                    {
                        await FlushLogsAsync(archiveLogs);
                    }
                }
            }

            _logger.LogInformation("Hourly transfer completed.");
        }

        private async Task ProcessLocationAsync(
            DateTime startUtc,
            DateTime endUtc,
            Location location,
            ConcurrentBag<CompressedEventLogs<SpeedEvent>> archiveLogs,
            CancellationToken cancellationToken)
        {
            await GetLogsAsync(
                startUtc,
                endUtc,
                _config.Source,
                archiveLogs,
                location,
                cancellationToken);
        }

        private async Task FlushLogsAsync(ConcurrentBag<CompressedEventLogs<SpeedEvent>> archiveLogs)
        {
            var toInsert = new List<CompressedEventLogs<SpeedEvent>>();
            while (archiveLogs.TryTake(out var item))
            {
                toInsert.Add(item);
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetService<EventLogContext>();
                if (context != null)
                {
                    context.CompressedEvents.AddRange(toInsert);
                    await context.SaveChangesAsync();
                    _logger.LogInformation($"Flushed {toInsert.Count} compressed event logs.");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Bulk flush failed: {ex.Message}. Falling back to individual inserts.");
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<EventLogContext>();
                if (context != null)
                {
                    foreach (var log in toInsert)
                    {
                        try
                        {
                            context.CompressedEvents.Add(log);
                            await context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Failed to insert single log for {log.LocationIdentifier} between {log.Start:HH:mm} and {log.End:HH:mm}: {ex.Message}");
                        }
                    }
                }
            }
        }

        private async Task GetLogsAsync(
            DateTime startUtc,
            DateTime endUtc,
            string sourceConnectionString,
            ConcurrentBag<CompressedEventLogs<SpeedEvent>> archiveLogs,
            Location location,
            CancellationToken cancellationToken)
        {
            var query = @"
                SELECT DISTINCT DetectorID, MPH, KPH, Timestamp
                  FROM MOE.dbo.Speed_Events
                 WHERE Timestamp >= @startUtc
                   AND Timestamp <  @endUtc
                   AND DetectorID LIKE @detectorPrefix";

            var eventLogs = new List<SpeedEvent>();

            using (var conn = new SqlConnection(sourceConnectionString))
            {
                await conn.OpenAsync(cancellationToken);
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@startUtc", startUtc);
                cmd.Parameters.AddWithValue("@endUtc", endUtc);
                cmd.Parameters.AddWithValue("@detectorPrefix", location.LocationIdentifier + "%");
                cmd.CommandTimeout = 120;

                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    eventLogs.Add(new SpeedEvent
                    {
                        DetectorId = reader.GetString(reader.GetOrdinal("DetectorID")),
                        Mph = reader.GetInt32(reader.GetOrdinal("MPH")),
                        Kph = reader.GetInt32(reader.GetOrdinal("KPH")),
                        Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp"))
                    });
                }
            }

            if (eventLogs.Count > 0)
            {
                var device = location.Devices
                    .FirstOrDefault(d => d.DeviceType == Utah.Udot.Atspm.Data.Enums.DeviceTypes.SpeedSensor);

                if (device != null)
                {
                    archiveLogs.Add(new CompressedEventLogs<SpeedEvent>
                    {
                        LocationIdentifier = location.LocationIdentifier,
                        DeviceId = device.Id,
                        Start = startUtc,
                        End = endUtc,
                        Data = eventLogs
                    });

                    _logger.LogInformation(
                        $"Retrieved {eventLogs.Count} events for {location.LocationIdentifier} between {startUtc:HH:mm} and {endUtc:HH:mm}");
                }
                else
                {
                    _logger.LogWarning($"No speed device found for {location.LocationIdentifier}");
                }
            }
            else
            {
                _logger.LogWarning(
                    $"No speed events for {location.LocationIdentifier} between {startUtc:HH:mm} and {endUtc:HH:mm}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
