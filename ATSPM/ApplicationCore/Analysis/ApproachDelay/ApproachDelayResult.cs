#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.ApproachDelay/ApproachDelayResult.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
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
    public class ApproachDelayResult : StartEndRange, IApproachDelay, ILocationPhaseLayer
    {
        [JsonIgnore]
        public IReadOnlyList<ApproachDelayPlan> Plans { get; set; }

        #region ILocationPhaseLayer

        /// <inheritdoc/>
        public string LocationIdentifier { get; set; }

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
