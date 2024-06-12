#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Analysis.Plans/ApproachDelayPlan.cs
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
        //    _vehicles.AddRange(range.Cast<Vehicle>().Where(w => InRange(w.Start) && InRange(w.End) && LocationIdentifier == w.LocationIdentifier));
        //}

        public override void AssignToPlan<T>(IEnumerable<T> range)
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}
