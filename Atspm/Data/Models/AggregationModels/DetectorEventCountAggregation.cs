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

#nullable disable

#pragma warning disable 

namespace Utah.Udot.Atspm.Data.Models
{
    /// <summary>
    /// Represents aggregated detector event activity for a specific approach.  
    /// These values help traffic engineers evaluate detector performance,
    /// activation frequency, and potential issues with detection hardware or placement.
    /// </summary>
    public partial class DetectorEventCountAggregation : AggregationApproachBase
    {
        /// <summary>
        /// The unique identifier for the primary detector associated with the approach.  
        /// Used to link event activity to a specific detection point in the field.
        /// </summary>
        public int DetectorPrimaryId { get; set; }

        /// <summary>
        /// Total number of detector events recorded during the aggregation period.  
        /// Useful for assessing detector responsiveness, traffic presence, and potential malfunctions.
        /// </summary>
        public int EventCount { get; set; }
    }
}