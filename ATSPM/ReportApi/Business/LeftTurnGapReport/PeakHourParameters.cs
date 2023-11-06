using System;

namespace ATSPM.ReportApi.Business.LeftTurnGapReport
{
    public class PeakHourParameters
    {
        public string SignalId { get; set; }
        public int ApproachId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int[] DaysOfWeek { get; set; }
    }
}
