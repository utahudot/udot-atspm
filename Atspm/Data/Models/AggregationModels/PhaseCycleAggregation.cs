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
    /// Represents aggregated phase cycle metrics for a signalized approach.  
    /// These values help traffic engineers evaluate phase timing consistency,
    /// cycle frequency, and the distribution of green, yellow, and red intervals.
    /// </summary>
    public partial class PhaseCycleAggregation : AggregationApproachBase, ILocationPhaseLayer
    {
        /// <summary>
        /// Total green time provided across all cycles, expressed in seconds.  
        /// Useful for evaluating phase allocation and identifying shifts in demand.
        /// </summary>
        public int GreenTime { get; set; }

        /// <summary>
        /// The number of times the phase began during the aggregation period.  
        /// Helps validate cycle counts and detect skipped or unused phases.
        /// </summary>
        public int PhaseBeginCount { get; set; }

        /// <summary>
        /// The phase number associated with this movement.  
        /// Corresponds to the controller’s configured phase for the approach.
        /// </summary>
        public int PhaseNumber { get; set; }

        /// <summary>
        /// Total red time accumulated across all cycles, expressed in seconds.  
        /// Useful for understanding cycle length distribution and clearance intervals.
        /// </summary>
        public int RedTime { get; set; }

        /// <summary>
        /// Total number of cycles measured from the start of one green interval
        /// to the start of the next green interval.  
        /// Often used to evaluate consistency in cycle length and coordination.
        /// </summary>
        public int TotalGreenToGreenCycles { get; set; }

        /// <summary>
        /// Total number of cycles measured from the start of one red interval
        /// to the start of the next red interval.  
        /// Helps assess cycle regularity and detect unusual phase behavior.
        /// </summary>
        public int TotalRedToRedCycles { get; set; }

        /// <summary>
        /// Total yellow time provided across all cycles, expressed in seconds.  
        /// Important for evaluating change interval adequacy and driver decision behavior.
        /// </summary>
        public int YellowTime { get; set; }
    }
}