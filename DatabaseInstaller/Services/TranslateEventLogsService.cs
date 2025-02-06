using DatabaseInstaller.Commands;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace DatabaseInstaller.Services
{
    public class TranslateEventLogsService : IHostedService
    {
        private readonly ILogger<TranslateEventLogsService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILocationRepository _locationRepository;
        private readonly TransferCommandConfiguration _config;

        public TranslateEventLogsService(
            ILogger<TranslateEventLogsService> logger,
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
                var locations = _locationRepository.GetLatestVersionOfAllLocations(_config.Start).Where(l => l.Devices.Select(d => d.DeviceType).Contains(DeviceTypes.WavetronixSpeed));

                _logger.LogInformation("Processing date: {Date} with {LocationCount} locations.", date, locations.Count());

                foreach (var batch in locations.Batch(50))
                {
                    await ProcessBatchAsync(batch, date, cancellationToken);
                }
            }
        }

        private async Task ProcessBatchAsync(IEnumerable<Location> locations, DateTime date, CancellationToken cancellationToken)
        {
            await Parallel.ForEachAsync(locations, async (location, token) =>
            {
                string selectQuery = $"SELECT LogData FROM [dbo].[ControllerLogArchives] WHERE SignalId = {location.LocationIdentifier} AND ArchiveDate = '{date}'";

                try
                {
                    using (var connection = new SqlConnection(_config.Source))
                    {
                        await connection.OpenAsync(token);
                        using (var command = new SqlCommand(selectQuery, connection))
                        using (var reader = await command.ExecuteReaderAsync(token))
                        {
                            if (await reader.ReadAsync(token))
                            {
                                byte[] compressedData = (byte[])reader["LogData"];
                                byte[] decompressedData;

                                using (var memoryStream = new MemoryStream(compressedData))
                                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                                using (var decompressedStream = new MemoryStream())
                                {
                                    await gzipStream.CopyToAsync(decompressedStream, token);
                                    decompressedData = decompressedStream.ToArray();
                                }

                                string json = System.Text.Encoding.UTF8.GetString(decompressedData);
                                var jsonObject = JsonConvert.DeserializeObject<List<ControllerEventLog>>(json);
                                jsonObject.ForEach(x => x.SignalIdentifier = location.LocationIdentifier);

                                var indianaEvents = ConvertToCompressedEvents(jsonObject, location, DateOnly.FromDateTime(date));

                                await InsertWithRetryAsync(indianaEvents, location.LocationIdentifier, date, token);

                                _logger.LogInformation("Processed Location: {LocationId} for Date: {Date} with {RecordCount} records.",
                                    location.LocationIdentifier, date, jsonObject.Count);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing Location: {LocationId} for Date: {Date}", location.LocationIdentifier, date);
                }
            });
        }

        private async Task InsertWithRetryAsync(CompressedEventLogs<IndianaEvent> indianaEvents, string locationId, DateTime date, CancellationToken token)
        {
            int retryCount = 0;
            const int maxRetries = 4;
            const int delaySeconds = 30;

            while (retryCount < maxRetries)
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

                    _logger.LogInformation("Successfully inserted data for Location: {LocationId} on Date: {Date} after {Retries} attempt(s).",
                        locationId, date, retryCount + 1);
                    return; // Exit the retry loop on success
                }
                catch (Exception ex)
                {
                    retryCount++;

                    _logger.LogError(ex, "Error inserting data for Location: {LocationId} on Date: {Date}. Attempt {Retry}/{MaxRetries}. Retrying in {DelaySeconds} seconds...",
                        locationId, date, retryCount, maxRetries, delaySeconds);

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogError("Max retries reached. Failing to insert data for Location: {LocationId} on Date: {Date}.", locationId, date);
                        return;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), token);
                }
            }
        }

        private CompressedEventLogs<IndianaEvent> ConvertToCompressedEvents(List<ControllerEventLog> objectList, Location location, DateOnly archiveDate)
        {
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
                    // Ignore errors
                }
            }

            var deviceId = location.Devices.FirstOrDefault(x => x.DeviceType == DeviceTypes.SignalController)?.Id;
            if (deviceId == null) return null;

            return new CompressedEventLogs<IndianaEvent>
            {
                LocationIdentifier = location.LocationIdentifier,
                ArchiveDate = archiveDate,
                Data = indianaEvents,
                DeviceId = deviceId.Value
            };
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    public static class BatchExtensions
    {
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            return source
                .Select((item, index) => new { item, index })
                .GroupBy(x => x.index / batchSize)
                .Select(group => group.Select(x => x.item));
        }
    }
}
