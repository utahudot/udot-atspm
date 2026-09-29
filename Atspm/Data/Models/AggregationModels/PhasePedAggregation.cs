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
    /// Represents aggregated pedestrian‑related metrics for a signalized phase.  
    /// These values help traffic engineers evaluate pedestrian demand, delay,
    /// call registration behavior, and the consistency of pedestrian service.
    /// </summary>
    public partial class PhasePedAggregation : AggregationModelBase, ILocationPhaseLayer
    {
        /// <summary>
        /// The phase number associated with this left‑turn movement.  
        /// Corresponds to the controller’s configured phase for the approach.
        /// </summary>
        public int PhaseNumber { get; set; }

        /// <summary>
        /// Number of times the pedestrian interval began with a WALK indication.  
        /// Useful for validating pedestrian service frequency and identifying skipped service.
        /// </summary>
        public int PedBeginWalkCount { get; set; }

        /// <summary>
        /// Total number of pedestrian calls registered during the aggregation period.  
        /// Reflects how often pedestrians requested service via pushbutton or detection.
        /// </summary>
        public int PedCallsRegisteredCount { get; set; }

        /// <summary>
        /// Total number of pedestrian cycles served.  
        /// Represents the number of times the pedestrian phase was displayed.
        /// </summary>
        public int PedCycles { get; set; }

        /// <summary>
        /// Average pedestrian delay, in seconds.  
        /// A key performance measure for evaluating pedestrian level of service.
        /// </summary>
        public double PedDelay { get; set; }

        /// <summary>
        /// Total number of pedestrian service requests, including calls and detections.  
        /// Helps quantify overall pedestrian demand.
        /// </summary>
        public int PedRequests { get; set; }

        /// <summary>
        /// Estimated number of pedestrian calls inferred from detection or behavior  
        /// when a physical button press was not explicitly recorded.  
        /// Useful for identifying potential button failures or non‑actuated crossings.
        /// </summary>
        public int ImputedPedCallsRegistered { get; set; }

        /// <summary>
        /// Maximum observed pedestrian delay, in seconds.  
        /// Helps identify extreme wait times and potential accessibility concerns.
        /// </summary>
        public double MaxPedDelay { get; set; }

        /// <summary>
        /// Minimum observed pedestrian delay, in seconds.  
        /// Useful for understanding variability in pedestrian service.
        /// </summary>
        public double MinPedDelay { get; set; }

        /// <summary>
        /// Number of unique pedestrian detections recorded.  
        /// Helps validate pedestrian presence and evaluate detection system performance.
        /// </summary>
        public int UniquePedDetections { get; set; }
    }
}