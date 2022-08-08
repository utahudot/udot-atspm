using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class SpeedEvent
    {
        public string DetectorId { get; set; } = null!;
        public int Mph { get; set; }
        public int Kph { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
