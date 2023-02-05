using ATSPM.Application.Reports.Business.LeftTurnGapReport;
using ATSPM.Application.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Data.Models;
using ATSPM.Data.Enums;

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

        public LeftTurnReportController(ILogger<LeftTurnReportController> logger,
            IPhasePedAggregationRepository phasePedAggregationRepository,
            IApproachRepository approachRepository,
            IApproachCycleAggregationRepository approachCycleAggregationRepository,
            IPhaseTerminationAggregationRepository phaseTerminationAggregationRepository,
            ISignalRepository signalRepository,
            IDetectorRepository detectorRepository,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository,
            IPhaseLeftTurnGapAggregationRepository phaseLeftTurnGapAggregationRepository
,           IApproachSplitFailAggregationRepository approachSplitFailAggregationRepository,
            LeftTurnVolumeAnalysisService leftTurnVolumeAnalysisService
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
        }

        [HttpPost("/DataCheck")]
        public DataCheckResult Get([FromBody] DataCheckParameters parameters)
        //(string signalId, int approachId, DateTime startDate, DateTime endDate, int volumePerHourThreshold, 
        //double gapOutThreshold, double pedestrianThreshold, int[] daysOfWeek)
        {
            var amStartTime = new TimeSpan(6, 0, 0);
            var amEndTime = new TimeSpan(9, 0, 0);
            var pmStartTime = new TimeSpan(15, 0, 0);
            var pmEndTime = new TimeSpan(19, 0, 0);

            var approach = _approachRepository.Lookup(parameters.ApproachId);
            var dataCheck = new DataCheckResult();
            dataCheck.ApproachDescriptions = approach.Description;
            dataCheck.GapOutOk = false;
            dataCheck.LeftTurnVolumeOk = false;
            dataCheck.PedCycleOk = false;
            dataCheck.InsufficientDetectorEventCount = false;
            dataCheck.InsufficientCycleAggregation = false;
            dataCheck.InsufficientPhaseTermination = false;
            var detectors = new List<Data.Models.Detector>();
            //if(approach.Detectors.Any(d => d.DetectionIDs.Contains(4)))
            var movementTypes = new List<MovementTypes>() { MovementTypes.L };
            foreach (var detector in approach.Detectors.Where(d => movementTypes.Contains(d.MovementTypeId)).ToList())
            {
                if (!_detectorEventCountAggregationRepository.DetectorEventCountAggregationExists(detector.Id, parameters.StartDate.Add(amStartTime), parameters.StartDate.Add(amEndTime)) &&
                    !_detectorEventCountAggregationRepository.DetectorEventCountAggregationExists(detector.Id, parameters.StartDate.Add(pmStartTime), parameters.StartDate.Add(pmEndTime)))
                {
                    dataCheck.InsufficientDetectorEventCount = true;
                    break;
                }
            }
            int opposingPhase = LeftTurnReportPreCheck.GetOpposingPhase(approach);
            if (approach.ProtectedPhaseNumber != 0)
            {
                CheckTablesForData(approach.SignalId, approach.ProtectedPhaseNumber, parameters, amStartTime, amEndTime, pmStartTime, pmEndTime, dataCheck, opposingPhase);
            }
            else if (approach.PermissivePhaseNumber.HasValue)
            {
                CheckTablesForData(approach.SignalId, approach.PermissivePhaseNumber.Value, parameters, amStartTime, amEndTime, pmStartTime, pmEndTime, dataCheck, opposingPhase);
            }

            if (dataCheck.InsufficientDetectorEventCount || dataCheck.InsufficientCycleAggregation || dataCheck.InsufficientPhaseTermination)
                return dataCheck;
            LeftTurnReportPreCheck leftTurnReportPreCheck = new LeftTurnReportPreCheck(_phasePedAggregationRepository, _approachRepository,
                _approachCycleAggregationRepository, _signalRepository, _detectorRepository,
                _detectorEventCountAggregationRepository, _phaseTerminationAggregationRepository);

            var flowRate = LeftTurnReportPreCheck.GetAMPMPeakFlowRate(parameters.SignalId, parameters.ApproachId, parameters.StartDate, parameters.EndDate, amStartTime,
            amEndTime, pmStartTime, pmEndTime, parameters.DaysOfWeek, _signalRepository, _approachRepository, _detectorEventCountAggregationRepository);
            dataCheck.LeftTurnVolumeOk = flowRate.First().Value >= parameters.VolumePerHourThreshold
                || flowRate.Last().Value >= parameters.VolumePerHourThreshold;

            var gapOut = leftTurnReportPreCheck.GetAMPMPeakGapOut(parameters.SignalId, parameters.ApproachId, parameters.StartDate, parameters.EndDate, amStartTime,
            amEndTime, pmStartTime, pmEndTime, parameters.DaysOfWeek);
            dataCheck.GapOutOk = gapOut.First().Value <= parameters.GapOutThreshold && gapOut.Last().Value <= parameters.GapOutThreshold;

            var pedestrianPercentage = leftTurnReportPreCheck.GetAMPMPeakPedCyclesPercentages(parameters.SignalId, parameters.ApproachId, parameters.StartDate, parameters.EndDate, amStartTime,
            amEndTime, pmStartTime, pmEndTime, parameters.DaysOfWeek);
            dataCheck.PedCycleOk = pedestrianPercentage.First().Value <= parameters.PedestrianThreshold && pedestrianPercentage.Last().Value <= parameters.PedestrianThreshold;

            return dataCheck;
        }

        private void CheckTablesForData(string signalId, int phaseNumber, DataCheckParameters parameters, TimeSpan amStartTime, TimeSpan amEndTime, TimeSpan pmStartTime,
            TimeSpan pmEndTime, DataCheckResult dataCheck, int opposingPhase)
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
            var amStartTime = new TimeSpan(6, 0, 0);
            var amEndTime = new TimeSpan(9, 0, 0);
            var pmStartTime = new TimeSpan(15, 0, 0);
            var pmEndTime = new TimeSpan(18, 0, 0);
            PeakHourResult result = new PeakHourResult();
            var peakResult = LeftTurnReportPreCheck.GetAMPMPeakFlowRate(parameters.SignalId, parameters.ApproachId, parameters.StartDate, parameters.EndDate, amStartTime,
            amEndTime, pmStartTime, pmEndTime, parameters.DaysOfWeek, _signalRepository, _approachRepository, _detectorEventCountAggregationRepository);
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
            var startTime = new TimeSpan(parameters.StartHour, parameters.StartMinute, 0);
            var endTime = new TimeSpan(parameters.EndHour, parameters.EndMinute, 0);
            var gapOutAnalysis = new LeftTurnGapDurationAnalysis(_approachRepository, _detectorRepository, _detectorEventCountAggregationRepository,
                _phaseLeftTurnGapAggregationRepository, _signalRepository);
            GapDurationResult gapDurationResult = gapOutAnalysis.GetPercentOfGapDuration(parameters.SignalId, parameters.ApproachId, parameters.StartDate, parameters.EndDate,
                startTime, endTime, parameters.DaysOfWeek);

            return gapDurationResult;
        }

        [HttpPost("/SplitFail")]
        public SplitFailResult GetSplitFailAnalysis(ReportParameters parameters)
        {
            var startTime = new TimeSpan(parameters.StartHour, parameters.StartMinute, 0);
            var endTime = new TimeSpan(parameters.EndHour, parameters.EndMinute, 0);
            var splitFailResult = new SplitFailResult();
            var splitFailAnalysis = new LeftTurnSplitFailAnalysis(_approachRepository, _approachSplitFailAggregationRepository);
            splitFailResult = splitFailAnalysis.GetSplitFailPercent(parameters.ApproachId, parameters.StartDate, parameters.EndDate, startTime, endTime, parameters.DaysOfWeek);

            return splitFailResult;
        }

        [HttpPost("/PedActuation")]
        public PedActuationResult GetPedActuationAnalysis(ReportParameters parameters)
        {
            var startTime = new TimeSpan(parameters.StartHour, parameters.StartMinute, 0);
            var endTime = new TimeSpan(parameters.EndHour, parameters.EndMinute, 0);

            var pedAnalysis = new LeftTurnPedestrianAnalysisService(_signalRepository, _approachRepository, _phasePedAggregationRepository,
                _approachCycleAggregationRepository);
            var pedActuationResult = new PedActuationResult();
            pedActuationResult = pedAnalysis.GetPedestrianPercentage(parameters.SignalId, parameters.ApproachId, parameters.StartDate, parameters.EndDate, startTime, endTime, parameters.DaysOfWeek);

            return pedActuationResult;
        }

        [HttpPost("/Volume")]
        public LeftTurnVolumeValue GetVolumeAnalysis(ReportParameters parameters)
        {
            var startTime = new TimeSpan(parameters.StartHour, parameters.StartMinute, 0);
            var endTime = new TimeSpan(parameters.EndHour, parameters.EndMinute, 0);

            var volumeResult = leftTurnVolumeAnalysisService.GetLeftTurnVolumeStats(parameters.SignalId, parameters.ApproachId, parameters.StartDate, parameters.EndDate,
                startTime, endTime, parameters.DaysOfWeek);

            return volumeResult;
        }
    }
}



