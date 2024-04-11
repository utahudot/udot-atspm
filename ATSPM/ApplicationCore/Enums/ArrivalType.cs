using ATSPM.Data.Enums;

namespace ATSPM.Application.Enums
{
    /// <summary>
    /// Arrival type of <see cref="IndianaEnumerations.VehicleDetectorOn"/> events
    /// </summary>
    public enum ArrivalType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// <see cref="IndianaEnumerations.VehicleDetectorOn"/> arrival on green
        /// </summary>
        ArrivalOnGreen,

        /// <summary>
        /// <see cref="IndianaEnumerations.VehicleDetectorOn"/> arrival on yellow
        /// </summary>
        ArrivalOnYellow,

        /// <summary>
        /// <see cref="IndianaEnumerations.VehicleDetectorOn"/> arrival on red
        /// </summary>
        ArrivalOnRed
    }
}
