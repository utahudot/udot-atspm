using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Analysis.Workflows;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.Atspm.Services.SpeedManagement
{
    public class SpeedManagementHourlyAggregationService
    {
        private IDetectorRepository detectorRepository;
        private IEventLogRepository eventLogRepository;
        private readonly IServiceProvider serviceProvider;

        public SpeedManagementHourlyAggregationService(
            IServiceProvider serviceProvider,
            IDetectorRepository detectorRepository,
            IEventLogRepository eventLogRepository)
        {
            this.eventLogRepository = eventLogRepository;
            this.detectorRepository = detectorRepository;
            this.serviceProvider = serviceProvider;
        }

        public async Task SpeedManagementAggregateHourlySpeeds(DateTime startDate, DateTime endDate, IEnumerable<IGrouping<Guid, SegmentIdEntityId>> groupedSegmentEntitiesForEachSegment)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var detectorRepository = scope.ServiceProvider.GetRequiredService<IDetectorRepository>();
                var locationRepository = scope.ServiceProvider.GetRequiredService<ILocationRepository>();
                var eventLogRepository = scope.ServiceProvider.GetRequiredService<IEventLogRepository>();
                var hourlySpeedRepository = scope.ServiceProvider.GetRequiredService<IHourlySpeedRepository>();
                var timeline = new Timeline<StartEndRange>(startDate, endDate.AddDays(1), TimeSpan.FromHours(1));
                var workflow = new AggregateDetectorEventCountWorkflow(new AggregationWorkflowOptions
                {
                    Timeline = timeline
                });

                await workflow.Initialize();

                var detectorEventCounts = new List<DetectorEventCountAggregation>();
                var collectDetectorEventCounts = new ActionBlock<IEnumerable<DetectorEventCountAggregation>>(
                    aggregations => detectorEventCounts.AddRange(aggregations));
                workflow.Output.LinkTo(collectDetectorEventCounts, new DataflowLinkOptions { PropagateCompletion = true });

                var locationEventsByLocationId = new Dictionary<string, List<CompressedEventLogBase>>();
                var locationsByPrimaryId = new Dictionary<int, Location>();
                var workflowInputsByLocationId = new Dictionary<string, Tuple<Location, IEnumerable<EventLogModelBase>>>();
                var segmentData = new List<(Guid SegmentId, long SourceId, Detector Detector, HashSet<int> DetectorPrimaryIds, List<SpeedEvent> SpeedEvents)>();

                foreach (var segmentEntity in groupedSegmentEntitiesForEachSegment)
                {
                    var segmentEntityList = segmentEntity.ToList();
                    var segmentId = segmentEntity.Key;
                    var sourceId = segmentEntityList.FirstOrDefault()?.SourceId ?? 1;
                    var entityList = segmentEntityList.Select(s => s.SourceEntityId).ToList();
                    var detectorsInSegment = detectorRepository.GetList()
                        .Where(d => entityList.Contains(d.DectectorIdentifier)).ToList();

                    var distinctDetectorsInSegment = detectorsInSegment
                        .DistinctBy(i => i.DectectorIdentifier)
                        .ToList();

                    var locationByDetectorId = distinctDetectorsInSegment
                        .Select(detector => new
                        {
                            Detector = detector,
                            Location = GetLocationForDetector(detector, locationRepository, locationsByPrimaryId)
                        })
                        .Where(detectorLocation => detectorLocation.Location != null)
                        .ToDictionary(detectorLocation => detectorLocation.Detector.Id, detectorLocation => detectorLocation.Location);

                    var detectorsInSegmentGroupedByLocation = distinctDetectorsInSegment
                        .Where(detector => locationByDetectorId.ContainsKey(detector.Id))
                        .GroupBy(detector => locationByDetectorId[detector.Id].LocationIdentifier)
                        .ToList();

                    if (detectorsInSegmentGroupedByLocation.Count() == 0)
                    {
                        continue;
                    }

                    var speedEvents = new List<SpeedEvent>();
                    var detectorPrimaryIds = distinctDetectorsInSegment.Select(d => d.Id).ToHashSet();

                    foreach (var detectorsGroupedByLocation in detectorsInSegmentGroupedByLocation)
                    {
                        var locationId = detectorsGroupedByLocation.Key;

                        if (!locationEventsByLocationId.TryGetValue(locationId, out var locationsEvents))
                        {
                            locationsEvents = await eventLogRepository.GetData(locationId, startDate, endDate.AddDays(1)).ToListAsync();
                            locationEventsByLocationId.Add(locationId, locationsEvents);
                        }

                        var detectorIdentifiers = detectorsGroupedByLocation.Select(d => d.DectectorIdentifier).ToHashSet();
                        speedEvents.AddRange(GetSpeedEventsForDetectors(locationsEvents, detectorIdentifiers));

                        if (!workflowInputsByLocationId.ContainsKey(locationId))
                        {
                            var location = locationByDetectorId[detectorsGroupedByLocation.First().Id];
                            workflowInputsByLocationId.Add(locationId, Tuple.Create<Location, IEnumerable<EventLogModelBase>>(location, GetIndianaEvents(locationsEvents)));
                        }
                    }

                    //Why this works is because we only ever use the speedlimit from the detector for the segment.
                    var detector = detectorsInSegmentGroupedByLocation.FirstOrDefault().FirstOrDefault();
                    segmentData.Add((segmentId, sourceId, detector, detectorPrimaryIds, speedEvents));
                }

                foreach (var input in workflowInputsByLocationId.Values)
                {
                    await workflow.Input.SendAsync(input);
                }

                workflow.Input.Complete();
                await collectDetectorEventCounts.Completion;

                var detectorEventCountsByDetector = detectorEventCounts.ToLookup(d => d.DetectorPrimaryId);
                var hourlySpeeds = segmentData
                    .SelectMany(segment => BuildHourlySpeeds(
                        segment.SegmentId,
                        segment.SourceId,
                        segment.Detector,
                        segment.SpeedEvents,
                        segment.DetectorPrimaryIds.SelectMany(id => detectorEventCountsByDetector[id])))
                    .ToList();

                if (hourlySpeeds.Count > 0)
                {
                    await hourlySpeedRepository.AddHourlySpeedsAsync(hourlySpeeds);
                }
            }
        }

        private static Location GetLocationForDetector(Detector detector, ILocationRepository locationRepository, Dictionary<int, Location> locationsByPrimaryId)
        {
            if (!locationsByPrimaryId.TryGetValue(detector.Approach.LocationId, out var location))
            {
                location = locationRepository.GetVersionByIdDetached(detector.Approach.LocationId);
                locationsByPrimaryId.Add(detector.Approach.LocationId, location);
            }

            return location;
        }

        private static List<EventLogModelBase> GetIndianaEvents(IEnumerable<CompressedEventLogBase> locationEvents)
        {
            return locationEvents
                .Where(l => l.DataType == typeof(IndianaEvent))
                .SelectMany(s => s.Data)
                .ToList();
        }

        private static List<SpeedEvent> GetSpeedEventsForDetectors(IEnumerable<CompressedEventLogBase> locationEvents, HashSet<string> detectorIdentifiers)
        {
            return locationEvents
                .Where(l => l.DataType == typeof(SpeedEvent))
                .SelectMany(s => s.Data)
                .Cast<SpeedEvent>()
                .Where(speed => detectorIdentifiers.Contains(speed.DetectorId))
                .ToList();
        }

        private static IEnumerable<HourlySpeed> BuildHourlySpeeds(
            Guid segmentId,
            long sourceId,
            Detector detector,
            IEnumerable<SpeedEvent> speedEvents,
            IEnumerable<DetectorEventCountAggregation> detectorEventCounts)
        {
            var speedEventsHourlyBin = speedEvents
                .GroupBy(speed => new DateTime(speed.Timestamp.Year, speed.Timestamp.Month, speed.Timestamp.Day, speed.Timestamp.Hour, 0, 0))
                .ToDictionary(group => group.Key, group => group.ToList());

            var detectorEventsHourlyBin = detectorEventCounts
                .GroupBy(detectorEventCount => detectorEventCount.Start)
                .ToDictionary(group => group.Key, group => group.Sum(detectorEventCount => detectorEventCount.EventCount));

            var combinedHourlyKeys = speedEventsHourlyBin.Keys
                .Union(detectorEventsHourlyBin.Keys)
                .OrderBy(key => key)
                .ToList();

            foreach (var timeStamp in combinedHourlyKeys)
            {
                speedEventsHourlyBin.TryGetValue(timeStamp, out var matchingSpeedEvents);
                matchingSpeedEvents ??= new List<SpeedEvent>();

                detectorEventsHourlyBin.TryGetValue(timeStamp, out var flowCount);

                var combinedSpeeds = matchingSpeedEvents
                    .Select(s => (double)s.Mph)
                    .ToList();

                var averageSpeed = combinedSpeeds.Count > 0 ? Math.Round(combinedSpeeds.Average(), 1) : 0.0;
                var fifteenthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 15), 1) : (double?)null;
                var eightyFifthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 85), 1) : (double?)null;
                var ninetyFifthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 95), 1) : (double?)null;
                var ninetyNinthPercentile = combinedSpeeds.Count > 0 ? Math.Round(AtspmMath.Percentile(combinedSpeeds, 99), 1) : (double?)null;
                var minSpeed = combinedSpeeds.Count > 0 ? combinedSpeeds.Min() : (double?)null;
                var maxSpeed = combinedSpeeds.Count > 0 ? combinedSpeeds.Max() : (double?)null;
                var speedFlow = combinedSpeeds.Count;
                var speedLimit = detector.Approach.Mph;
                var violations = speedLimit.HasValue ? combinedSpeeds.Count(speed => speed - speedLimit.Value > 2) : 0;
                var extremeViolations = speedLimit.HasValue ? combinedSpeeds.Count(speed => speed - speedLimit.Value > 7) : 0;
                var percentObserved = flowCount > 0 ? ((double)speedFlow / flowCount) * 100 : 0;

                yield return new HourlySpeed
                {
                    Date = timeStamp.Date,
                    BinStartTime = timeStamp,
                    SegmentId = segmentId,
                    SourceId = sourceId,
                    PercentObserved = percentObserved,
                    Average = averageSpeed,
                    FifteenthSpeed = fifteenthPercentile,
                    EightyFifthSpeed = eightyFifthPercentile,
                    NinetyFifthSpeed = ninetyFifthPercentile,
                    NinetyNinthSpeed = ninetyNinthPercentile,
                    Violation = violations,
                    ExtremeViolation = extremeViolations,
                    Flow = flowCount,
                    MinSpeed = minSpeed,
                    MaxSpeed = maxSpeed,
                    SourceDataAnalyzed = true
                };
            }
        }
    }
}
