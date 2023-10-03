using ATSPM.Data.Enums;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.TurningMovementCounts
{
    public class Lane
    {
        public int? LaneNumber { get; set; }
        public string MovementType { get; set; }
        public List<LaneVolume> Volume { get; set; }
        public LaneTypes LaneType { get; set; }
    }
}