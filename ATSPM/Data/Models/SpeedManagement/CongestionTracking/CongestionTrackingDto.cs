
namespace ATSPM.Data.Models.SpeedManagement.CongestionTracking
{
    public class CongestionTrackingDto
    {
        public int SegmentId { get; set; }
        public string SegmentName { get; set; }
        public double StartingMilePoint { get; set; }
        public double EndingMilePoint { get; set; }
        public int SpeedLimit { get; set; }
        public List<CongestionDailyDataDto> Data { get; set; }
    }
}
