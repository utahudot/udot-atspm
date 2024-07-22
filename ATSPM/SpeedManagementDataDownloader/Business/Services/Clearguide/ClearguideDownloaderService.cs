using Newtonsoft.Json.Linq;
using SpeedManagementDataDownloader.Common.Dtos;
using SpeedManagementDataDownloader.Common.EntityTable;
using SpeedManagementDataDownloader.Common.HourlySpeeds;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeedManagementDataDownloader.Business.Services.Clearguide
{
    public class ClearguideDownloaderService : IDataDownloader
    {
        private IRouteEntityTableRepository routeEntityTableRepository;
        private IHourlySpeedRepository hourlySpeedRepository;


        static readonly string API_URL = "https://api.iteris-clearguide.com/v1/link/timeseries/";
        static readonly string CUSTOMER_KEY = "ut";
        static readonly string ROUTE_ID_TYPE = "customer_route_number";
        static readonly string METRIC = "avg_speed";
        static readonly string GRANULARITY = "hour";
        static readonly string INCLUDE_HOLIDAYS = "true";
        static readonly int confidenceId = 4;
        static readonly int sourceId = 3;

        public ClearguideDownloaderService(IRouteEntityTableRepository routeEntityTableRepository, IHourlySpeedRepository hourlySpeedRepository)
        {
            this.routeEntityTableRepository = routeEntityTableRepository;
            this.hourlySpeedRepository = hourlySpeedRepository;
        }

        public async Task Download(DateTime startDate, DateTime endDate)
        {

            List<RouteEntityWithSpeed> routeEntities = await routeEntityTableRepository.GetEntitiesWithSpeedForSourceId(sourceId);
            var routes = routeEntities.GroupBy(r => r.RouteId).ToList();

            long START_TIMESTAMP = ((DateTimeOffset)startDate).ToUnixTimeSeconds();
            long END_TIMESTAMP = ((DateTimeOffset)endDate).ToUnixTimeSeconds() - 3600;

            ClearGuideApiHandler cgApiHandler = new ClearGuideApiHandler("slarson@avenueconsultants.com", "Toad22#24#14");
            int maxParallelTasks = 10;
            SemaphoreSlim semaphore = new SemaphoreSlim(maxParallelTasks);

            List<Task> tasks = new List<Task>();

            foreach (var route in routes)
            {
                await semaphore.WaitAsync(); // Wait until a semaphore slot is available.
                tasks.Add(ProcessRouteAsync(START_TIMESTAMP, END_TIMESTAMP, cgApiHandler, route, semaphore));
            }
            await Task.WhenAll(tasks);

            Console.WriteLine("Finished adding all speed data for all routes");
        }

        private async Task ProcessRouteAsync(long START_TIMESTAMP, long END_TIMESTAMP, ClearGuideApiHandler cgApiHandler, IGrouping<long, RouteEntityWithSpeed> routeWithEntities, SemaphoreSlim semaphore)
        {
            var speeds = new ConcurrentBag<HourlySpeedWithEntityId>();

            try
            {
                foreach (RouteEntityWithSpeed routeEntity in routeWithEntities)
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
                                    RouteId = routeEntity.RouteId,
                                    SourceId = sourceId,
                                    ConfidenceId = confidenceId,
                                    Average = avg,
                                    Violation = routeEntity.SpeedLimit > 0 && avg > routeEntity.SpeedLimit ? (long)(avg - routeEntity.SpeedLimit) : 0
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
                    RouteId = g.First().RouteId,
                    SourceId = g.First().SourceId,
                    ConfidenceId = g.First().ConfidenceId,
                    Average = (int)Math.Round(g.Average(s => s.Average)), // Aggregate average speed.
                    Violation = g.Sum(s => s.Violation) // Sum up the violations.
                });
        }
    }
}
