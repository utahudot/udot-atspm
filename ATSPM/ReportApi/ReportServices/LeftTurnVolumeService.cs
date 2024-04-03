using ATSPM.Application.Business;
using ATSPM.Application.Business.LeftTurnGapReport;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Left turn gap analysis report service
    /// </summary>
    public class LeftTurnVolumeService : ReportServiceBase<VolumeOptions, VolumeResult>
    {
        private readonly ILocationRepository locationRepository;
        private readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;
        private readonly VolumeService volumeService;
        private readonly ILogger<LeftTurnSplitFailService> logger;

        /// <inheritdoc/>
        public LeftTurnVolumeService(
            ILocationRepository locationRepository,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            VolumeService volumeService,
            ILogger<LeftTurnSplitFailService> logger)
        {
            this.locationRepository = locationRepository;
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            this.volumeService = volumeService;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<VolumeResult> ExecuteAsync(VolumeOptions options, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var location = locationRepository.GetLatestVersionOfLocation(options.LocationIdentifier, options.Start);
            var approach = location.Approaches.Where(a => a.Id == options.ApproachId).FirstOrDefault();
            var startTime = new TimeSpan(options.StartHour, options.StartMinute, 0);
            var endTime = new TimeSpan(options.EndHour, options.EndMinute, 0);
            var detectorEventCountAggregations = detectorEventCountAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, options.Start, options.End).ToList();
            var volumeResult = volumeService.GetLeftTurnVolumeStats(location, approach, options, startTime, endTime, detectorEventCountAggregations);
            return volumeResult;
        }
    }
}
