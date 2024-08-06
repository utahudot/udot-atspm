namespace ATSPM.Data.Models.SpeedManagementConfigModels
{
    public class Impact
    {
        public Guid? Id { get; set; }
        public string? Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public double StartMile { get; set; }
        public double EndMile { get; set; }
        public Guid ImpactTypeId { get; set; }
        public ImpactType? ImpactType { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }
        public List<Guid>? SegmentIds { get; set; }
    }
}