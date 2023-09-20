using ATSPM.Data.Enums;
using ATSPM.Domain.Common;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// Defines the start and end of a red to red cycle
    /// which is the time between two <see cref="DataLoggerEnum.PhaseEndYellowChange"/> events including
    /// <see cref="DataLoggerEnum.PhaseBeginGreen"/> and <see cref="DataLoggerEnum.PhaseBeginYellowChange"/>
    /// </summary>
    public interface ICycle : IStartEndRange
    {
        /// <summary>
        /// The total green time is defined as the time from start of <see cref="DataLoggerEnum.PhaseBeginGreen"/> to the start of <see cref="DataLoggerEnum.PhaseBeginYellowChange"/> in seconds
        /// </summary>
        double TotalGreenTime { get; }

        /// <summary>
        /// The total yellow time is defined as the time from start of <see cref="DataLoggerEnum.PhaseBeginYellowChange"/> to the second <see cref="DataLoggerEnum.PhaseEndYellowChange"/> in seconds
        /// </summary>
        double TotalYellowTime { get; }

        /// <summary>
        /// The total red time is defined as the first <see cref="DataLoggerEnum.PhaseEndYellowChange"/> to the <see cref="DataLoggerEnum.PhaseBeginGreen"/> in seconds
        /// </summary>
        double TotalRedTime { get; }

        /// <summary>
        /// The total time is defined as the time between the first and second <see cref="DataLoggerEnum.PhaseEndYellowChange"/> in seconds
        /// </summary>
        double TotalTime { get; }
    }
}
