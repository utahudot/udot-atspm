using ATSPM.Data.Models;

namespace ATSPM.Data.Enums
{
    /// <summary>
    /// Status for <see cref="Device"/>
    /// </summary>
    public enum DeviceStatus
    {
        /// <summary>
        /// Unknown device status
        /// </summary>
        Unknown,
        
        /// <summary>
        /// Device has been decommissioned
        /// </summary>
        Decommissioned,
        
        /// <summary>
        /// Device is inactive
        /// </summary>
        Inactive,
        
        /// <summary>
        /// Device is active
        /// </summary>
        Active,
        
        /// <summary>
        /// Device is being tested
        /// </summary>
        Testing,
        
        /// <summary>
        /// Device is being staged
        /// </summary>
        Staging
    }
}
