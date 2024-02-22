using System;

namespace ATSPM.ReportApi.Business.LeftTurnGapReport
{
    public class DataCheckParameters
    {
        public string locationId { get; set; }
        public int ApproachId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int VolumePerHourThreshold { get; set; }
        public double GapOutThreshold { get; set; }
        public double PedestrianThreshold { get; set; }
        public int[] DaysOfWeek { get; set; }
    }
}
