using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related signal
    /// </summary>
    public interface IRelatedSignal
    {
        /// <summary>
        /// Related signal
        /// </summary>
        int SignalId { get; set; }
        
        /// <summary>
        /// Signal
        /// </summary>
        Signal Signal { get; set; }
    }

    public interface IRelatedRouteSignal
    {
        int RouteSignalId { get; set; }
        RouteSignal RouteSignal { get; set; }
    }

    /// <summary>
    /// Related approach
    /// </summary>
    public interface IRelatedApproach
    {
        /// <summary>
        /// Related approach
        /// </summary>
        int ApproachId { get; set; }
        
        /// <summary>
        /// Approach
        /// </summary>
        Approach Approach { get; set; }
    }

    public interface IRelatedDetector
    {
        int DetectorId { get; set; }
        Detector Detector { get; set; }
    }

    public interface IRelatedLaneType
    {
        LaneTypes LaneTypeId { get; set; }
        LaneType LaneType { get; set; }
    }

    public interface IRelatedDetectionHardware
    {
        DetectionHardwareTypes DetectionHardwareId { get; set; }
        DetectionHardware DetectionHardware { get; set; }
    }

    public interface IRelatedDetectorComments
    {
        ICollection<DetectorComment> DetectorComments { get; set; }
    }

    public interface IRelatedDetectionTypes
    {
        ICollection<DetectionType> DetectionTypes { get; set; }
    }

    public interface IRelatedApplicationSettings
    {
        ICollection<ApplicationSetting> ApplicationSettings { get; set; }
    }

    public interface IRelatedApplication
    {
        ApplicationTypes ApplicationId { get; set; }
        Application Application { get; set; }
    }
}
