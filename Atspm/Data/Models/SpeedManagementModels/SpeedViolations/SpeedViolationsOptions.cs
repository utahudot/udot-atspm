namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.SpeedViolations
{
    public class SpeedViolationsOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? StartTime { get; set; } = null;
        public DateTime? EndTime { get; set; } = null;
        public List<int>? DaysOfWeek { get; set; } = null; //1 (Sunday) through 7 (Saturday)
        public List<DateTime>? SpecificDays { get; set; } = null;
        public List<Guid> SegmentIds { get; set; }
    }
}