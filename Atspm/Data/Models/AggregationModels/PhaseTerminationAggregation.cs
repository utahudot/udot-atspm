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
    /// Represents aggregated phase termination metrics for a signalized phase.  
    /// These values help traffic engineers understand how often a phase ends due to
    /// gap‑out, max‑out, force‑off, or other termination conditions, which is essential
    /// for evaluating detector performance, timing parameters, and phase utilization.
    /// </summary>
    public partial class PhaseTerminationAggregation : AggregationModelBase, ILocationPhaseLayer
    {
        /// <summary>
        /// Number of times the phase terminated due to a force‑off.  
        /// Indicates that coordination or timing constraints ended the phase
        /// before demand was fully served.
        /// </summary>
        public int ForceOffs { get; set; }

        /// <summary>
        /// Number of times the phase terminated due to a gap‑out.  
        /// Occurs when no vehicles are detected within the allowable passage time,
        /// often indicating low or intermittent demand.
        /// </summary>
        public int GapOuts { get; set; }

        /// <summary>
        /// Number of times the phase terminated due to max‑out.  
        /// Suggests that demand exceeded the available green time and the phase
        /// reached its programmed maximum duration.
        /// </summary>
        public int MaxOuts { get; set; }

        /// <summary>
        /// The phase number associated with this movement.  
        /// Corresponds to the controller’s configured phase for the approach.
        /// </summary>
        public int PhaseNumber { get; set; }

        /// <summary>
        /// Number of phase terminations that did not match a known termination type.  
        /// Useful for identifying data anomalies or controller behaviors outside
        /// standard termination classifications.
        /// </summary>
        public int Unknown { get; set; }
    }
}