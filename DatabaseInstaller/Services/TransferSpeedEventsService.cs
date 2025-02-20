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
                var locations = _locationRepository
                .GetLatestVersionOfAllLocations(date)
                .Where(l => l.Approaches
                    .SelectMany(a => a.Detectors)
                    .SelectMany(d => d.DetectionTypes)
                    .Any(dt => (int)dt.Id == 3)
                    && l.VersionAction != Utah.Udot.Atspm.Data.Enums.LocationVersionActions.Delete)
                .ToList();

                try
                {
                    var archiveLogs = new ConcurrentBag<CompressedEventLogs<SpeedEvent>>();

                    // Process locations in batches of 10
                    var locationBatches = locations.Select((location, index) => new { location, index })
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
                                        await ProcessLocationAsync(DateOnly.FromDateTime(date), location, archiveLogs, cancellationToken);
                                        break; // Break out of the retry loop on success
                                    }
                                    catch (Exception ex) when (attempt < retryCount)
                                    {
                                        _logger.LogInformation($"Attempt {attempt} failed for location {location}. Retrying...");
                                        await Task.Delay(TimeSpan.FromSeconds(30)); // Wait before retrying
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogInformation($"Failed to process location {location} after {retryCount} attempts: {ex.Message}");
                                        break; // Exit the loop on a permanent failure
                                    }
                                }
                            }));
                        }

                        await Task.WhenAll(tasks);

                        if (archiveLogs.Count > 50)
                        {
                            await FlushLogsAsync(archiveLogs);
                        }
                    }

                    // Final flush after processing all batches
                    if (archiveLogs.Any())
                    {
                        await FlushLogsAsync(archiveLogs);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error inserting data: {ex.Message}");
                }
            }

            _logger.LogInformation("Execution completed.");
        }

        private async Task ProcessLocationAsync(
            DateOnly date,
            Location location,
            ConcurrentBag<CompressedEventLogs<SpeedEvent>> archiveLogs,
            CancellationToken cancellationToken)
        {
            await GetLogsAsync(date, _config.Source, archiveLogs, location, cancellationToken);
        }

        private async Task FlushLogsAsync(ConcurrentBag<CompressedEventLogs<SpeedEvent>> archiveLogs)
        {
            var logsToInsert = new List<CompressedEventLogs<SpeedEvent>>();
            while (archiveLogs.TryTake(out var log))
            {
                logsToInsert.Add(log);
            }

            await InsertLogsAsync(logsToInsert);
        }

        private async Task InsertLogsAsync(List<CompressedEventLogs<SpeedEvent>> archiveLogs)
        {
            _logger.LogInformation($"Inserting {archiveLogs.Count} archive logs");
            const int maxRetryCount = 3; // Number of retries
            int delay = 500; // Initial delay in milliseconds

            for (int retry = 0; retry <= maxRetryCount; retry++)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetService<EventLogContext>();

                        if (context != null)
                        {
                            foreach (var archiveLog in archiveLogs)
                            {
                                context.CompressedEvents.Add(archiveLog);
                            }

                            await context.SaveChangesAsync(); // Ensure this is awaited
                        }
                    }

                    // Exit the loop if successful
                    return;
                }
                catch (Exception ex) when (retry < maxRetryCount)
                {
                    _logger.LogWarning($"InsertLogsAsync failed (attempt {retry + 1} of {maxRetryCount + 1}). Retrying in {delay}ms. Error: {ex.Message}");

                    await Task.Delay(delay);
                    delay *= 2; // Double the delay for the next retry
                }
                catch (Exception ex)
                {
                    _logger.LogError($"InsertLogsAsync failed after {maxRetryCount + 1} attempts. Switching to individual log insertion. Error: {ex.Message}");
                }
            }

            // Fallback: Insert logs one by one
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<EventLogContext>();

                if (context != null)
                {
                    foreach (var archiveLog in archiveLogs)
                    {
                        try
                        {
                            context.CompressedEvents.Add(archiveLog);
                            await context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Failed to insert individual archive log (Location: {archiveLog.LocationIdentifier}, Date: {archiveLog.ArchiveDate}). Error: {ex.Message}");
                        }
                    }
                }
            }
        }



        private async Task GetLogsAsync(
            DateOnly dateToRetrieve,
            string sourceConnectionString,
            ConcurrentBag<CompressedEventLogs<SpeedEvent>> archiveLogs,
            Location location,
            CancellationToken cancellationToken)
        {
            var locationIdentifier = location.LocationIdentifier;
            string selectQuery = $"Select DISTINCT [DetectorID],[MPH],[KPH],[timestamp] FROM [MOE].[dbo].[Speed_Events] Where timestamp between '{dateToRetrieve}' and '{dateToRetrieve.AddDays(1)}' and DetectorID like '{location.LocationIdentifier}%'";

            using (var connection = new SqlConnection(sourceConnectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var selectCommand = new SqlCommand(selectQuery, connection))
                {
                    var eventLogs = new List<SpeedEvent>();
                    selectCommand.CommandTimeout = 120;

                    using (var reader = await selectCommand.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            try
                            {
                                var speedEvent = new SpeedEvent
                                {
                                    Timestamp = (DateTime)reader["Timestamp"], 
                                    DetectorId = (String)reader["DetectorID"],
                                    Mph = (int)reader["MPH"],
                                    Kph = (int)reader["KPH"]
                                };

                                eventLogs.Add(speedEvent);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogInformation($" Event: {location}-{reader["Timestamp"]} Detector Id:{reader["DetectorID"]} MPH:{reader["MPH"]} KPH:{reader["KPH"]} Error reading record: {ex.Message}");
                            }
                        }
                    }

                    if (eventLogs.Count > 0)
                    {
                        var device = location.Devices.FirstOrDefault(d => d.DeviceType == Utah.Udot.Atspm.Data.Enums.DeviceTypes.SpeedSensor);
                        if (device != null)
                        {
                            var archiveLog = new CompressedEventLogs<SpeedEvent>
                            {
                                LocationIdentifier = location.LocationIdentifier,
                                DeviceId = device.Id,
                                ArchiveDate = dateToRetrieve,
                                Data = eventLogs
                            };

                            archiveLogs.Add(archiveLog);
                            _logger.LogInformation($"{eventLogs.Count} Speed logs retrieved {location.LocationIdentifier} on {dateToRetrieve}");
                        }
                        else
                        {
                            _logger.LogWarning($"No speed device found for signal {location.LocationIdentifier}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"No speed records found for {location.LocationIdentifier} on {dateToRetrieve}");
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;



    }
}
