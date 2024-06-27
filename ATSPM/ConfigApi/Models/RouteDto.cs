using ATSPM.Data.Enums;

namespace ATSPM.ConfigApi.Models
{
    public class RouteIdDto
    {
        public int RouteId { get; set; }
    }

    public class RouteDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public ICollection<RouteLocationDto> RouteLocations { get; set; }
    }

    public class RouteLocationDto
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public int PrimaryPhase { get; set; }
        public int OpposingPhase { get; set; }
        public int PrimaryDirectionId { get; set; }
        public string PrimaryDirectionDescription { get; set; }
        public int OpposingDirectionId { get; set; }
        public string OpposingDirectionDescription { get; set; }
        public bool IsPrimaryOverlap { get; set; }
        public bool IsOpposingOverlap { get; set; }
        public int? PreviousLocationDistanceId { get; set; }
        public RouteDistanceDto PreviousLocationDistance { get; set; }
        public int? NextLocationDistanceId { get; set; }
        public RouteDistanceDto NextLocationDistance { get; set; }
        public string LocationIdentifier { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public int? LocationId { get; set; }
        public int RouteId { get; set; }
        public virtual ICollection<RouteApproachDto>? Approaches { get; set; } = new HashSet<RouteApproachDto>();
    }

    public class RouteDistanceDto
    {
        public int Id { get; set; }
        public double Distance { get; set; }
        public string LocationIdentifierA { get; set; }
        public string LocationIdentifierB { get; set; }
    }

    public class RouteApproachDto
    {
        public string Description { get; set; }
        public int? Mph { get; set; }
        public int ProtectedPhaseNumber { get; set; }
        public bool IsProtectedPhaseOverlap { get; set; }
        public int? PermissivePhaseNumber { get; set; }
        public bool IsPermissivePhaseOverlap { get; set; }
        public int? PedestrianPhaseNumber { get; set; }
        public bool IsPedestrianPhaseOverlap { get; set; }
        public string PedestrianDetectors { get; set; }
        public int LocationId { get; set; }
        public virtual ICollection<RouteDetectorDto>? Detectors { get; set; } = new HashSet<RouteDetectorDto>();
    }

    public class RouteDetectorDto
    {
        public string DectectorIdentifier { get; set; }
        public int DetectorChannel { get; set; }
        public int? DistanceFromStopBar { get; set; }
        public int? MinSpeedFilter { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateDisabled { get; set; }
        public int? LaneNumber { get; set; }
        public MovementTypes MovementType { get; set; }
        public LaneTypes LaneType { get; set; }
        public DetectionHardwareTypes DetectionHardware { get; set; }
        public int? DecisionPoint { get; set; }
        public int? MovementDelay { get; set; }
        public double LatencyCorrection { get; set; }
        public int ApproachId { get; set; }
    }

}
