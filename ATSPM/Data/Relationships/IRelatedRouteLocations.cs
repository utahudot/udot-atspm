using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related route Locations
    /// </summary>
    public interface IRelatedRouteLocations
    {
        /// <summary>
        /// Collection of route Locations
        /// </summary>
        ICollection<RouteLocation> RouteLocations { get; set; }
    }
}
