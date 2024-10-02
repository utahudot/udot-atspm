using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;
using Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices;

namespace SpeedManagementImporter.Services.Clearguide
{
    public class ClearguideFileDownloaderService : IDataDownloader
    {
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly MonthlyAggregationService monthlyAggregationService;
        private readonly ISegmentEntityRepository segmentEntityRepository;
        private readonly ITempDataRepository tempDataRepository;
        private readonly ISegmentRepository segmentRepository;
        static readonly int sourceId = 3;
        static readonly int confidenceId = 4;

        public ClearguideFileDownloaderService(ISegmentEntityRepository segmentEntityRepository, IHourlySpeedRepository hourlySpeedRepository, ITempDataRepository tempDataRepository, ISegmentRepository segmentRepository, MonthlyAggregationService monthlyAggregationService)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.segmentEntityRepository = segmentEntityRepository;
            this.tempDataRepository = tempDataRepository;
            this.segmentRepository = segmentRepository;
            this.monthlyAggregationService = monthlyAggregationService;
        }

        public async Task DeleteSegmentData(List<string> providedSegments)
        {
            List<Segment> segments = new List<Segment>();
            foreach (var segmentId in providedSegments)
            {
                var segment = await segmentRepository.LookupAsync(Guid.Parse(segmentId));
                if (segment == null) continue;
                segments.Add(segment);
            }
            var segmentIds = segments.Select(s => s.Id).ToList();
            if (segmentIds.Count == 1)
            {
                await hourlySpeedRepository.DeleteBySegment(segmentIds.FirstOrDefault());
                await monthlyAggregationService.DeleteMonthlyAggregationBySegmentId(segmentIds.FirstOrDefault());
            }
            else
            {
                await hourlySpeedRepository.DeleteBySegments(segmentIds);
                await monthlyAggregationService.DeleteMonthlyAggregationBySegmentIds(segmentIds);
            }
        }

        public async Task Download(DateTime startDate, DateTime endDate, List<string>? providedSegments)
        {
            //Have options to have segments ids provided in the beginning
            List<Segment> segments = new List<Segment>();
            if (providedSegments != null && providedSegments.Count > 0)
            {
                var segmentIds = providedSegments.Select(s => Guid.Parse(s)).ToList();
                segments = await segmentRepository.GetSegmentsDetailsWithEntity(segmentIds);
            }
            else
            {
                segments = segmentRepository.AllSegmentsWithEntity(sourceId);
            }

            List<SegmentEntityWithSpeed> segmentEntities = segments.SelectMany(segment => segment.Entities.Select(entity => new SegmentEntityWithSpeed
            {
                SpeedLimit = segment.SpeedLimit,
                EntityId = entity.EntityId,
                SourceId = entity.SourceId,
                SegmentId = entity.SegmentId,
                EntityType = entity.EntityType,
                Length = entity.Length,
            })).ToList();

            var routes = segmentEntities.GroupBy(r => r.SegmentId).ToList();

            var entityData = await tempDataRepository.GetHourlyAggregatedDataForAllSegments();

            var settings = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount // or set to a specific value
            };

            // Dataflow block to process each route
            var transformBlock = new TransformBlock<IGrouping<Guid, SegmentEntityWithSpeed>, List<HourlySpeed>>(async route =>
            {
                return DownloadDataToHourlyTable(route, entityData);
            }, settings);

            // Dataflow block to save aggregated speeds to the repository
            var actionBlock = new ActionBlock<List<HourlySpeed>>(async aggregatedSpeeds =>
            {
                if (aggregatedSpeeds.Count > 0)
                {
                    await hourlySpeedRepository.AddHourlySpeedsAsync(aggregatedSpeeds);
                    Console.WriteLine($"Finished adding aggregated data to Speed Table for route {aggregatedSpeeds[0].SegmentId} in group.");
                }
            }, settings);

            // Link the blocks
            transformBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

            // Post each route to the transform block
            foreach (var route in routes)
            {
                transformBlock.Post(route);
            }

            // Signal that no more data will be posted to the transform block
            transformBlock.Complete();

            // Wait for the entire pipeline to finish processing
            await actionBlock.Completion;
        }

        private List<HourlySpeed> DownloadDataToHourlyTable(IGrouping<Guid, SegmentEntityWithSpeed> route, List<TempDataWithDataQuility> entityData)
        {
            var speeds = new List<HourlySpeedWithEntityId>();
            foreach (SegmentEntityWithSpeed routeEntity in route)
            {
                var dataForEntity = entityData.Where(e => e.EntityId == routeEntity.EntityId).ToList();
                foreach (TempDataWithDataQuility entity in dataForEntity)
                {
                    HourlySpeedWithEntityId speed = new()
                    {
                        EntityId = routeEntity.EntityId,
                        Date = entity.BinStartTime,
                        BinStartTime = entity.BinStartTime,
                        SegmentId = routeEntity.SegmentId,
                        SourceId = sourceId,
                        PercentObserved = entity.DataQuality * 100,
                        Average = entity.Average,
                        Violation = null,
                        ExtremeViolation = null,
                        Length = routeEntity.Length
                    };
                    speeds.Add(speed);
                }
            }
            var aggregatedData = AggregateSpeedsByTimeBin(speeds).ToList();
            return aggregatedData;
        }

        private IEnumerable<HourlySpeed> AggregateSpeedsByTimeBin(List<HourlySpeedWithEntityId> speeds)
        {
            return speeds
                .GroupBy(s => s.BinStartTime)
                .Select(g => new HourlySpeedWithEntityId
                {
                    BinStartTime = g.Key,
                    Date = g.First().Date,
                    SegmentId = g.First().SegmentId,
                    SourceId = g.First().SourceId,
                    PercentObserved = g.First().PercentObserved,
                    Average = (g.Sum(s => s.Average * s.Length) / g.Sum(s => s.Length)), // Aggregate average speed.
                    Violation = g.Sum(s => s.Violation), // Sum up the violations.
                    MinSpeed = g.Min(s => s.Average),
                    MaxSpeed = g.Max(s => s.Average)
                });
        }
    }
}
