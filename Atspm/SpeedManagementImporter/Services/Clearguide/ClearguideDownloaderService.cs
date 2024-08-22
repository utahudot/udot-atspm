using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.Common;
using ATSPM.Data.Models.SpeedManagementAggregation;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SpeedManagementImporter.Business.Clearguide;
using System.Collections.Concurrent;

namespace SpeedManagementImporter.Services.Clearguide
{
    public class ClearguideDownloaderService : IDataDownloader
    {
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly ISegmentEntityRepository segmentEntityRepository;
        private IConfigurationRoot configuration;


        static readonly string API_URL = "https://api.iteris-clearguide.com/v1/link/timeseries/";
        static readonly string CUSTOMER_KEY = "ut";
        static readonly string ROUTE_ID_TYPE = "customer_route_number";
        static readonly string METRIC = "avg_speed";
        static readonly string GRANULARITY = "hour";
        static readonly string INCLUDE_HOLIDAYS = "true";
        static readonly int confidenceId = 4;
        static readonly int sourceId = 3;

        public ClearguideDownloaderService(ISegmentEntityRepository segmentEntityRepository, IHourlySpeedRepository hourlySpeedRepository, IConfigurationRoot configuration)
        {
            this.segmentEntityRepository = segmentEntityRepository;
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.configuration = configuration;
        }

        public async Task Download(DateTime startDate, DateTime endDate)
        {

            List<SegmentEntityWithSpeed> routeEntities = await segmentEntityRepository.GetEntitiesWithSpeedForSourceId(sourceId);
            var routes = routeEntities.GroupBy(r => r.SegmentId).ToList();

            long START_TIMESTAMP = ((DateTimeOffset)startDate).ToUnixTimeSeconds();
            long END_TIMESTAMP = ((DateTimeOffset)endDate).ToUnixTimeSeconds() - 3600;

            var username = configuration["Clearguide:Username"];
            var password = configuration["Clearguide:Password"];

            if (username == null || password == null)
            {
                return;
            }

            var cgApiHandler = new ClearguideApiHandler(username, password);
            int maxParallelTasks = 1;
            var semaphore = new SemaphoreSlim(maxParallelTasks);

            List<Task> tasks = new List<Task>();

            foreach (var route in routes)
            {
                await semaphore.WaitAsync(); // Wait until a semaphore slot is available.
                tasks.Add(ProcessRouteAsync(START_TIMESTAMP, END_TIMESTAMP, cgApiHandler, route, semaphore));
            }
            await Task.WhenAll(tasks);

            Console.WriteLine("Finished adding all speed data for all routes");
        }

        private async Task ProcessRouteAsync(long START_TIMESTAMP, long END_TIMESTAMP, ClearguideApiHandler cgApiHandler, IGrouping<Guid, SegmentEntityWithSpeed> routeWithEntities, SemaphoreSlim semaphore)
        {
            var speeds = new ConcurrentBag<HourlySpeedWithEntityId>();
            var tasks = new List<Task>();

            try
            {
                using (var throttle = new SemaphoreSlim(3)) // Limit the number of concurrent tasks
                {
                    foreach (SegmentEntityWithSpeed routeEntity in routeWithEntities)
                    {
                        await throttle.WaitAsync();

                        tasks.Add(Task.Run(async () =>
                        {
                            string query = ConstructApiQuery(routeEntity.EntityId, START_TIMESTAMP, END_TIMESTAMP);
                            try
                            {
                                var response = await cgApiHandler.CallAsync(query);

                                if (response != null)
                                {
                                    JObject json = JObject.Parse(response);
                                    JToken speedData = json["series"]["all"]["avg_speed"]["data"];

                                    foreach (var data in speedData)
                                    {
                                        DateTime date = DateTimeOffset.FromUnixTimeSeconds((long)data[0]).DateTime;
                                        int avg = (int)Math.Round((double)data[1]);
                                        HourlySpeedWithEntityId speed = new()
                                        {
                                            EntityId = routeEntity.EntityId,
                                            Date = date,
                                            BinStartTime = date,
                                            SegmentId = routeEntity.SegmentId,
                                            SourceId = sourceId,
                                            ConfidenceId = confidenceId,
                                            Average = avg,
                                            Violation = routeEntity.SpeedLimit > 0 && avg > routeEntity.SpeedLimit ? (long)(avg - routeEntity.SpeedLimit) : 0,
                                            Length = routeEntity.Length
                                        };
                                        speeds.Add(speed);
                                    }

                                    Console.WriteLine($"Finished processing data for route: {routeEntity.EntityId}");
                                }
                                else
                                {
                                    Console.WriteLine($"Failed to fetch data for route: {routeEntity.EntityId} with URL: {query}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing route: {routeEntity.EntityId} with URL: {query}. Exception: {ex.Message}");
                            }
                            finally
                            {
                                throttle.Release();
                            }
                        }));
                    }

                    await Task.WhenAll(tasks);
                }

                var aggregatedSpeeds = AggregateSpeedsByTimeBin(speeds);

                await hourlySpeedRepository.AddHourlySpeedsAsync(aggregatedSpeeds.ToList());
                Console.WriteLine($"Finished adding aggregated data to Speed Table for routes in group.");
            }
            finally
            {
                semaphore.Release(); // Release the semaphore slot.
            }
        }

        private string ConstructApiQuery(long entityId, long startTimestamp, long endTimestamp)
        {
            return $"{API_URL}?customer_key={CUSTOMER_KEY}&link_id={entityId}&s_timestamp={startTimestamp}&e_timestamp={endTimestamp}&metrics={METRIC}&holidays={INCLUDE_HOLIDAYS}&granularity={GRANULARITY}";
        }

        private IEnumerable<HourlySpeed> AggregateSpeedsByTimeBin(ConcurrentBag<HourlySpeedWithEntityId> speeds)
        {
            return speeds
                .GroupBy(s => s.BinStartTime)
                .Select(g => new HourlySpeedWithEntityId
                {
                    BinStartTime = g.Key,
                    Date = g.First().Date,
                    SegmentId = g.First().SegmentId,
                    SourceId = g.First().SourceId,
                    ConfidenceId = g.First().ConfidenceId,
                    Average = (int)(g.Sum(s => s.Average * s.Length) / g.Sum(s => s.Length)), // Aggregate average speed.
                    Violation = g.Sum(s => s.Violation) // Sum up the violations.
                });
        }
    }
}
