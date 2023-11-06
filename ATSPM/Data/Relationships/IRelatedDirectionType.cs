using ATSPM.Data.Enums;
using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related direction type
    /// </summary>
    public interface IRelatedDirectionType
    {
        /// <summary>
        /// Related direction type
        /// </summary>
        DirectionTypes DirectionTypeId { get; set; }

        /// <summary>
        /// Direction type
        /// </summary>
        DirectionType DirectionType { get; set; }
    }
}
