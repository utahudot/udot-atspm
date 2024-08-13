namespace ArchiveLogs.Models
{
    public class PortlandCsv
    {
        public string DetectorID { get; set; }
        public int DetChannel { get; set; }
        public string Direction { get; set; }
        public int Phase { get; set; }
        public int? PermPhase { get; set; }
        public bool Overlap { get; set; }
        public bool Enabled { get; set; }
        public string DetectionTypes { get; set; }
        public string DetectionHardware { get; set; }
        public int LatencyCorrection { get; set; }
        public string MovementType { get; set; }
        public int LaneNumber { get; set; }
        public string LaneType { get; set; }
        public int? MPH { get; set; }
        public int? DistFromStopBar { get; set; }
        public int? DecisionPoint { get; set; }
        public int? MoveDelay { get; set; }
        public int? MinSpeedFilter { get; set; }
        public string Comment { get; set; }
    }
}
