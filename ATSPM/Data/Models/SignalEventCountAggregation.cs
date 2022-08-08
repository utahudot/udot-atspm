using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class SignalEventCountAggregation
    {
        public DateTime BinStartTime { get; set; }
        public string SignalId { get; set; } = null!;
        public int EventCount { get; set; }
    }
}
