using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class PhaseSplitMonitorAggregation
    {
        public DateTime BinStartTime { get; set; }
        public string SignalId { get; set; } = null!;
        public int PhaseNumber { get; set; }
        public int EightyFifthPercentileSplit { get; set; }
        public int SkippedCount { get; set; }
    }
}
