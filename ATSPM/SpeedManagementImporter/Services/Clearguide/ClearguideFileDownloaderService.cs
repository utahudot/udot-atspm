using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.Common;
using ATSPM.Data.Models.SpeedManagementAggregation;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace SpeedManagementImporter.Services.Clearguide
{
    public class ClearguideFileDownloaderService : IDataDownloader
    {
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly ISegmentEntityRepository segmentEntityRepository;
        static readonly int sourceId = 3;
        static readonly int confidenceId = 4;

        public ClearguideFileDownloaderService(IHourlySpeedRepository hourlySpeedRepository, ISegmentEntityRepository segmentEntityRepository)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.segmentEntityRepository = segmentEntityRepository;
        }

        public async Task Download(DateTime startDate, DateTime endDate)
        {
            List<SegmentEntityWithSpeed> routeEntities = await segmentEntityRepository.GetEntitiesWithSpeedForSourceId(sourceId);
            var routes = routeEntities.GroupBy(r => r.SegmentId).ToList();


            foreach (var route in routes)
            {
                ProcessRouteAsync(route);
            }

            Console.WriteLine("Finished adding all speed data for all routes");
        }

        private async Task ProcessRouteAsync(IGrouping<Guid, SegmentEntityWithSpeed> routeWithEntities)
        {
            var speeds = new ConcurrentBag<HourlySpeedWithEntityId>();
            var tasks = new List<Task>();

                //using (var throttle = new SemaphoreSlim(3)) // Limit the number of concurrent tasks
                //{
                //    foreach (SegmentEntityWithSpeed routeEntity in routeWithEntities)
                //    {

                //        try
                //        {
                //            JObject json = JObject.Parse(response);
                //            JToken speedData = json["series"]["all"]["avg_speed"]["data"];

                //            foreach (var data in speedData)
                //            {
                //                DateTime date = DateTimeOffset.FromUnixTimeSeconds((long)data[0]).DateTime;
                //                int avg = (int)Math.Round((double)data[1]);
                //                HourlySpeedWithEntityId speed = new()
                //                {
                //                    EntityId = routeEntity.EntityId,
                //                    Date = date,
                //                    BinStartTime = date,
                //                    SegmentId = routeEntity.SegmentId,
                //                    SourceId = sourceId,
                //                    ConfidenceId = confidenceId,
                //                    Average = avg,
                //                    Violation = routeEntity.SpeedLimit > 0 && avg > routeEntity.SpeedLimit ? (long)(avg - routeEntity.SpeedLimit) : 0,
                //                    Length = routeEntity.Length
                //                };
                //                speeds.Add(speed);
                //            }

                //            Console.WriteLine($"Finished processing data for route: {routeEntity.EntityId}");
                //        }
                //        catch (Exception ex)
                //        {
                //            Console.WriteLine($"Error processing route: {routeEntity.EntityId} with URL: {query}. Exception: {ex.Message}");
                //        }
                //        finally
                //        {
                //            throttle.Release();
                //        }
                //    }

                //    await Task.WhenAll(tasks);
                //}

                var aggregatedSpeeds = AggregateSpeedsByTimeBin(speeds);

                await hourlySpeedRepository.AddHourlySpeedsAsync(aggregatedSpeeds.ToList());
                Console.WriteLine($"Finished adding aggregated data to Speed Table for routes in group.");
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
