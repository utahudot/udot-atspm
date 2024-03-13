//using ATSPM.Application.Business;
//using ATSPM.Application.Business.LeftTurnGapReport;
//using ATSPM.Application.Repositories.AggregationRepositories;
//using ATSPM.Application.Repositories.ConfigurationRepositories;

//namespace ATSPM.ReportApi.ReportServices
//{
//    /// <summary>
//    /// Left turn gap analysis report service
//    /// </summary>
//    public class GapDurationService : ReportServiceBase<GapDurationOptions, GapDurationResult>
//    {
//        private readonly IApproachRepository approachRepository;
//        private readonly ILocationRepository locationRepository;
//        private readonly IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository;
//        private readonly LeftTurnGapDurationService leftTurnGapDurationService;
//        private readonly ILogger<GapDurationService> logger;

//        /// <inheritdoc/>
//        public GapDurationService(
//            IApproachRepository approachRepository,
//            ILocationRepository locationRepository,
//            IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository,
//            LeftTurnGapDurationService leftTurnGapDurationService,
//            ILogger<GapDurationService> logger)
//        {
//            this.approachRepository = approachRepository;
//            this.locationRepository = locationRepository;
//            this.phaseLeftTurnGapAggregationRepository = phaseLeftTurnGapAggregationRepository;
//            this.leftTurnGapDurationService = leftTurnGapDurationService;
//            this.logger = logger;
//        }

//        /// <inheritdoc/>
//        public override async Task<GapDurationResult> ExecuteAsync(GapDurationOptions options, IProgress<int> progress = null, CancellationToken cancelToken = default)
//        {
//            var location = locationRepository.GetLatestVersionOfLocation(options.LocationIdentifier, options.Start);
//            var approach = location.Approaches.Where(a => a.Id == options.ApproachId).FirstOrDefault();
//            var startTime = new TimeSpan(options.StartHour, options.StartMinute, 0);
//            var endTime = new TimeSpan(options.EndHour, options.EndMinute, 0);
//            var leftTurnAggregations = phaseLeftTurnGapAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, options.Start, options.End).ToList();
//            GapDurationResult gapDurationResult = leftTurnGapDurationService.GetPercentOfGapDuration(approach, options.Start, options.End,
//            startTime, endTime, options.DaysOfWeek, leftTurnAggregations, 1);

//            return gapDurationResult;
//        }
//    }
//}
