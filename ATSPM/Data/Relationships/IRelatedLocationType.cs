using ATSPM.Data.Models.ConfigurationModels;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related location type
    /// </summary>
    public interface IRelatedLocationType
    {
        /// <summary>
        /// Related location type
        /// </summary>
        int LocationTypeId { get; set; }

        /// <summary>
        /// Location type
        /// </summary>
        LocationType LocationType { get; set; }
    }
}
