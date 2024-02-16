using ATSPM.Application.Extensions;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;

namespace ATSPM.ReportApi.TempExtensions
{
    public static class ApproachExtensions
    {
        public static List<Detector> GetDetectorsForMetricType(this Approach approach, int metricTypeID)
        {
            var detectorsForMetricType = new List<Detector>();
            if (approach.Detectors != null)
            {
                foreach (var d in approach.Detectors)
                {
                    if (d.SupportsMetricType(metricTypeID))
                    {
                        detectorsForMetricType.Add(d);
                    }
                }
            }
            return detectorsForMetricType;
        }

        public static List<Detector> GetAllDetectorsOfDetectionType(this Approach approach, DetectionTypes detectionType)
        {
            if (approach.Detectors != null)
            {
                List<Detector> result = new List<Detector>();
                foreach (var d in approach.Detectors)
                    if (d.DetectionTypes != null)
                    {
                        var detectionTypes = d.DetectionTypes.Select(t => t.Id).ToList();
                        if (detectionTypes.Contains(detectionType))
                        {
                            result.Add(d);
                        }
                    }
                return result;
            }
            return new List<Detector>();
        }

        public static List<int> GetCycleEventCodes(this Approach approach, bool getPermissivePhase)
        {
            return getPermissivePhase && approach.IsPermissivePhaseOverlap || !getPermissivePhase && approach.IsProtectedPhaseOverlap
                ? new List<int> { 61, 63, 64, 66 }
                : new List<int> { 1, 8, 9 };
        }

        public static List<int> GetDetailedCycleEventCodes(this Approach approach, bool getPermissivePhase)
        {
            return getPermissivePhase && approach.IsPermissivePhaseOverlap || !getPermissivePhase && approach.IsProtectedPhaseOverlap
                ? new List<int> { 61, 63, 64, 66 }
                : new List<int> { 1, 3, 8, 9, 11 };
        }

        public static List<DataLoggerEnum> GetPedestrianCycleEventCodes(this Approach approach)
        {
            return approach.IsPedestrianPhaseOverlap ? new List<DataLoggerEnum>
            {
                DataLoggerEnum.PedestrianOverlapBeginWalk,
                DataLoggerEnum.PedestrianOverlapBeginClearance,
                DataLoggerEnum.PedestrianCallRegistered,
                DataLoggerEnum.PedDetectorOn
            } : new List<DataLoggerEnum>
            {
                DataLoggerEnum.PedestrianBeginWalk,
                DataLoggerEnum.PedestrianBeginChangeInterval,
                DataLoggerEnum.PedestrianCallRegistered,
                DataLoggerEnum.PedDetectorOn
            };

        }

        public static List<int> GetPedDetectorsFromApproach(this Approach approach)
        {
            return !string.IsNullOrEmpty(approach.PedestrianDetectors) ? approach.PedestrianDetectors.Split(new char[] { ',', '-' }).Select(int.Parse).ToList() : new List<int>() { approach.ProtectedPhaseNumber };
        }


    }
}
