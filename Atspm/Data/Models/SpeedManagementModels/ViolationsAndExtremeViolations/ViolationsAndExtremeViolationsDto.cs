namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.ViolationsAndExtremeViolations
{
    public class ViolationsAndExtremeViolationsDto
    {
        public Guid SegmentId { get; set; }
        public string SegmentName { get; set; }
        public DateTime Time { get; set; }
        public double StartingMilePoint { get; set; }
        public double EndingMilePoint { get; set; }
        public long SpeedLimit { get; set; }
        public long Flow { get; set; }
        public long Violations { get; set; }
        public long ExteremeViolations { get; set; }
    }
}