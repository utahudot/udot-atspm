﻿#region license
// Copyright 2024 Utah Departement of Transportation
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

using Newtonsoft.Json;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Interfaces;
using Utah.Udot.NetStandardToolkit.Common;

#nullable disable

#pragma warning disable 

namespace Utah.Udot.Atspm.Data.Models
{
    /// <summary>
    /// Aggregation model base for models used in Atspm data aggregations
    /// </summary>
    public abstract class AggregationModelBase : StartEndRange, ILocationLayer
    {
        ///<inheritdoc/>
        [JsonIgnore]
        public string LocationIdentifier { get; set; }

        [Obsolete("this has bee replaced with StartEndRange")]
        [JsonIgnore]
        public DateTime BinStartTime { get; set; }
    }

    public partial class ApproachPcdAggregation : AggregationModelBase, ILocationApproachLayer
    {
        //public string LocationIdentifier { get; set; }
        public int PhaseNumber { get; set; }

        ///<inheritdoc/>
        public int ApproachId { get; set; }

        public bool IsProtectedPhase { get; set; }
        public int ArrivalsOnGreen { get; set; }
        public int ArrivalsOnRed { get; set; }
        public int ArrivalsOnYellow { get; set; }
        public int Volume { get; set; }
        public int TotalDelay { get; set; }
    }

    public partial class ApproachSpeedAggregation : AggregationModelBase, ILocationApproachLayer
    {
        //public string LocationIdentifier { get; set; }

        ///<inheritdoc/>
        public int ApproachId { get; set; }

        public int SummedSpeed { get; set; }
        public int SpeedVolume { get; set; }
        public int Speed85th { get; set; }
        public int Speed15th { get; set; }
    }

    public partial class ApproachSplitFailAggregation : AggregationModelBase, ILocationApproachLayer
    {
        //public string LocationIdentifier { get; set; }
        public int PhaseNumber { get; set; }

        ///<inheritdoc/>
        public int ApproachId { get; set; }

        public bool IsProtectedPhase { get; set; }
        public int SplitFailures { get; set; }
        public int GreenOccupancySum { get; set; }
        public int RedOccupancySum { get; set; }
        public int GreenTimeSum { get; set; }
        public int RedTimeSum { get; set; }
        public int Cycles { get; set; }
    }

    public partial class ApproachYellowRedActivationAggregation : AggregationModelBase, ILocationApproachLayer
    {
        //public string LocationIdentifier { get; set; }
        public int PhaseNumber { get; set; }

        ///<inheritdoc/>
        public int ApproachId { get; set; }

        public bool IsProtectedPhase { get; set; }
        public int SevereRedLightViolations { get; set; }
        public int TotalRedLightViolations { get; set; }
        public int YellowActivations { get; set; }
        public int ViolationTime { get; set; }
        public int Cycles { get; set; }
    }

    /// <summary>
    /// Detector event cout aggregation
    /// </summary>
    public partial class DetectorEventCountAggregation : AggregationModelBase, ILocationApproachLayer
    {
        /// <inheritdoc/>
        //public string LocationIdentifier { get; set; }


        ///<inheritdoc/>
        public int ApproachId { get; set; }

        /// <summary>
        /// Detector id
        /// </summary>
        public int DetectorPrimaryId { get; set; }

        /// <summary>
        /// Sum of <see cref="IndianaEnumerations.VehicleDetectorOn"/> events
        /// </summary>
        public int EventCount { get; set; }

        /// <inheritdoc/>
        public override string ToString() => JsonConvert.SerializeObject(this);
    }

    public partial class PhaseCycleAggregation : AggregationModelBase, ILocationApproachLayer, ILocationPhaseLayer
    {
        //public string LocationIdentifier { get; set; }

        ///<inheritdoc/>
        public int ApproachId { get; set; }

        public int PhaseNumber { get; set; }
        public int RedTime { get; set; }
        public int YellowTime { get; set; }
        public int GreenTime { get; set; }
        public int TotalRedToRedCycles { get; set; }
        public int TotalGreenToGreenCycles { get; set; }
    }

    public partial class PhaseLeftTurnGapAggregation : AggregationModelBase, ILocationApproachLayer, ILocationPhaseLayer
    {
        //public string LocationIdentifier { get; set; }

        ///<inheritdoc/>
        public int PhaseNumber { get; set; }

        ///<inheritdoc/>
        public int ApproachId { get; set; }

        public int GapCount1 { get; set; }
        public int GapCount2 { get; set; }
        public int GapCount3 { get; set; }
        public int GapCount4 { get; set; }
        public int GapCount5 { get; set; }
        public int GapCount6 { get; set; }
        public int GapCount7 { get; set; }
        public int GapCount8 { get; set; }
        public int GapCount9 { get; set; }
        public int GapCount10 { get; set; }
        public int GapCount11 { get; set; }
        public double SumGapDuration1 { get; set; }
        public double SumGapDuration2 { get; set; }
        public double SumGapDuration3 { get; set; }
        public double SumGreenTime { get; set; }
    }

    public partial class PhasePedAggregation : AggregationModelBase, ILocationPhaseLayer
    {
        ///<inheritdoc/>
        public int PhaseNumber { get; set; }
        public int PedCycles { get; set; }
        public int PedDelay { get; set; }
        public int MinPedDelay { get; set; }
        public int MaxPedDelay { get; set; }
        public int ImputedPedCallsRegistered { get; set; }
        public int UniquePedDetections { get; set; }
        public int PedBeginWalkCount { get; set; }
        public int PedCallsRegisteredCount { get; set; }
        public int PedRequests { get; set; }
    }

    public partial class PhaseSplitMonitorAggregation : AggregationModelBase, ILocationPhaseLayer
    {
        //public string LocationIdentifier { get; set; }

        ///<inheritdoc/>
        public int PhaseNumber { get; set; }

        public int EightyFifthPercentileSplit { get; set; }
        public int SkippedCount { get; set; }
    }

    /// <summary>
    /// Phase termination aggregation
    /// </summary>
    public partial class PhaseTerminationAggregation : AggregationModelBase, ILocationPhaseLayer
    {
        //public string LocationIdentifier { get; set; }

        /// <inheritdoc/>
        public int PhaseNumber { get; set; }

        /// <summary>
        /// Sum of consecutive <see cref="4"/> events
        /// </summary>
        public int GapOuts { get; set; }

        /// <summary>
        /// Sum of consecutive <see cref="IndianaEnumerations.PhaseMaxOut"/> events
        /// </summary>
        public int ForceOffs { get; set; }

        /// <summary>
        /// Sum of consecutive <see cref="6"/> events
        /// </summary>
        public int MaxOuts { get; set; }

        /// <summary>
        /// Sum of consecutive <see cref="IndianaEnumerations.PhaseGreenTermination"/> events
        /// </summary>
        public int Unknown { get; set; }

        /// <inheritdoc/>
        public override string ToString() => JsonConvert.SerializeObject(this);
    }

    /// <summary>
    /// Preemption aggregation
    /// </summary>
    public partial class PreemptionAggregation : AggregationModelBase, ILocationLayer
    {
        /// <inheritdoc/>
        //public string LocationIdentifier { get; set; }

        /// <summary>
        /// Prempt number
        /// </summary>
        public int PreemptNumber { get; set; }

        /// <summary>
        /// <see cref="IndianaEnumerations.PreemptCallInputOn"/> Set when preemption input is activated
        /// </summary>
        public int PreemptRequests { get; set; }

        /// <summary>
        /// <see cref="IndianaEnumerations.PreemptEntryStarted"/> Set when preemption delay expires
        /// </summary>
        public int PreemptServices { get; set; }

        /// <inheritdoc/>
        public override string ToString() => JsonConvert.SerializeObject(this);
    }

    /// <summary>
    /// Priority request aggregation
    /// </summary>
    public partial class PriorityAggregation : AggregationModelBase, ILocationLayer
    {
        /// <inheritdoc/>
        //public string LocationIdentifier { get; set; }

        /// <summary>
        /// Priority number
        /// </summary>
        public int PriorityNumber { get; set; }

        ///<summary>
        /// <see cref="112"/> Set when request for priority is received
        ///</summary>
        public int PriorityRequests { get; set; }

        ///<summary>
        /// <see cref="113"/> Set when controller is adjusting active cycle to accommodate early service to TSP phases
        ///</summary>
        public int PriorityServiceEarlyGreen { get; set; }

        ///<summary>
        /// <see cref="114"/> Set when controller is adjusting active cycle to accommodate extended service to TSP phases
        ///</summary>
        public int PriorityServiceExtendedGreen { get; set; }

        /// <inheritdoc/>
        public override string ToString() => JsonConvert.SerializeObject(this);
    }

    public partial class SignalEventCountAggregation : AggregationModelBase, ILocationLayer
    {
        //public string LocationIdentifier { get; set; }
        public int EventCount { get; set; }
    }

    /// <summary>
    /// Signal plan aggregation
    /// </summary>
    public partial class SignalPlanAggregation : AggregationModelBase, IPlanLayer
    {
        /// <inheritdoc/>
        //public string LocationIdentifier { get; set; }

        /// <inheritdoc/>
        public int PlanNumber { get; set; }
    }
}