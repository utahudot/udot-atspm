using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related location
    /// </summary>
    public interface IRelatedLocation
    {
        /// <summary>
        /// Related location
        /// </summary>
        int LocationId { get; set; }
        
        /// <summary>
        /// Location
        /// </summary>
        Location Location { get; set; }
    }
}
