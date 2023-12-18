using ATSPM.Domain.Common;
using ATSPM.Data.Enums;
using System.Text.Json;
using System;
using System.Collections.Generic;

namespace ATSPM.Application.Analysis.Plans
{
    /// <summary>
    /// Base for Location controller plans which are derrived from <see cref="DataLoggerEnum.CoordPatternChange"/> events
    /// </summary>
    public class Plan : StartEndRange, IPlan
    {
        #region IPlan

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

        /// <inheritdoc/>
        public int PlanNumber { get; set; }

        /// <inheritdoc/>
        public virtual void AssignToPlan<T>(IEnumerable<T> range) where T : IStartEndRange
        {
            throw new NotImplementedException();
        }

        #endregion

        public override bool InRange(DateTime time)
        {
            return time >= Start && time <= End;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
