using ATSPM.Data.Enums;
using ATSPM.Domain.Common;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// Defines <see cref="DataLoggerEnum.DetectorOn"/> event volume information for cycles
    /// </summary>
    public interface ICycleVolume : IStartEndRange //: ICycle
    {
        /// <summary>
        /// The total delay of <see cref="DataLoggerEnum.DetectorOn"/> events
        /// arriving on red before a <see cref="DataLoggerEnum.PhaseBeginGreen"/> event
        /// </summary>
        double TotalDelay { get; }

        /// <summary>
        /// The total amount of <see cref="DataLoggerEnum.DetectorOn"/> events with corrected timestamps
        /// arriving within cycle
        /// </summary>
        double TotalVolume { get; }
    }
}
