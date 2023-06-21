using ATSPM.Data.Enums;

namespace ATSPM.Application.Enums
{
    /// <summary>
    /// Arrival type of <see cref="DataLoggerEnum.DetectorOn"/> events
    /// </summary>
    public enum ArrivalType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// <see cref="DataLoggerEnum.DetectorOn"/> arrival on green
        /// </summary>
        ArrivalOnGreen,

        /// <summary>
        /// <see cref="DataLoggerEnum.DetectorOn"/> arrival on yellow
        /// </summary>
        ArrivalOnYellow,

        /// <summary>
        /// <see cref="DataLoggerEnum.DetectorOn"/> arrival on red
        /// </summary>
        ArrivalOnRed
    }
}
