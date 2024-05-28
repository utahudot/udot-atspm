using NetTopologySuite.Geometries;

namespace ATSPM.Data.Models.SpeedManagementConfigModels
{
    public class Route
    {
        public int Id { get; set; }
        public int UdotRouteNumber { get; set; }
        public double StartMilePoint { get; set; }
        public double EndMilePoint { get; set; }
        public string FunctionalType { get; set; }
        public string Name { get; set; }
        public string Direction { get; set; }
        public int SpeedLimit { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public Geometry Shape { get; set; }
        public string? AlternateIdentifier { get; set; }
        public virtual List<RouteEntity> RouteEntities { get; set; }

    }
}