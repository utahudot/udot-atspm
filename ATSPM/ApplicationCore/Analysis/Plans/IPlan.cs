using ATSPM.Data.Enums;
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;

namespace ATSPM.Application.Analysis.Plans
{
    /// <summary>
    /// Definition for signal controller plans which are derrived from <see cref="DataLoggerEnum.CoordPatternChange"/> events
    /// </summary>
    public interface IPlan : IStartEndRange, ISignalLayer
    {
        /// <summary>
        /// Plan number as derrived from the event parameter on <see cref="DataLoggerEnum.CoordPatternChange"/> event
        /// </summary>
        int PlanNumber { get; set; }

        /// <summary>
        /// Tries to assign an <see cref="IStartEndRange"/> object to the plan
        /// </summary>
        /// <param name="range"></param>
        /// <returns>Returns true if successful</returns>
        bool TryAssignToPlan(IStartEndRange range);
    }
}
