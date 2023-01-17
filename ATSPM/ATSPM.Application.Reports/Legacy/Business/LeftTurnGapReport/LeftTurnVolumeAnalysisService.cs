using System;
using System.Collections.Generic;
using System.Linq;
using ATSPM.Data.Models;
using ATSPM.Application.Repositories;
using ATSPM.Application.Extensions;
using ATSPM.Data.Enums;

namespace Legacy.Common.Business.LeftTurnGapReport
{
    public class LeftTurnVolumeAnalysisService
    {
        private readonly LeftTurnReportPreCheckService leftTurnReportPreCheckService;
        private readonly IDetectorRepository detectorRepository;
        private readonly IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository;

        public LeftTurnVolumeAnalysisService(
            LeftTurnReportPreCheckService leftTurnReportPreCheckService,
            IDetectorRepository detectorRepository,
            IDetectorEventCountAggregationRepository detectorEventCountAggregationRepository
            )
        {
            this.leftTurnReportPreCheckService = leftTurnReportPreCheckService;
            this.detectorRepository = detectorRepository;
            this.detectorEventCountAggregationRepository = detectorEventCountAggregationRepository;
        }

        public LeftTurnVolumeValue GetLeftTurnVolumeStats(
            string signalId,
            DirectionTypes directionType,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            Dictionary<TimeSpan, int> peaks = leftTurnReportPreCheckService.GetAMPMPeakFlowRate(
                signalId,
                directionType,
                start,
                end,
                new TimeSpan(6, 0, 0),
                new TimeSpan(9, 0, 0),
                new TimeSpan(15, 0, 0),
                new TimeSpan(18, 0, 0));
            Dictionary<TimeSpan, int> peaksToUse = new Dictionary<TimeSpan, int>();
            if (peaks.Keys.First() >= startTime && peaks.Keys.First() <= endTime)
                peaksToUse.Add(peaks.First().Key, 0);
            if (peaks.Keys.Last() >= startTime && peaks.Keys.Last() <= endTime)
                peaksToUse.Add(peaks.Last().Key, 0);
            if (peaks.Count == 0)
                throw new NotSupportedException("Peak hours must be included in the selected time range");
            LeftTurnVolumeValue leftTurnVolumeValue = new LeftTurnVolumeValue();
            var detectors = leftTurnReportPreCheckService.GetLeftTurnDetectors(signalId, directionType);
            var approach = leftTurnReportPreCheckService.GetLTPhaseNumberPhaseTypeByDirection(signalId, directionType);
            int opposingPhase = leftTurnReportPreCheckService.GetOpposingPhase(approach);
            var opposingLanes = GetDetectorsByPhase(signalId, opposingPhase);
            leftTurnVolumeValue.OpposingLanes = opposingLanes.Count;
            List<DetectorEventCountAggregation> leftTurnVolumeAggregation = GetDetectorVolumebyDetector(detectors, start, end, startTime, endTime);
            List<DetectorEventCountAggregation> opposingVolumeAggregations = GetDetectorVolumebyDetector(opposingLanes, start, end, startTime, endTime);
            double leftTurnVolume = leftTurnVolumeAggregation.Sum(l => l.EventCount);
            double opposingVolume = opposingVolumeAggregations.Sum(o => o.EventCount);
            double crossVolumeProduct = leftTurnVolume * opposingVolume;
            SetCrossProductReview(leftTurnVolumeValue, crossVolumeProduct);
            ApproachType approachType = GetApproachType(approach);
            SetDecisionBoundariesReview(leftTurnVolumeValue, leftTurnVolume, opposingVolume, approachType);
            return leftTurnVolumeValue;
        }

        private void SetCrossProductReview(LeftTurnVolumeValue leftTurnVolumeValue, double crossVolumeProduct)
        {
            if (leftTurnVolumeValue.OpposingLanes == 1)
            {
                leftTurnVolumeValue.CrossProductReview = crossVolumeProduct > 50000;
            }
            else
            {
                leftTurnVolumeValue.CrossProductReview = crossVolumeProduct > 100000;
            }
        }

        private void SetDecisionBoundariesReview(
            LeftTurnVolumeValue leftTurnVolumeValue,
            double leftTurnVolume,
            double opposingVolume,
            ApproachType approachType)
        {
            switch (approachType)
            {
                case ApproachType.Permissive:
                    if (leftTurnVolumeValue.OpposingLanes == 1)
                    {
                        leftTurnVolumeValue.DecisionBoundariesReview = 9519 < leftTurnVolume * Math.Pow(opposingVolume, .706);
                    }
                    else if (leftTurnVolumeValue.OpposingLanes > 1)
                    {
                        leftTurnVolumeValue.DecisionBoundariesReview = 7974 < 2 * leftTurnVolume * Math.Pow(opposingVolume, .642);
                    }
                    break;
                case ApproachType.PermissiveProtected:
                    if (leftTurnVolumeValue.OpposingLanes == 1)
                    {
                        leftTurnVolumeValue.DecisionBoundariesReview = 4638 < leftTurnVolume * Math.Pow(opposingVolume, .500);
                    }
                    else if (leftTurnVolumeValue.OpposingLanes > 1)
                    {
                        leftTurnVolumeValue.DecisionBoundariesReview = 3782 < 2 * leftTurnVolume * Math.Pow(opposingVolume, .404);
                    }
                    break;
                case ApproachType.Protected:
                    if (leftTurnVolumeValue.OpposingLanes == 1)
                    {
                        leftTurnVolumeValue.DecisionBoundariesReview = 3693 < leftTurnVolume * Math.Pow(opposingVolume, .425);
                    }
                    else if (leftTurnVolumeValue.OpposingLanes > 1)
                    {
                        leftTurnVolumeValue.DecisionBoundariesReview = 3782 < 2 * leftTurnVolume * Math.Pow(opposingVolume, .404);
                    }
                    break;
                default:
                    leftTurnVolumeValue.DecisionBoundariesReview = false;
                    break;

            }
        }

        private ApproachType GetApproachType(Approach approach)
        {
            ApproachType approachType;
            if (approach.ProtectedPhaseNumber == 0 && approach.PermissivePhaseNumber.HasValue)
                approachType = ApproachType.Permissive;
            else if (approach.ProtectedPhaseNumber != 0 && approach.PermissivePhaseNumber.HasValue)
                approachType = ApproachType.PermissiveProtected;
            else
                approachType = ApproachType.Protected;
            return approachType;
        }

        public List<Detector> GetDetectorsByPhase(string signalId, int phase)   
        {
            return detectorRepository.GetDetectorsBySignalID(signalId).Where(d => d.Approach.ProtectedPhaseNumber == phase).ToList();
        }

        public List<DetectorEventCountAggregation> GetDetectorVolumebyDetector(List<Detector> detectors, DateTime start,
            DateTime end, TimeSpan startTime, TimeSpan endTime)
        {
            var detectorAggregations = new List<DetectorEventCountAggregation>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                foreach (var detector in detectors)
                {
                    detectorAggregations.AddRange(detectorEventCountAggregationRepository
                        .GetDetectorEventCountAggregationByDetectorIdAndDateRange(detector.Id, tempDate.Add(startTime), tempDate.Add(endTime)));
                }
            }
            return detectorAggregations;
        }

    }
    
    public class LeftTurnVolumeValue
    {
        public int OpposingLanes { get; set; }
        public bool CrossProductReview { get; set; }
        public bool DecisionBoundariesReview { get; set; }
    }

    enum ApproachType { Permissive, Protected, PermissiveProtected };
}


