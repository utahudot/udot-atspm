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
    public class PeakHourService : ReportServiceBase<LeftTurnGapDataCheckOptions, LeftTurnGapDataCheckResult>
    {
        private readonly IApproachRepository approachRepository;
        private readonly ILocationRepository LocationRepository;
        private readonly IPhaseCycleAggregationRepository phaseCycleAggregationRepository;
        private readonly IPhaseTerminationAggregationRepository phaseTerminationAggregationRepository;
        private readonly IPhasePedAggregationRepository phasePedAggregationRepository;
        private readonly IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository;
        private readonly IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository;
        private readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;
        private readonly LeftTurnReportService leftTurnReportService;
        private readonly ILogger<LeftTurnGapReportDataCheckService> logger;

        /// <inheritdoc/>
        public PeakHourService(
            IApproachRepository approachRepository,
            ILocationRepository LocationRepository,
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
            this.LocationRepository = LocationRepository;
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
        public override async Task<PeakHourResult> ExecuteAsync(PeakHourOptions options, IProgress<int> progress = null, CancellationToken cancelToken = default)
        {

            var amStartTime = new TimeSpan(6, 0, 0);
            var amEndTime = new TimeSpan(9, 0, 0);
            var pmStartTime = new TimeSpan(15, 0, 0);
            var pmEndTime = new TimeSpan(19, 0, 0);
            PeakHourResult result = new PeakHourResult();
            var peakResult = leftTurnReportService.GetAMPMPeakFlowRate(options.LocationIdentifier, options.ApproachId, options.Start, options.End, amStartTime,
            amEndTime, pmStartTime, pmEndTime, options.DaysOfWeek, );
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
            dataCheck.InsufficientPhaseTermination = CheckDataForPeakPeriods(terminationAggregations.Where(a => a.PhaseNumber == primaryPhase), options.Start, options.End, amStartTime, amEndTime, pmStartTime, pmEndTime);
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

                var test = phaseCycleAggregationRepository.GetList().ToList();
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

        private bool CheckDataForPeakPeriods(IEnumerable<AggregationModelBase> aggregations, DateTime start, DateTime end, TimeSpan amStartTime, TimeSpan amEndTime, TimeSpan pmStartTime,
            TimeSpan pmEndTime)
        {
            if (!aggregations.Any(d => d.Start >= start.Add(amStartTime) && d.Start <= start.Add(amEndTime)) &&
                    !aggregations.Any(d => d.Start >= start.Add(pmEndTime) && d.Start <= start.Add(pmEndTime)))
            {
                return true;
            }
            return false;
        }


    }
}
