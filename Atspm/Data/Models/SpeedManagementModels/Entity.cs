using NetTopologySuite.Geometries;

namespace Utah.Udot.Atspm.Data.Models.SpeedManagementModels
{
    public class Entity
    {
        public Guid Id { get; set; }
        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public string? Name { get; set; } = null;
        public string Direction { get; set; }
        public long SourceId { get; set; }
        public double Length { get; set; }
        public Geometry Geometry { get; set; }
        public string? GeometryWKT { get; set; } = null;
        public string Version { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } = null;
        public bool Active { get; set; }
        public virtual List<Segment>? Segments { get; set; } = null;
        public bool? IsWithin50Ft { get; set; } = null;
    }
}