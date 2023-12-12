using ATSPM.Application.Extensions;
using ATSPM.Data.Models;

namespace ATSPM.ReportApi.TempExtensions
{
    public static class SignalExtensions
    {
        public static string SignalDescription(this Location signal)
        {
            return $"#{signal.LocationIdentifier} - {signal.PrimaryName} & {signal.SecondaryName}";
        }

        public static List<Approach> GetApproachesForSignalThatSupportMetric(this Location signal, int metricTypeID)
        {
            var approachesForMeticType = new List<Approach>();
            foreach (var a in signal.Approaches)
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

        public static List<Detector> GetDetectorsForSignalThatSupportMetric(this Location signal, int metricTypeID)
        {
            var detectorsForMetricType = new List<Detector>();
            foreach (var a in signal.Approaches)
                foreach (var d in a.Detectors)
                    if (d.SupportsMetricType(metricTypeID))
                    {
                        detectorsForMetricType.Add(d);
                        break;
                    }
            //return approachesForMeticType;
            return detectorsForMetricType;
        }

        public static List<Detector> GetDetectorsForSignal(this Location signal)
        {
            var detectors = new List<Detector>();
            if (signal.Approaches != null)
            {
                foreach (var a in signal.Approaches.OrderBy(a => a.ProtectedPhaseNumber))
                    foreach (var d in a.Detectors)
                        detectors.Add(d);
            }

            return detectors.OrderBy(d => d.Id).ToList();
        }
    }
}
