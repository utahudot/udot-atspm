using ATSPM.Application.Extensions;
using ATSPM.Application.Reports.Business.LeftTurnGapReport;
using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Reports.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeftTurnReportController : ControllerBase
    {

        private readonly ILogger<LeftTurnReportController> _logger;
        private readonly IPhasePedAggregationRepository _phasePedAggregationRepository;
        private readonly IApproachRepository _approachRepository;
        private readonly IApproachCycleAggregationRepository _approachCycleAggregationRepository;
        private readonly IPhaseTerminationAggregationRepository _phaseTerminationAggregationRepository;
        private readonly ISignalRepository _signalRepository;
        private readonly IDetectorRepository _detectorRepository;
        private readonly IDetectorEventCountAggregationRepository _detectorEventCountAggregationRepository;
        private readonly IPhaseLeftTurnGapAggregationRepository _phaseLeftTurnGapAggregationRepository;
        private readonly IApproachSplitFailAggregationRepository _approachSplitFailAggregationRepository;
        private readonly LeftTurnVolumeAnalysisService leftTurnVolumeAnalysisService;
        private readonly LeftTurnReportPreCheckService leftTurnReportPreCheckService;
        private readonly LeftTurnPedestrianAnalysisService leftTurnPedestrianAnalysisService;
        private readonly LeftTurnGapDurationAnalysis leftTurnGapDurationAnalysis;

        public LeftTurnReportController(ILogger<LeftTurnReportController> logger,
            IPhasePedAggregationRepository phasePedAggregationRepository,
            IApproachRepository approachRepository,
            IApproachCycleAggregationRepository approachCycleAggregationRepository,
            IPhaseTerminationAggregationRepository phaseTerminationAggregationRepository,
            ISignalRepository signalRepository,
            IDetectorRepository detectorRepository,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository,
            IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository,
            LeftTurnVolumeAnalysisService leftTurnVolumeAnalysisService,
            LeftTurnReportPreCheckService leftTurnReportPreCheckService,
            LeftTurnPedestrianAnalysisService leftTurnPedestrianAnalysisService,
            LeftTurnGapDurationAnalysis leftTurnGapDurationAnalysis
            )
        {
            _logger = logger;
            _phasePedAggregationRepository = phasePedAggregationRepository;
            _approachRepository = approachRepository;
            _approachCycleAggregationRepository = approachCycleAggregationRepository;
            _phaseTerminationAggregationRepository = phaseTerminationAggregationRepository;
            _signalRepository = signalRepository;
            _detectorRepository = detectorRepository;
            _detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
            _phaseLeftTurnGapAggregationRepository = phaseLeftTurnGapAggregationRepository;
            _approachSplitFailAggregationRepository = approachSplitFailAggregationRepository;
            this.leftTurnVolumeAnalysisService = leftTurnVolumeAnalysisService;
            this.leftTurnReportPreCheckService = leftTurnReportPreCheckService;
            this.leftTurnPedestrianAnalysisService = leftTurnPedestrianAnalysisService;
            this.leftTurnGapDurationAnalysis = leftTurnGapDurationAnalysis;
        }

        [HttpPost("/DataCheck")]
        public DataCheckResult Get([FromBody] DataCheckParameters parameters)
        {
            var amStartTime = new TimeSpan(6, 0, 0);
            var amEndTime = new TimeSpan(9, 0, 0);
            var pmStartTime = new TimeSpan(15, 0, 0);
            var pmEndTime = new TimeSpan(19, 0, 0);

            var approach = _approachRepository.Lookup(parameters.ApproachId);
            DataCheckResult dataCheck = SetDataCheckDefaults(approach);
            dataCheck.InsufficientDetectorEventCount = CheckDetectorEventCountAggregationsExist(
                parameters,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime,
                approach);
            int opposingPhase = leftTurnReportPreCheckService.GetOpposingPhase(approach);
            int? phaseNumber = null;

            if (approach.ProtectedPhaseNumber != 0)
            {
                phaseNumber = approach.ProtectedPhaseNumber;
            }
            else if (approach.PermissivePhaseNumber.HasValue)
            {
                phaseNumber = approach.PermissivePhaseNumber.Value;
            }

            if (phaseNumber.HasValue)
            {
                CheckTablesForData(
                    approach.SignalId,
                    phaseNumber.Value,
                    parameters,
                    amStartTime,
                    amEndTime,
                    pmStartTime,
                    pmEndTime,
                    dataCheck,
                    opposingPhase);
            }

            if (dataCheck.InsufficientDetectorEventCount || dataCheck.InsufficientCycleAggregation || dataCheck.InsufficientPhaseTermination)
                return dataCheck;
            List<Detector> leftTurndetectors = GetLeftTurnLaneByLaneDetectorsForSignal(approach.Signal);
            if (!leftTurndetectors.Any())
            {
                throw new NotSupportedException("No Left Turn Detectors found");
            }
            List<DetectorEventCountAggregation> leftTurnVolumeAggregations =
                GetDetectorVolumebyDetector(
                    leftTurndetectors,
                    parameters.StartDate,
                    parameters.EndDate,
                    amStartTime,
                    amEndTime,
                    pmStartTime,
                    pmEndTime,
                    parameters.DaysOfWeek);

            List<Detector> detectors = GetAllLaneByLaneDetectorsForSignal(approach.Signal);
            if (!detectors.Any())
            {
                throw new NotSupportedException("No Detectors found");
            }
            List<DetectorEventCountAggregation> volumeAggregations =
                GetDetectorVolumebyDetector(
                    detectors,
                    parameters.StartDate,
                    parameters.EndDate,
                    amStartTime,
                    amEndTime,
                    pmStartTime,
                    pmEndTime,
                    parameters.DaysOfWeek);
            if (!volumeAggregations.Any())
            {
                throw new NotSupportedException("No Detector Activation Aggregations found");
            }

            var flowRate = leftTurnReportPreCheckService.GetAMPMPeakFlowRate(
                parameters.StartDate,
                parameters.EndDate,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime,
                parameters.DaysOfWeek,
                approach,
                leftTurnVolumeAggregations,
                volumeAggregations);
            dataCheck.LeftTurnVolumeOk = flowRate.First().Value >= parameters.VolumePerHourThreshold
                || flowRate.Last().Value >= parameters.VolumePerHourThreshold;

            var phaseTerminationAggregations = Enumerable.Range(0, (parameters.EndDate - parameters.StartDate).Days + 1)
                .Select(offset => parameters.StartDate.AddDays(offset))
                .Select(tempDate => _phaseTerminationAggregationRepository.GetPhaseTerminationsAggregationBySignalIdPhaseNumberAndDateRange(
                    approach.SignalId,
                    approach.PermissivePhaseNumber ?? approach.ProtectedPhaseNumber,
                    tempDate.Add(amStartTime),
                    tempDate.Add(amEndTime)))
                .SelectMany(phaseTerminationsForDate => phaseTerminationsForDate)
                .ToList();

            List<PhaseCycleAggregation> cycleAggregations = GetCycleAggregations(
                parameters,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime,
                approach,
                opposingPhase);

            var gapOut = leftTurnReportPreCheckService.GetAMPMPeakGapOut(
                parameters.StartDate,
                parameters.EndDate,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime,
                parameters.DaysOfWeek,
                approach,
                phaseTerminationAggregations,
                leftTurnVolumeAggregations,
                volumeAggregations,
                cycleAggregations);
            dataCheck.GapOutOk = gapOut.First().Value <= parameters.GapOutThreshold && gapOut.Last().Value <= parameters.GapOutThreshold;

            var phasePedAggregations = Enumerable.Range(0, (parameters.EndDate - parameters.StartDate).Days + 1)
                .Select(offset => parameters.StartDate.AddDays(offset))
                .Select(tempDate => _phasePedAggregationRepository.GetPhasePedsAggregationBySignalIdPhaseNumberAndDateRange(
                    approach.SignalId,
                    approach.PermissivePhaseNumber ?? approach.ProtectedPhaseNumber,
                    tempDate.Add(amStartTime),
                    tempDate.Add(amEndTime)))
                .SelectMany(phasePedsForDate => phasePedsForDate)
                .ToList();

            var pedestrianPercentage = leftTurnReportPreCheckService.GetAMPMPeakPedCyclesPercentages(
                parameters.ApproachId,
                parameters.StartDate,
                parameters.EndDate,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime,
                parameters.DaysOfWeek,
                approach,
                leftTurnVolumeAggregations,
                volumeAggregations,
                cycleAggregations,
                phasePedAggregations);
            dataCheck.PedCycleOk = pedestrianPercentage.First().Value <= parameters.PedestrianThreshold && pedestrianPercentage.Last().Value <= parameters.PedestrianThreshold;

            return dataCheck;
        }

        private bool CheckDetectorEventCountAggregationsExist(
            DataCheckParameters parameters,
            TimeSpan amStartTime,
            TimeSpan amEndTime,
            TimeSpan pmStartTime,
            TimeSpan pmEndTime,
            Approach approach)
        {
            var movementTypes = new List<MovementTypes>() { MovementTypes.L };
            foreach (var detector in approach.Detectors.Where(d => movementTypes.Contains(d.MovementTypeId)).ToList())
            {
                if (!_detectorEventCountAggregationRepository.DetectorEventCountAggregationExists(
                    detector.Id,
                    parameters.StartDate.Add(amStartTime),
                    parameters.StartDate.Add(amEndTime)) &&
                    !_detectorEventCountAggregationRepository.DetectorEventCountAggregationExists(
                    detector.Id,
                    parameters.StartDate.Add(pmStartTime),
                    parameters.StartDate.Add(pmEndTime)))
                {
                    return true;
                }
            }
            return false;
        }

        private static DataCheckResult SetDataCheckDefaults(Approach approach)
        {
            return new DataCheckResult
            {
                ApproachDescriptions = approach.Description,
                GapOutOk = false,
                LeftTurnVolumeOk = false,
                PedCycleOk = false,
                InsufficientDetectorEventCount = false,
                InsufficientCycleAggregation = false,
                InsufficientPhaseTermination = false
            };
        }

        private List<PhaseCycleAggregation> GetCycleAggregations(
            DataCheckParameters parameters,
            TimeSpan amStartTime,
            TimeSpan amEndTime,
            TimeSpan pmStartTime,
            TimeSpan pmEndTime,
            Approach approach,
            int opposingPhase)
        {
            var cycleAggregations = Enumerable.Range(0, (parameters.EndDate - parameters.StartDate).Days + 1)
                             .Select(offset => parameters.StartDate.AddDays(offset))
                             .Select(tempDate => _approachCycleAggregationRepository.GetApproachCyclesAggregationBySignalIdPhaseAndDateRange(
                                 approach.SignalId,
                                 opposingPhase,
                                 tempDate.Add(amStartTime),
                                 tempDate.Add(amEndTime)))
                             .SelectMany(cycleAggregationsForDate => cycleAggregationsForDate)
                             .ToList();
            cycleAggregations.AddRange(Enumerable.Range(0, (parameters.EndDate - parameters.StartDate).Days + 1)
                 .Select(offset => parameters.StartDate.AddDays(offset))
                 .Select(tempDate => _approachCycleAggregationRepository.GetApproachCyclesAggregationBySignalIdPhaseAndDateRange(
                     approach.SignalId,
                     opposingPhase,
                     tempDate.Add(pmStartTime),
                     tempDate.Add(pmEndTime)))
                 .SelectMany(cycleAggregationsForDate => cycleAggregationsForDate)
                 .ToList());
            return cycleAggregations;
        }

        private void CheckTablesForData(
            string signalId,
            int phaseNumber,
            DataCheckParameters parameters,
            TimeSpan amStartTime,
            TimeSpan amEndTime,
            TimeSpan pmStartTime,
            TimeSpan pmEndTime,
            DataCheckResult dataCheck,
            int opposingPhase)
        {
            if (!_approachCycleAggregationRepository.Exists(signalId, phaseNumber, parameters.StartDate.Add(amStartTime), parameters.StartDate.Add(amEndTime)) &&
                                !_approachCycleAggregationRepository.Exists(signalId, phaseNumber, parameters.StartDate.Add(pmStartTime), parameters.StartDate.Add(pmEndTime)))
            {
                dataCheck.InsufficientCycleAggregation = true;
            }
            if (!_phaseTerminationAggregationRepository.Exists(signalId, phaseNumber,
                parameters.StartDate.Add(amStartTime), parameters.StartDate.Add(amEndTime)) &&
                   !_phaseTerminationAggregationRepository.Exists(signalId, phaseNumber,
                parameters.StartDate.Add(pmStartTime), parameters.StartDate.Add(pmEndTime)))
            {
                dataCheck.InsufficientPhaseTermination = true;
            }
            if (!_phasePedAggregationRepository.Exists(signalId, opposingPhase,
                parameters.StartDate.Add(amStartTime), parameters.StartDate.Add(amEndTime)) &&
                   !_phasePedAggregationRepository.Exists(signalId, opposingPhase,
                parameters.StartDate.Add(pmStartTime), parameters.StartDate.Add(pmEndTime)))
            {
                dataCheck.InsufficientPedAggregations = true;
            }
            if (!_approachSplitFailAggregationRepository.Exists(signalId, phaseNumber,
                parameters.StartDate.Add(amStartTime), parameters.StartDate.Add(amEndTime)) &&
                   !_phasePedAggregationRepository.Exists(signalId, phaseNumber,
                parameters.StartDate.Add(pmStartTime), parameters.StartDate.Add(pmEndTime)))
            {
                dataCheck.InsufficientSplitFailAggregations = true;
            }
            if (!_phaseLeftTurnGapAggregationRepository.Exists(signalId, opposingPhase,
                parameters.StartDate.Add(amStartTime), parameters.StartDate.Add(amEndTime)) &&
                   !_phasePedAggregationRepository.Exists(signalId, phaseNumber,
                parameters.StartDate.Add(pmStartTime), parameters.StartDate.Add(pmEndTime)))
            {
                dataCheck.InsufficientLeftTurnGapAggregations = true;
            }
        }

        [HttpPost("/PeakHours")]
        public PeakHourResult GetPeakHours(PeakHourParameters parameters)
        {
            var approach = _approachRepository.Lookup(parameters.ApproachId);
            var amStartTime = new TimeSpan(6, 0, 0);
            var amEndTime = new TimeSpan(9, 0, 0);
            var pmStartTime = new TimeSpan(15, 0, 0);
            var pmEndTime = new TimeSpan(18, 0, 0);
            PeakHourResult result = new PeakHourResult();
            List<Detector> leftTurndetectors = GetLeftTurnLaneByLaneDetectorsForSignal(approach.Signal);
            if (!leftTurndetectors.Any())
            {
                throw new NotSupportedException("No Left Turn Detectors found");
            }
            List<DetectorEventCountAggregation> leftTurnVolumeAggregations =
                GetDetectorVolumebyDetector(
                    leftTurndetectors,
                    parameters.StartDate,
                    parameters.EndDate,
                    amStartTime,
                    amEndTime,
                    pmStartTime,
                    pmEndTime,
                    parameters.DaysOfWeek);
            List<Detector> detectors = GetAllLaneByLaneDetectorsForSignal(approach.Signal);
            if (!detectors.Any())
            {
                throw new NotSupportedException("No Detectors found");
            }
            List<DetectorEventCountAggregation> volumeAggregations =
                GetDetectorVolumebyDetector(
                    detectors,
                    parameters.StartDate,
                    parameters.EndDate,
                    amStartTime,
                    amEndTime,
                    pmStartTime,
                    pmEndTime,
                    parameters.DaysOfWeek);
            if (!volumeAggregations.Any())
            {
                throw new NotSupportedException("No Detector Activation Aggregations found");
            }
            var peakResult = leftTurnReportPreCheckService.GetAMPMPeakFlowRate(
                parameters.StartDate,
                parameters.EndDate,
                amStartTime,
                amEndTime,
                pmStartTime,
                pmEndTime,
                parameters.DaysOfWeek,
                approach,
                leftTurnVolumeAggregations,
                volumeAggregations);
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

        [HttpPost("/GapDuration")]
        public GapDurationResult GetGapDurationAnalysis(ReportParameters parameters)
        {
            var signal = _signalRepository.GetLatestVersionOfSignal(parameters.SignalId, parameters.StartDate);
            var startTime = new TimeSpan(parameters.StartHour, parameters.StartMinute, 0);
            var endTime = new TimeSpan(parameters.EndHour, parameters.EndMinute, 0);
            GapDurationResult gapDurationResult = leftTurnGapDurationAnalysis.GetPercentOfGapDuration(
                parameters.SignalId,
                parameters.ApproachId,
                parameters.StartDate,
                parameters.EndDate,
                startTime,
                endTime,
                parameters.DaysOfWeek,
                signal);

            return gapDurationResult;
        }

        [HttpPost("/SplitFail")]
        public SplitFailResult GetSplitFailAnalysis(ReportParameters parameters)
        {
            var startTime = new TimeSpan(parameters.StartHour, parameters.StartMinute, 0);
            var endTime = new TimeSpan(parameters.EndHour, parameters.EndMinute, 0);
            var splitFailResult = new SplitFailResult();
            var splitFailAnalysis = new LeftTurnSplitFailAnalysis(_approachRepository, _approachSplitFailAggregationRepository);
            splitFailResult = splitFailAnalysis.GetSplitFailPercent(
                parameters.ApproachId,
                parameters.StartDate,
                parameters.EndDate,
                startTime,
                endTime,
                parameters.DaysOfWeek);

            return splitFailResult;
        }

        [HttpPost("/PedActuation")]
        public PedActuationResult GetPedActuationAnalysis(ReportParameters parameters)
        {
            var startTime = new TimeSpan(parameters.StartHour, parameters.StartMinute, 0);
            var endTime = new TimeSpan(parameters.EndHour, parameters.EndMinute, 0);
            var signal = _signalRepository.GetLatestVersionOfSignal(parameters.SignalId, parameters.StartDate);
            var approach = signal.Approaches.Where(a => a.Id == parameters.ApproachId).FirstOrDefault();
            int opposingPhase = leftTurnReportPreCheckService.GetOpposingPhase(approach);
            var pedAggregations = new List<PhasePedAggregation>();
            List<PhaseCycleAggregation> cycleAggregations = new List<PhaseCycleAggregation>();

            for (var tempDate = parameters.StartDate.Date; tempDate <= parameters.EndDate; tempDate = tempDate.AddDays(1))
            {
                if (parameters.DaysOfWeek.Contains((int)tempDate.DayOfWeek))
                {
                    pedAggregations.AddRange(
                        _phasePedAggregationRepository.GetPhasePedsAggregationBySignalIdPhaseNumberAndDateRange(
                            parameters.SignalId,
                            opposingPhase,
                            tempDate.Date.Add(startTime),
                            tempDate.Date.Add(endTime)));
                    cycleAggregations.AddRange(
                       _approachCycleAggregationRepository.GetApproachCyclesAggregationBySignalIdPhaseAndDateRange(
                           parameters.SignalId,
                           opposingPhase,
                           tempDate.Date.Add(startTime),
                           tempDate.Date.Add(endTime)));
                }
            }

            return leftTurnPedestrianAnalysisService.GetPedestrianPercentage(
                signal,
                approach,
                parameters.StartDate,
                parameters.EndDate,
                startTime,
                endTime,
                parameters.DaysOfWeek,
                pedAggregations,
                opposingPhase,
                cycleAggregations);

        }

        [HttpPost("/Volume")]
        public LeftTurnVolumeValue GetVolumeAnalysis(ReportParameters parameters)
        {
            var approach = _approachRepository.Lookup(parameters.ApproachId);
            var startTime = new TimeSpan(parameters.StartHour, parameters.StartMinute, 0);
            var endTime = new TimeSpan(parameters.EndHour, parameters.EndMinute, 0);
            List<Detector> leftTurndetectors = GetLeftTurnLaneByLaneDetectorsForSignal(approach.Signal);
            if (!leftTurndetectors.Any())
            {
                throw new NotSupportedException("No Left Turn Detectors found");
            }
            List<DetectorEventCountAggregation> leftTurnVolumeAggregationsPeakHours =
                GetDetectorVolumebyDetector(
                    leftTurndetectors,
                    parameters.StartDate,
                    parameters.EndDate,
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(9, 0, 0),
                    new TimeSpan(15, 0, 0),
                    new TimeSpan(18, 0, 0),
                    parameters.DaysOfWeek);

            List<Detector> detectors = GetAllLaneByLaneDetectorsForSignal(approach.Signal);
            if (!detectors.Any())
            {
                throw new NotSupportedException("No Detectors found");
            }
            List<DetectorEventCountAggregation> volumeAggregations =
                GetDetectorVolumebyDetector(
                    detectors,
                    parameters.StartDate,
                    parameters.EndDate,
                    new TimeSpan(6, 0, 0),
                    new TimeSpan(9, 0, 0),
                    new TimeSpan(15, 0, 0),
                    new TimeSpan(18, 0, 0),
                    parameters.DaysOfWeek);
            if (!volumeAggregations.Any())
            {
                throw new NotSupportedException("No Detector Activation Aggregations found");
            }

            int opposingPhase = leftTurnReportPreCheckService.GetOpposingPhase(approach);
            List<MovementTypes> movementTypes = new List<MovementTypes>() { MovementTypes.T, MovementTypes.TR, MovementTypes.TL };
            List<Detector> opposingDetectors =
                GetOpposingDetectors(opposingPhase, approach.Signal, movementTypes);

            List<DetectorEventCountAggregation> leftTurnVolumeAggregation =
               GetDetectorVolumebyDetector(detectors, parameters.StartDate, parameters.EndDate, startTime, endTime);
            List<DetectorEventCountAggregation> opposingVolumeAggregations =
                GetDetectorVolumebyDetector(opposingDetectors, parameters.StartDate, parameters.EndDate, startTime, endTime);

            var volumeResult = leftTurnVolumeAnalysisService.GetLeftTurnVolumeStats(
                approach,
                parameters.StartDate,
                parameters.EndDate,
                startTime,
                endTime,
                parameters.DaysOfWeek,
                leftTurnVolumeAggregationsPeakHours,
                volumeAggregations,
                leftTurnVolumeAggregation,
                opposingVolumeAggregations,
                opposingDetectors,
                opposingPhase);

            return volumeResult;
        }

        private List<DetectorEventCountAggregation> GetDetectorVolumebyDetector(
            List<Detector> detectors,
            DateTime startDate,
            DateTime endDate,
            TimeSpan amStartTime,
            TimeSpan amEndTime,
            TimeSpan pmStartTime,
            TimeSpan pmEndTime,
            int[] daysOfWeek)
        {
            var detectorAggregations = new List<DetectorEventCountAggregation>();
            for (var tempDate = startDate.Date; tempDate <= endDate; tempDate = tempDate.AddDays(1))
            {
                if (daysOfWeek.Contains((int)startDate.DayOfWeek))
                    foreach (var detector in detectors)
                    {
                        detectorAggregations.AddRange(_detectorEventCountAggregationRepository
                            .GetDetectorEventCountAggregationByDetectorIdAndDateRange(detector.Id, tempDate.Add(amStartTime), tempDate.Add(amEndTime)));
                        detectorAggregations.AddRange(_detectorEventCountAggregationRepository
                            .GetDetectorEventCountAggregationByDetectorIdAndDateRange(detector.Id, tempDate.Add(pmStartTime), tempDate.Add(pmEndTime)));
                    }
            }
            return detectorAggregations;
        }

        private List<Detector> GetLeftTurnLaneByLaneDetectorsForSignal(
            Signal signal)
        {
            var detectors = signal.GetDetectors();
            List<Detector> detectorsList = new List<Detector>();
            foreach (var detector in detectors)
            {
                var detectionTypeIdList = detector.DetectionTypes.Select(d => d.Id).ToList();
                if (detectionTypeIdList.Contains(DetectionTypes.LLC) && detector.MovementTypeId == MovementTypes.L)
                    detectorsList.Add(detector);
            }
            if (detectorsList.Count > 0)
                return detectorsList;

            foreach (var detector in detectors)
            {
                var detectionTypeIdList = detector.DetectionTypes.Select(d => d.Id).ToList();
                if (detectionTypeIdList.Contains(DetectionTypes.SBP) && detector.MovementTypeId == MovementTypes.L)
                    detectorsList.Add(detector);
            }
            return detectorsList;
        }



        private List<Detector> GetAllLaneByLaneDetectorsForSignal(
            Signal signal)
        {
            var detectors = signal.GetDetectors();
            List<Detector> detectorsList = new List<Detector>();
            foreach (var detector in detectors)
            {
                var detectionTypeIdList = detector.DetectionTypes.Select(d => d.Id).ToList();
                if (detectionTypeIdList.Contains(DetectionTypes.LLC))
                    detectorsList.Add(detector);
            }
            if (detectorsList.Count > 0)
                return detectorsList;

            foreach (var detector in detectors)
            {
                var detectionTypeIdList = detector.DetectionTypes.Select(d => d.Id).ToList();
                if (detectionTypeIdList.Contains(DetectionTypes.SBP))
                    detectorsList.Add(detector);
            }
            return detectorsList;
        }

        public List<Data.Models.DetectorEventCountAggregation> GetDetectorVolumebyDetector(List<Data.Models.Detector> detectors, DateTime start,
           DateTime end, TimeSpan startTime, TimeSpan endTime)
        {
            var detectorAggregations = new List<ATSPM.Data.Models.DetectorEventCountAggregation>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                foreach (var detector in detectors)
                {
                    _detectorEventCountAggregationRepository.GetDetectorEventCountAggregationByDetectorIdAndDateRange(detector.Id, tempDate.Add(startTime), tempDate.Add(endTime));
                }
            }
            return detectorAggregations;
        }

        public static List<Data.Models.Detector> GetOpposingDetectors(
            int opposingPhase,
            Data.Models.Signal signal,
            List<MovementTypes> movementTypes)
        {
            return signal
                            .Approaches
                            .Where(a => a.ProtectedPhaseNumber == opposingPhase)
                            .SelectMany(a => a.Detectors)
                            .Where(d => movementTypes.Contains(d.MovementTypeId) && d.DetectionTypes.First().Id == DetectionTypes.LLC)
                            .ToList();
        }

    }
}



