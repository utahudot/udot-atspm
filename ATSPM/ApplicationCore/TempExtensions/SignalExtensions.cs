using ATSPM.Application.Extensions;
using ATSPM.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ATSPM.Application.TempExtensions
{
    public static class LocationExtensions
    {
        public static string LocationDescription(this Location Location)
        {
            return $"#{Location.LocationIdentifier} - {Location.PrimaryName} & {Location.SecondaryName}";
        }

        public static List<Approach> GetApproachesForLocationThatSupportMetric(this Location Location, int metricTypeID)
        {
            var approachesForMeticType = new List<Approach>();
            foreach (var a in Location.Approaches)
                foreach (var d in a.Detectors)
                    if (d.SupportsMetricType(metricTypeID))
                    {
                        approachesForMeticType.Add(a);
                        break;
                    }
            //return approachesForMeticType;
            return approachesForMeticType.OrderBy(a => a.PermissivePhaseNumber).ThenBy(a => a.ProtectedPhaseNumber).ThenBy(a => a.DirectionType.Description)
                .ToList();
        }

        public static List<Detector> GetDetectorsForLocationThatSupportMetric(this Location Location, int metricTypeID)
        {
            var detectorsForMetricType = new List<Detector>();
            foreach (var a in Location.Approaches)
                foreach (var d in a.Detectors)
                    if (d.SupportsMetricType(metricTypeID))
                    {
                        detectorsForMetricType.Add(d);
                        break;
                    }
            //return approachesForMeticType;
            return detectorsForMetricType;
        }

        public static List<Detector> GetDetectorsForLocation(this Location Location)
        {
            var detectors = new List<Detector>();
            if (Location.Approaches != null)
            {
                foreach (var a in Location.Approaches.OrderBy(a => a.ProtectedPhaseNumber))
                    foreach (var d in a.Detectors)
                        detectors.Add(d);
            }

            return detectors.OrderBy(d => d.Id).ToList();
        }
    }
}
