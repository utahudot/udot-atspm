using System;
using System.Collections.Generic;

#nullable disable

namespace ControllerLogger.Models
{
    public partial class SpeedEvent
    {
        public string DetectorId { get; set; }
        public int Mph { get; set; }
        public int Kph { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
