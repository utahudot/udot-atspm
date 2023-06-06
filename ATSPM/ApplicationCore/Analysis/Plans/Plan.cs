using ATSPM.Domain.Common;
using ATSPM.Data.Enums;
using System.Text.Json;
using ATSPM.Data.Interfaces;

namespace ATSPM.Application.Analysis.Plans
{
    /// <summary>
    /// Base for signal controller plans which are derrived from <see cref="DataLoggerEnum.CoordPatternChange"/> events
    /// </summary>
    public abstract class Plan : StartEndRange, IPlan
    {
        #region ISignalLayer

        /// <inheritdoc/>
        public string SignalIdentifier { get; set; }

        #endregion

        #region IPlan

        /// <inheritdoc/>
        public int PlanNumber { get; set; }

        /// <inheritdoc/>
        public abstract bool TryAssignToPlan(IStartEndRange range);

        #endregion

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
