using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related region
    /// </summary>
    public interface IRelatedRegion
    {
        /// <summary>
        /// Related region
        /// </summary>
        int RegionId { get; set; }
        
        /// <summary>
        /// Region
        /// </summary>
        Region Region { get; set; }
    }
}
