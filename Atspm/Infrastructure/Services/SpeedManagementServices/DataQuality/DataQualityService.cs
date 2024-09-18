using Utah.Udot.Atspm.Business.SpeedManagement.DataQuality;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Enums.SpeedManagement;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace Utah.Udot.ATSPM.Infrastructure.Services.SpeedManagementServices.DataQuality
{
    public class DataQualityService : ReportServiceBase<DataQualityOptions, List<DataQualitySource>>
    {
        private readonly IHourlySpeedRepository hourlySpeedRepository;
        private readonly ISegmentRepository segmentRepository;

        public DataQualityService(IHourlySpeedRepository hourlySpeedRepository, ISegmentRepository segmentRepository)
        {
            this.hourlySpeedRepository = hourlySpeedRepository;
            this.segmentRepository = segmentRepository;
        }

        public override async Task<List<DataQualitySource>> ExecuteAsync(DataQualityOptions parameter, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var thresholdDate = DateTime.UtcNow.AddYears(-2).AddMonths(-1);
            var result = new List<DataQualitySource>();
            if (parameter.StartDate < thresholdDate || parameter.StartDate > parameter.EndDate)
            {
                return null;
            }
            //Get all the sources from the SourceEnum
            var sources = Enum.GetValues(typeof(SourceEnum)).Cast<SourceEnum>().ToList();
            var hourlyAggregations = await hourlySpeedRepository.HourlyAggregationsForSegmentInTimePeriod(parameter.SegmentIds, parameter.StartDate, parameter.EndDate);
            List<Segment> segments = await segmentRepository.GetSegmentsDetail(parameter.SegmentIds);
            foreach (var source in sources)
            {
                var dataSource = new DataQualitySource
                {
                    SourceId = (int)source,
                    Name = source.GetDisplayName(),
                    Segments = segments.Select(segment => new DataQualitySegment
                    {
                        SegmentId = segment.Id,
                        SegmentName = segment.Name,
                        StartingMilePoint = segment.StartMilePoint,
                        EndingMilePoint = segment.EndMilePoint,
                        DataPoints = hourlyAggregations
                            .Where(aggregation => aggregation.SourceId == (int)source && aggregation.SegmentId == segment.Id)
                            .Select(aggregation => new DataPoint<long>
                            (
                                aggregation.BinStartTime,
                                aggregation.ConfidenceId
                            )).ToList()
                    }).ToList()
                };
                result.Add(dataSource);
            }
            return result;
        }
    }
}