using System;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class PeakHourParameters
    {
        public string locationId { get; set; }
        public int ApproachId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int[] DaysOfWeek { get; set; }
    }
}
