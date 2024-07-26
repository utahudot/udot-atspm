using NetTopologySuite.Geometries;

namespace ATSPM.Data.Models.SpeedManagementConfigModels
{
    public class Segment
    {
        public string Id { get; set; }
        public string UdotRouteNumber { get; set; }
        public double StartMilePoint { get; set; }
        public double EndMilePoint { get; set; }
        public string FunctionalType { get; set; }
        public string Name { get; set; }
        public string? Direction { get; set; }
        public long SpeedLimit { get; set; }
        public string? Region { get; set; }
        public string? City { get; set; }
        public string? County { get; set; }
        public Geometry Shape { get; set; }
        public string? ShapeWKT { get; set; }
        public string? AlternateIdentifier { get; set; }
        public string? AccessCategory { get; set; }
        public int? Offset { get; set; }
        public virtual List<SegmentEntity>? RouteEntities { get; set; }

    }
}