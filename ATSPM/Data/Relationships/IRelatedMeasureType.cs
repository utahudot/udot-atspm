using ATSPM.Data.Enums;
using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related measure type
    /// </summary>
    public interface IRelatedMeasureType
    {
        /// <summary>
        /// Related measure type
        /// </summary>
        int MeasureTypeId { get; set; }

        /// <summary>
        /// Measure type
        /// </summary>
        MeasureType MeasureType { get; set; }
    }
}
