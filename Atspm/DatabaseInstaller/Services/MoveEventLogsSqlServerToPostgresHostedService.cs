#region license
// Copyright 2026 Utah Departement of Transportation
// for DatabaseInstaller - DatabaseInstaller.Services/MoveEventLogsSqlServerToPostgresHostedService.cs
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
using Npgsql;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Models;
using Utah.Udot.Atspm.Infrastructure.Configuration;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Specifications;
using Utah.Udot.NetStandardToolkit.Extensions;

namespace DatabaseInstaller.Services
{
    public class MoveEventLogsSqlServerToPostgresHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MoveEventLogsSqlServerToPostgresHostedService> _logger;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly TransferCommandConfiguration _config;
        private readonly ILocationRepository _locationRepository;
        private readonly IOptionsMonitor<DatabaseConfiguration> _databaseConfiguration;

        public MoveEventLogsSqlServerToPostgresHostedService(
            IServiceProvider serviceProvider,
            IOptions<TransferCommandConfiguration> config,
            IOptionsMonitor<DatabaseConfiguration> databaseConfiguration,
            ILogger<MoveEventLogsSqlServerToPostgresHostedService> logger,
            IHostApplicationLifetime lifetime,
            ILocationRepository locationRepository)
        {
            _serviceProvider = serviceProvider;
            _config = config.Value;
            _databaseConfiguration = databaseConfiguration;
            _logger = logger;
            _lifetime = lifetime;
            _locationRepository = locationRepository;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var locations = GetTargetLocations();
                var destinationConnectionString = GetDestinationConnectionString();
                var resumePlan = _config.Resume
                    ? await BuildResumePlanAsync(
                        locations.Select(l => l.LocationIdentifier).Distinct().ToList(),
                        destinationConnectionString,
                        cancellationToken)
                    : ResumePlan.Empty;

                var parallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = GetMaxConcurrency()
                };

                await Parallel.ForEachAsync(locations, parallelOptions, async (location, token) =>
                {
                    await ProcessLocationAsync(location, destinationConnectionString, resumePlan, token);
                });

                _logger.LogInformation("Event logs moved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while moving event logs.");
            }
            finally
            {
                _lifetime.StopApplication();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private List<Location> GetTargetLocations()
        {
            var query = _locationRepository
                .GetList()
                .Include(l => l.Devices)
                .AsQueryable();

            if (_config.Device.HasValue)
            {
                query = query.Where(l => l.Devices
                    .Any(d => d.DeviceType == (DeviceTypes)_config.Device));
            }

            if (!string.IsNullOrEmpty(_config.Locations))
            {
                var locationIdentifiers = _config.Locations.Split(',', StringSplitOptions.RemoveEmptyEntries);
                query = query
                    .Where(l => locationIdentifiers.Contains(l.LocationIdentifier));
            }

            return query
                .FromSpecification(new ActiveLocationSpecification())
                .GroupBy(l => l.LocationIdentifier)
                .Select(g => g.OrderByDescending(l => l.Start).First())
                .ToList();
        }

        private async Task ProcessLocationAsync(
            Location location,
            string destinationConnectionString,
            ResumePlan resumePlan,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing location {LocationId}", location.LocationIdentifier);

            var datesToProcess = GetDatesToProcess(location.LocationIdentifier, resumePlan).ToList();
            if (datesToProcess.Count == 0)
            {
                _logger.LogInformation(
                    "Skipping location {LocationId}; resume plan found no source days missing from the destination.",
                    location.LocationIdentifier);
                return;
            }

            await using var sourceConnection = new SqlConnection(_config.Source);
            await sourceConnection.OpenAsync(cancellationToken);

            await using var destinationConnection = new NpgsqlConnection(destinationConnectionString);
            await destinationConnection.OpenAsync(cancellationToken);

            foreach (var date in datesToProcess)
            {
                await ProcessDateAsync(location, date, resumePlan, sourceConnection, destinationConnection, cancellationToken);
            }
        }

        private IEnumerable<DateTime> GetDatesToProcess(string locationIdentifier, ResumePlan resumePlan)
        {
            for (var date = _config.Start.Date;
                 date <= _config.End.Date;
                 date = date.AddDays(1))
            {
                if (!_config.Resume || !resumePlan.ShouldSkip(locationIdentifier, date, out _))
                {
                    yield return date;
                }
            }
        }

        private async Task ProcessDateAsync(
            Location location,
            DateTime date,
            ResumePlan resumePlan,
            SqlConnection sourceConnection,
            NpgsqlConnection destinationConnection,
            CancellationToken cancellationToken)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);

            if (_config.Resume)
            {
                if (resumePlan.ShouldSkip(location.LocationIdentifier, dayStart, out var status))
                {
                    if (status.SourceCount == 0)
                    {
                        _logger.LogInformation(
                            "No source compressed rows for {LocationId} on {Date}; skipping.",
                            location.LocationIdentifier,
                            dayStart);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Skipping {LocationId} on {Date}; destination contains all {SourceCount} source rows ({DestinationCount} destination rows in range).",
                            location.LocationIdentifier,
                            dayStart,
                            status.SourceCount,
                            status.DestinationCount);
                    }

                    return;
                }

                _logger.LogInformation(
                    "Resuming {LocationId} on {Date}; destination is missing {MissingCount} of {SourceCount} source rows ({DestinationCount} destination rows in range).",
                    location.LocationIdentifier,
                    dayStart,
                    status.MissingCount,
                    status.SourceCount,
                    status.DestinationCount);
            }

            const string selectQuery = """
                SELECT [LocationIdentifier], [DeviceId], [DataType], [Start], [End], [Data]
                FROM [CompressedEvents]
                WHERE [LocationIdentifier] = @locationIdentifier
                  AND [End] > @start
                  AND [Start] < @end
                ORDER BY [Start], [End], [DeviceId], [DataType]
                """;

            await using var command = new SqlCommand(selectQuery, sourceConnection);
            command.Parameters.AddWithValue("@locationIdentifier", location.LocationIdentifier);
            command.Parameters.AddWithValue("@start", dayStart);
            command.Parameters.AddWithValue("@end", dayEnd);
            command.CommandTimeout = 120;

            var batchSize = GetCopyBatchSize();
            var rows = new List<CompressedEventLogRow>(batchSize);
            var inserted = 0;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                rows.Add(new CompressedEventLogRow(
                    reader.GetString(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.GetDateTime(3),
                    reader.GetDateTime(4),
                    reader.IsDBNull(5) ? null : (byte[])reader[5]));

                if (rows.Count >= batchSize)
                {
                    inserted += await InsertBatchAsync(destinationConnection, rows, cancellationToken);
                    rows.Clear();
                }
            }

            if (rows.Count > 0)
            {
                inserted += await InsertBatchAsync(destinationConnection, rows, cancellationToken);
            }

            _logger.LogInformation(
                "Copied {Count} compressed rows for {LocationId} on {Date}",
                inserted,
                location.LocationIdentifier,
                dayStart);
        }

        private async Task<ResumePlan> BuildResumePlanAsync(
            IReadOnlyCollection<string> locationIdentifiers,
            string destinationConnectionString,
            CancellationToken cancellationToken)
        {
            if (locationIdentifiers.Count == 0)
            {
                return ResumePlan.Empty;
            }

            var rangeStart = _config.Start.Date;
            var rangeEnd = _config.End.Date.AddDays(1);
            var sourceCounts = new Dictionary<ResumeDayKey, int>();
            var destinationCounts = new Dictionary<ResumeDayKey, int>();
            var missingCounts = new Dictionary<ResumeDayKey, int>();
            var sourceKeyCount = 0;
            var destinationKeyCount = 0;

            _logger.LogInformation(
                "Building resume plan for {LocationCount} locations from {Start} through {End}.",
                locationIdentifiers.Count,
                rangeStart,
                rangeEnd.AddDays(-1));

            await using var sourceConnection = new SqlConnection(_config.Source);
            await sourceConnection.OpenAsync(cancellationToken);

            await using var destinationConnection = new NpgsqlConnection(destinationConnectionString);
            await destinationConnection.OpenAsync(cancellationToken);

            foreach (var locationBatch in locationIdentifiers.Chunk(500))
            {
                var sourceKeys = await GetSourceKeysAsync(
                    locationBatch,
                    rangeStart,
                    rangeEnd,
                    sourceConnection,
                    cancellationToken);

                var destinationKeys = await GetDestinationKeysAsync(
                    locationBatch,
                    rangeStart,
                    rangeEnd,
                    destinationConnection,
                    cancellationToken);

                sourceKeyCount += sourceKeys.Count;
                destinationKeyCount += destinationKeys.Count;

                foreach (var key in sourceKeys)
                {
                    foreach (var day in GetOverlappedDays(key.Start, key.End, rangeStart, rangeEnd))
                    {
                        Increment(sourceCounts, new ResumeDayKey(key.LocationIdentifier, day));
                    }

                    if (destinationKeys.Contains(key))
                    {
                        continue;
                    }

                    foreach (var day in GetOverlappedDays(key.Start, key.End, rangeStart, rangeEnd))
                    {
                        Increment(missingCounts, new ResumeDayKey(key.LocationIdentifier, day));
                    }
                }

                foreach (var key in destinationKeys)
                {
                    foreach (var day in GetOverlappedDays(key.Start, key.End, rangeStart, rangeEnd))
                    {
                        Increment(destinationCounts, new ResumeDayKey(key.LocationIdentifier, day));
                    }
                }
            }

            var statuses = sourceCounts.ToDictionary(
                pair => pair.Key,
                pair => new ResumeDayStatus(
                    pair.Value,
                    destinationCounts.TryGetValue(pair.Key, out var destinationCount) ? destinationCount : 0,
                    missingCounts.TryGetValue(pair.Key, out var missingCount) ? missingCount : 0));

            var completeDays = statuses.Count(pair => pair.Value.MissingCount == 0);
            var partialDays = statuses.Count - completeDays;

            _logger.LogInformation(
                "Resume plan built from {SourceKeyCount} source keys and {DestinationKeyCount} destination keys: {CompleteDays} complete source days, {PartialDays} partial source days.",
                sourceKeyCount,
                destinationKeyCount,
                completeDays,
                partialDays);

            return new ResumePlan(statuses);
        }

        private static async Task<HashSet<CompressedEventLogKey>> GetSourceKeysAsync(
            IReadOnlyCollection<string> locationIdentifiers,
            DateTime rangeStart,
            DateTime rangeEnd,
            SqlConnection sourceConnection,
            CancellationToken cancellationToken)
        {
            var locationParameters = string.Join(", ", locationIdentifiers.Select((_, index) => $"@locationIdentifier{index}"));
            var selectQuery = $"""
                SELECT [LocationIdentifier], [DeviceId], [DataType], [Start], [End]
                FROM [CompressedEvents]
                WHERE [LocationIdentifier] IN ({locationParameters})
                  AND [End] > @start
                  AND [Start] < @end
                """;

            await using var command = new SqlCommand(selectQuery, sourceConnection);
            AddLocationParameters(command, locationIdentifiers);
            command.Parameters.AddWithValue("@start", rangeStart);
            command.Parameters.AddWithValue("@end", rangeEnd);
            command.CommandTimeout = 120;

            var keys = new HashSet<CompressedEventLogKey>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                keys.Add(new CompressedEventLogKey(
                    reader.GetString(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.GetDateTime(3),
                    reader.GetDateTime(4)));
            }

            return keys;
        }

        private static async Task<HashSet<CompressedEventLogKey>> GetDestinationKeysAsync(
            IReadOnlyCollection<string> locationIdentifiers,
            DateTime rangeStart,
            DateTime rangeEnd,
            NpgsqlConnection destinationConnection,
            CancellationToken cancellationToken)
        {
            var locationParameters = string.Join(", ", locationIdentifiers.Select((_, index) => $"@locationIdentifier{index}"));
            var selectQuery = $"""
                SELECT "LocationIdentifier", "DeviceId", "DataType", "Start", "End"
                FROM "CompressedEvents"
                WHERE "LocationIdentifier" IN ({locationParameters})
                  AND "End" > @start
                  AND "Start" < @end
                """;

            await using var command = new NpgsqlCommand(selectQuery, destinationConnection);
            AddLocationParameters(command, locationIdentifiers);
            command.Parameters.AddWithValue("start", rangeStart);
            command.Parameters.AddWithValue("end", rangeEnd);
            command.CommandTimeout = 120;

            var keys = new HashSet<CompressedEventLogKey>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                keys.Add(new CompressedEventLogKey(
                    reader.GetString(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.GetDateTime(3),
                    reader.GetDateTime(4)));
            }

            return keys;
        }

        private static void AddLocationParameters(SqlCommand command, IReadOnlyCollection<string> locationIdentifiers)
        {
            var index = 0;
            foreach (var locationIdentifier in locationIdentifiers)
            {
                command.Parameters.AddWithValue($"@locationIdentifier{index}", locationIdentifier);
                index++;
            }
        }

        private static void AddLocationParameters(NpgsqlCommand command, IReadOnlyCollection<string> locationIdentifiers)
        {
            var index = 0;
            foreach (var locationIdentifier in locationIdentifiers)
            {
                command.Parameters.AddWithValue($"locationIdentifier{index}", locationIdentifier);
                index++;
            }
        }

        private static void Increment(Dictionary<ResumeDayKey, int> counts, ResumeDayKey key)
        {
            counts[key] = counts.TryGetValue(key, out var count) ? count + 1 : 1;
        }

        private static IEnumerable<DateTime> GetOverlappedDays(
            DateTime start,
            DateTime end,
            DateTime rangeStart,
            DateTime rangeEnd)
        {
            var overlapStart = start > rangeStart ? start : rangeStart;
            var overlapEnd = end < rangeEnd ? end : rangeEnd;

            if (overlapEnd <= overlapStart)
            {
                yield break;
            }

            for (var day = overlapStart.Date; day < overlapEnd; day = day.AddDays(1))
            {
                yield return day;
            }
        }

        private async Task<int> InsertBatchAsync(
            NpgsqlConnection destinationConnection,
            List<CompressedEventLogRow> rows,
            CancellationToken cancellationToken)
        {
            if (rows.Count == 0)
            {
                return 0;
            }

            const string insertQuery = """
                INSERT INTO "CompressedEvents" ("LocationIdentifier", "DeviceId", "DataType", "Start", "End", "Data")
                VALUES (@locationIdentifier, @deviceId, @dataType, @start, @end, @data)
                ON CONFLICT ("LocationIdentifier", "DeviceId", "DataType", "Start", "End") DO NOTHING
                """;

            await using var transaction = await destinationConnection.BeginTransactionAsync(cancellationToken);
            await using var batch = new NpgsqlBatch(destinationConnection, transaction);

            foreach (var row in rows)
            {
                var batchCommand = new NpgsqlBatchCommand(insertQuery);
                batchCommand.Parameters.AddWithValue("locationIdentifier", row.LocationIdentifier);
                batchCommand.Parameters.AddWithValue("deviceId", row.DeviceId);
                batchCommand.Parameters.AddWithValue("dataType", row.DataType);
                batchCommand.Parameters.AddWithValue("start", row.Start);
                batchCommand.Parameters.AddWithValue("end", row.End);
                batchCommand.Parameters.AddWithValue("data", row.Data is null ? DBNull.Value : row.Data);
                batch.BatchCommands.Add(batchCommand);
            }

            var inserted = await batch.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return inserted;
        }

        private string GetDestinationConnectionString()
        {
            var settings = _databaseConfiguration.Get(nameof(EventLogContext));

            if (settings is null)
            {
                throw new InvalidOperationException("DatabaseConfiguration:EventLogContext is not configured.");
            }

            if (!string.Equals(settings.DBType, "PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"DatabaseConfiguration:EventLogContext must be PostgreSQL for copy-sql, but was '{settings.DBType}'.");
            }

            return settings.BuildConnectionString();
        }

        private int GetMaxConcurrency()
        {
            if (_config.MaxConcurrency < 1)
            {
                _logger.LogWarning("MaxConcurrency was set to {MaxConcurrency}; defaulting to 1.", _config.MaxConcurrency);
                return 1;
            }

            return _config.MaxConcurrency;
        }

        private int GetCopyBatchSize()
        {
            if (_config.CopyBatchSize < 1)
            {
                _logger.LogWarning("CopyBatchSize was set to {CopyBatchSize}; defaulting to 1.", _config.CopyBatchSize);
                return 1;
            }

            return _config.CopyBatchSize;
        }

        private sealed record CompressedEventLogRow(
            string LocationIdentifier,
            int DeviceId,
            string DataType,
            DateTime Start,
            DateTime End,
            byte[]? Data);

        private sealed record CompressedEventLogKey(
            string LocationIdentifier,
            int DeviceId,
            string DataType,
            DateTime Start,
            DateTime End);

        private sealed record ResumeDayKey(string LocationIdentifier, DateTime Day);

        private sealed record ResumeDayStatus(int SourceCount, int DestinationCount, int MissingCount);

        private sealed class ResumePlan
        {
            public static readonly ResumePlan Empty = new(new Dictionary<ResumeDayKey, ResumeDayStatus>());

            private readonly IReadOnlyDictionary<ResumeDayKey, ResumeDayStatus> _statuses;

            public ResumePlan(IReadOnlyDictionary<ResumeDayKey, ResumeDayStatus> statuses)
            {
                _statuses = statuses;
            }

            public bool ShouldSkip(string locationIdentifier, DateTime day, out ResumeDayStatus status)
            {
                if (!_statuses.TryGetValue(new ResumeDayKey(locationIdentifier, day.Date), out status!))
                {
                    status = new ResumeDayStatus(0, 0, 0);
                    return true;
                }

                return status.MissingCount == 0;
            }
        }
    }
}
