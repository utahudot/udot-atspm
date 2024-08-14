using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.Common;
using ATSPM.Data.Models.SpeedManagementAggregation;
using CsvHelper;
using System.Collections.Concurrent;
using System.Globalization;

namespace SpeedManagementImporter.Services.Clearguide
{
    public class ClearguideFileDownloaderService : IFileDataDownloader
    {
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly ISegmentEntityRepository segmentEntityRepository;
        private readonly ITempDataRepository tempDataRepository;
        static readonly int sourceId = 3;
        static readonly int confidenceId = 4;

        public ClearguideFileDownloaderService(ISegmentEntityRepository segmentEntityRepository, IHourlySpeedRepository hourlySpeedRepository, ITempDataRepository tempDataRepository)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.segmentEntityRepository = segmentEntityRepository;
            this.tempDataRepository = tempDataRepository;
        }

        public Task Download(DateTime startDate, DateTime endDate)
        {

            throw new NotImplementedException();
        }

        public async Task Download(string filePath)
        {
            List<SegmentEntityWithSpeed> segmentEntities = await segmentEntityRepository.GetEntitiesWithSpeedForSourceId(sourceId);
            var routes = segmentEntities.GroupBy(r => r.SegmentId).ToList();

            int maxParallelTasks = 1;
            var semaphore = new SemaphoreSlim(maxParallelTasks);

            var dataBySegmentId = new Dictionary<long, List<HourlySpeedWithEntityId>>();

            ReadThroughFile(filePath, segmentEntities, dataBySegmentId);

            List<Task> tasks = new List<Task>();

            // Process each route using the data loaded from the file
            foreach (var route in routes)
            {
                await semaphore.WaitAsync(); // Wait until a semaphore slot is available.
                tasks.Add(ProcessRouteAsync(route, filePath, semaphore));
            }
            await Task.WhenAll(tasks);

            Console.WriteLine("Finished adding all speed data for all routes");
        }

        private void ReadThroughFile(string filePath, List<SegmentEntityWithSpeed> segmentEntities, Dictionary<long, List<HourlySpeedWithEntityId>> dataBySegmentId)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    // Use a custom type converter to handle scientific notation
                    var sourceIdString = csv.GetField("source_id");
                    if (long.TryParse(sourceIdString, NumberStyles.Any, CultureInfo.InvariantCulture, out long sourceId))
                    {
                        var routeEntity = segmentEntities.FirstOrDefault(e => e.EntityId == sourceId);
                        if (routeEntity != null)
                        {
                            // Collect the data
                            DateTime localTimestamp = csv.GetField<DateTime>("local_timestamp");
                            var avgSpeedMph = csv.GetField<double>("avg_speed_mph");

                            HourlySpeedWithEntityId speed = new()
                            {
                                EntityId = routeEntity.EntityId,
                                Date = localTimestamp,
                                BinStartTime = localTimestamp,
                                SegmentId = routeEntity.SegmentId,
                                SourceId = sourceId,
                                ConfidenceId = confidenceId,
                                Average = avgSpeedMph,
                                Violation = routeEntity.SpeedLimit > 0 && avgSpeedMph > routeEntity.SpeedLimit ? (long)(avgSpeedMph - routeEntity.SpeedLimit) : 0,
                                Length = routeEntity.Length
                            };

                            // Add the data to the dictionary
                            if (!dataBySegmentId.ContainsKey(sourceId))
                            {
                                dataBySegmentId[sourceId] = new List<HourlySpeedWithEntityId>();
                                Console.WriteLine($"Adding Data to List First {routeEntity.EntityId}");
                            }
                            Console.WriteLine($"Adding Data to List {routeEntity.EntityId} with Count: {dataBySegmentId[sourceId].Count}");
                            dataBySegmentId[sourceId].Add(speed);
                        }
                    }
                }
            }
        }

        private async Task ProcessRouteAsync(IGrouping<Guid, SegmentEntityWithSpeed> routeWithEntities, string filePath, SemaphoreSlim semaphore)
        {
            //    var speeds = new ConcurrentBag<HourlySpeedWithEntityId>();
            //    var tasks = new List<Task>();

            //    try
            //    {
            //        // Load data from the file and process it based on routeWithEntities
            //        using (var reader = new StreamReader(filePath))
            //        using (var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture))
            //        {
            //            // Assuming the file has a header
            //            csv.Read();
            //            csv.ReadHeader();

            //            while (csv.Read())
            //            {
            //                var entityId = csv.GetField<long>("source_id");
            //                var routeEntity = routeWithEntities.FirstOrDefault(e => e.EntityId == entityId);
            //                if (routeEntity != null)
            //                {
            //                    // Process the data for the matching segmentId
            //                    tasks.Add(Task.Run(() =>
            //                    {
            //                        DateTime localTimestamp = csv.GetField<DateTime>("local_timestamp");
            //                        int avgSpeedMph = csv.GetField<int>("avg_speed_mph");

            //                        HourlySpeedWithEntityId speed = new()
            //                        {
            //                            EntityId = routeEntity.EntityId,
            //                            Date = localTimestamp,
            //                            BinStartTime = localTimestamp,
            //                            SegmentId = routeEntity.SegmentId,
            //                            SourceId = sourceId,
            //                            ConfidenceId = confidenceId,
            //                            Average = avgSpeedMph,
            //                            Violation = routeEntity.SpeedLimit > 0 && avgSpeedMph > routeEntity.SpeedLimit ? (long)(avgSpeedMph - routeEntity.SpeedLimit) : 0,
            //                            Length = routeEntity.Length
            //                        };
            //                        speeds.Add(speed);
            //                    }));
            //                }
            //            }
            //        }

            //        await Task.WhenAll(tasks);

            //        var aggregatedSpeeds = AggregateSpeedsByTimeBin(speeds);

            //        await hourlySpeedRepository.AddHourlySpeedsAsync(aggregatedSpeeds.ToList());
            //        Console.WriteLine($"Finished adding aggregated data to Speed Table for routes in group.");
            //    }
            //    finally
            //    {
            //        semaphore.Release(); // Release the semaphore slot.
            //    }
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
