using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Analysis.Workflows;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;

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
                var detectorRepository = scope.ServiceProvider.GetService<IDetectorRepository>();
                var workflow = new DetectorHourlySpeedAggregationWorkflow(scope.ServiceProvider.GetService<IServiceScopeFactory>());
                List<Tuple<Guid, Detector, List<SpeedEvent>, List<IndianaEvent>>> allEvents = new();
                foreach (var segmentEntity in groupedSegmentEntitiesForEachSegment)
                {
                    var segmentId = segmentEntity.Key;
                    var detectorsInSegmentGroupedByLocation = new List<IGrouping<string, Detector>>();
                    using (var detectorScope = serviceProvider.CreateScope())
                    {
                        var entityList = segmentEntity.Select(s => s.SourceEntityId).ToList();
                        var detectors = detectorRepository.GetList()
                        .Where(d => entityList.Contains(d.DectectorIdentifier)).ToList();
                        detectorsInSegmentGroupedByLocation = detectors.DistinctBy(i => i.DectectorIdentifier).ToList()
                        .GroupBy(g => g.Approach.Location.LocationIdentifier).ToList();
                    }
                    if (detectorsInSegmentGroupedByLocation.Count() == 0)
                    {
                        continue;
                    }
                    List<IndianaEvent> indianaEvents = new();
                    List<SpeedEvent> speedEvents = new();
                    foreach (var detectorsGroupedByLocation in detectorsInSegmentGroupedByLocation)
                    {
                        var locationId = detectorsGroupedByLocation.Key;
                        var locationsEvents = new List<CompressedEventLogBase>();
                        using (var eventScope = serviceProvider.CreateScope())
                        {
                            var eventLogRepository = eventScope.ServiceProvider.GetService<IEventLogRepository>();
                            locationsEvents = eventLogRepository.GetArchivedEvents(locationId, startDate, endDate.AddDays(1)).ToList();
                        }

                        speedEvents.AddRange(locationsEvents.Where(l => l.DataType == typeof(SpeedEvent)).SelectMany(s => s.Data).Cast<SpeedEvent>().ToList());
                        indianaEvents.AddRange(locationsEvents.Where(l => l.DataType == typeof(IndianaEvent)).SelectMany(s => s.Data).Cast<IndianaEvent>()
                            .Where(log => log.EventCode == 82 && detectorsGroupedByLocation.Select(s => s.DetectorChannel).Contains(log.EventParam))
                            .ToList());
                    }
                    //Why this works is because we only ever use the speedlimit from the detector for the segment.
                    var detector = detectorsInSegmentGroupedByLocation.FirstOrDefault().FirstOrDefault();

                    var segmentEventsTuple = Tuple.Create(segmentId, detector, speedEvents, indianaEvents);
                    allEvents.Add(segmentEventsTuple);
                }

                await workflow.Input.SendAsync(allEvents);
                workflow.Input.Complete();
                await Task.WhenAll(workflow.Steps.Select(s => s.Completion));
            }
        }

    }
}
