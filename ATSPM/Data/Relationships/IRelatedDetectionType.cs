using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related detection types
    /// </summary>
    public interface IRelatedDetectionType
    {
        /// <summary>
        /// Colleciton of detection types
        /// </summary>
        ICollection<DetectionType> DetectionTypes { get; set; }
    }
}
