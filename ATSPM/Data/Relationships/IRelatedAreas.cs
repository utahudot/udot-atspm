using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related areas
    /// </summary>
    public interface IRelatedAreas
    {
        /// <summary>
        /// Collection of areas
        /// </summary>
        ICollection<Area> Areas { get; set; }
    }
}
