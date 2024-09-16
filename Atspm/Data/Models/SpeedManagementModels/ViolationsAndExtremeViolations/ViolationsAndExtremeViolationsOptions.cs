namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.ViolationsAndExtremeViolations
{
    public class ViolationsAndExtremeViolationsOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Guid> SegmentIds { get; set; }
    }
}