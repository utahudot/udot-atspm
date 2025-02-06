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
    public class TransferEventLogsHostedService : IHostedService
    {
        private readonly ILogger<TransferEventLogsHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILocationRepository _locationRepository;
        private readonly TransferCommandConfiguration _config;

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
            var locations = _locationRepository.GetLatestVersionOfAllLocations(_config.Start);

            for (var date = _config.Start; date <= _config.End; date = date.AddDays(1))
            {
                try
                {
                    var locationBatches = locations.Select((location, index) => new { location, index })
                                                   .GroupBy(x => x.index / 50)
                                                   .Select(g => g.Select(x => x.location).ToList());

                    foreach (var batch in locationBatches)
                    {
                        var archiveLogs = new ConcurrentBag<CompressedEventLogs<IndianaEvent>>();
                        var tasks = batch.Select(location => ProcessLocationAsync(DateOnly.FromDateTime(date), location, archiveLogs, cancellationToken)).ToList();
                        await Task.WhenAll(tasks);

                        if (archiveLogs.Any())
                        {
                            await FlushLogsAsync(archiveLogs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing data for date {date}: {ex.Message}");
                }
            }

            _logger.LogInformation("Execution completed.");
        }

        private async Task InsertLogsAsync(List<CompressedEventLogs<IndianaEvent>> archiveLogs)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<EventLogContext>();
                if (context == null) return;

                context.CompressedEvents.AddRange(archiveLogs);
                await context.SaveChangesAsync();
            }
        }

        private async Task ProcessLocationAsync(
            DateOnly date,
            Location location,
            ConcurrentBag<CompressedEventLogs<IndianaEvent>> archiveLogs,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Processing location {location.LocationIdentifier} for date {date}");
            await GetLogsAsync(date, _config.Source, archiveLogs, location, cancellationToken);
            _logger.LogInformation($"Finished processing location {location.LocationIdentifier} for date {date}");
        }

        private async Task FlushLogsAsync(ConcurrentBag<CompressedEventLogs<IndianaEvent>> archiveLogs)
        {
            var logsToInsert = archiveLogs.ToList();
            archiveLogs.Clear();

            _logger.LogInformation($"Flushing {logsToInsert.Count} logs to database");
            await InsertLogsAsync(logsToInsert);
        }

        private async Task GetLogsAsync(
            DateOnly dateToRetrieve,
            string sourceConnectionString,
            ConcurrentBag<CompressedEventLogs<IndianaEvent>> archiveLogs,
            Location location,
            CancellationToken cancellationToken)
        {
            string selectQuery = $"SELECT SignalId, Timestamp, EventCode, EventParam FROM [dbo].[Controller_Event_Log] " +
                                 $"WHERE SignalId = '{location.LocationIdentifier}' " +
                                 $"AND Timestamp >= '{dateToRetrieve}' AND Timestamp < '{dateToRetrieve.AddDays(1)}'";

            using (var connection = new SqlConnection(sourceConnectionString))
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
                                _logger.LogInformation($"Error reading record for {location.LocationIdentifier}: {ex.Message}");
                            }
                        }
                    }

                    _logger.LogInformation($"Found {eventLogs.Count} events for location {location.LocationIdentifier} on {dateToRetrieve}");

                    if (eventLogs.Any())
                    {
                        var device = location.Devices.FirstOrDefault(d => d.DeviceType == Utah.Udot.Atspm.Data.Enums.DeviceTypes.SignalController);
                        if (device != null)
                        {
                            var logEntry = new CompressedEventLogs<IndianaEvent>
                            {
                                LocationIdentifier = location.LocationIdentifier,
                                DeviceId = device.Id,
                                ArchiveDate = dateToRetrieve,
                                Data = eventLogs
                            };

                            archiveLogs.Add(logEntry);
                            _logger.LogInformation($"Added log entry for location {location.LocationIdentifier} with {eventLogs.Count} events");
                        }
                        else
                        {
                            _logger.LogWarning($"No device found for location {location.LocationIdentifier}");
                        }
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
