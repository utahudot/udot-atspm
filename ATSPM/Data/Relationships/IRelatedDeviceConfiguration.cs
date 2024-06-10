using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related device configuration
    /// </summary>
    public interface IRelatedDeviceConfiguration
    {
        /// <summary>
        /// Related Id
        /// </summary>
        int? DeviceConfigurationId { get; set; }

        /// <summary>
        /// Related device configuration
        /// </summary>
        DeviceConfiguration? DeviceConfiguration { get; set; }
    }
}
