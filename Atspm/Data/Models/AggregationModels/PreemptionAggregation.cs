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

#nullable disable

#pragma warning disable 

namespace Utah.Udot.Atspm.Data.Models
{
    /// <summary>
    /// Represents aggregated preemption activity for a signalized location.  
    /// These values help traffic engineers evaluate how often preemption is requested,
    /// how frequently it is served, and which preempt sequence is being activated.
    /// </summary>
    public partial class PreemptionAggregation : AggregationModelBase, ILocationLayer
    {
        /// <summary>
        /// The identifier of the preempt sequence (e.g., emergency vehicle, railroad, transit).  
        /// Used to distinguish between different types of preemption operations.
        /// </summary>
        public int PreemptNumber { get; set; }

        /// <summary>
        /// Total number of preemption requests received during the aggregation period.  
        /// Reflects how often priority vehicles or systems attempted to initiate preemption.
        /// </summary>
        public int PreemptRequests { get; set; }

        /// <summary>
        /// Total number of preemption services actually delivered.  
        /// Useful for identifying missed requests, controller limitations, or conflicting operations.
        /// </summary>
        public int PreemptServices { get; set; }

        public override string ToString()
        {
            return $"result: {Start} - {End} - {LocationIdentifier} - {PreemptNumber} - {PreemptRequests} - {PreemptServices}";
        }
    }
}