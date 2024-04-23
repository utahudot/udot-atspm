using System;

namespace ATSPM.Application.Business.LeftTurnGapReport
{
    public class LeftTurnGapReportOptions
    {
        public string LocationIdentifier { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int[] ApproachIds { get; set; }
        public int[] DaysOfWeek { get; set; }
        public int? StartHour { get; set; }
        public int? StartMinute { get; set; }
        public int? EndHour { get; set; }
        public int? EndMinute { get; set; }
        public bool GetAMPMPeakPeriod { get; set; }
        public bool GetAMPMPeakHour { get; set; }
        public bool Get24HourPeriod { get; set; }
        public bool GetGapReport { get; set; }
        public double AcceptableGapPercentage { get; set; }
        public bool GetSplitFail { get; set; }
        public double AcceptableSplitFailPercentage { get; set; }
        public bool GetPedestrianCall { get; set; }
        public bool GetConflictingVolume { get; set; }
    }
}
