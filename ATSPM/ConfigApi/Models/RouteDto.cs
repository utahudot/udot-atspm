using ATSPM.Data.Enums;
using ATSPM.Data.Models;

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
        public int Order { get; set; }

        /// <summary>
        /// Primary phase number
        /// </summary>
        public int PrimaryPhase { get; set; }

        /// <summary>
        /// Opposing phase number
        /// </summary>
        public int OpposingPhase { get; set; }

        /// <summary>
        /// Related primary direction
        /// </summary>
        public DirectionTypes PrimaryDirectionId { get; set; }

        /// <summary>
        /// Related opposing direction
        /// </summary>
        public DirectionTypes OpposingDirectionId { get; set; }

        /// <summary>
        /// Is primary overlap
        /// </summary>
        public bool IsPrimaryOverlap { get; set; }

        /// <summary>
        /// Is opposing overlap
        /// </summary>
        public bool IsOpposingOverlap { get; set; }

        /// <summary>
        /// Related previous location distance
        /// </summary>
        public int? PreviousLocationDistanceId { get; set; }
        public RouteDistanceDto PreviousLocationDistance { get; set; }

        /// <summary>
        /// Related next location distance
        /// </summary>
        public int? NextLocationDistanceId { get; set; }
        public RouteDistanceDto NextLocationDistance { get; set; }

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }


        /// <inheritdoc/>
        public int RouteId { get; set; }

        public virtual ICollection<RouteApproachesDto>? Approaches { get; set; } = new HashSet<RouteApproachesDto>();
    }

    public class RouteDistanceDto
    {
        public int Id { get; set; }
        public double Distance { get; set; }
        public string LocationIdentifierA { get; set; }
        public string LocationIdentifierB { get; set; }
    }

    public class RouteApproachesDto
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
        public virtual ICollection<RouteDetectorsDto>? Detectors { get; set; } = new HashSet<RouteDetectorsDto>();
    }

    public class RouteDetectorsDto
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
