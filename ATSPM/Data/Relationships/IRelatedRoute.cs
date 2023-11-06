using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related Route
    /// </summary>
    public interface IRelatedRoute
    {
        /// <summary>
        /// Related Route
        /// </summary>
        int RouteId { get; set; }
        
        /// <summary>
        /// Route
        /// </summary>
        Route Route { get; set; }
    }
}
