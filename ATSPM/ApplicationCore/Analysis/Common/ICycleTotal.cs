using ATSPM.Data.Enums;
using ATSPM.Domain.Common;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// Defines the start and end of a red to red cycle
    /// which is the time between two <see cref="IndianaEnumerations.PhaseEndYellowChange"/> events including
    /// <see cref="IndianaEnumerations.PhaseBeginGreen"/> and <see cref="IndianaEnumerations.PhaseBeginYellowChange"/>
    /// </summary>
    public interface ICycleTotal : IStartEndRange
    {
        /// <summary>
        /// The total green time is defined as the time from start of <see cref="IndianaEnumerations.PhaseBeginGreen"/> to the start of <see cref="IndianaEnumerations.PhaseBeginYellowChange"/> in seconds
        /// </summary>
        double TotalGreenTime { get; }

        /// <summary>
        /// The total yellow time is defined as the time from start of <see cref="IndianaEnumerations.PhaseBeginYellowChange"/> to the second <see cref="IndianaEnumerations.PhaseEndYellowChange"/> in seconds
        /// </summary>
        double TotalYellowTime { get; }

        /// <summary>
        /// The total red time is defined as the first <see cref="IndianaEnumerations.PhaseEndYellowChange"/> to the <see cref="IndianaEnumerations.PhaseBeginGreen"/> in seconds
        /// </summary>
        double TotalRedTime { get; }

        /// <summary>
        /// The total time is defined as the time between the first and second <see cref="IndianaEnumerations.PhaseEndYellowChange"/> in seconds
        /// </summary>
        double TotalTime { get; }
    }
}
