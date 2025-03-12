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
using Google.Protobuf.WellKnownTypes;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;
using Org.BouncyCastle.Cms;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Polly;
using System;
using System.Data;
using System.IO.Compression;
using System.Text.Json.Nodes;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Common;
using Utah.Udot.NetStandardToolkit.Extensions;
using Utah.Udot.Atspm.Extensions;

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

                _logger.LogInformation("Processing date: {Date} with {LocationCount} locations.", date, locations.Count());

                foreach (var batch in locations.Batch(batchCount))
                {
                    await ProcessBatchAsync(batch, date, cancellationToken);
                }
            }
        }

        private async Task ProcessBatchAsync(IEnumerable<Location> locations, DateTime date, CancellationToken cancellationToken)
        {
            await Parallel.ForEachAsync(locations, async (location, token) =>
            {
                var indianaEvents = new CompressedEventLogs<IndianaEvent>();
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    indianaEvents = await GetCompressedEvents(date, location);
                });
                if (indianaEvents != null && indianaEvents.Data.Any())
                {
                    await _retryPolicy.ExecuteAsync(async () =>
                    {
                        await InsertWithRetryAsync(indianaEvents, location.LocationIdentifier, date, cancellationToken);
                    });
                }

            });
            //foreach (var location in locations)
            //{
            //    var indianaEvents = new CompressedEventLogs<IndianaEvent>();
            //    await _retryPolicy.ExecuteAsync(async () =>
            //    {
            //        indianaEvents = await GetCompressedEvents(date, location);
            //    });
            //    if (indianaEvents != null &&  indianaEvents.Data.Any())
            //    {
            //        await _retryPolicy.ExecuteAsync(async () =>
            //        {
            //            await InsertWithRetryAsync(indianaEvents, location.LocationIdentifier, date, cancellationToken);
            //        });
            //    }
            //}
        }

        private async Task<CompressedEventLogs<IndianaEvent>> GetCompressedEvents(DateTime date, Location location)
        {
            string selectQuery = $"SELECT LogData FROM [dbo].[ControllerLogArchives] where SignalId = {location.LocationIdentifier} and ArchiveDate = '{date.Date}'";
            var jsonObject = new List<ControllerEventLog>();
            await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(_config.Source))
                    {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand(selectQuery, connection))
                        {
                            _logger.LogInformation($"Reading data from table for {location.LocationIdentifier} on {date}");
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    byte[] compressedData = (byte[])reader["LogData"];

                                    byte[] decompressedData;

                                    using (MemoryStream memoryStream = new MemoryStream(compressedData))
                                    {
                                        using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                                        {
                                            using (MemoryStream decompressedStream = new MemoryStream())
                                            {
                                                gzipStream.CopyTo(decompressedStream);
                                                decompressedData = decompressedStream.ToArray();
                                            }
                                        }
                                    }

                                    _logger.LogInformation($"Data read from table for {location.LocationIdentifier} on {date}");
                                    string json = System.Text.Encoding.UTF8.GetString(decompressedData);
                                    if (string.IsNullOrEmpty(json))
                                    {
                                        _logger.LogError("No data found for Location: {LocationId} on Date: {Date}.", location.LocationIdentifier, date);
                                    }

                                    // Deserialize the JSON to an object
                                    jsonObject = JsonConvert.DeserializeObject<List<ControllerEventLog>>(json);
                                    jsonObject.ForEach(x => x.SignalIdentifier = location.LocationIdentifier);
                                    
                                }
                                else
                                {
                                    _logger.LogError("No data found for Location: {LocationId} on Date: {Date}.", location.LocationIdentifier, date);
                                }   
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading data for Location: {LocationId} on Date: {Date}.", location.LocationIdentifier, date);
                    throw;
                }
            });

            return ConvertToCompressedEvents(jsonObject, location, DateOnly.FromDateTime(date));
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

        private CompressedEventLogs<IndianaEvent> ConvertToCompressedEvents(List<ControllerEventLog> objectList, Location location, DateOnly archiveDate)
        {
            var compressedEvents = new CompressedEventLogs<IndianaEvent>() { Data = new List<IndianaEvent>()};
            var indianaEvents = new List<IndianaEvent>();
            foreach (var item in objectList.Distinct())
            {
                try
                {
                    if (item.EventParam < 32000)
                    {
                        indianaEvents.Add(new IndianaEvent
                        {
                            LocationIdentifier = item.SignalIdentifier,
                            Timestamp = item.Timestamp,
                            EventCode = (short)item.EventCode,
                            EventParam = (short)item.EventParam
                        });
                    }
                }
                catch
                {
                    _logger.LogWarning("Error converting data for Location: {LocationId} on Date: {Date}.",
                        location.LocationIdentifier, archiveDate);
                }
            }

            var deviceId = location.Devices.FirstOrDefault(x => x.DeviceType == DeviceTypes.SignalController)?.Id;
            if (deviceId == null)
            {
                _logger.LogError("No device found for Location: {LocationId} on Date: {Date}.", location.LocationIdentifier, archiveDate);       
            }
            else
            {
                compressedEvents = new CompressedEventLogs<IndianaEvent>
                {
                    LocationIdentifier = location.LocationIdentifier,
                    ArchiveDate = archiveDate,
                    Data = indianaEvents,
                    DeviceId = deviceId.Value
                };
            }
            return compressedEvents;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
