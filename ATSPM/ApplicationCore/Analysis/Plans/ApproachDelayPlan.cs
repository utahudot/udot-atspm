using ATSPM.Application.Analysis.Common;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ATSPM.Application.Analysis.Plans
{
    /// <summary>
    /// Plan specific to approach delay information
    /// </summary>
    public class ApproachDelayPlan : Plan, IApproachDelay
    {
        private readonly List<Vehicle> _vehicles = new();

        #region IApproachDelay

        /// <inheritdoc/>
        public double AverageDelay => Vehicles.Average(a => a.Delay);

        /// <inheritdoc/>
        public double TotalDelay => Vehicles.Sum(s => s.Delay) / 3600;

        /// <inheritdoc/>
        [JsonIgnore]
        public IReadOnlyList<Vehicle> Vehicles => _vehicles;

        #endregion

        #region IPlan

        /// <inheritdoc/>
        public override bool TryAssignToPlan(IStartEndRange range)
        {
            if (InRange(range.Start) && InRange(range.End))
            {
                if (range is Vehicle v && SignalIdentifier == v.SignalIdentifier)
                {
                    _vehicles.Add(v);
                    return true;
                }
            }

            return false;
        }

        #endregion

        
    }
}
