using ATSPM.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Extensions
{
    public static class SignalExtensions
    {
        public static List<int> GetPhasesForSignal(this Signal signal)
        {
            var phases = new List<int>();
            foreach (var a in signal.Approaches)
            {
                if (a.PermissivePhaseNumber != null)
                    phases.Add(a.PermissivePhaseNumber.Value);
                phases.Add(a.ProtectedPhaseNumber);
            }
            return phases.Select(p => p).Distinct().ToList();
        }

        public static string GetSignalLocation(this Signal signal)
        {
            return signal.PrimaryName + " @ " + signal.SecondaryName;
        }


        public static List<Detector> GetDetectorsForSignal(this Signal signal)
        {
            var detectors = signal.Approaches.SelectMany(a => a.Detectors);
            return detectors.OrderBy(d => d.DetectorId).ToList();
        }

        public static bool Equals(this Signal signal, Signal signalToCompare)
        {
            if (signalToCompare != null
                && signal.SignalId == signalToCompare.SignalId
                && signal.PrimaryName == signalToCompare.PrimaryName
                && signal.SecondaryName == signalToCompare.SecondaryName
                && signal.Ipaddress == signalToCompare.Ipaddress
                && signal.Latitude == signalToCompare.Latitude
                && signal.Longitude == signalToCompare.Longitude
                && signal.RegionId == signalToCompare.RegionId
                && signal.ControllerTypeId == signalToCompare.ControllerTypeId
                && signal.Enabled == signalToCompare.Enabled
                && signal.Pedsare1to1 == signalToCompare.Pedsare1to1
                && signal.Approaches.Count() == signalToCompare.Approaches.Count()
            )
                return true;
            return false;
        }

        public static List<Approach> GetApproachesForSignalThatSupportMetric(this Signal signal,int metricTypeID)
        {
            var approachesForMeticType = new List<Approach>();
            foreach (var a in signal.Approaches)
                foreach (var d in a.Detectors)
                    if (d.DetectorSupportsThisMetric(metricTypeID))
                    {
                        approachesForMeticType.Add(a);
                        break;
                    }
            //return approachesForMeticType;
            return approachesForMeticType.OrderBy(a => a.PermissivePhaseNumber).ThenBy(a => a.ProtectedPhaseNumber).ThenBy(a => a.DirectionType.Description)
                .ToList();
        }

        public static List<DirectionType> GetAvailableDirections(this Signal signal)
        {
            var directions = signal.Approaches.Select(a => a.DirectionType).Distinct().ToList();
            return directions;
        }

        //public static List<Detector> GetDetectorsForSignalThatSupportAMetric(this Signal signal,int MetricTypeID)
        //{
        //    var gdr =
        //        DetectorRepositoryFactory.Create();
        //    var detectors = new List<Detector>();
        //    foreach (var d in GetDetectorsForSignal())
        //        if (gdr.CheckReportAvialbility(d.DetectorID, MetricTypeID))
        //            detectors.Add(d);
        //    return detectors;
        //}
    }
}
