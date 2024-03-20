using ATSPM.Data.Enums;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Analysis.Common
{
    /// <summary>
    /// Defines the approach delay results
    /// </summary>
    public interface IApproachDelay : IStartEndRange
    {
        /// <summary>
        /// The average delay of <see cref="IndianaEnumerations.DetectorOn"/> arrival on red events
        /// </summary>
        double AverageDelay { get; }

        /// <summary>
        /// The total delay of <see cref="IndianaEnumerations.DetectorOn"/> arrival on red events
        /// </summary>
        double TotalDelay { get; }

        /// <summary>
        /// <see cref="IndianaEnumerations.DetectorOn"/> arrival on red events
        /// </summary>
        IReadOnlyList<Vehicle> Vehicles { get; }
    }
}
