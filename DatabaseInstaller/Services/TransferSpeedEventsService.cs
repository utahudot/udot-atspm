﻿
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
            var locations = _locationRepository.GetLatestVersionOfAllLocations(_config.Start);

            for (var date = _config.Start; date <= _config.End; date = date.AddDays(1))
            {
                try
                {
                    var archiveLogs = new ConcurrentBag<CompressedEventLogs<SpeedEvent>>();

                    // Process locations in batches of 10
                    var locationBatches = locations.Select((location, index) => new { location, index })
                                                   .GroupBy(x => x.index / 10)
                                                   .Select(g => g.Select(x => x.location).ToList());

                    foreach (var batch in locationBatches)
                    {
                        var tasks = batch.Select(location => ProcessLocationAsync(DateOnly.FromDateTime(date), location, archiveLogs, cancellationToken));
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
                    Console.WriteLine($"Executing query: {selectQuery}");

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
                        var device = location.Devices.FirstOrDefault(d => d.DeviceType == Utah.Udot.Atspm.Data.Enums.DeviceTypes.WavetronixSpeed);
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
                        }
                        else
                        {
                            _logger.LogWarning($"No speed device found for signal {location.LocationIdentifier}");
                        }
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;



    }
}