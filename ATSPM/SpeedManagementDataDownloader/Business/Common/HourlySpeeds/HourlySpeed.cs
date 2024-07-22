using System;

namespace SpeedManagementDataDownloader.Common.HourlySpeeds
{
    public class HourlySpeed
    {
        public DateTime Date { get; set; }
        public DateTime BinStartTime { get; set; }
        public long RouteId { get; set; }
        public long SourceId { get; set; }
        public long ConfidenceId { get; set; }
        public int Average { get; set; }
        public int? FifteenthSpeed { get; set; }
        public int? EightyFifthSpeed { get; set; }
        public int? NinetyFifthSpeed { get; set; }
        public int? NinetyNinthSpeed { get; set; }
        public long? Violation { get; set; }
        public int? Flow { get; set; }
    }
}
