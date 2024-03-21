using ATSPM.Application.Business;
using ATSPM.Application.Business.LeftTurnGapReport;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data.Models;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Left turn gap analysis report service
    /// </summary>
    public class LeftTurnGapDurationService : ReportServiceBase<GapDurationOptions, GapDurationResult>
    {
        private readonly ILocationRepository locationRepository;
        private readonly IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository;
        private readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;
        private readonly GapDurationService leftTurnGapDurationService;
        private readonly LeftTurnReportService leftTurnReportService;
        private readonly ILogger<LeftTurnGapDurationService> logger;

        /// <inheritdoc/>
        public LeftTurnGapDurationService(
            ILocationRepository locationRepository,
            IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            GapDurationService leftTurnGapDurationService,
            LeftTurnReportService leftTurnReportService,
            ILogger<LeftTurnGapDurationService> logger)
        {
            this.locationRepository = locationRepository;
            this.phaseLeftTurnGapAggregationRepository = phaseLeftTurnGapAggregationRepository;
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            this.leftTurnGapDurationService = leftTurnGapDurationService;
            this.leftTurnReportService = leftTurnReportService;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<GapDurationResult> ExecuteAsync(GapDurationOptions options, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var location = locationRepository.GetLatestVersionOfLocation(options.LocationIdentifier, options.Start);
            var approach = location.Approaches.Where(a => a.Id == options.ApproachId).FirstOrDefault();
            var startTime = new TimeSpan(options.StartHour, options.StartMinute, 0);
            var endTime = new TimeSpan(options.EndHour, options.EndMinute, 0);
            var opposingPhase = leftTurnReportService.GetOpposingPhase(approach);
            var leftTurnAggregations = phaseLeftTurnGapAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, options.Start, options.End).ToList();
            var leftTurnDetectors = approach.Detectors.Where(d => d.MovementType == Data.Enums.MovementTypes.L && d.DetectionTypes.Select(dt => dt.Id).Contains(Data.Enums.DetectionTypes.LLC));
            int totalActivations = GetLeftTurnActivations(options, startTime, endTime, leftTurnDetectors);
            GapDurationResult gapDurationResult = leftTurnGapDurationService.GetPercentOfGapDuration(approach, options.Start, options.End,
            startTime, endTime, options.DaysOfWeek, location, totalActivations, leftTurnAggregations, opposingPhase);

            return gapDurationResult;
        }

        private int GetLeftTurnActivations(GapDurationOptions options, TimeSpan startTime, TimeSpan endTime, IEnumerable<Detector> leftTurnDetectors)
        {
            int totalActivations = 0;
            for (var tempDate = options.Start.Date; tempDate <= options.End; tempDate = tempDate.AddDays(1))
            {
                var detectorActivations = detectorEventCountAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, tempDate, tempDate.AddDays(1));
                foreach (var detector in leftTurnDetectors)
                {
                    totalActivations += detectorActivations.Where(d => d.Start >= tempDate.Date.Add(startTime) && d.Start <= tempDate.Date.Add(endTime)).Sum(d => d.EventCount);
                }
            }

            return totalActivations;
        }
    }
}
