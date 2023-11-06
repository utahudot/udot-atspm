using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related detector comments
    /// </summary>
    public interface IRelatedDetectorComments
    {
        /// <summary>
        /// Collection of detector comments
        /// </summary>
        ICollection<DetectorComment> DetectorComments { get; set; }
    }
}
