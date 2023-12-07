using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related devices
    /// </summary>
    public interface IRelatedDevices
    {
        /// <summary>
        /// Collection of devices
        /// </summary>
        ICollection<Device> Devices { get; set; }
    }
}
