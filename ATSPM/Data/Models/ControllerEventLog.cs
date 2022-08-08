using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class ControllerEventLog
    {
        public string SignalId { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public int EventCode { get; set; }
        public int EventParam { get; set; }
    }
}
