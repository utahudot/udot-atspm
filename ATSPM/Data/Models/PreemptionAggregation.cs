using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class PreemptionAggregation
    {
        public DateTime BinStartTime { get; set; }
        public string SignalId { get; set; } = null!;
        public int PreemptNumber { get; set; }
        public int PreemptRequests { get; set; }
        public int PreemptServices { get; set; }
    }
}
