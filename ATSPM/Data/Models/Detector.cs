using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class Detector
    {
        public Detector()
        {
            DetectorComments = new HashSet<DetectorComment>();
            DetectionTypes = new HashSet<DetectionType>();
        }

        public int Id { get; set; }
        public string DetectorId { get; set; } = null!;
        public int DetChannel { get; set; }
        public int? DistanceFromStopBar { get; set; }
        public int? MinSpeedFilter { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateDisabled { get; set; }
        public int? LaneNumber { get; set; }
        public int? MovementTypeId { get; set; }
        public int? LaneTypeId { get; set; }
        public int? DecisionPoint { get; set; }
        public int? MovementDelay { get; set; }
        public int ApproachId { get; set; }
        public int DetectionHardwareId { get; set; }
        public double LatencyCorrection { get; set; }

        public virtual Approach Approach { get; set; } = null!;
        public virtual DetectionHardware DetectionHardware { get; set; } = null!;
        public virtual LaneType? LaneType { get; set; }
        public virtual MovementType? MovementType { get; set; }
        public virtual ICollection<DetectorComment> DetectorComments { get; set; }

        public virtual ICollection<DetectionType> DetectionTypes { get; set; }
    }
}
