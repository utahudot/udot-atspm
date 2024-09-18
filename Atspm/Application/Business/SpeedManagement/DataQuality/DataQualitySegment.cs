using Utah.Udot.Atspm.Data.Models.SpeedManagementModels;

namespace Utah.Udot.Atspm.Business.SpeedManagement.DataQuality
{
    public class DataQualitySegment
    {
        public Guid SegmentId { get; set; }
        public string SegmentName { get; set; }
        public double StartingMilePoint { get; set; }
        public double EndingMilePoint { get; set; }
        public List<DataPoint<long>> DataPoints { get; set; }
    }
}