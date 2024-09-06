namespace Utah.Udot.Atspm.Configuration
{
    /// <summary>
    /// Watchdog utility configuration
    /// </summary>
    public class WatchdogConfiguration
    {
        public DateTime ScanDate { get; set; } = DateTime.Today.AddDays(-1);
        public int PreviousDayPMPeakStart { get; set; } = 18;
        public int PreviousDayPMPeakEnd { get; set; } = 17;
        public bool WeekdayOnly { get; set; } = true;
        public int ScanDayStartHour { get; set; }
        public int ScanDayEndHour { get; set; }


        public int ConsecutiveCount { get; set; } = 3;
        public int LowHitThreshold { get; set; } = 50;
        public int MaximumPedestrianEvents { get; set; } = 200;
        public int MinimumRecords { get; set; } = 500;
        public int MinPhaseTerminations { get; set; } = 50;
        public double PercentThreshold { get; set; } = .9;

        public bool EmailAllErrors { get; set; }
        public string DefaultEmailAddress { get; set; }


        public DateTime AnalysisStart => ScanDate.Date + new TimeSpan(ScanDayStartHour, 0, 0);

        public DateTime AnalysisEnd => ScanDate.Date + new TimeSpan(ScanDayEndHour, 0, 0);
    }
}
