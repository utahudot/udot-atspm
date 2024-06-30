using ATSPM.Data.Enums;
using ATSPM.Data.Models;

namespace ATSPM.ConfigApi.Models
{
    public class DetectorDto
    {
        public int? Id { get; set; }
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
        public int? ApproachId { get; set; }
        public ICollection<DetectionType> DetectionTypes { get; set; }
    }
}
