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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Polly;
using Polly.Retry;
using Microsoft.EntityFrameworkCore;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;
using Polly.Contrib.WaitAndRetry;
using Utah.Udot.Atspm.Extensions;

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
                    medianFirstRetryDelay: TimeSpan.FromSeconds(10), // Initial 5-second delay
                    retryCount: 5 // First 5 retries at 5s
                )
            .Concat(new[]
                {
                    TimeSpan.FromMinutes(5),
                    TimeSpan.FromMinutes(30)
                })
            .Concat(Enumerable.Repeat(TimeSpan.FromDays(1), 24)), // Next 24 retries every 1 hour
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds} seconds due to {exception.Message}");
                }
            );

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
            //var locationIdentifiers = new List<string>
            //{
            //    "6039",
            //    "6087",
            //    "6078",
            //    "6014",
            //    "6099",
            //    "6079",
            //    "6200",
            //    "6201",
            //    "6401",
            //    "6511",
            //    "6392",
            //    "6391",
            //    "6035",
            //    "6036",
            //    "6032",
            //    "6037",
            //    "6038",
            //    "6206",
            //    "6635",
            //    "6634",
            //    "6633",
            //    "6417",
            //    "6461",
            //    "6325",
            //    "6328",
            //    "6324",
            //    "6322",
            //    "6330",
            //    "6321",
            //    "6316",
            //    "6317",
            //    "6402",
            //    "6404",
            //    "6315",
            //    "6319",
            //    "6320",
            //    "6628",
            //    "6627",
            //    "6619",
            //    "6625",
            //    "6626",
            //    "6624",
            //    "6642",
            //    "6643",
            //    "6462",
            //    "6410",
            //    "6463",
            //    "6409",
            //    "6408",
            //    "6407",
            //    "6406",
            //    "6427",
            //    "6405",
            //    "6465",
            //    "6641",
            //    "6411",
            //    "6147",
            //    "6137",
            //    "6141",
            //    "6133",
            //    "6139",
            //    "6142",
            //    "6131",
            //    "6393",
            //    "6394",
            //    "6303",
            //    "6308",
            //    "6311",
            //    "6313",
            //    "6314",
            //    "6525",
            //    "6526",
            //    "6527",
            //    "6528",
            //    "6530",
            //    "6326",
            //    "6327",
            //    "6449",
            //    "6448",
            //    "6447",
            //    "6446",
            //    "6445",
            //    "6444",
            //    "6443",
            //    "6442",
            //    "6023",
            //    "6074",
            //    "6024",
            //    "6025",
            //    "6016",
            //    "6028",
            //    "6134",
            //    "6132",
            //    "6026",
            //    "6022",
            //    "6021",
            //    "6020",
            //    "6066",
            //    "6067",
            //    "6065",
            //    "6061",
            //    "6017",
            //    "6652",
            //    "6654",
            //    "6090",
            //    "6012",
            //    "6329",
            //    "6312",
            //    "6519",
            //    "6532",
            //    "6416",
            //    "6415",
            //    "6413",
            //    "6412",
            //    "6398",
            //    "6399",
            //    "6387",
            //    "6388",
            //    "6502",
            //    "6503",
            //    "6504",
            //    "6506",
            //    "6306",
            //    "6518",
            //    "6521",
            //    "6082",
            //    "6081",
            //    "6080",
            //    "6198",
            //    "6323",
            //    "6516",
            //    "6515",
            //    "6514",
            //    "6513",
            //    "6512",
            //    "6310",
            //    "6309",
            //    "6423",
            //    "6520",
            //    "6529",
            //    "6304",
            //    "6305",
            //    "6302",
            //    "6510",
            //    "6509",
            //    "6524",
            //    "6522"
            //};
            for (var date = _config.Start; date <= _config.End; date = date.AddDays(1))
            {
                _logger.LogInformation($"Processing data for {date}");

                var locationsQuery = _locationRepository.GetList()
                    .Include(s => s.Devices) // Always include Devices
                    .AsQueryable(); // Ensure it remains IQueryable<Location>
                locationsQuery = locationsQuery
                        .Where(l => l.Devices.Any(d => d.DeviceType == DeviceTypes.SpeedSensor)); // Use Any() for efficient filtering
                if (_config.Device != null)
                {
                    locationsQuery = locationsQuery
                        .Where(l => l.Devices.Any(d => d.DeviceType == (DeviceTypes)_config.Device)); // Use Any() for efficient filtering
                }

                var locations = locationsQuery
                    .FromSpecification(new ActiveLocationSpecification()) // Apply specification
                    .GroupBy(r => r.LocationIdentifier)
                    .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault()) // Get the latest location per identifier
                    .ToList();

                int batchCount = _config.Batch ?? 100;

                foreach (var batch in locations.Batch(batchCount))
                {

                    try
                    {
                        // Process each batch in parallel, but process only one batch at a time.
                        await Task.WhenAll(batch.Select(location =>
                            ProcessLocationAsync(DateOnly.FromDateTime(date), location, cancellationToken)));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing batch for {date}: {ex.Message}");
                    }
                }
            }

            _logger.LogInformation("Execution completed.");
        }


            /// <summary>
            /// Processes a single location for a specific date. Retrieves the logs and, if any are found,
            /// immediately attempts to insert them into the database using retry logic.
            /// </summary>
            private async Task ProcessLocationAsync(DateOnly date, Location location, CancellationToken cancellationToken)
        {
            var logEntry = new CompressedEventLogs<IndianaEvent>();
            logEntry = await GetLogsAsync(date, _config.Source, location, cancellationToken);
            
            if (logEntry != null)
            {
                await InsertLogsWithRetryAsync(logEntry);
            }
            else
            {
                _logger.LogWarning($"No logs found for location {location.LocationIdentifier} on {date}");
            }
        }

    private async Task<CompressedEventLogs<IndianaEvent>?> GetLogsAsync(
        DateOnly dateToRetrieve,
        string sourceConnectionString,
        Location location,
        CancellationToken cancellationToken)
    {
        string connectionString = $"{sourceConnectionString};Max Pool Size=200;Connection Timeout=60;";

        string selectQuery = $"SELECT SignalId, Timestamp, EventCode, EventParam FROM [dbo].[Controller_Event_Log] " +
                             $"WHERE SignalId = '{location.LocationIdentifier}' " +
                             $"AND Timestamp >= '{dateToRetrieve}' AND Timestamp < '{dateToRetrieve.AddDays(1)}'";

        // Define retry policy with incremental backoff (30s, 60s, 90s, 120s)
        

        try
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync(cancellationToken);  // If this fails, Polly will retry

                    using (var selectCommand = new SqlCommand(selectQuery, connection))
                    {
                        var eventLogs = new List<IndianaEvent>();
                        selectCommand.CommandTimeout = 120;  // Ensure command timeout is set

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
                                    _logger.LogWarning($"Error reading record for {location.LocationIdentifier} on {dateToRetrieve}: {ex.Message}");
                                }
                            }
                        }

                        _logger.LogInformation($"Found {eventLogs.Count} events for location {location.LocationIdentifier} on {dateToRetrieve}");

                        if (eventLogs.Any())
                        {
                            var device = location.Devices
                                .FirstOrDefault(d => d.DeviceType == Utah.Udot.Atspm.Data.Enums.DeviceTypes.SignalController);

                            if (device != null)
                            {
                                return new CompressedEventLogs<IndianaEvent>
                                {
                                    LocationIdentifier = location.LocationIdentifier,
                                    DeviceId = device.Id,
                                    ArchiveDate = dateToRetrieve,
                                    Data = eventLogs
                                };
                            }
                            else
                            {
                                _logger.LogWarning($"No device found for location {location.LocationIdentifier}");
                            }
                        }
                    }
                }

                return null;
            });
        }
        catch (Exception ex)
        {
            // If all retries fail, log an error but do NOT throw the exception
            _logger.LogError($"Failed to get logs for {location.LocationIdentifier} on {dateToRetrieve} after multiple attempts: {ex.Message}");
            return null; // Continue processing other locations
        }
    }





    /// <summary>
    /// Attempts to insert the log entry into the database with retry logic.
    /// Retries up to 4 times with a 30-second delay between attempts.
    /// </summary>
    private async Task InsertLogsWithRetryAsync(CompressedEventLogs<IndianaEvent> archiveLog)
    {

            await _retryPolicy.ExecuteAsync(async () =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetService<EventLogContext>();
                    if (context == null)
                    {
                        _logger.LogError("EventLogContext is not available.");
                        return;
                    }
                    context.CompressedEvents.Add(archiveLog);
                    await context.SaveChangesAsync();
                }

                _logger.LogInformation($"Successfully inserted log for location {archiveLog.LocationIdentifier} on {archiveLog.ArchiveDate}");
                return;
            });
            
    }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
