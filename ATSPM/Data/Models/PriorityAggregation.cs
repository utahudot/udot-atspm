using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class PriorityAggregation
    {
        public DateTime BinStartTime { get; set; }
        public string SignalId { get; set; } = null!;
        public int PriorityNumber { get; set; }
        public int PriorityRequests { get; set; }
        public int PriorityServiceEarlyGreen { get; set; }
        public int PriorityServiceExtendedGreen { get; set; }
    }
}
