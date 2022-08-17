using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Data.Relationships
{
    internal interface IRelatedSignal
    {
        int SignalId { get; set; }
        Signal Signal { get; set; }
    }

    internal interface IRelatedSignals
    {
        ICollection<Signal> Signals { get; set; }
    }

    internal interface IRelatedControllerType
    {
        int ControllerTypeId { get; set; }
        ControllerType ControllerType { get; set; }
    }

    internal interface IRelatedJurisdiction
    {
        int JurisdictionId { get; set; }
        Jurisdiction Jurisdiction { get; set; }
    }

    internal interface IRelatedRegion
    {
        int RegionId { get; set; }
        Region Region { get; set; }
    }

    internal interface IRelatedVersionAction
    {
        SignaVersionActions VersionActionId { get; set; }
        VersionAction VersionAction { get; set; }
    }

    internal interface IRelatedRoute
    {
        int RouteId { get; set; }
        Route Route { get; set; }
    }

    internal interface IRelatedRouteSignal
    {
        int RouteSignalId { get; set; }
        RouteSignal RouteSignal { get; set; }
    }

    internal interface IRelatedRouteSignals
    {
        ICollection<RouteSignal> RouteSignals { get; set; }
    }

    internal interface IRelatedRoutePhaseDirections
    {
        ICollection<RoutePhaseDirection> RoutePhaseDirections { get; set; }
    }

    internal interface IRelatedDirectionType
    {
        DirectionType DirectionType { get; set; }
        DirectionTypes DirectionTypeId { get; set; }
    }

    internal interface IRelatedApproaches
    {
        ICollection<Approach> Approaches { get; set; }
    }

    internal interface IRelatedApproach
    {
        int ApproachId { get; set; }
        Approach Approach { get; set; }
    }

    internal interface IRelatedDetector
    {
        int DetectorId { get; set; }
        Detector Detector { get; set; }
    }

    internal interface IRelatedDetectors
    {
        ICollection<Detector> Detectors { get; set; }
    }

    internal interface IRelatedLaneType
    {
        LaneTypes LaneTypeId { get; set; }
        LaneType LaneType { get; set; }
    }

    internal interface IRelatedMovementType
    {
        MovementTypes MovementTypeId { get; set; }
        MovementType MovementType { get; set; }
    }

    internal interface IRelatedDetectionHardware
    {
        int DetectionHardwareId { get; set; }
        DetectionHardware DetectionHardware { get; set; }
    }

    internal interface IRelatedDetectorComments
    {
        ICollection<DetectorComment> DetectorComments { get; set; }
    }

    internal interface IRelatedDetectionTypes
    {
        ICollection<DetectionType> DetectionTypes { get; set; }
    }

    internal interface IRelatedApplicationSettings
    {
        ICollection<ApplicationSetting> ApplicationSettings { get; set; }
    }

    internal interface IRelatedApplication
    {
        ApplicationTypes ApplicationId { get; set; }
        Application Application { get; set; }
    }
}
