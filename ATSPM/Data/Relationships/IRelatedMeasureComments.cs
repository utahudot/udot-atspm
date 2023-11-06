using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related measure comments
    /// </summary>
    public interface IRelatedMeasureComments
    {
        /// <summary>
        /// Collection of measure comments
        /// </summary>
        ICollection<MeasureComment> MeasureComments { get; set; }
    }
}
