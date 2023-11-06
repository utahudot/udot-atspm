using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Realted measure types
    /// </summary>
    public interface IRelatedMeasureTypes
    {
        /// <summary>
        /// Collection of measure types
        /// </summary>
        ICollection<MeasureType> MeasureTypes { get; set; }
    }
}
