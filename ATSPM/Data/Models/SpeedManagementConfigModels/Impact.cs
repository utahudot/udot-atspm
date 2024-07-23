using NetTopologySuite.Geometries;

namespace ATSPM.Data.Models.SpeedManagementConfigModels
{
    public class Impact
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public string StartMile { get; set; }
        public string EndMile { get; set; }
        public Geometry Shape { get; set; }
        public int ImpactTypeId { get; set; }
        public ImpactType ImpactType { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public Guid? DeletedBy { get; set; }
        public List<Route> Segments { get; set; }
    }
}