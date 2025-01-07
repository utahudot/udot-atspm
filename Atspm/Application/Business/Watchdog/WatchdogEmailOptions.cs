namespace Utah.Udot.Atspm.Business.Watchdog
{
    //TODO: this needs to be added into WatchdogConfiguration and config json needs to be changed from flat
    public class WatchdogEmailOptions
    {
        public DateTime ScanDate { get; set; }
        public int ScanDayStartHour { get; set; }
        public int ScanDayEndHour { get; set; }
        public int PreviousDayPMPeakStart { get; set; }
        public int PreviousDayPMPeakEnd { get; set; }
        public int RampMainlineStartHour { get; set; }
        public int RampMainlineEndHour { get; set; }
        public int RampStuckQueueStartHour { get; set; }
        public int RampStuckQueueEndHour { get; set; }
        public bool EmailAllErrors { get; set; }
        public string DefaultEmailAddress { get; set; }
        public bool WeekdayOnly { get; set; }
    }
}
