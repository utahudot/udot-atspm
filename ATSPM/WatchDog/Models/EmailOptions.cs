namespace WatchDog.Models
{
    public class EmailOptions
    {
        public DateTime ScanDate { get; set; }
        public int ScanDayStartHour { get; set; }
        public int ScanDayEndHour { get; set; }
        public int PreviousDayPMPeakStart { get; set; }
        public int PreviousDayPMPeakEnd { get; set; }
        public bool EmailAllErrors { get; set; }
        public string DefaultEmailAddress { get; set; }
        public bool WeekdayOnly { get; set; }
    }
}