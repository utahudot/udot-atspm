#region license
// Copyright 2026 Utah Departement of Transportation
// for DataApi - Utah.Udot.Atspm.DataApi.Services/EventLogImporterService.cs
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly.Retry;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace Utah.Udot.Atspm.DataApi.Services
{
    /// <inheritdoc/>
    public class EventLogImporterService(
        AsyncRetryPolicy retryPolicy,
        IServiceScopeFactory serviceScopeFactory,
        ILocationRepository locationRepository,
        IDeviceRepository deviceRepository,
        ILogger<EventLogImporterService> logger) : IEventLogImporterService
    {
        /// <inheritdoc/>
        public IReadOnlyList<CompressedEventLogs<IndianaEvent>> CompressEvents(
            string locationIdentifier,
            IReadOnlyCollection<IndianaEvent> events)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(locationIdentifier);
            ArgumentNullException.ThrowIfNull(events);

            if (events.Count == 0)
            {
                return [];
            }

            var location = locationRepository.GetLatestVersionOfLocation(locationIdentifier)
                ?? throw new InvalidOperationException(
                    $"No location found for LocationIdentifier: {locationIdentifier}");

            var activeDevices = deviceRepository.GetActiveDevicesByLocation(location.Id);
            var device = activeDevices.FirstOrDefault(
                    d => d.DeviceType == DeviceTypes.SignalController)
                ?? activeDevices.FirstOrDefault(
                    d => d.DeviceType == DeviceTypes.RampController)
                ?? throw new InvalidOperationException(
                    $"No active controller found for LocationIdentifier: {locationIdentifier}");

            return events
                .OrderBy(e => e.Timestamp)
                .GroupBy(e => DateOnly.FromDateTime(e.Timestamp))
                .OrderBy(group => group.Key)
                .Select(group =>
                {
                    var dayEvents = group.ToList();
                    return new CompressedEventLogs<IndianaEvent>
                    {
                        LocationIdentifier = locationIdentifier,
                        DeviceId = device.Id,
                        Start = dayEvents[0].Timestamp,
                        End = dayEvents[^1].Timestamp,
                        Data = dayEvents
                    };
                })
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<bool> InsertLogsWithRetryAsync(
            IReadOnlyCollection<CompressedEventLogs<IndianaEvent>> archiveLogs,
            CancellationToken cancelToken = default)
        {
            ArgumentNullException.ThrowIfNull(archiveLogs);

            if (archiveLogs.Count == 0)
            {
                return false;
            }

            foreach (var archiveLog in archiveLogs)
            {
                try
                {
                    await retryPolicy.ExecuteAsync(
                        async token =>
                        {
                            using var scope = serviceScopeFactory.CreateScope();
                            var context = scope.ServiceProvider
                                .GetRequiredService<EventLogContext>();

                            context.CompressedEvents.Add(archiveLog);

                            try
                            {
                                await context.SaveChangesAsync(token);
                            }
                            catch (DbUpdateException ex) when (IsDuplicateKey(ex))
                            {
                                logger.LogInformation(
                                    "Event log already exists for {LocationIdentifier}, device {DeviceId}, {Start}–{End}",
                                    archiveLog.LocationIdentifier,
                                    archiveLog.DeviceId,
                                    archiveLog.Start,
                                    archiveLog.End);
                            }
                        },
                        cancelToken);
                }
                catch (OperationCanceledException) when (cancelToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Failed to insert event log for {LocationIdentifier}, device {DeviceId}, {Start}–{End}",
                        archiveLog.LocationIdentifier,
                        archiveLog.DeviceId,
                        archiveLog.Start,
                        archiveLog.End);
                    return false;
                }
            }

            return true;
        }

        private static bool IsDuplicateKey(DbUpdateException exception) =>
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            };
    }
}
