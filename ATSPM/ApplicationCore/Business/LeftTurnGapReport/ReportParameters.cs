using System;

namespace ATSPM.ReportApi.Business.LeftTurnGapReport
{
    public class ReportParameters
    {
        public string locationId { get; set; }
        public int ApproachId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public int EndHour { get; set; }
        public int EndMinute { get; set; }
        public int[] DaysOfWeek { get; set; }

    }
}
