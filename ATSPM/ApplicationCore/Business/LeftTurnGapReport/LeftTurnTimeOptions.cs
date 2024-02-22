namespace ATSPM.ReportApi.Business.LeftTurnGapReport
{
    public enum TimeOptions { PeakHour, PeakPeriod, FullDay, Custom }
    public class LeftTurnTimeOptions
    {
        public TimeOptions SelectedTimeOption { get; set; }
        public int? StartHour { get; set; }
        public int? StartMinute { get; set; }
        public int? EndHour { get; set; }
        public int? EndMinute { get; set; }

    }
}
