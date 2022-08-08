using System;
using System.Collections.Generic;

namespace Data.Models
{
    public partial class ApproachPcdAggregation
    {
        public DateTime BinStartTime { get; set; }
        public string SignalId { get; set; } = null!;
        public int ApproachId { get; set; }
        public int PhaseNumber { get; set; }
        public bool IsProtectedPhase { get; set; }
        public int ArrivalsOnGreen { get; set; }
        public int ArrivalsOnRed { get; set; }
        public int ArrivalsOnYellow { get; set; }
        public int Volume { get; set; }
        public int TotalDelay { get; set; }
    }
}
