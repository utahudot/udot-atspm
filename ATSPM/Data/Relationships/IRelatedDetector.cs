using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related detector
    /// </summary>
    public interface IRelatedDetector
    {
        /// <summary>
        /// Related detector
        /// </summary>
        int DetectorId { get; set; }
        
        /// <summary>
        /// Detector
        /// </summary>
        Detector Detector { get; set; }
    }
}
