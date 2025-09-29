using DatabaseInstaller.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using System.Globalization;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace DatabaseInstaller.Services
{
    public class TranslateCSVEventLogsService : IHostedService
    {
        private readonly ILogger<TranslateCSVEventLogsService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILocationRepository _locationRepository;
        private readonly CSVTransferCommandConfiguration _config;
        private readonly AsyncRetryPolicy _retryPolicy;

        // Parsers now return object, but internally are Task<IReadOnlyList<T>>
        private static readonly Dictionary<string, Func<string, Task<object>>> _parsers =
            new(StringComparer.OrdinalIgnoreCase)
            {
            { "IndianaEvent", async file => await ParseIndianaEvents(file) },
            { "EnhancedEventLog", async file => await ParseEnhancedEvents(file) }
            };

        public TranslateCSVEventLogsService(
            ILogger<TranslateCSVEventLogsService> logger,
            IServiceProvider serviceProvider,
            ILocationRepository locationRepository,
            IOptions<CSVTransferCommandConfiguration> config)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _locationRepository = locationRepository;
            _config = config.Value;

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    sleepDurations: Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(10), retryCount: 5),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception,
                            "Retry {RetryCount} after {DelaySeconds}s while processing CSV",
                            retryCount, timeSpan.TotalSeconds);
                    });
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing CSV file: {FilePath} as {DataType}", _config.FilePath, _config.DataType);

            if (!File.Exists(_config.FilePath))
            {
                _logger.LogError("CSV file not found at path: {FilePath}", _config.FilePath);
                return;
            }

            if (!_parsers.TryGetValue(_config.DataType, out var parser))
            {
                _logger.LogError("Unsupported DataType: {DataType}", _config.DataType);
                return;
            }

            var parsed = await parser(_config.FilePath);
            if (parsed is not IEnumerable<EventLogModelBase> events || !events.Any())
            {
                _logger.LogWarning("No events parsed from CSV.");
                return;
            }

            if (string.IsNullOrEmpty(_config.LocationIdentifier))
            {
                _logger.LogWarning("No location identifier passed.");
                return;
            }

            var locationsQuery = _locationRepository.GetList()
                .Include(s => s.Devices)
                .FromSpecification(new ActiveLocationSpecification())
                .Where(l => l.LocationIdentifier == _config.LocationIdentifier);

            var locations = locationsQuery
                .GroupBy(r => r.LocationIdentifier)
                .Select(g => g.OrderByDescending(r => r.Start).FirstOrDefault())
                .ToList();

            foreach (var location in locations)
            {
                _logger.LogInformation("Processing location: {LocationId}", location.LocationIdentifier);

                if (_config.DataType.Equals("IndianaEvent", StringComparison.OrdinalIgnoreCase)
                    && parsed is IReadOnlyList<IndianaEvent> indianaEvents)
                {
                    var hourlyLogs = ConvertToHourlyCompressedEvents(indianaEvents, location);
                    if (hourlyLogs.Any())
                    {
                        await InsertLogsWithRetryAsync(hourlyLogs);
                    }
                }
                else if (_config.DataType.Equals("EnhancedEventLog", StringComparison.OrdinalIgnoreCase)
                         && parsed is IReadOnlyList<EnhancedEventLog> enhancedEvents)
                {
                    var hourlyLogs = ConvertToHourlyCompressedEvents(enhancedEvents, location);
                    if (hourlyLogs.Any())
                    {
                        await InsertLogsWithRetryAsync(hourlyLogs);
                    }
                }
            }

            _logger.LogInformation("CSV processing completed.");
        }

        private static async Task<IReadOnlyList<IndianaEvent>> ParseIndianaEvents(string filePath)
        {
            var events = new List<IndianaEvent>();

            await foreach (var line in File.ReadLinesAsync(filePath))
            {
                if (line.StartsWith("eventCode", StringComparison.OrdinalIgnoreCase)) // skip header
                    continue;

                var parts = line.Split(',');
                if (parts.Length < 4) continue;

                if (int.TryParse(parts[0], out var eventCode) &&
                    int.TryParse(parts[1], out var eventParam) &&
                    DateTime.TryParse(parts[2], CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp))
                {
                    events.Add(new IndianaEvent
                    {
                        EventCode = (short)eventCode,
                        EventParam = (short)eventParam,
                        Timestamp = timestamp,
                        LocationIdentifier = parts[3]
                    });
                }
            }

            return events;
        }

        private static async Task<IReadOnlyList<EnhancedEventLog>> ParseEnhancedEvents(string filePath)
        {
            var events = new List<EnhancedEventLog>();

            await foreach (var line in File.ReadLinesAsync(filePath))
            {
                if (line.StartsWith("zoneId", StringComparison.OrdinalIgnoreCase)) // skip header
                    continue;

                var parts = line.Split(',');
                if (parts.Length < 10) continue;

                if (long.TryParse(parts[0], out var zoneId) &&
                    double.TryParse(parts[3], out var length) &&
                    DateTime.TryParse(parts[8], CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp))
                {
                    events.Add(new EnhancedEventLog
                    {
                        ZoneId = zoneId,
                        ZoneName = parts[1],
                        ObjectType = parts[2],
                        Direction = parts[4],
                        Length = length,
                        DetectorId = parts[5],
                        Mph = int.TryParse(parts[6], out var mph) ? mph : 0,
                        Kph = int.TryParse(parts[7], out var kph) ? kph : 0,
                        Timestamp = timestamp,
                        LocationIdentifier = parts[9]
                    });
                }
            }

            return events;
        }

        private List<CompressedEventLogs<IndianaEvent>> ConvertToHourlyCompressedEvents(
            IReadOnlyList<IndianaEvent> events, Location location)
        {
            var hourlyCompressedEvents = new List<CompressedEventLogs<IndianaEvent>>();
            var deviceId = location.Devices.FirstOrDefault(x => x.DeviceType == DeviceTypes.SignalController)?.Id;

            if (deviceId == null)
            {
                _logger.LogError("No device found for Location: {LocationId}", location.LocationIdentifier);
                return hourlyCompressedEvents;
            }

            foreach (var hourGroup in events.GroupBy(e => new { e.Timestamp.Date, e.Timestamp.Hour }))
            {
                var start = new DateTime(hourGroup.Key.Date.Year, hourGroup.Key.Date.Month, hourGroup.Key.Date.Day, hourGroup.Key.Hour, 0, 0);
                var end = start.AddHours(1);

                hourlyCompressedEvents.Add(new CompressedEventLogs<IndianaEvent>
                {
                    LocationIdentifier = location.LocationIdentifier,
                    ArchiveDate = DateOnly.FromDateTime(start),
                    DeviceId = deviceId.Value,
                    Start = start,
                    End = end,
                    Data = hourGroup.ToList(),
                    DataType = typeof(IndianaEvent)
                });
            }

            return hourlyCompressedEvents;
        }

        private List<CompressedEventLogs<EnhancedEventLog>> ConvertToHourlyCompressedEvents(
            IReadOnlyList<EnhancedEventLog> events, Location location)
        {
            var hourlyCompressedEvents = new List<CompressedEventLogs<EnhancedEventLog>>();
            var deviceId = location.Devices.FirstOrDefault(x => x.DeviceType == DeviceTypes.SignalController)?.Id;

            if (deviceId == null)
            {
                _logger.LogError("No device found for Location: {LocationId}", location.LocationIdentifier);
                return hourlyCompressedEvents;
            }

            foreach (var hourGroup in events.GroupBy(e => new { e.Timestamp.Date, e.Timestamp.Hour }))
            {
                var start = new DateTime(hourGroup.Key.Date.Year, hourGroup.Key.Date.Month, hourGroup.Key.Date.Day, hourGroup.Key.Hour, 0, 0);
                var end = start.AddHours(1);

                hourlyCompressedEvents.Add(new CompressedEventLogs<EnhancedEventLog>
                {
                    LocationIdentifier = location.LocationIdentifier,
                    ArchiveDate = DateOnly.FromDateTime(start),
                    DeviceId = deviceId.Value,
                    Start = start,
                    End = end,
                    Data = hourGroup.ToList(),
                    DataType = typeof(EnhancedEventLog)
                });
            }

            return hourlyCompressedEvents;
        }

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
                await context.SaveChangesAsync();

                _logger.LogInformation("Inserted {Count} IndianaEvent hourly logs for {Date}",
                    hourlyLogs.Count, hourlyLogs.FirstOrDefault()?.ArchiveDate);
            });
        }

        private async Task InsertLogsWithRetryAsync(List<CompressedEventLogs<EnhancedEventLog>> hourlyLogs)
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
                await context.SaveChangesAsync();

                _logger.LogInformation("Inserted {Count} EnhancedEventLog hourly logs for {Date}",
                    hourlyLogs.Count, hourlyLogs.FirstOrDefault()?.ArchiveDate);
            });
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
