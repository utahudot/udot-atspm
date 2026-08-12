#region license
// Copyright 2026 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Services/TransferSpeedEventsService.cs
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
            for (var date = _config.Start; date <= _config.End; date = date.AddDays(1))
            {
                for (int hour = 0; hour < 24; hour++)
                {
                    var periodStart = new DateTime(DateOnly.FromDateTime(date), new TimeOnly(hour, 0));
                    var periodEnd = periodStart.AddHours(1);

                    _logger.LogInformation(
                        $"Processing speed events from {periodStart:yyyy-MM-dd HH:mm} to {periodEnd:HH:mm}");

                    var locations = GetSpeedLocations(periodStart);

                    if (locations.Count == 0)
                    {
                        _logger.LogWarning("No configured speed locations were found for {PeriodStart}.", periodStart);
                        continue;
                    }

                    var archiveLogs = await GetHourlyLogsWithRetryAsync(
                        periodStart,
                        periodEnd,
                        locations,
                        cancellationToken);

                    if (archiveLogs == null)
                    {
                        _logger.LogError(
                            "Skipping speed events from {PeriodStart} to {PeriodEnd} because the hourly query failed.",
                            periodStart,
                            periodEnd);
                        continue;
                    }

                    if (archiveLogs.Count > 0)
                    {
                        await FlushLogsAsync(archiveLogs, cancellationToken);
                    }
                }
            }

            _logger.LogInformation("Hourly transfer completed.");
        }

        private List<Location> GetSpeedLocations(DateTime periodStart)
        {
            var latestLocationIds = _locationRepository.GetList()
                .Where(location => location.Start <= periodStart)
                .FromSpecification(new ActiveLocationSpecification())
                .GroupBy(location => location.LocationIdentifier)
                .Select(group => group
                    .OrderByDescending(location => location.Start)
                    .Select(location => location.Id)
                    .First())
                .ToList();

            const int batchSize = 100;
            var locations = new List<Location>(latestLocationIds.Count);

            for (var index = 0; index < latestLocationIds.Count; index += batchSize)
            {
                var batchIds = latestLocationIds
                    .Skip(index)
                    .Take(batchSize)
                    .ToList();

                var batch = _locationRepository.GetList()
                    .Where(location => batchIds.Contains(location.Id))
                    .Where(location => location.Devices.Any(device => device.DeviceType == DeviceTypes.SpeedSensor))
                    .Include(location => location.Devices)
                    .AsNoTracking()
                    .ToList();

                locations.AddRange(batch);
            }

            return locations;
        }

        private async Task<List<CompressedEventLogs<SpeedEvent>>?> GetHourlyLogsWithRetryAsync(
            DateTime periodStart,
            DateTime periodEnd,
            IReadOnlyCollection<Location> locations,
            CancellationToken cancellationToken)
        {
            const int retryCount = 2;

            for (var attempt = 1; attempt <= retryCount; attempt++)
            {
                try
                {
                    return await GetHourlyLogsAsync(
                        periodStart,
                        periodEnd,
                        _config.Source,
                        locations,
                        cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex) when (attempt < retryCount)
                {
                    var retryDelay = TimeSpan.FromSeconds(attempt * 5);
                    _logger.LogWarning(
                        "Hourly query attempt {Attempt} of {RetryCount} failed for {PeriodStart}: {Message}. Retrying in {DelaySeconds} seconds...",
                        attempt,
                        retryCount,
                        periodStart,
                        ex.Message,
                        retryDelay.TotalSeconds);
                    await Task.Delay(retryDelay, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Hourly query failed after {RetryCount} attempts for {PeriodStart}.",
                        retryCount,
                        periodStart);
                }
            }

            return null;
        }

        private async Task FlushLogsAsync(
            IReadOnlyCollection<CompressedEventLogs<SpeedEvent>> archiveLogs,
            CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetService<EventLogContext>();
                if (context != null)
                {
                    context.CompressedEvents.AddRange(archiveLogs);
                    await context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Flushed {LogCount} compressed event logs.", archiveLogs.Count);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Bulk flush failed: {ex.Message}. Falling back to individual inserts.");
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<EventLogContext>();
                if (context != null)
                {
                    foreach (var log in archiveLogs)
                    {
                        try
                        {
                            context.CompressedEvents.Add(log);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Failed to insert single log for {log.LocationIdentifier} between {log.Start:HH:mm} and {log.End:HH:mm}: {ex.Message}");
                        }
                    }
                }
            }
        }

        private async Task<List<CompressedEventLogs<SpeedEvent>>> GetHourlyLogsAsync(
            DateTime periodStart,
            DateTime periodEnd,
            string sourceConnectionString,
            IReadOnlyCollection<Location> locations,
            CancellationToken cancellationToken)
        {
            var query = @"
                SELECT DetectorID, MPH, KPH, Timestamp
                  FROM MOE.dbo.Speed_Events
                 WHERE Timestamp >= @periodStart
                   AND Timestamp <  @periodEnd";

            var locationsByIdentifier = locations
                .GroupBy(location => location.LocationIdentifier, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            var locationIdentifierLengths = locationsByIdentifier.Keys
                .Select(identifier => identifier.Length)
                .Distinct()
                .OrderByDescending(length => length)
                .ToArray();

            var eventsByLocation = new Dictionary<string, List<SpeedEvent>>(StringComparer.OrdinalIgnoreCase);
            var unmatchedEventCount = 0;

            using (var conn = new SqlConnection(sourceConnectionString))
            {
                await conn.OpenAsync(cancellationToken);
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@periodStart", periodStart);
                cmd.Parameters.AddWithValue("@periodEnd", periodEnd);
                cmd.CommandTimeout = 120;

                using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                var detectorIdOrdinal = reader.GetOrdinal("DetectorID");
                var mphOrdinal = reader.GetOrdinal("MPH");
                var kphOrdinal = reader.GetOrdinal("KPH");
                var timestampOrdinal = reader.GetOrdinal("Timestamp");

                while (await reader.ReadAsync(cancellationToken))
                {
                    var detectorId = reader.GetString(detectorIdOrdinal);
                    var locationIdentifier = FindLocationIdentifier(
                        detectorId,
                        locationsByIdentifier,
                        locationIdentifierLengths);

                    if (locationIdentifier == null)
                    {
                        unmatchedEventCount++;
                        continue;
                    }

                    if (!eventsByLocation.TryGetValue(locationIdentifier, out var eventLogs))
                    {
                        eventLogs = new List<SpeedEvent>();
                        eventsByLocation.Add(locationIdentifier, eventLogs);
                    }

                    eventLogs.Add(new SpeedEvent
                    {
                        DetectorId = detectorId,
                        Mph = reader.GetInt32(mphOrdinal),
                        Kph = reader.GetInt32(kphOrdinal),
                        Timestamp = reader.GetDateTime(timestampOrdinal)
                    });
                }
            }

            var archiveLogs = new List<CompressedEventLogs<SpeedEvent>>(eventsByLocation.Count);
            var retrievedEventCount = 0;

            foreach (var (locationIdentifier, eventLogs) in eventsByLocation)
            {
                var location = locationsByIdentifier[locationIdentifier];
                var device = location.Devices.FirstOrDefault(d => d.DeviceType == DeviceTypes.SpeedSensor);

                if (device != null)
                {
                    archiveLogs.Add(new CompressedEventLogs<SpeedEvent>
                    {
                        LocationIdentifier = locationIdentifier,
                        DeviceId = device.Id,
                        Start = periodStart,
                        End = periodEnd,
                        Data = eventLogs
                    });
                    retrievedEventCount += eventLogs.Count;
                }
            }

            _logger.LogInformation(
                "Retrieved {EventCount} speed events for {LocationCount} locations between {PeriodStart} and {PeriodEnd}.",
                retrievedEventCount,
                archiveLogs.Count,
                periodStart,
                periodEnd);

            if (unmatchedEventCount > 0)
            {
                _logger.LogWarning(
                    "Ignored {UnmatchedEventCount} speed events whose DetectorID did not begin with a configured speed location identifier.",
                    unmatchedEventCount);
            }

            return archiveLogs;
        }

        private static string? FindLocationIdentifier(
            string detectorId,
            IReadOnlyDictionary<string, Location> locationsByIdentifier,
            IReadOnlyCollection<int> locationIdentifierLengths)
        {
            foreach (var length in locationIdentifierLengths)
            {
                if (detectorId.Length >= length)
                {
                    var candidate = detectorId[..length];
                    if (locationsByIdentifier.ContainsKey(candidate))
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
