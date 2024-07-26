
namespace ATSPM.Data.Models.SpeedManagement.CongestionTracking
{
    public class CongestionTrackingDto
    {
        public string SegmentId { get; set; }
        public string SegmentName { get; set; }
        public double StartingMilePoint { get; set; }
        public double EndingMilePoint { get; set; }
        public long SpeedLimit { get; set; }
        public List<CongestionDailyDataDto> Data { get; set; }
    }
}
