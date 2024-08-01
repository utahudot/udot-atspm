namespace ATSPM.Data.Models.SpeedManagementConfigModels
{
    public class SegmentEntity
    {
        public int EntityId { get; set; }
        public int SourceId { get; set; }
        public string SegmentId { get; set; }
        public Segment Segment { get; set; }
    }
}

