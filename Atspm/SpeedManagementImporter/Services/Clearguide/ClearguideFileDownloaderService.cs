﻿using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.Common;
using ATSPM.Data.Models.SpeedManagementAggregation;
using ATSPM.Data.Models.SpeedManagementConfigModels;
using System.Threading.Tasks.Dataflow;

namespace SpeedManagementImporter.Services.Clearguide
{
    public class ClearguideFileDownloaderService : IDataDownloader
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

        public async Task Download(DateTime startDate, DateTime endDate)
        {
            List<SegmentEntityWithSpeed> segmentEntities = await segmentEntityRepository.GetEntitiesWithSpeedForSourceId(sourceId);
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

        private List<HourlySpeed> DownloadDataToHourlyTable(IGrouping<Guid, SegmentEntityWithSpeed> route, List<TempData> entityData)
        {
            var speeds = new List<HourlySpeedWithEntityId>();
            foreach (SegmentEntityWithSpeed routeEntity in route)
            {
                var dataForEntity = entityData.Where(e => e.EntityId == routeEntity.EntityId).ToList();
                foreach (TempData entity in dataForEntity)
                {
                    HourlySpeedWithEntityId speed = new()
                    {
                        EntityId = routeEntity.EntityId,
                        Date = entity.BinStartTime,
                        BinStartTime = entity.BinStartTime,
                        SegmentId = routeEntity.SegmentId,
                        SourceId = sourceId,
                        ConfidenceId = confidenceId,
                        Average = entity.Average,
                        Violation = routeEntity.SpeedLimit > 0 && entity.Average > routeEntity.SpeedLimit ? (long)(entity.Average - routeEntity.SpeedLimit) : 0,
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
                    ConfidenceId = g.First().ConfidenceId,
                    Average = (g.Sum(s => s.Average * s.Length) / g.Sum(s => s.Length)), // Aggregate average speed.
                    Violation = g.Sum(s => s.Violation) // Sum up the violations.
                });
        }
    }
}