using ATSPM.Data.Enums;

namespace ATSPM.Application.Enums
{
    /// <summary>
    /// Arrival type of <see cref="82"/> events
    /// </summary>
    public enum ArrivalType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// <see cref="82"/> arrival on green
        /// </summary>
        ArrivalOnGreen,

        /// <summary>
        /// <see cref="82"/> arrival on yellow
        /// </summary>
        ArrivalOnYellow,

        /// <summary>
        /// <see cref="82"/> arrival on red
        /// </summary>
        ArrivalOnRed
    }
}
