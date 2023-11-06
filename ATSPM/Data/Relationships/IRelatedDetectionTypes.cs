using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related detection types
    /// </summary>
    public interface IRelatedDetectionTypes
    {
        /// <summary>
        /// Collection of detection types
        /// </summary>
        ICollection<DetectionType> DetectionTypes { get; set; }
    }
}
