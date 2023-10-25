using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related detectors
    /// </summary>
    public interface IRelatedDetectors
    {
        /// <summary>
        /// Collection of detectors
        /// </summary>
        ICollection<Detector> Detectors { get; set; }
    }
}
