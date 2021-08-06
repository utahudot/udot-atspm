using System;
using System.Collections.Generic;

#nullable disable

namespace ControllerLogger.Models
{
    public partial class SignalEventCountAggregation
    {
        public DateTime BinStartTime { get; set; }
        public string SignalId { get; set; }
        public int EventCount { get; set; }
    }
}
