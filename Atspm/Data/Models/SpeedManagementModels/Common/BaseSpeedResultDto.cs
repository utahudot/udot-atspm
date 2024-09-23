namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common
{
    public class BaseSpeedResultDto
    {
        public Guid SegmentId { get; set; }
        public string SegmentName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double StartingMilePoint { get; set; }
        public double EndingMilePoint { get; set; }
        public long SpeedLimit { get; set; }
    }
}
