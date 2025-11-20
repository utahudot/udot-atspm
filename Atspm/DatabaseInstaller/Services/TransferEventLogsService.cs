#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Services/TransferEventLogsService.cs
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
    public class TransferEventLogsHostedService : IHostedService
    {
        private readonly ILogger<TransferEventLogsHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILocationRepository _locationRepository;
        private readonly TransferCommandConfiguration _config;
        private readonly AsyncRetryPolicy _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                sleepDurations: Backoff.DecorrelatedJitterBackoffV2(
                    TimeSpan.FromSeconds(10),
                    retryCount: 5
                )
                .Concat(new[]
                {
                    TimeSpan.FromMinutes(5),
                    TimeSpan.FromMinutes(30)
                })
                .Concat(Enumerable.Repeat(TimeSpan.FromDays(1), 24)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds} seconds due to {exception.Message}");
                });

        public TransferEventLogsHostedService(
            ILogger<TransferEventLogsHostedService> logger,
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
            for (var date = _config.Start; date < _config.End.AddDays(1); date = date.AddHours(1))
            {
                _logger.LogInformation($"Processing data for {date}");

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

                var hourLogs = new ConcurrentBag<CompressedEventLogs<IndianaEvent>>();

                await Task.WhenAll(locations.Select(async location =>
                {
                    try
                    {
                        var log = await GetLogsAsync(date, date.AddHours(1), _config.Source, location, cancellationToken);
                        if (log != null)
                        {
                            hourLogs.Add(log);
                        }
                        else
                        {
                            _logger.LogWarning($"No logs found for location {location.LocationIdentifier} on {date}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to retrieve logs for location {location.LocationIdentifier} on {date}: {ex.Message}");
                    }
                }));
                await InsertLogsWithRetryAsync(hourLogs.ToList());

            }

            _logger.LogInformation("Execution completed.");
        }

        private async Task<CompressedEventLogs<IndianaEvent>?> GetLogsAsync(
            DateTime start,
            DateTime end,
            string sourceConnectionString,
            Location location,
            CancellationToken cancellationToken)
        {
            string connectionString = $"{sourceConnectionString};Max Pool Size=200;Connection Timeout=60;";
            string selectQuery = $"SELECT SignalId, Timestamp, EventCode, EventParam FROM [dbo].[Controller_Event_Log] " +
                                 $"WHERE SignalId = '{location.LocationIdentifier}' " +
                                 $"AND Timestamp >= '{start}' AND Timestamp < '{end}'";

            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync(cancellationToken);

                        using (var selectCommand = new SqlCommand(selectQuery, connection))
                        {
                            var eventLogs = new List<IndianaEvent>();
                            selectCommand.CommandTimeout = 120;
                            using (var reader = await selectCommand.ExecuteReaderAsync(cancellationToken))
                            {
                                while (await reader.ReadAsync(cancellationToken))
                                {
                                    try
                                    {
                                        var eventLog = new IndianaEvent
                                        {
                                            Timestamp = (DateTime)reader["Timestamp"],
                                            EventCode = Convert.ToInt16(reader["EventCode"]),
                                            EventParam = Convert.ToInt16(reader["EventParam"])
                                        };
                                        eventLogs.Add(eventLog);

                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning($"Error reading record for {location.LocationIdentifier} on {start}: {ex.Message}");

                                    }
                                }
                            }

                            _logger.LogInformation($"Found {eventLogs.Count} events for location {location.LocationIdentifier} on {start}");

                            if (eventLogs.Any())
                            {
                                var device = location.Devices
                                    .FirstOrDefault(d => d.DeviceType == DeviceTypes.SignalController);

                                if (device != null)
                                {
                                    return new CompressedEventLogs<IndianaEvent>
                                    {
                                        LocationIdentifier = location.LocationIdentifier,
                                        DeviceId = device.Id,
                                        //ArchiveDate = DateOnly.FromDateTime(start),
                                        Start = start,
                                        End = end,
                                        Data = eventLogs
                                    };
                                }

                                _logger.LogWarning($"No device found for location {location.LocationIdentifier}");
                            }
                        }
                    }

                    return null;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get logs for {location.LocationIdentifier} on {start} after multiple attempts: {ex.Message}");
                return null;
            }
        }

        private async Task InsertLogsWithRetryAsync(List<CompressedEventLogs<IndianaEvent>> archiveLogs)
        {
            var batchNum = 1;
            var batchSize = _config.Batch.HasValue ? _config.Batch.Value : 500;
            foreach (var logs in archiveLogs.Batch(batchSize))
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
                    context.CompressedEvents.AddRange(logs);
                    //context.CompressedEvents.Add(archiveLog);
                    await context.SaveChangesAsync();

                    _logger.LogInformation($"Successfully inserted batch number {batchNum} of size {logs.Count()} for {archiveLogs.FirstOrDefault()?.Start}");
                    batchNum++;
                });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
