namespace WatchDog.Services
{

    public class LoggingOptions
    {
        public DateTime ScanDate { get; set; }
        public int ScanDayStartHour { get; set; }
        public int ScanDayEndHour { get; set; }
        public int ConsecutiveCount { get; set; }
        public int MinPhaseTerminations { get; set; }
        public double PercentThreshold { get; set; }
        public int MaxDegreeOfParallelism { get; set; }
        public int PreviousDayPMPeakStart { get; set; }
        public int PreviousDayPMPeakEnd { get; set; }
        public int MinimumRecords { get; set; }
        public int LowHitThreshold { get; set; }
        public int MaximumPedestrianEvents { get; set; }
        public bool WeekdayOnly { get; set; }

        public DateTime AnalysisStart
        {
            get
            {
                var startHour = new TimeSpan(ScanDayStartHour, 0, 0);
                return ScanDate.Date + startHour;
            }
        }
        public DateTime AnalysisEnd
        {
            get
            {
                var endHour = new TimeSpan(ScanDayEndHour, 0, 0);
                return ScanDate.Date + endHour;
            }
        }
    }

}
