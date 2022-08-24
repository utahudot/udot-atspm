using ATSPM.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Extensions
{
    public static class ApproachExtensions
    {
        public static bool Equals(this Approach approach, Approach approachToCompare)
        {
            if (approachToCompare != null
                && approach.SignalId == approachToCompare.SignalId
                && approach.ApproachId == approachToCompare.ApproachId
                && approach.DirectionTypeId == approachToCompare.DirectionTypeId
                && approach.Description == approachToCompare.Description
                && approach.Mph == approachToCompare.Mph
                && approach.Detectors == approachToCompare.Detectors
                && approach.ProtectedPhaseNumber == approachToCompare.ProtectedPhaseNumber
                && approach.IsProtectedPhaseOverlap == approachToCompare.IsProtectedPhaseOverlap
                && approach.PermissivePhaseNumber == approachToCompare.PermissivePhaseNumber
                //&& approach.PedestrianPhaseNumber == approachToCompare.PedestrianPhaseNumber
                //&& approach.IsPedestrianPhaseOverlap == approachToCompare.IsPedestrianPhaseOverlap
                //&& approach.PedestrianDetectors == approachToCompare.PedestrianDetectors

            )
                return true;
            return false;
        }

        

        public static Approach CopyApproachCommonProperties(this Approach approach, Approach approachToCopy, bool isVersionOrSignalCopy)
        {
            var newApproach = new Approach();
            newApproach.SignalId = approachToCopy.SignalId;
            newApproach.VersionId = approachToCopy.VersionId;
            newApproach.DirectionTypeId = approachToCopy.DirectionTypeId;
            if (!isVersionOrSignalCopy)
                newApproach.Description = approachToCopy.Description + " Copy";
            else
                newApproach.Description = approachToCopy.Description;
            newApproach.Mph = approachToCopy.Mph;
            newApproach.ProtectedPhaseNumber = approachToCopy.ProtectedPhaseNumber;
            newApproach.IsProtectedPhaseOverlap = approachToCopy.IsProtectedPhaseOverlap;
            newApproach.IsPermissivePhaseOverlap = approachToCopy.IsPermissivePhaseOverlap;
            newApproach.PermissivePhaseNumber = approachToCopy.PermissivePhaseNumber;
            //newApproach.PedestrianPhaseNumber = approachToCopy.PedestrianPhaseNumber;
            //newApproach.IsPedestrianPhaseOverlap = approachToCopy.IsPedestrianPhaseOverlap;
            //newApproach.PedestrianDetectors = approachToCopy.PedestrianDetectors;
            newApproach.Detectors = new List<Detector>();
            return newApproach;
        }        

        private static Approach SetDetChannelWhenMultipleDetectorsExist(Approach newApproach)
        {
            var detChannel = newApproach.Detectors.ToList()[0].DetChannel + 1;
            for (var i = 1; i < newApproach.Detectors.Count; i++)
            {
                newApproach.Detectors.ToList()[i].DetChannel = detChannel;
                newApproach.Detectors.ToList()[i].DetectorId = newApproach.SignalId +
                                                               detChannel;
                detChannel++;
            }
            return newApproach;
        }


        public static List<Detector> GetDetectorsForMetricType(this Approach approach, int metricTypeID)
        {
            var detectorsForMetricType = new List<Detector>();
            if (approach.Detectors != null)
            {
                foreach (var d in approach.Detectors)
                {
                    if (d.DetectorSupportsThisMetric(metricTypeID))
                    {
                        detectorsForMetricType.Add(d);
                    }
                }
            }
            return detectorsForMetricType;
        }
    }
}
