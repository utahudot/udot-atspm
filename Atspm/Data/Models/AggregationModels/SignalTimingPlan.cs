#region license
// Copyright 2026 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models/AggregationModelBase.cs
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

using Utah.Udot.Atspm.Data.Interfaces;
using Utah.Udot.NetStandardToolkit.Common;

#nullable disable

#pragma warning disable 

namespace Utah.Udot.Atspm.Data.Models
{
    /// <summary>
    /// Represents aggregated information for a specific signal timing plan.  
    /// Timing plans define coordinated operation, cycle length, offsets, and splits,
    /// and this model allows performance data to be grouped by the active plan.
    /// </summary>
    public partial class SignalTimingPlan : StartEndRange, ILocationLayer, IPlanLayer
    {
        /// <inheritdoc cref="ILocationLayer.LocationIdentifier"/>
        public string LocationIdentifier { get; set; }

        /// <inheritdoc cref="IPlanLayer.PlanNumber"/>
        public short PlanNumber { get; set; }

        /// <summary>
        /// Calculates true if the timing plan is valid, which is determined by whether the end time is greater than the start time.
        /// </summary>
        public bool Valid { get; private set; }
    }
}