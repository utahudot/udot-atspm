using ATSPM.Data.Enums;
using Reports.Business.Common;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.TurningMovementCounts
{
    public class Lane
    {
        public int? LaneNumber { get; set; }
        public string MovementType { get; set; }
        public List<DataPointForInt> Volume { get; set; }
        public LaneTypes LaneType { get; set; }
    }
}