namespace Utah.Udot.Atspm.Business.Watchdog
{
    //TODO: this needs to be added into WatchdogConfiguration and config json needs to be changed from flat
    public class WatchdogLoggingOptions
    {
        public DateTime ScanDate { get; set; }
        public int ScanDayStartHour { get; set; }
        public int ScanDayEndHour { get; set; }
        public int ConsecutiveCount { get; set; }
        public int MinPhaseTerminations { get; set; }
        public double PercentThreshold { get; set; }
        public int PreviousDayPMPeakStart { get; set; }
        public int PreviousDayPMPeakEnd { get; set; }
        public int MinimumRecords { get; set; }
        public int LowHitThreshold { get; set; }
        public int MaximumPedestrianEvents { get; set; }
        public bool WeekdayOnly { get; set; }
        public int RampMainlineStartHour { get; set; }
        public int RampMainlineEndHour { get; set; }
        public int RampStuckQueueStartHour { get; set; }
        public int RampStuckQueueEndHour { get; set; }


        public DateTime AnalysisStart => ScanDate.Date + new TimeSpan(ScanDayStartHour, 0, 0);

        public DateTime AnalysisEnd => ScanDate.Date + new TimeSpan(ScanDayEndHour, 0, 0);
    }
}
