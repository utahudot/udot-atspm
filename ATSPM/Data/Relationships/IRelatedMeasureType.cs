using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related measure type
    /// </summary>
    public interface IRelatedMeasureType
    {
        /// <summary>
        /// Measure type
        /// </summary>
        MeasureType MeasureType { get; set; }
    }
}
