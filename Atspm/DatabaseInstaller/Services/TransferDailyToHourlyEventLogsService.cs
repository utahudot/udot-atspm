using DatabaseInstaller.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Polly;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IO.Compression;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Microsoft.Extensions.DependencyInjection;
using Utah.Udot.Atspm.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;
using Npgsql;

namespace DatabaseInstaller.Services
{
    public class TransferDailyToHourlyEventLogsService : IHostedService
    {
        private readonly ILogger<TransferEventLogsHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILocationRepository _locationRepository;
        private readonly TransferDailyToHourlyConfiguration _config;
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

        public TransferDailyToHourlyEventLogsService(
            ILogger<TransferEventLogsHostedService> logger,
            IServiceProvider serviceProvider,
            ILocationRepository locationRepository,
            IOptions<TransferDailyToHourlyConfiguration> config)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _locationRepository = locationRepository;
            _config = config.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var locationsQuery = _locationRepository.GetList()
                .Include(s => s.Devices)
                .AsQueryable();

            if (_config.Device != null)
            {
                locationsQuery = locationsQuery
                    .Where(l => l.Devices.Any(d => d.DeviceType == (DeviceTypes)_config.Device));
            }

            var locations = locationsQuery
                .FromSpecification(new ActiveLocationSpecification())
                .GroupBy(r => r.LocationIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();

            for (var date = _config.Start; date <= _config.End; date = date.AddDays(1)) // Process daily logs
            {
                _logger.LogInformation($"Processing daily log for {date.Date}");

                var allHourlyLogs = new ConcurrentBag<CompressedEventLogs<IndianaEvent>>();

                await Task.WhenAll(locations.Select(async location =>
                {
                    try
                    {
                        // Get the daily logs
                        var dailyLogs = await GetDecompressedEvents(date, location);

                        if (dailyLogs != null && dailyLogs.Any())
                        {
                            // Convert to hourly compressed logs
                            var hourlyLogs = ConvertToHourlyCompressedEvents(dailyLogs, location, date);

                            if (hourlyLogs.Count != 0)
                            {
                                await InsertLogsWithRetryAsync(hourlyLogs.ToList());
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"No logs found for location {location.LocationIdentifier} on {date.Date}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to process logs for location {location.LocationIdentifier} on {date.Date}: {ex.Message}");
                    }
                }));
            }

            _logger.LogInformation("Execution completed.");
        }


        private async Task<List<IndianaEvent>> GetDecompressedEvents(DateTime date, Location location)
        {
            string selectQuery = $"SELECT \"Data\"  FROM {_config.SourceTable} WHERE \"LocationIdentifier\" = '{location.LocationIdentifier}' AND \"ArchiveDate\" = '{date.Date}'";
            var jsonObject = new List<IndianaEvent>();

            await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    using (var connection = new NpgsqlConnection(_config.Source))
                    {
                        connection.Open();

                        using (NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection))
                        {
                            using (NpgsqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    byte[] compressedData = (byte[])reader["Data"];

                                    using (MemoryStream memoryStream = new MemoryStream(compressedData))
                                    using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                                    using (StreamReader streamReader = new StreamReader(gzipStream))
                                    {
                                        string json = await streamReader.ReadToEndAsync();
                                        jsonObject = JsonConvert.DeserializeObject<List<IndianaEvent>>(json);
                                        jsonObject.ForEach(x => x.LocationIdentifier = location.LocationIdentifier);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading daily data for Location: {LocationId} on Date: {Date}.", location.LocationIdentifier, date);
                    throw;
                }
            });

            return jsonObject;
        }

        private List<CompressedEventLogs<IndianaEvent>> ConvertToHourlyCompressedEvents(List<IndianaEvent> objectList, Location location, DateTime date)
        {
            var hourlyCompressedEvents = new List<CompressedEventLogs<IndianaEvent>>();

            // Group events by hour
            var groupedByHour = objectList.GroupBy(e => e.Timestamp.Hour);

            foreach (var hourGroup in groupedByHour)
            {
                int hour = hourGroup.Key;
                var indianaEvents = new List<IndianaEvent>();

                foreach (var item in hourGroup)
                {
                    if (item.EventParam < 32000)
                    {
                        indianaEvents.Add(new IndianaEvent
                        {
                            LocationIdentifier = item.LocationIdentifier,
                            Timestamp = item.Timestamp,
                            EventCode = (short)item.EventCode,
                            EventParam = (short)item.EventParam
                        });
                    }
                }

                if (indianaEvents.Any())
                {
                    // Calculate Start and End time for this hour
                    DateTime start = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
                    DateTime end = start.AddHours(1); // One hour range

                    // Compress data
                    byte[] compressedData;
                    using (MemoryStream memoryStream = new MemoryStream())
                    using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                    using (StreamWriter writer = new StreamWriter(gzipStream))
                    {
                        string json = JsonConvert.SerializeObject(indianaEvents);
                        writer.Write(json);
                        writer.Flush();
                        gzipStream.Flush();
                        compressedData = memoryStream.ToArray();
                    }

                    var deviceId = location.Devices.FirstOrDefault(x => x.DeviceType == DeviceTypes.SignalController)?.Id;
                    if (deviceId == null)
                    {
                        _logger.LogError("No device found for Location: {LocationId} on Date: {Date}.", location.LocationIdentifier, DateOnly.FromDateTime(date));
                    }
                    else
                    {
                        var compressedEventLog = new CompressedEventLogs<IndianaEvent>
                        {
                            LocationIdentifier = location.LocationIdentifier,
                            ArchiveDate = DateOnly.FromDateTime(date),
                            DeviceId = deviceId.Value,
                            Start = start,
                            End = end,
                            Data = indianaEvents // Assuming Data is for reference/debugging
                        };
                        hourlyCompressedEvents.Add(compressedEventLog);
                    }
                }
            }

            return hourlyCompressedEvents;
        }

        //private async Task InsertHourlyDataAsync(List<CompressedEventLogs<IndianaEvent>> hourlyEvents, string locationId, DateTime date, CancellationToken token)
        //{
        //    try
        //    {
        //        using (var scope = _serviceProvider.CreateScope())
        //        {
        //            var context = scope.ServiceProvider.GetService<EventLogContext>();

        //            if (context != null && hourlyEvents.Any())
        //            {
        //                foreach (var eventLog in hourlyEvents)
        //                {
        //                    context.CompressedEvents.Add(eventLog);
        //                }
        //                await context.SaveChangesAsync(token);
        //            }
        //        }

        //        _logger.LogInformation("Successfully inserted hourly data for Location: {LocationId} on Date: {Date}.", locationId, date);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error inserting hourly data for Location: {LocationId} on Date: {Date}.", locationId, date);
        //        throw;
        //    }
        ////}

        private async Task InsertLogsWithRetryAsync(List<CompressedEventLogs<IndianaEvent>> hourlyLogs)
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
                context.CompressedEvents.AddRange(hourlyLogs);
                //context.CompressedEvents.Add(archiveLog);
                await context.SaveChangesAsync();

                _logger.LogInformation($"Successfully inserted log on {hourlyLogs.FirstOrDefault()?.Start}");
            });
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
