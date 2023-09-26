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
        public void GetAvailableMetrics()
        {
            throw new NotImplementedException();
        }

        public override string? ToString()
        {
            return $"{SignalIdentifier} - {PrimaryName} {SecondaryName}";
        }

        #region obsolete

        //[NotMapped]
        //[Obsolete("Use ToString instead", true)]
        //public string SignalDescription => SignalId + " - " + PrimaryName + " " + SecondaryName;

        //[NotMapped]
        //[Obsolete("This method is not currently being used")]
        //public List<ControllerEventLog> PlanEvents { get; set; }

        //[NotMapped]
        //[Obsolete("This should be used locally, not at the model level", true)]
        //public List<Signal> VersionList { get; set; }

        //[NotMapped]
        //[Obsolete("This method is not currently being used")]
        //public DateTime FirstDate => Convert.ToDateTime("1/1/2011");

        //[NotMapped]
        //[Obsolete("This method is not currently being used")]
        //public string SelectListName => throw new NotImplementedException();

        //[Obsolete("This method is not currently being used")]
        //public void SetPlanEvents(DateTime startTime, DateTime endTime)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("This should be a private method in the script generator")]
        //public string GetMetricTypesString()
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("This should be a private method in the script generator")]
        //public string GetAreasString()
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use extension method GetPhases", true)]
        //public List<int> GetPhasesForSignal()
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("This method is not currently being used")]
        //public string GetSignalLocation()
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use extension method GetDetectors", true)]
        //public List<Detector> GetDetectorsForSignal()
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("This should be a local method in WatchDog", true)]
        //public List<Detector> GetDetectorsForSignalThatSupportAMetric(int MetricTypeID)
        //{
        //    throw new NotImplementedException();

        //    //var gdr =
        //    //    DetectorRepositoryFactory.Create();
        //    //var detectors = new List<Detector>();
        //    //foreach (var d in GetDetectorsForSignal())
        //    //    if (gdr.CheckReportAvialbility(d.DetectorID, MetricTypeID))
        //    //        detectors.Add(d);
        //    //return detectors;
        //}

        //[Obsolete("Use extension method GetDetector", true)]
        //public Detector GetDetectorForSignalByChannel(int detectorChannel)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("This method is not currently being used")]
        //public bool CheckReportAvailabilityForSignal(int MetricTypeID)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use extension method GetDetectors", true)]
        //public List<Detector> GetDetectorsForSignalThatSupportAMetricByApproachDirection(int MetricTypeID, string Direction)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use extension method GetDetectors", true)]
        //public List<Detector> GetDetectorsForSignalThatSupportAMetricByPhaseNumber(int metricTypeId, int phaseNumber)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use extension method GetDetectors", true)]
        //public List<Detector> GetDetectorsForSignalByPhaseNumber(int phaseNumber)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("this should be a separate operation in ViewModels")]
        //public List<MetricType> GetAvailableMetricsVisibleToWebsite()
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use extension method GetAvailableMetrics, Union with BasicMetrics in service", true)]
        //public List<MetricType> GetAvailableMetrics()
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Areas are pulled in with Signal")]
        //public List<Area> GetAreas()
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("This method is not currently being used")]
        //private List<MetricType> GetBasicMetrics()
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("This method is not currently being used and should be overriden or use IEqualityComparer")]
        //public bool Equals(Signal signalToCompare)
        //{
        //    throw new NotImplementedException();
        //}

        ////[Obsolete("This method is not currently being used")]
        ////private bool CompareSignalProperties(Signal signalToCompare)
        ////{
        ////    throw new NotImplementedException();
        ////}

        //[Obsolete("This should be a combination of ICloneable and private methods in the few places this is used", true)]
        //public static Signal CopyVersion(Signal origVersion)
        //{
        //    throw new NotImplementedException();
        //}

        ////[Obsolete("This should be a combination of ICloneable and private methods in the few places this is used", true)]
        ////private static void CopyCommonSignalSettings(Signal origSignal, Signal newSignal)
        ////{
        ////    throw new NotImplementedException();
        ////}

        //[Obsolete("This should be a combination of ICloneable and private methods in the few places this is used", true)]
        //public static Signal CopySignal(Signal origSignal, string newSignalID)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use extension method GetApproaches", true)]
        //public List<Approach> GetApproachesForSignalThatSupportMetric(int metricTypeID)
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("Use extension method GetAvailableDirections", true)]
        //public List<DirectionType> GetAvailableDirections()
        //{
        //    throw new NotImplementedException();
        //}

        //[Obsolete("This method is not currently being used")]
        //internal List<Approach> GetApproachesForAggregation()
        //{
        //    throw new NotImplementedException();

        //    //List<Approach> approachesToReturn = new List<Approach>();
        //    //if (Approaches != null)
        //    //{
        //    //    var approaches = Approaches.Where(a => a.IsPedestrianPhaseOverlap == false && a.IsPermissivePhaseOverlap == false && a.IsProtectedPhaseOverlap == false);
        //    //    foreach (var approach in approaches)
        //    //    {
        //    //        if ((!approachesToReturn.Select(a => a.ProtectedPhaseNumber).Contains(approach.ProtectedPhaseNumber) && approach.ProtectedPhaseNumber != 0)
        //    //            || (approach.PermissivePhaseNumber != null && !approachesToReturn.Select(a => a.PermissivePhaseNumber).Contains(approach.PermissivePhaseNumber)))
        //    //        {
        //    //            approachesToReturn.Add(approach);
        //    //        }
        //    //    }
        //    //}
        //    //return approachesToReturn;
        //}

        #endregion
    }
}
