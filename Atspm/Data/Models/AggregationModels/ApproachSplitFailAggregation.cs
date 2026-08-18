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
    /// Represents aggregated split failure metrics for a signalized approach.  
    /// These values help traffic engineers evaluate whether a phase is receiving
    /// adequate green time and how often queues fail to clear within the allotted split.
    /// </summary>
    public partial class ApproachSplitFailAggregation : AggregationApproachBase
    {
        /// <summary>
        /// Total number of cycles observed during the aggregation period.  
        /// Provides context for interpreting split failures and occupancy patterns.
        /// </summary>
        public int Cycles { get; set; }

        /// <summary>
        /// Sum of detector occupancy during the green interval, expressed in seconds.  
        /// High green occupancy may indicate sustained demand or insufficient green time.
        /// </summary>
        public int GreenOccupancySum { get; set; }

        /// <summary>
        /// Total green time provided across all cycles, expressed in seconds.  
        /// Useful for evaluating phase allocation and comparing demand to supply.
        /// </summary>
        public int GreenTimeSum { get; set; }

        /// <summary>
        /// Indicates whether the phase operates as a protected movement.  
        /// Protected phases often exhibit different split failure characteristics
        /// compared to permissive or protected‑permissive operations.
        /// </summary>
        public bool IsProtectedPhase { get; set; }

        /// <summary>
        /// The phase number associated with this approach.  
        /// Corresponds to the controller’s configured phase for the movement.
        /// </summary>
        public int PhaseNumber { get; set; }

        /// <summary>
        /// Sum of detector occupancy during the red interval, expressed in seconds.  
        /// Elevated red occupancy often signals residual queues or unmet demand.
        /// </summary>
        public int RedOccupancySum { get; set; }

        /// <summary>
        /// Total red time across all cycles, expressed in seconds.  
        /// Helps contextualize red occupancy and queue persistence.
        /// </summary>
        public int RedTimeSum { get; set; }

        /// <summary>
        /// Number of cycles in which the queue failed to clear before the end of green.  
        /// A key indicator of insufficient green allocation or oversaturated conditions.
        /// </summary>
        public int SplitFailures { get; set; }
    }
}