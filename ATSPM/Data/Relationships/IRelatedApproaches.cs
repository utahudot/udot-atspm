using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related approaches
    /// </summary>
    public interface IRelatedApproaches
    {
        /// <summary>
        /// Collection of approaches
        /// </summary>
        ICollection<Approach> Approaches { get; set; }
    }
}
