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
    public partial class Approach
    {
        public override string? ToString()
        {
            return $"{Signal} {DirectionTypeId} Phase: {ProtectedPhaseNumber}";
        }

        #region Obsolete

        [NotMapped]
        [Obsolete("this is only used in SignalController, model should not be modified for this", true)]
        public string Index { get; set; }

        [NotMapped]
        [Obsolete("Use ToString instead", true)]
        public string ApproachRouteDescription => throw new NotImplementedException();

        //[Obsolete("Use extension method GetAllDetectorsOfDetectionType", true)]
        //public List<Detector> GetAllDetectorsOfDetectionType(int detectionTypeId)
        //{
        //    throw new NotImplementedException();
        //}

        [Obsolete("This method is not currently being used")]
        public static Approach CreateNewApproachWithDefaultValues(Signal signal, DirectionType dir, ConfigContext db)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This method is not currently being used")]
        public static void AddDefaultObjectsToApproach(Approach appr, ConfigContext db)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This should be a combination of ICloneable and private methods in the few places this is used", true)]
        public static Approach CopyApproachCommonProperties(Approach approachToCopy, bool isVersionOrSignalCopy)
        {
            throw new NotImplementedException();
        }

        [Obsolete("This should be a combination of ICloneable and private methods in the few places this is used", true)]
        public static Approach CopyApproach(int approachIDToCopy)
        {
            throw new NotImplementedException();
        }

        //[Obsolete("Use extension method SetDetChannelWhenMultipleDetectorsExist", true)]
        //private static Approach SetDetChannelWhenMultipleDetectorsExist(Approach newApproach)
        //{
        //    throw new NotImplementedException();
        //}

        [Obsolete("This should be a combination of ICloneable and private methods in the few places this is used", true)]
        public static Approach CopyApproachForSignal(Approach approachToCopy)
        {
            throw new NotImplementedException();
        }

        //[Obsolete("Use extension method GetDetectorsForMetricType", true)]
        //public List<Detector> GetDetectorsForMetricType(int metricTypeID)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion
    }
}
