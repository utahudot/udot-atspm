using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Data.Relationships
{
    public interface IRelatedSignal
    {
        int SignalId { get; set; }
        Signal Signal { get; set; }
    }

    public interface IRelatedRoute
    {
        int RouteId { get; set; }
        Route Route { get; set; }
    }

    public interface IRelatedRouteSignal
    {
        int RouteSignalId { get; set; }
        RouteSignal RouteSignal { get; set; }
    }

    public interface IRelatedRouteSignals
    {
        ICollection<RouteSignal> RouteSignals { get; set; }
    }

    public interface IRelatedRoutePhaseDirections
    {
        ICollection<RoutePhaseDirection> RoutePhaseDirections { get; set; }
    }

    public interface IRelatedDirectionType
    {
        DirectionType DirectionType { get; set; }
        DirectionTypes DirectionTypeId { get; set; }
    }

    public interface IRelatedApproach
    {
        int ApproachId { get; set; }
        Approach Approach { get; set; }
    }

    public interface IRelatedDetector
    {
        int DetectorId { get; set; }
        Detector Detector { get; set; }
    }

    public interface IRelatedDetectors
    {
        ICollection<Detector> Detectors { get; set; }
    }

    public interface IRelatedLaneType
    {
        LaneTypes LaneTypeId { get; set; }
        LaneType LaneType { get; set; }
    }

    public interface IRelatedMovementType
    {
        MovementTypes MovementTypeId { get; set; }
        MovementType MovementType { get; set; }
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
