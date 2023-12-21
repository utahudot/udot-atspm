using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related locations
    /// </summary>
    public interface IRelatedLocations
    {
        /// <summary>
        /// Collection of locations
        /// </summary>
        ICollection<Location> Locations { get; set; }
    }
}
