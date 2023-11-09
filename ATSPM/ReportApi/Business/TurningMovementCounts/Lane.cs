using ATSPM.Data.Enums;
using ATSPM.ReportApi.Business.Common;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.TurningMovementCounts
{
    public class Lane
    {
        public int? LaneNumber { get; set; }
        public string MovementType { get; set; }
        public List<DataPointForInt> Volume { get; set; }
        public LaneTypes LaneType { get; set; }
    }
}