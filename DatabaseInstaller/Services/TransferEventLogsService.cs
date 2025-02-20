
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

            // Get only the locations that match the provided identifiers.
            var locations = _locationRepository
                .GetLatestVersionOfAllLocations(_config.Start);
                //.Where(l => locationIdentifiers.Contains(l.LocationIdentifier));

            // Process each date in the range.
            for (var date = _config.Start; date <= _config.End; date = date.AddDays(1))
            {
                try
                {
                    // Optionally, you can batch these tasks if needed.
                    var tasks = locations
                        .Select(location => ProcessLocationAsync(DateOnly.FromDateTime(date), location, cancellationToken))
                        .ToList();

                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing data for date {date}: {ex.Message}");
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
            _logger.LogInformation($"Processing location {location.LocationIdentifier} for date {date}");
            var logEntry = await GetLogsAsync(date, _config.Source, location, cancellationToken);

            if (logEntry != null)
            {
                await InsertLogsWithRetryAsync(logEntry);
            }

            _logger.LogInformation($"Finished processing location {location.LocationIdentifier} for date {date}");
        }

        /// <summary>
        /// Retrieves event logs for the specified location and date.
        /// Returns a compressed log entry if any events are found; otherwise, returns null.
        /// </summary>
        private async Task<CompressedEventLogs<IndianaEvent>?> GetLogsAsync(
            DateOnly dateToRetrieve,
            string sourceConnectionString,
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
                        var device = location.Devices
                            .FirstOrDefault(d => d.DeviceType == Utah.Udot.Atspm.Data.Enums.DeviceTypes.SignalController);

                        if (device != null)
                        {
                            var logEntry = new CompressedEventLogs<IndianaEvent>
                            {
                                LocationIdentifier = location.LocationIdentifier,
                                DeviceId = device.Id,
                                ArchiveDate = dateToRetrieve,
                                Data = eventLogs
                            };

                            _logger.LogInformation($"Prepared log entry for location {location.LocationIdentifier} with {eventLogs.Count} events");
                            return logEntry;
                        }
                        else
                        {
                            _logger.LogWarning($"No device found for location {location.LocationIdentifier}");
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Attempts to insert the log entry into the database with retry logic.
        /// Retries up to 4 times with a 30-second delay between attempts.
        /// </summary>
        private async Task InsertLogsWithRetryAsync(CompressedEventLogs<IndianaEvent> archiveLog)
        {
            const int maxRetries = 4;
            const int delaySeconds = 30;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
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
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Attempt {attempt} of {maxRetries} to insert log for location {archiveLog.LocationIdentifier} failed: {ex.Message}");

                    if (attempt == maxRetries)
                    {
                        _logger.LogError("Max retry attempts reached. Failing insertion.");
                        throw;
                    }
                    else
                    {
                        _logger.LogInformation($"Waiting {delaySeconds} seconds before retrying insertion for location {archiveLog.LocationIdentifier}...");
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
