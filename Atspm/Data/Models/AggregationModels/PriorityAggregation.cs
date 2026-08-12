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
    /// Represents aggregated priority‑service activity for a signalized location.  
    /// These values help traffic engineers evaluate how often priority is requested
    /// and how frequently the controller provides early‑green or extended‑green service.
    /// </summary>
    public partial class PriorityAggregation : AggregationModelBase, ILocationLayer
    {
        /// <summary>
        /// The identifier of the priority request type (e.g., transit, freight, bicycle).  
        /// Used to distinguish between different priority strategies operating at the location.
        /// </summary>
        public int PriorityNumber { get; set; }

        /// <summary>
        /// Total number of priority requests received during the aggregation period.  
        /// Reflects how often priority‑eligible vehicles or systems attempted to influence signal timing.
        /// </summary>
        public int PriorityRequests { get; set; }

        /// <summary>
        /// Number of times the controller provided early‑green service in response to a priority request.  
        /// Early‑green shortens the red interval to reduce delay for priority vehicles.
        /// </summary>
        public int PriorityServiceEarlyGreen { get; set; }

        /// <summary>
        /// Number of times the controller extended the green interval to serve a priority request.  
        /// Extended‑green helps priority vehicles clear the intersection without stopping.
        /// </summary>
        public int PriorityServiceExtendedGreen { get; set; }
    }
}