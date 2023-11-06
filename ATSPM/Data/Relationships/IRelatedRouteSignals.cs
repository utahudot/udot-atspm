using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related route signals
    /// </summary>
    public interface IRelatedRouteSignals
    {
        /// <summary>
        /// Collection of route signals
        /// </summary>
        ICollection<RouteSignal> RouteSignals { get; set; }
    }
}
