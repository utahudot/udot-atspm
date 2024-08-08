namespace ATSPM.Data.Models.SpeedManagement.SpeedOverTime
{
    public class SpeedOverTimeOptions
    {
        public TimeOptionsEnum TimeOptions {  get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<DayOfWeek> DaysOfWeek { get; set; }
        public string SegmentId { get; set; }
        public long SourceId {  get; set; }
    }
}
