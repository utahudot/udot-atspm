using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Data.Models
{
    //TODO: Remove this when everything is cleaned up
    public partial class Signal
    {
        public override string? ToString()
        {
            return $"{SignalId} - {PrimaryName} {SecondaryName}";
        }

        #region obsolete

        [NotMapped]
        [Obsolete("Use ToString instead", true)]
        public string SignalDescription => SignalId + " - " + PrimaryName + " " + SecondaryName;

        [NotMapped]
        [Obsolete("This method is not currently being used")]
        public List<ControllerEventLog> PlanEvents { get; set; }

        [NotMapped]
        [Obsolete("This should be used locally, not at the model level", true)]
        public List<Signal> VersionList { get; set; }

        [NotMapped]
        [Obsolete("This method is not currently being used")]
        public DateTime FirstDate => Convert.ToDateTime("1/1/2011");

        [NotMapped]
        [Obsolete("This method is not currently being used")]
        public string SelectListName => throw new NotImplementedException();

        [Obsolete("This method is not currently being used")]
        public void SetPlanEvents(DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This should be a private method in the script generator")]
        public string GetMetricTypesString()
        {
            throw new NotImplementedException();
        }

        [Obsolete("This should be a private method in the script generator")]
        public string GetAreasString()
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use extension method GetPhases", true)]
        public List<int> GetPhasesForSignal()
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method is not currently being used")]
        public string GetSignalLocation()
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use extension method GetDetectors", true)]
        public List<Detector> GetDetectorsForSignal()
        {
            throw new NotImplementedException();
        }

        [Obsolete("This should be a local method in WatchDog", true)]
        public List<Detector> GetDetectorsForSignalThatSupportAMetric(int MetricTypeID)
        {
            throw new NotImplementedException();

            //var gdr =
            //    DetectorRepositoryFactory.Create();
            //var detectors = new List<Detector>();
            //foreach (var d in GetDetectorsForSignal())
            //    if (gdr.CheckReportAvialbility(d.DetectorID, MetricTypeID))
            //        detectors.Add(d);
            //return detectors;
        }

        [Obsolete("Use extension method GetDetector", true)]
        public Detector GetDetectorForSignalByChannel(int detectorChannel)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method is not currently being used")]
        public bool CheckReportAvailabilityForSignal(int MetricTypeID)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use extension method GetDetectors", true)]
        public List<Detector> GetDetectorsForSignalThatSupportAMetricByApproachDirection(int MetricTypeID, string Direction)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use extension method GetDetectors", true)]
        public List<Detector> GetDetectorsForSignalThatSupportAMetricByPhaseNumber(int metricTypeId, int phaseNumber)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use extension method GetDetectors", true)]
        public List<Detector> GetDetectorsForSignalByPhaseNumber(int phaseNumber)
        {
            throw new NotImplementedException();
        }

        [Obsolete("this should be a separate operation in ViewModels")]
        public List<MetricType> GetAvailableMetricsVisibleToWebsite()
        {
            throw new NotImplementedException();
        }

        #endregion





























        public List<MetricType> GetAvailableMetrics()
        {
            var repository =
                MetricTypeRepositoryFactory.Create();

            var availableMetrics = repository.GetBasicMetrics();
            foreach (var d in GetDetectorsForSignal())
                foreach (var dt in d.DetectionTypes)
                    if (dt.DetectionTypeID != 1)
                        foreach (var m in dt.MetricTypes)
                            availableMetrics.Add(m);
            return availableMetrics.Distinct().ToList();
        }

        public List<Area> GetAreas()
        {
            var repository =
                AreaRepositoryFactory.Create();

            var areas = repository.GetListOfAreasForSignal(SignalID);
            return areas.ToList();
        }

        private List<MetricType> GetBasicMetrics()
        {
            var repository =
                MetricTypeRepositoryFactory.Create();
            return repository.GetBasicMetrics();
        }

        public bool Equals(Signal signalToCompare)
        {
            return CompareSignalProperties(signalToCompare);
        }

        private bool CompareSignalProperties(Signal signalToCompare)
        {
            if (signalToCompare != null
                && SignalID == signalToCompare.SignalID
                && PrimaryName == signalToCompare.PrimaryName
                && SecondaryName == signalToCompare.SecondaryName
                && IPAddress == signalToCompare.IPAddress
                && Latitude == signalToCompare.Latitude
                && Longitude == signalToCompare.Longitude
                && RegionID == signalToCompare.RegionID
                && ControllerTypeID == signalToCompare.ControllerTypeID
                && Enabled == signalToCompare.Enabled
                && Pedsare1to1 == signalToCompare.Pedsare1to1
                && Approaches.Count() == signalToCompare.Approaches.Count()
            )
                return true;
            return false;
        }

        public static Signal CopyVersion(Signal origVersion)
        {
            var signalRepository = Repositories.SignalsRepositoryFactory.Create();
            var newVersion = new Signal();
            CopyCommonSignalSettings(origVersion, newVersion);
            newVersion.SignalID = origVersion.SignalID;
            newVersion.IPAddress = newVersion.IPAddress;
            newVersion.Start = DateTime.Now;
            newVersion.Note = "Copy of " + origVersion.Note;
            newVersion.Comments = new List<MetricComment>();
            newVersion.VersionList = signalRepository.GetAllVersionsOfSignalBySignalID(newVersion.SignalID);
            return newVersion;
        }

        private static void CopyCommonSignalSettings(Signal origSignal, Signal newSignal)
        {
            newSignal.IPAddress = "10.10.10.10";
            newSignal.PrimaryName = origSignal.PrimaryName;
            newSignal.SecondaryName = origSignal.SecondaryName;
            newSignal.Longitude = origSignal.Longitude;
            newSignal.Latitude = origSignal.Latitude;
            newSignal.RegionID = origSignal.RegionID;
            newSignal.ControllerTypeID = origSignal.ControllerTypeID;
            newSignal.Enabled = origSignal.Enabled;
            newSignal.Pedsare1to1 = origSignal.Pedsare1to1;
            newSignal.Approaches = new List<Approach>();
            newSignal.JurisdictionId = origSignal.JurisdictionId;

            if (origSignal.Approaches != null)
                foreach (var a in origSignal.Approaches)
                {
                    var aForNewSignal =
                        Approach.CopyApproachForSignal(a); //this does the db.Save inside.
                    newSignal.Approaches.Add(aForNewSignal);
                }
        }

        public static Signal CopySignal(Signal origSignal, string newSignalID)
        {
            var newSignal = new Signal();

            CopyCommonSignalSettings(origSignal, newSignal);

            newSignal.SignalID = newSignalID;

            return newSignal;
        }

        public List<Approach> GetApproachesForSignalThatSupportMetric(int metricTypeID)
        {
            var approachesForMeticType = new List<Approach>();
            foreach (var a in Approaches)
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

        public List<DirectionType> GetAvailableDirections()
        {
            var directions = Approaches.Select(a => a.DirectionType).Distinct().ToList();
            return directions;
        }

        internal List<Approach> GetApproachesForAggregation()
        {
            List<Approach> approachesToReturn = new List<Approach>();
            if (Approaches != null)
            {
                var approaches = Approaches.Where(a => a.IsPedestrianPhaseOverlap == false && a.IsPermissivePhaseOverlap == false && a.IsProtectedPhaseOverlap == false);
                foreach (var approach in approaches)
                {
                    if ((!approachesToReturn.Select(a => a.ProtectedPhaseNumber).Contains(approach.ProtectedPhaseNumber) && approach.ProtectedPhaseNumber != 0)
                        || (approach.PermissivePhaseNumber != null && !approachesToReturn.Select(a => a.PermissivePhaseNumber).Contains(approach.PermissivePhaseNumber)))
                    {
                        approachesToReturn.Add(approach);
                    }
                }
            }
            return approachesToReturn;
        }
    }
}
