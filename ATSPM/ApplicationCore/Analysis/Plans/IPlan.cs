using ATSPM.Data.Enums;
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using System.Collections;
using System.Collections.Generic;

namespace ATSPM.Application.Analysis.Plans
{
    /// <summary>
    /// Definition for signal controller plans which are derrived from <see cref="DataLoggerEnum.CoordPatternChange"/> events
    /// </summary>
    public interface IPlan : IStartEndRange, ISignalLayer, IPlanLayer
    {
        /// <summary>
        /// Tries to assign an <see cref="IStartEndRange"/> object to the plan
        /// </summary>
        /// <param name="range"></param>
        /// <returns>Returns true if successful</returns>
        void AssignToPlan<T>(IEnumerable<T> range) where T : IStartEndRange;
    }
}
