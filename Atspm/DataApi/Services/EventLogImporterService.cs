using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace Utah.Udot.ATSPM.DataApi.Services
{
    public class EventLogImporterService
    {
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
        private readonly IServiceProvider _serviceProvider;
        private ILocationRepository _locationRepository;
        private IDeviceRepository _deviceRepository;

        public EventLogImporterService(AsyncRetryPolicy retryPolicy, IServiceProvider serviceProvider, ILocationRepository locationRepository, IDeviceRepository deviceRepository)
        {
            _retryPolicy = retryPolicy;
            _serviceProvider = serviceProvider;
            _locationRepository = locationRepository;
            _deviceRepository = deviceRepository;
        }

        public List<CompressedEventLogs<IndianaEvent>> CompressEvents(string locationIdentifier, List<IndianaEvent> events)
        {
            var location = _locationRepository.GetLatestVersionOfLocation(locationIdentifier);
            var results = new List<CompressedEventLogs<IndianaEvent>>();

            if (!events.Any())
                return results;

            var device = _deviceRepository.GetActiveDevicesByLocation(location.Id)
                .FirstOrDefault(d => d.DeviceType == DeviceTypes.SignalController);

            if (device == null)
                return results;

            // Sort the events by timestamp
            var orderedEvents = events.OrderBy(e => e.Timestamp).ToList();

            // Group by the day (you can adjust to local time if needed)
            var groupedByDay = orderedEvents
                .GroupBy(e => DateOnly.FromDateTime(e.Timestamp))
                .OrderBy(g => g.Key);

            var archiveDate = DateOnly.FromDateTime(DateTime.Now);

            foreach (var dayGroup in groupedByDay)
            {
                var dayEvents = dayGroup.ToList();
                var start = dayEvents.First().Timestamp;
                var end = dayEvents.Last().Timestamp;

                var compressedLog = new CompressedEventLogs<IndianaEvent>
                {
                    LocationIdentifier = locationIdentifier,
                    DeviceId = device.Id,
                    //ArchiveDate = archiveDate, //TODO - re-add this when database is upgraded
                    Start = start,
                    End = end,
                    Data = dayEvents
                };

                results.Add(compressedLog);
            }

            return results;
        }




        public async Task<bool> InsertLogWithRetryAsync(CompressedEventLogs<IndianaEvent> archiveLog)
        {
            if (archiveLog == null)
            {
                return false; // Nothing to insert
            }

            try
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetService<EventLogContext>();

                    if (context == null)
                    {
                        throw new InvalidOperationException("EventLogContext is not available.");
                    }

                    context.CompressedEvents.Add(archiveLog);

                    try
                    {
                        await context.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex) when (
                        ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                    {
                        // Duplicate key — log already exists, skip insert
                        var doubleKey = true;
                    }
                });

                return true; // Successfully inserted (or already exists)
            }
            catch (Exception e)
            {
                // Retry policy failed
                return false;
            }
        }

        public async Task<bool> InsertLogsWithRetryAsync(List<CompressedEventLogs<IndianaEvent>> archiveLogs)
        {
            if (archiveLogs == null || !archiveLogs.Any())
            {
                return false; // Nothing to insert
            }

            try
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetService<EventLogContext>();

                    if (context == null)
                    {
                        throw new InvalidOperationException("EventLogContext is not available.");
                    }

                    context.CompressedEvents.AddRange(archiveLogs);

                    try
                    {
                        await context.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex) when (
                        ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                    {
                        // Handle duplicate key 
                    }
                });

                return true; // Successfully inserted (or already exists)
            }
            catch (Exception e)
            {
                return false;
            }
        }



    }
}
