using ATSPM.Data.Enums;

namespace ATSPM.ConfigApi.Models
{
    public class ApproachDto
    {
        public int Id { get; set; }
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
        public DirectionTypes DirectionTypeId { get; set; }
        public ICollection<DetectorDto> Detectors { get; set; }
    }
}
