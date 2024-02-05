using ATSPM.Application.Extensions;
using ATSPM.Application.Repositories;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.ReportApi.Business.LeftTurnGapReport
{
    public class LeftTurnVolumeAnalysisService
    {
        private readonly LeftTurnReportPreCheckService leftTurnReportPreCheckService;

        public LeftTurnVolumeAnalysisService(
            LeftTurnReportPreCheckService leftTurnReportPreCheckService)
        {
            this.leftTurnReportPreCheckService = leftTurnReportPreCheckService;
        }

        public LeftTurnVolumeValue GetLeftTurnVolumeStats(
            Approach approach,
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            int[] daysOfWeek,
            List<DetectorEventCountAggregation> leftTurnDetectorEventCountAggregations,
            List<DetectorEventCountAggregation> volumeCountAggregtions,
            List<DetectorEventCountAggregation> leftTurnVolumeAggregation,
            List<DetectorEventCountAggregation> opposingVolumeAggregations,
            List<Detector> opposingDetectors,
            int opposingPhase)
        {

            Dictionary<TimeSpan, int> peaks = leftTurnReportPreCheckService.GetAMPMPeakFlowRate(
                start,
                end,
                new TimeSpan(6, 0, 0),
                new TimeSpan(9, 0, 0),
                new TimeSpan(15, 0, 0),
                new TimeSpan(18, 0, 0),
                daysOfWeek,
                approach,
                leftTurnDetectorEventCountAggregations,
                volumeCountAggregtions
                );
            //Need a test that looks at the volume and the opposing volume
            LeftTurnVolumeValue leftTurnVolumeValue = new LeftTurnVolumeValue();
            var detectors = leftTurnReportPreCheckService.GetLeftTurnDetectors(approach);
            leftTurnVolumeValue.OpposingLanes = opposingDetectors.Count;
            double leftTurnVolume = leftTurnVolumeAggregation.Sum(l => l.EventCount);
            double opposingVolume = opposingVolumeAggregations.Sum(o => o.EventCount);
            double crossVolumeProduct = GetCrossProduct(leftTurnVolume, opposingVolume);
            leftTurnVolumeValue.CrossProductValue = crossVolumeProduct;
            leftTurnVolumeValue.LeftTurnVolume = leftTurnVolume;
            leftTurnVolumeValue.OpposingThroughVolume = opposingVolume;
            leftTurnVolumeValue.CrossProductReview = GetCrossProductReview(crossVolumeProduct, leftTurnVolumeValue.OpposingLanes);
            ApproachType approachType = GetApproachType(approach);
            SetDecisionBoundariesReview(leftTurnVolumeValue, leftTurnVolume, opposingVolume, approachType);
            leftTurnVolumeValue.DemandList = GetDemandList(start, end, startTime, endTime, daysOfWeek, leftTurnVolumeAggregation);
            leftTurnVolumeValue.Direction = approach.DirectionType.Abbreviation + approach.Detectors.FirstOrDefault()?.MovementType;
            leftTurnVolumeValue.OpposingDirection = approach.Location.Approaches.Where(a => a.ProtectedPhaseNumber == opposingPhase).FirstOrDefault()?.DirectionType.Abbreviation;
            return leftTurnVolumeValue;
        }

        public static Dictionary<DateTime, double> GetDemandList(
            DateTime start,
            DateTime end,
            TimeSpan startTime,
            TimeSpan endTime,
            int[] daysOfWeek,
            List<DetectorEventCountAggregation> leftTurnVolumeAggregation)
        {
            var demandList = new Dictionary<DateTime, double>();
            for (var tempDate = start.Date; tempDate <= end; tempDate = tempDate.AddDays(1))
            {
                if (daysOfWeek.Contains((int)tempDate.DayOfWeek))
                {
                    for (var tempstart = tempDate.Date.Add(startTime); tempstart < tempDate.Add(endTime); tempstart = tempstart.AddMinutes(15))
                    {
                        demandList.Add(tempstart, leftTurnVolumeAggregation.Where(v => v.BinStartTime >= tempstart && v.BinStartTime < tempstart.AddMinutes(15)).Sum(v => v.EventCount));
                    }
                }
            }
            return demandList;
        }

        public static double GetCrossProduct(double leftTurnVolume, double opposingVolume)
        {
            return leftTurnVolume * opposingVolume;
        }



        public static bool GetCrossProductReview(double crossVolumeProduct, int opposingLanes)
        {
            if (opposingLanes <= 1)
            {
                return crossVolumeProduct > 50000;
            }
            else
            {
                return crossVolumeProduct > 100000;
            }
        }

        public static void SetDecisionBoundariesReview(
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
                        leftTurnVolumeValue.CalculatedVolumeBoundary = leftTurnVolume * Math.Pow(opposingVolume, .706);
                        leftTurnVolumeValue.DecisionBoundariesReview = 9519 < leftTurnVolumeValue.CalculatedVolumeBoundary;
                    }
                    else if (leftTurnVolumeValue.OpposingLanes > 1)
                    {
                        leftTurnVolumeValue.CalculatedVolumeBoundary = 2 * leftTurnVolume * Math.Pow(opposingVolume, .642);
                        leftTurnVolumeValue.DecisionBoundariesReview = 7974 < leftTurnVolumeValue.CalculatedVolumeBoundary;
                    }
                    break;
                case ApproachType.PermissiveProtected:
                    if (leftTurnVolumeValue.OpposingLanes == 1)
                    {
                        leftTurnVolumeValue.CalculatedVolumeBoundary = leftTurnVolume * Math.Pow(opposingVolume, .500);
                        leftTurnVolumeValue.DecisionBoundariesReview = 4638 < leftTurnVolumeValue.CalculatedVolumeBoundary;
                    }
                    else if (leftTurnVolumeValue.OpposingLanes > 1)
                    {
                        leftTurnVolumeValue.CalculatedVolumeBoundary = 2 * leftTurnVolume * Math.Pow(opposingVolume, .404);
                        leftTurnVolumeValue.DecisionBoundariesReview = 3782 < leftTurnVolumeValue.CalculatedVolumeBoundary;
                    }
                    break;
                case ApproachType.Protected:
                    if (leftTurnVolumeValue.OpposingLanes == 1)
                    {
                        leftTurnVolumeValue.CalculatedVolumeBoundary = leftTurnVolume * Math.Pow(opposingVolume, .425);
                        leftTurnVolumeValue.DecisionBoundariesReview = 3693 < leftTurnVolumeValue.CalculatedVolumeBoundary;
                    }
                    else if (leftTurnVolumeValue.OpposingLanes > 1)
                    {
                        leftTurnVolumeValue.CalculatedVolumeBoundary = 2 * leftTurnVolume * Math.Pow(opposingVolume, .404);
                        leftTurnVolumeValue.DecisionBoundariesReview = 3782 < leftTurnVolumeValue.CalculatedVolumeBoundary;
                    }
                    break;
                default:
                    leftTurnVolumeValue.CalculatedVolumeBoundary = 0;
                    leftTurnVolumeValue.DecisionBoundariesReview = false;
                    break;

            }
        }

        public static ApproachType GetApproachType(Approach approach)
        {
            if (approach.ProtectedPhaseNumber == 0 && approach.PermissivePhaseNumber.HasValue)
                return ApproachType.Permissive;
            else if (approach.ProtectedPhaseNumber != 0 && approach.PermissivePhaseNumber.HasValue)
                return ApproachType.PermissiveProtected;
            else
                return ApproachType.Protected;
        }
    }

    public class LeftTurnVolumeValue
    {
        public int OpposingLanes { get; set; }
        public bool CrossProductReview { get; set; }
        public bool DecisionBoundariesReview { get; set; }
        public double LeftTurnVolume { get; set; }
        public double OpposingThroughVolume { get; set; }
        public double CrossProductValue { get; set; }
        public double CalculatedVolumeBoundary { get; set; }
        public Dictionary<DateTime, double> DemandList { get; set; }
        public string Direction { get; internal set; }
        public string OpposingDirection { get; internal set; }
    }

    public enum ApproachType { Permissive, Protected, PermissiveProtected };
}


