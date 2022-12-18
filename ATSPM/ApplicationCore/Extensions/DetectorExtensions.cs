using ATSPM.Data.Models;
using System;

namespace ATSPM.Application.Extensions
{
    public static class DetectorExtensions
    {
        public static bool Equals(this Detector detector, Detector graphDetectorToCompare)
        {
            if (graphDetectorToCompare != null
                && detector.DetectorId == graphDetectorToCompare.DetectorId
                && detector.DetChannel == graphDetectorToCompare.DetChannel
                && detector.DistanceFromStopBar == graphDetectorToCompare.DistanceFromStopBar
                && detector.MinSpeedFilter == graphDetectorToCompare.MinSpeedFilter
                && detector.DateAdded == graphDetectorToCompare.DateAdded
                //&& detector.DetectionTypeIds == graphDetectorToCompare.DetectionTypeIDs
                && detector.DecisionPoint == graphDetectorToCompare.DecisionPoint
                && detector.MovementDelay == graphDetectorToCompare.MovementDelay
                && detector.LaneNumber == graphDetectorToCompare.LaneNumber
            )
                return true;
            return false;
        }

        public static double GetOffset(this Detector detector)
        {
            if (detector.DecisionPoint == null)
                detector.DecisionPoint = 0;
            if (detector.Approach.Mph.HasValue && detector.Approach.Mph > 0)
            {
                return Convert.ToDouble((detector.DistanceFromStopBar / (detector.Approach.Mph * 1.467) - detector.DecisionPoint) * 1000);
            }
            else
            {
                return 0;
            }

        }

        public static Signal GetTheSignalThatContainsThisDetector(this Detector detector)
        {
            return detector.Approach.Signal;
        }

        public static bool DetectorSupportsThisMetric(this Detector detector, int metricID)
        {
            var result = false;
            if (detector.DetectionTypeDetectors != null)
            {
                foreach (var dt in detector.DetectionTypeDetectors)
                {
                    foreach (var m in dt.DetectionType.DetectionTypeMetricTypes)
                    {
                        if (m.MetricTypeMetricId == metricID)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public static int GetDefaultPhaseNumberByDirectionsAndMovementTypes(this Detector detector, int directionType, bool isLeft)
        {
            var phaseNumber = 0;

            switch (directionType)
            {
                case 1: //NB
                    if (!isLeft)
                        phaseNumber = 2;
                    else
                        phaseNumber = 1;
                    break;
                case 2: //SB
                    if (!isLeft)
                        phaseNumber = 4;
                    else
                        phaseNumber = 3;
                    break;
                case 3: //EB
                    if (!isLeft)
                        phaseNumber = 8;
                    else
                        phaseNumber = 7;
                    break;
                case 4: //WB
                    if (!isLeft)
                        phaseNumber = 6;
                    else
                        phaseNumber = 5;
                    break;
            }

            return phaseNumber;
        }
    }
}
