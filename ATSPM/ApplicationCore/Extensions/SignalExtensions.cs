using ATSPM.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ATSPM.Application.Extensions
{
    public static class SignalExtensions
    {
        public static string SignalDescription(this Signal signal)
        {
            return $"{signal.SignalId} - {signal.PrimaryName} {signal.SecondaryName}";
        }

        public static List<Approach> GetApproachesForSignalThatSupportMetric(this Signal signal, int metricTypeID)
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

        public static List<Detector> GetDetectorsForSignal(this Signal signal)
        {
            var detectors = new List<Detector>();
            if (signal.Approaches != null)
            {
                foreach (var a in signal.Approaches.OrderBy(a => a.ProtectedPhaseNumber))
                    foreach (var d in a.Detectors)
                        detectors.Add(d);
            }

            return detectors.OrderBy(d => d.DetectorId).ToList();
        }
    }
}
