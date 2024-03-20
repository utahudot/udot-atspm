using ATSPM.Data.Enums;
using System.Collections.Generic;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// Defines <see cref="IndianaEnumerations.DetectorOn"/> event arrival ratios for cycle
    /// </summary>
    public interface ICycleRatios : ICycleArrivals
    {
        /// <summary>
        /// The percentage of total green arrivals vs total volume
        /// </summary>
        double PercentArrivalOnGreen { get; }

        /// <summary>
        /// The percentage of total yellow arrivals vs total volume
        /// </summary>
        double PercentArrivalOnYellow { get; }

        /// <summary>
        /// The percentage of total red arrivals vs total volume
        /// </summary>
        double PercentArrivalOnRed { get; }

        /// <summary>
        /// The percentage of green time vs total time
        /// </summary>
        double PercentGreen { get; }

        /// <summary>
        /// The percentage of yellow time vs total time
        /// </summary>
        double PercentYellow { get; }

        /// <summary>
        /// The percentage of red time vs total time
        /// </summary>
        double PercentRed { get; }

        /// <summary>
        /// The proportion of arrivals on green vs green time
        /// </summary>
        double PlatoonRatio { get; }


        /// <summary>
        /// <see cref="ICycleArrivals"/> that are used to derrive the <see cref="ICycleRatios"/>
        /// </summary>
        IReadOnlyList<ICycleArrivals> ArrivalCycles { get; }
    }
}
