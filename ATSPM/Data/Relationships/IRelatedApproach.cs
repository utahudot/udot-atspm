using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related approach
    /// </summary>
    public interface IRelatedApproach
    {
        /// <summary>
        /// Related approach
        /// </summary>
        int ApproachId { get; set; }
        
        /// <summary>
        /// Approach
        /// </summary>
        Approach Approach { get; set; }
    }
}
