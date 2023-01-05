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
    public partial class Detector
    {
        #region Obsolete

        //[NotMapped]
        //[Obsolete("this is referencing MVC 'HtmlListInfo' and should be in the model", true)]
        //public HtmlListInfo htmlListInfo;

        [NotMapped]
        [Obsolete("DetectionTypes are already part of model", true)]
        public List<DetectionType> AllDetectionTypes { get; set; }

        [NotMapped]
        [Obsolete("Get all DetectionHardwareTypes from the enum", true)]
        public List<DetectionHardware> AllHardwareTypes { get; set; }

        [NotMapped]
        [Obsolete("This method is not currently being used")]
        public string[] DetectionIDs { get; set; }

        [Obsolete("this is only used in SignalController, model should not be modified for this", true)]
        [NotMapped]
        public string Index { get; set; }

        [Obsolete("This method is not currently being used")]
        public List<ControllerEventLog> GetDetectorOnEvents(DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use ControllerEventLog repo instead", true)]
        public int GetVolumeForPeriod(DateTime StartDate, DateTime EndDate)
        {
            throw new NotImplementedException();
        }

        //[Obsolete("Use extension method GetOffset", true)]
        //public double GetOffset()
        //{
        //    throw new NotImplementedException();
        //}

        [Obsolete("This should be a combination of ICloneable and private methods in the few places this is used", true)]
        public static Detector CopyDetector(int ID, bool increaseChannel) //still need to add this detector to the collection of its associated Approach
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method is not currently being used")]
        public Signal GetTheSignalThatContainsThisDetector()
        {
            throw new NotImplementedException();
        }

        [Obsolete("Use extension method SupportsMetricType", true)]
        public bool DetectorSupportsThisMetric(int metricID)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method is not currently being used")]
        public static int GetDefaultPhaseNumberByDirectionsAndMovementTypes(int directionType, bool isLeft)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
