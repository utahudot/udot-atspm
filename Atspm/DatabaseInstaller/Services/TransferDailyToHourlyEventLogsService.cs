#region license
// Copyright 2025 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Services/TransferDailyToHourlyEventLogsService.cs
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
using Newtonsoft.Json;
using Npgsql;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using System.Collections.Concurrent;
using System.IO.Compression;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

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
        private static readonly Dictionary<string, Type> _eventTypeMap = new Dictionary<string, Type>
        {
            { "Indiana", typeof(IndianaEvent) },
            { "Speed", typeof(SpeedEvent) },
        };

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

            for (var date = _config.Start; date <= _config.End; date = date.AddDays(1))
            {
                _logger.LogInformation($"Processing daily log for {date.Date}");

                if (_config.DataType == "IndianaEvent")
                {
                    var allHourlyLogs = new ConcurrentBag<CompressedEventLogs<IndianaEvent>>();

                    await Task.WhenAll(locations.Select(async location =>
                    {
                        var dailyLogs = await GetDecompressedEvents<IndianaEvent>(date, location);
                        if (dailyLogs?.Any() == true)
                        {
                            var hourlyLogs = ConvertToHourlyCompressedEvents(dailyLogs, location, date);
                            if (hourlyLogs.Count > 0)
                                await InsertLogsWithRetryAsync(hourlyLogs.ToList());
                        }
                    }));
                }
                else if (_config.DataType == "SpeedEvent")
                {
                    var allHourlyLogs = new ConcurrentBag<CompressedEventLogs<SpeedEvent>>();

                    await Task.WhenAll(locations.Select(async location =>
                    {
                        var dailyLogs = await GetDecompressedEvents<SpeedEvent>(date, location);
                        if (dailyLogs?.Any() == true)
                        {
                            var hourlyLogs = ConvertToHourlyCompressedEvents(dailyLogs, location, date);
                            if (hourlyLogs.Count > 0)
                                await InsertLogsWithRetryAsync(hourlyLogs.ToList());
                        }
                    }));
                }
                else
                {
                    _logger.LogWarning("Unsupported DataType configured: {DataType}", _config.DataType);
                }
            }

            _logger.LogInformation("Execution completed.");
        }


        //private async Task<List<IndianaEvent>> GetDecompressedEvents(DateTime date, Location location)
        //{
        //    string selectQuery = $"SELECT \"Data\"  FROM {_config.SourceTable} WHERE \"LocationIdentifier\" = '{location.LocationIdentifier}' AND \"ArchiveDate\" = '{date.Date}' AND \"DataType\" = '{_config.DataType}'";
        //    var jsonObject = new List<EventLogModelBase>();

        //    await _retryPolicy.ExecuteAsync(async () =>
        //    {
        //        try
        //        {
        //            using (var connection = new NpgsqlConnection(_config.Source))
        //            {
        //                connection.Open();

        //                using (NpgsqlCommand command = new NpgsqlCommand(selectQuery, connection))
        //                {
        //                    using (NpgsqlDataReader reader = command.ExecuteReader())
        //                    {
        //                        if (reader.Read())
        //                        {
        //                            byte[] compressedData = (byte[])reader["Data"];

        //                            using (MemoryStream memoryStream = new MemoryStream(compressedData))
        //                            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
        //                            using (StreamReader streamReader = new StreamReader(gzipStream))
        //                            {
        //                                string json = await streamReader.ReadToEndAsync();
        //                                jsonObject = JsonConvert.DeserializeObject<List<IndianaEvent>>(json);
        //                                jsonObject.ForEach(x => x.LocationIdentifier = location.LocationIdentifier);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error reading daily data for Location: {LocationId} on Date: {Date}.", location.LocationIdentifier, date);
        //            throw;
        //        }
        //    });

        //    return jsonObject;
        //}

        private async Task<List<T>> GetDecompressedEvents<T>(DateTime date, Location location) where T : EventLogModelBase
        {
            string selectQuery = $@"
                SELECT ""Data"", ""DataType""
                FROM {_config.SourceTable}
                WHERE ""LocationIdentifier"" = '{location.LocationIdentifier}'
                AND ""ArchiveDate"" = '{date.Date}'
                AND ""DataType"" = '{_config.DataType}'";

            var result = new List<T>();

            await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    using (var connection = new NpgsqlConnection(_config.Source))
                    {
                        connection.Open();

                        using (var command = new NpgsqlCommand(selectQuery, connection))
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                byte[] compressedData = (byte[])reader["Data"];

                                using (var memoryStream = new MemoryStream(compressedData))
                                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                                using (var streamReader = new StreamReader(gzipStream))
                                {
                                    string json = await streamReader.ReadToEndAsync();
                                    var deserialized = JsonConvert.DeserializeObject<List<T>>(json);

                                    foreach (var item in deserialized)
                                    {
                                        item.LocationIdentifier = location.LocationIdentifier;
                                        result.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading data for Location: {LocationId} on Date: {Date}", location.LocationIdentifier, date);
                    throw;
                }
            });

            return result;
        }

        private List<CompressedEventLogs<T>> ConvertToHourlyCompressedEvents<T>(List<T> objectList, Location location, DateTime date)
            where T : EventLogModelBase, new()
        {
            var hourlyCompressedEvents = new List<CompressedEventLogs<T>>();
            var deviceId = location.Devices.FirstOrDefault(x => x.DeviceType == DeviceTypes.SignalController)?.Id;

            if (deviceId == null)
            {
                _logger.LogError("No device found for Location: {LocationId} on Date: {Date}.",
                    location.LocationIdentifier, DateOnly.FromDateTime(date));
                return hourlyCompressedEvents;
            }

            foreach (var hourGroup in objectList.GroupBy(e => e.Timestamp.Hour))
            {
                var hour = hourGroup.Key;
                var filteredEvents = hourGroup.ToList();
                if (!filteredEvents.Any()) continue;

                var start = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
                var end = start.AddHours(1);

                //byte[] compressedData = CompressEvents(filteredEvents);

                hourlyCompressedEvents.Add(new CompressedEventLogs<T>
                {
                    LocationIdentifier = location.LocationIdentifier,
                    //ArchiveDate = DateOnly.FromDateTime(date),
                    DeviceId = deviceId.Value,
                    Start = start,
                    End = end,
                    Data = filteredEvents // Still included for debug/reference
                });
            }

            return hourlyCompressedEvents;
        }

        private byte[] CompressEvents<T>(List<T> events)
        {
            using var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
            using (var writer = new StreamWriter(gzipStream))
            {
                string json = JsonConvert.SerializeObject(events);
                writer.Write(json);
            }
            return memoryStream.ToArray();
        }

        private async Task InsertLogsWithRetryAsync<T>(List<CompressedEventLogs<T>> hourlyLogs) where T : EventLogModelBase
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
