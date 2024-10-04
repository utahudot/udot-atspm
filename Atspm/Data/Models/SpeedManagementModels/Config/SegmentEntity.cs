namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config
{
    public class SegmentEntity
    {
        public string EntityId { get; set; }
        public long SourceId { get; set; }
        public Guid SegmentId { get; set; }
        public string EntityType { get; set; }
        public double Length { get; set; }
        public Segment? Segment { get; set; }
    }
}

