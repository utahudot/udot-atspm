using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.Data.Models.AggregationModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class VolumeService
    {
        private readonly LeftTurnReportService leftTurnReportService;

        public VolumeService(
            LeftTurnReportService leftTurnReportPreCheckService)
        {
            this.leftTurnReportService = leftTurnReportPreCheckService;
        }

        public VolumeResult GetLeftTurnVolumeStats(
            Location location,
            Approach approach,
            VolumeOptions options,
            TimeSpan startTime,
            TimeSpan endTime,
            List<DetectorEventCountAggregation> volumeAggregations)
        {
            var opposingPhase = leftTurnReportService.GetOpposingPhase(approach);
            var leftTurnDetectors = leftTurnReportService.GetLeftTurnDetectors(approach);
            var opposingDetectors = leftTurnReportService.GetOpposingDetectors(opposingPhase, location, new List<MovementTypes> { MovementTypes.T, MovementTypes.TR, MovementTypes.TL });
            var leftTurnDetectorAggregations = new List<DetectorEventCountAggregation>();
            var opposingDetectorAggregations = new List<DetectorEventCountAggregation>();
            foreach (var detector in leftTurnDetectors)
            {
                leftTurnDetectorAggregations.AddRange(volumeAggregations.Where(d => d.DetectorPrimaryId == detector.Id));
            }
            foreach (var detector in opposingDetectors)
            {
                opposingDetectorAggregations.AddRange(volumeAggregations.Where(d => d.DetectorPrimaryId == detector.Id));
            }

            Dictionary<TimeSpan, int> peaks = leftTurnReportService.GetAMPMPeakFlowRate(
                approach,
                options.Start,
                options.End,
                new TimeSpan(6, 0, 0),
                new TimeSpan(9, 0, 0),
                new TimeSpan(15, 0, 0),
                new TimeSpan(18, 0, 0),
                options.DaysOfWeek,
                volumeAggregations
                );

            //Need a test that looks at the volume and the opposing volume
            VolumeResult leftTurnVolumeValue = new VolumeResult();
            leftTurnVolumeValue.OpposingLanes = opposingDetectors.Count;
            double leftTurnVolume = leftTurnDetectorAggregations.Sum(l => l.EventCount);
            double opposingVolume = opposingDetectorAggregations.Sum(o => o.EventCount);
            double crossVolumeProduct = GetCrossProduct(leftTurnVolume, opposingVolume);
            leftTurnVolumeValue.CrossProductValue = crossVolumeProduct;
            leftTurnVolumeValue.LeftTurnVolume = leftTurnVolume;
            leftTurnVolumeValue.OpposingThroughVolume = opposingVolume;
            leftTurnVolumeValue.CrossProductReview = GetCrossProductReview(crossVolumeProduct, leftTurnVolumeValue.OpposingLanes);
            ApproachType approachType = GetApproachType(approach);
            SetDecisionBoundariesReview(leftTurnVolumeValue, leftTurnVolume, opposingVolume, approachType);
            leftTurnVolumeValue.DemandList = GetDemandList(options.Start, options.End, startTime, endTime, options.DaysOfWeek, leftTurnDetectorAggregations);
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
                        demandList.Add(tempstart, leftTurnVolumeAggregation.Where(v => v.Start >= tempstart && v.Start < tempstart.AddMinutes(15)).Sum(v => v.EventCount));
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
            VolumeResult leftTurnVolumeValue,
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

    public enum ApproachType { Permissive, Protected, PermissiveProtected };
}


