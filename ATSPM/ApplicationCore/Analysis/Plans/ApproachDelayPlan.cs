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
        public IReadOnlyList<Vehicle> Vehicles { get; set; }

        //{
        //    get { return _vehicles; }
        //    set { AssignToPlan(value); }
        //}

        #endregion

        #region IPlan


        /// <inheritdoc/>
        //public override void AssignToPlan<T>(IEnumerable<T> range)
        //{
        //    _vehicles.AddRange(range.Cast<Vehicle>().Where(w => InRange(w.Start) && InRange(w.End) && SignalIdentifier == w.SignalIdentifier));
        //}

        public override void AssignToPlan<T>(IEnumerable<T> range)
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}
