using ATSPM.Application.Business;
using ATSPM.Application.Business.LeftTurnGapReport;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Left turn gap analysis report service
    /// </summary>
    public class LeftTurnPedActuationService : ReportServiceBase<PedActuationOptions, PedActuationResult>
    {
        private readonly ILocationRepository locationRepository;
        private readonly PedActuationService pedActuationService;
        private readonly IPhasePedAggregationRepository phasePedAggregationRepository;
        private readonly IPhaseCycleAggregationRepository phaseCycleAggregationRepository;
        private readonly LeftTurnReportService leftTurnReportService;
        private readonly ILogger<LeftTurnPedActuationService> logger;

        /// <inheritdoc/>
        public LeftTurnPedActuationService(
            ILocationRepository locationRepository,
            PedActuationService pedActuationService,
            IPhasePedAggregationRepository phasePedAggregationRepository,
            IPhaseCycleAggregationRepository phaseCycleAggregationRepository,
            LeftTurnReportService leftTurnReportService,
            ILogger<LeftTurnPedActuationService> logger)
        {
            this.locationRepository = locationRepository;
            this.pedActuationService = pedActuationService;
            this.phasePedAggregationRepository = phasePedAggregationRepository;
            this.phaseCycleAggregationRepository = phaseCycleAggregationRepository;
            this.leftTurnReportService = leftTurnReportService;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<PedActuationResult> ExecuteAsync(PedActuationOptions options, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {
            var location = locationRepository.GetLatestVersionOfLocation(options.LocationIdentifier, options.Start);
            var approach = location.Approaches.Where(a => a.Id == options.ApproachId).FirstOrDefault();
            var startTime = new TimeSpan(options.StartHour, options.StartMinute, 0);
            var endTime = new TimeSpan(options.EndHour, options.EndMinute, 0);
            var pedActuationResult = new PedActuationResult();
            var pedAggregations = phasePedAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, options.Start, options.End).ToList();
            var cycelAggregations = phaseCycleAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, options.Start, options.End).ToList();
            var opposingPhase = leftTurnReportService.GetOpposingPhase(approach);
            pedActuationResult = pedActuationService.GetPedestrianPercentage(location, approach, options, startTime, endTime, pedAggregations, opposingPhase, cycelAggregations);
            return pedActuationResult;
        }
    }
}
