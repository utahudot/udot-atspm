using ATSPM.Application.Business;
using ATSPM.Application.Business.LeftTurnGapReport;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Left turn gap analysis report service
    /// </summary>
    public class LeftTurnPeakHourService : ReportServiceBase<PeakHourOptions, PeakHourResult>
    {
        private readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;
        private readonly ILocationRepository locationRepository;
        private readonly LeftTurnReportService leftTurnReportService;
        private readonly ILogger<LeftTurnPeakHourService> logger;

        /// <inheritdoc/>
        public LeftTurnPeakHourService(
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            ILocationRepository locationRepository,
            LeftTurnReportService leftTurnReportPreCheckService,
            ILogger<LeftTurnPeakHourService> logger)
        {
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            this.locationRepository = locationRepository;
            this.leftTurnReportService = leftTurnReportPreCheckService;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<PeakHourResult> ExecuteAsync(PeakHourOptions options, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var amStartTime = new TimeSpan(6, 0, 0);
            var amEndTime = new TimeSpan(9, 0, 0);
            var pmStartTime = new TimeSpan(15, 0, 0);
            var pmEndTime = new TimeSpan(19, 0, 0);

            var location = locationRepository.GetLatestVersionOfLocation(options.LocationIdentifier, options.Start);
            var approach = location.Approaches.Where(a => a.Id == options.ApproachId).FirstOrDefault();
            var detectorAggregations = detectorEventCountAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, options.Start, options.End).ToList();
            PeakHourResult result = new PeakHourResult();
            var peakResult = leftTurnReportService.GetAMPMPeakFlowRate(
                approach,
                options.Start,
                options.End,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime,
                options.DaysOfWeek,
                detectorAggregations);
            var amPeak = peakResult.First();
            result.AmStartHour = amPeak.Key.Hours;
            result.AmStartMinute = amPeak.Key.Minutes;
            result.AmEndHour = amPeak.Key.Hours + 1;
            result.AmEndMinute = amPeak.Key.Minutes;

            var pmPeak = peakResult.Last();
            result.PmStartHour = pmPeak.Key.Hours;
            result.PmStartMinute = pmPeak.Key.Minutes;
            result.PmEndHour = pmPeak.Key.Hours + 1;
            result.PmEndMinute = pmPeak.Key.Minutes;

            return result;
        }
    }
}
