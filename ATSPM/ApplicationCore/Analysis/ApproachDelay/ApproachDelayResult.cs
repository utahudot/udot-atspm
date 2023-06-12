using ATSPM.Application.Analysis.Common;
using ATSPM.Application.Analysis.Plans;
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ATSPM.Application.Analysis.ApproachDelay
{
    public class ApproachDelayResult : StartEndRange, IApproachDelay, ISignalPhaseLayer
    {
        [JsonIgnore]
        public IReadOnlyList<ApproachDelayPlan> Plans { get; set; } = new List<ApproachDelayPlan>();

        #region ISignalPhaseLayer

        /// <inheritdoc/>
        public string SignalIdentifier { get; set; }

        /// <inheritdoc/>
        public int PhaseNumber { get; set; }

        #endregion

        #region IApproachDelay

        /// <inheritdoc/>
        public double AverageDelay => Plans.Average(a => a.AverageDelay);

        /// <inheritdoc/>
        public double TotalDelay => Plans.Sum(s => s.TotalDelay);

        /// <inheritdoc/>
        [JsonIgnore]
        public IReadOnlyList<Vehicle> Vehicles => Plans.SelectMany(m => m.Vehicles).ToList();

        #endregion

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
