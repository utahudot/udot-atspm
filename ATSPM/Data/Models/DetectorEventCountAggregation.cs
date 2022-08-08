using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class DetectorEventCountAggregation
    {
        public DateTime BinStartTime { get; set; }
        public string SignalId { get; set; } = null!;
        public int ApproachId { get; set; }
        public int DetectorPrimaryId { get; set; }
        public int EventCount { get; set; }
    }
}
