using ATSPM.Application.Business;
using ATSPM.Application.Business.LeftTurnGapReport;
using ATSPM.Application.Repositories.AggregationRepositories;
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Application.TempExtensions;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;

namespace ATSPM.ReportApi.ReportServices
{
    /// <summary>
    /// Left turn gap analysis report service
    /// </summary>
    public class LeftTurnGapReportDataCheckService : ReportServiceBase<LeftTurnGapDataCheckOptions, LeftTurnGapDataCheckResult>
    {
        private readonly IApproachRepository approachRepository;
        private readonly ILocationRepository locationRepository;
        private readonly IPhaseCycleAggregationRepository phaseCycleAggregationRepository;
        private readonly IPhaseTerminationAggregationRepository phaseTerminationAggregationRepository;
        private readonly IPhasePedAggregationRepository phasePedAggregationRepository;
        private readonly IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository;
        private readonly IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository;
        private readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;
        private readonly LeftTurnReportService leftTurnReportService;
        private readonly ILogger<LeftTurnGapReportDataCheckService> logger;

        /// <inheritdoc/>
        public LeftTurnGapReportDataCheckService(
            IApproachRepository approachRepository,
            ILocationRepository locationRepository,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            IPhaseCycleAggregationRepository phaseCycleAggregationRepository,
            IPhaseTerminationAggregationRepository phaseTerminationAggregationRepository,
            IPhasePedAggregationRepository phasePedAggregationRepository,
            IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository,
            IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository,
            LeftTurnReportService leftTurnReportPreCheckService,
            ILogger<LeftTurnGapReportDataCheckService> logger)
        {
            this.approachRepository = approachRepository;
            this.locationRepository = locationRepository;
            this.phaseCycleAggregationRepository = phaseCycleAggregationRepository;
            this.phaseTerminationAggregationRepository = phaseTerminationAggregationRepository;
            this.phasePedAggregationRepository = phasePedAggregationRepository;
            this.approachSplitFailAggregationRepository = approachSplitFailAggregationRepository;
            this.phaseLeftTurnGapAggregationRepository = phaseLeftTurnGapAggregationRepository;
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            this.leftTurnReportService = leftTurnReportPreCheckService;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public override async Task<LeftTurnGapDataCheckResult> ExecuteAsync(LeftTurnGapDataCheckOptions options, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {

            var amStartTime = new TimeSpan(6, 0, 0);
            var amEndTime = new TimeSpan(9, 0, 0);
            var pmStartTime = new TimeSpan(15, 0, 0);
            var pmEndTime = new TimeSpan(19, 0, 0);

            options.GapOutThreshold = options.GapOutThreshold / 100;
            options.PedestrianThreshold = options.PedestrianThreshold / 100;

            var location = locationRepository.GetLatestVersionOfLocation(options.LocationIdentifier, options.Start);
            var approach = location.Approaches.Where(a => a.Id == options.ApproachId).FirstOrDefault();
            LeftTurnGapDataCheckResult dataCheck = InitializeDataCheckObject(approach, options);
            var detectors = new List<Detector>();
            List<DetectorEventCountAggregation> detectorAggregations;
            List<PhaseCycleAggregation> cycleAggregations;
            List<PhaseTerminationAggregation> terminationAggregations;
            List<ApproachSplitFailAggregation> splitFailAggregations;
            List<PhaseLeftTurnGapAggregation> leftTurnAggregations;
            List<PhasePedAggregation> pedAggregations;

            GetAggregations(
                options,
                out detectorAggregations,
                out cycleAggregations,
                out terminationAggregations,
                out splitFailAggregations,
                out leftTurnAggregations,
                out pedAggregations);

            if (!detectorAggregations.Any())
            {
                dataCheck.InsufficientDetectorEventCount = true;
            }
            if (!cycleAggregations.Any())
            {
                dataCheck.InsufficientCycleAggregation = true;
            }
            if (!terminationAggregations.Any())
            {
                dataCheck.InsufficientPhaseTermination = true;
            }
            if (!splitFailAggregations.Any())
            {
                dataCheck.InsufficientSplitFailAggregations = true;
            }
            if (!leftTurnAggregations.Any())
            {
                dataCheck.InsufficientLeftTurnGapAggregations = true;
            }
            if (!pedAggregations.Any())
            {
                dataCheck.InsufficientPedAggregations = true;
            }
            if (dataCheck.InsufficientDetectorEventCount && dataCheck.InsufficientCycleAggregation && dataCheck.InsufficientPhaseTermination && dataCheck.InsufficientSplitFailAggregations && dataCheck.InsufficientLeftTurnGapAggregations && dataCheck.InsufficientPedAggregations)
            {
                return dataCheck;
            }

            var primaryPhase = approach.ProtectedPhaseNumber == 0 && approach.PermissivePhaseNumber.HasValue ? approach.PermissivePhaseNumber.Value : approach.ProtectedPhaseNumber;
            var opposingPhase = leftTurnReportService.GetOpposingPhase(approach);

            CheckPeakPeriods(
                options,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime,
                approach,
                primaryPhase,
                opposingPhase,
                dataCheck,
                detectorAggregations,
                cycleAggregations,
                terminationAggregations,
                splitFailAggregations,
                leftTurnAggregations);

            if (dataCheck.InsufficientDetectorEventCount || dataCheck.InsufficientCycleAggregation || dataCheck.InsufficientPhaseTermination)
                return dataCheck;


            var flowRate = leftTurnReportService.GetAMPMPeakFlowRate(approach, options.Start, options.End, amStartTime, amEndTime, pmStartTime, pmEndTime, options.DaysOfWeek, detectorAggregations);


            dataCheck.LeftTurnVolumeOk = flowRate.First().Value >= options.VolumePerHourThreshold
            || flowRate.Last().Value >= options.VolumePerHourThreshold;
            var gapOut = leftTurnReportService.GetAMPMPeakGapOut(flowRate, approach, options.Start, options.End, amStartTime, cycleAggregations, terminationAggregations);
            dataCheck.GapOutOk = gapOut.First().Value <= options.GapOutThreshold && gapOut.Last().Value <= options.GapOutThreshold;
            var pedestrianPercentage = leftTurnReportService.GetAMPMPeakPedCyclesPercentages(flowRate, approach, opposingPhase, options.Start, options.End, cycleAggregations, pedAggregations);
            dataCheck.PedCycleOk = pedestrianPercentage.First().Value <= options.PedestrianThreshold && pedestrianPercentage.Last().Value <= options.PedestrianThreshold;

            return dataCheck;
        }

        private void CheckPeakPeriods(
            LeftTurnGapDataCheckOptions options,
            TimeSpan amStartTime,
            TimeSpan amEndTime,
            TimeSpan pmStartTime,
            TimeSpan pmEndTime,
            Approach? approach,
            int primaryPhase,
            int opposingPhase,
            LeftTurnGapDataCheckResult dataCheck,
            List<DetectorEventCountAggregation> detectorAggregations,
            List<PhaseCycleAggregation> cycleAggregations,
            List<PhaseTerminationAggregation> terminationAggregations,
            List<ApproachSplitFailAggregation> splitFailAggregations,
            List<PhaseLeftTurnGapAggregation> leftTurnAggregations)
        {
            var movementTypes = new List<int>() { 3 };
            foreach (var detector in approach.Detectors.Where(d => d.MovementType != null && movementTypes.Contains((int)d.MovementType)).ToList())
            {
                dataCheck.InsufficientDetectorEventCount = CheckDataForPeakPeriods(
                    detectorAggregations.Where(d => d.DetectorPrimaryId == detector.Id).ToList(),
                    options.Start,
                    options.End,
                    amStartTime,
                    amEndTime,
                    pmStartTime,
                    pmEndTime);
                //If any detectors have sufficient data, we can break out of the loop
                if (dataCheck.InsufficientDetectorEventCount == false)
                {
                    break;
                }
            }

            dataCheck.InsufficientCycleAggregation = CheckDataForPeakPeriods(cycleAggregations.Where(a => a.ApproachId == approach.Id), options.Start, options.End, amStartTime, amEndTime, pmStartTime, pmEndTime);
            dataCheck.InsufficientPhaseTermination = CheckDataForPeakPeriods(terminationAggregations.Where(a => a.PhaseNumber == opposingPhase), options.Start, options.End, amStartTime, amEndTime, pmStartTime, pmEndTime);
            dataCheck.InsufficientSplitFailAggregations = CheckDataForPeakPeriods(splitFailAggregations.Where(a => a.PhaseNumber == primaryPhase), options.Start, options.End, amStartTime, amEndTime, pmStartTime, pmEndTime);
            dataCheck.InsufficientLeftTurnGapAggregations = CheckDataForPeakPeriods(leftTurnAggregations.Where(a => a.PhaseNumber == opposingPhase), options.Start, options.End, amStartTime, amEndTime, pmStartTime, pmEndTime);
        }

        private static LeftTurnGapDataCheckResult InitializeDataCheckObject(Approach? approach, LeftTurnGapDataCheckOptions options)
        {
            var dataCheck = new LeftTurnGapDataCheckResult();
            dataCheck.ApproachId = approach.Id;
            dataCheck.locationIdentifier = approach.Location.LocationIdentifier;
            dataCheck.LocationDescription = approach.Location.LocationDescription();
            dataCheck.Start = options.Start;
            dataCheck.End = options.End;
            dataCheck.ApproachDescription = approach.Description;
            dataCheck.GapOutOk = false;
            dataCheck.LeftTurnVolumeOk = false;
            dataCheck.PedCycleOk = false;
            dataCheck.InsufficientDetectorEventCount = false;
            dataCheck.InsufficientCycleAggregation = false;
            dataCheck.InsufficientPhaseTermination = false;
            return dataCheck;
        }

        private void GetAggregations(
            LeftTurnGapDataCheckOptions options,
            out List<DetectorEventCountAggregation> detectorAggregations,
            out List<PhaseCycleAggregation> cycleAggregations,
            out List<PhaseTerminationAggregation> terminationAggregations,
            out List<ApproachSplitFailAggregation> splitFailAggregations,
            out List<PhaseLeftTurnGapAggregation> leftTurnAggregations,
            out List<PhasePedAggregation> pedAggregations)
        {
            try
            {
                cycleAggregations = phaseCycleAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, options.Start, options.End).ToList();
                detectorAggregations = detectorEventCountAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, options.Start, options.End).ToList();
                terminationAggregations = phaseTerminationAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, options.Start, options.End).ToList();
                pedAggregations = phasePedAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, options.Start, options.End).ToList();
                splitFailAggregations = approachSplitFailAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, options.Start, options.End).ToList();
                leftTurnAggregations = phaseLeftTurnGapAggregationRepository.GetAggregationsBetweenDates(options.LocationIdentifier, options.Start, options.End).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting aggregations", ex);
            }
        }

        private bool CheckDataForPeakPeriods(IEnumerable<AggregationModelBase> aggregations, DateTime start, DateTime end, TimeSpan amStartTime, TimeSpan amEndTime, TimeSpan pmStartTime, TimeSpan pmEndTime)
        {
            // Iterate over each day in the range
            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                // Calculate the start and end DateTime for AM and PM periods for the current day
                var amStart = date.Add(amStartTime);
                var amEnd = date.Add(amEndTime);
                var pmStart = date.Add(pmStartTime);
                var pmEnd = date.Add(pmEndTime);

                // Check if there are any aggregations in the AM period for the current day
                bool hasAMAggregations = aggregations.Any(d => d.Start >= amStart && d.Start <= amEnd);

                // Check if there are any aggregations in the PM period for the current day
                bool hasPMAggregations = aggregations.Any(d => d.Start >= pmStart && d.Start <= pmEnd);

                // If there are aggregations in either the AM or PM period, return false
                if (hasAMAggregations || hasPMAggregations)
                {
                    return false;
                }
            }

            // If we went through all days and found no aggregations in the AM or PM periods, return true
            return true;
        }

    }
}
