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

using Newtonsoft.Json;
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
        /// <inheritdoc cref="ILocationLayer.LocationIdentifier"/>
        [JsonIgnore]
        public string LocationIdentifier { get; set; }
    }

    public abstract class AggregationApproachBase : AggregationModelBase, ILocationApproachLayer
    {
        /// <summary>
        /// Id of approach assigned to the location
        /// </summary>
        public int ApproachId { get; set; }
    }

    /// <summary>
    /// Represents per‑approach Phase Change Diagram (PCD) aggregation metrics used to
    /// evaluate how vehicles arrive relative to signal indications. These values help
    /// traffic engineers assess progression quality, phase utilization, and delay.
    /// </summary>
    public partial class ApproachPcdAggregation : AggregationApproachBase
    {
        /// <summary>
        /// Number of vehicles that arrived during a green indication.
        /// Often used to evaluate progression efficiency and coordinated flow quality.
        /// </summary>
        public int ArrivalsOnGreen { get; set; }

        /// <summary>
        /// Number of vehicles that arrived during a red indication.
        /// Higher values may indicate poor progression or excessive upstream congestion.
        /// </summary>
        public int ArrivalsOnRed { get; set; }

        /// <summary>
        /// Number of vehicles that arrived during a yellow indication.
        /// Useful for understanding clearance behavior and potential safety concerns.
        /// </summary>
        public int ArrivalsOnYellow { get; set; }

        /// <summary>
        /// Indicates whether the phase operates as a protected movement.
        /// Protected phases typically have different arrival and delay characteristics
        /// compared to permissive or mixed movements.
        /// </summary>
        public bool IsProtectedPhase { get; set; }

        /// <summary>
        /// The phase number associated with this approach.
        /// Corresponds to the controller’s phase configuration for the movement.
        /// </summary>
        public int PhaseNumber { get; set; }

        /// <summary>
        /// Total delay experienced by vehicles on this approach, measured in seconds.
        /// A key performance indicator for evaluating signal timing effectiveness.
        /// </summary>
        public int TotalDelay { get; set; }

        /// <summary>
        /// Total vehicle volume for the aggregation period.
        /// Represents all arrivals regardless of signal state.
        /// </summary>
        public int Volume { get; set; }
    }

    /// <summary>
    /// Represents aggregated speed statistics for a signalized approach.  
    /// These metrics help traffic engineers evaluate operating speeds,
    /// progression quality, and potential safety concerns related to speed variance.
    /// </summary>
    public partial class ApproachSpeedAggregation : AggregationApproachBase
    {
        /// <summary>
        /// The 15th percentile speed for the approach.  
        /// Often used as an indicator of slower‑moving traffic and potential congestion.
        /// </summary>
        public int Speed15th { get; set; }

        /// <summary>
        /// The 85th percentile speed for the approach.  
        /// Commonly used as a surrogate safety measure and to assess whether drivers
        /// are traveling significantly above the expected operating speed.
        /// </summary>
        public int Speed85th { get; set; }

        /// <summary>
        /// The total number of speed samples collected.  
        /// Represents the volume of vehicles for which speed measurements were recorded.
        /// </summary>
        public int SpeedVolume { get; set; }

        /// <summary>
        /// The sum of all individual vehicle speeds recorded during the aggregation period.  
        /// Used in calculating average approach speed and evaluating progression performance.
        /// </summary>
        public int SummedSpeed { get; set; }
    }

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

    /// <summary>
    /// Represents aggregated yellow and red activation metrics for a signalized approach.  
    /// These values help traffic engineers assess red‑light running frequency, severity,
    /// and driver behavior during the yellow change and red clearance intervals.
    /// </summary>
    public partial class ApproachYellowRedActivationAggregation : AggregationApproachBase
    {
        /// <summary>
        /// Total number of cycles observed during the aggregation period.  
        /// Provides context for interpreting violation frequency and activation rates.
        /// </summary>
        public int Cycles { get; set; }

        /// <summary>
        /// Indicates whether the phase operates as a protected movement.  
        /// Protected phases often exhibit different violation patterns than permissive movements.
        /// </summary>
        public bool IsProtectedPhase { get; set; }

        /// <summary>
        /// The phase number associated with this approach.  
        /// Corresponds to the controller’s configured phase for the movement.
        /// </summary>
        public int PhaseNumber { get; set; }

        /// <summary>
        /// Number of red‑light violations classified as severe, typically based on
        /// extended entry time into the red interval.  
        /// Useful for identifying high‑risk behaviors and potential safety issues.
        /// </summary>
        public int SevereRedLightViolations { get; set; }

        /// <summary>
        /// Total number of red‑light violations recorded for the approach.  
        /// A key indicator of compliance and potential need for timing or enforcement review.
        /// </summary>
        public int TotalRedLightViolations { get; set; }

        /// <summary>
        /// Total time, in seconds, that vehicles were detected entering during the red interval.  
        /// Helps quantify the severity and duration of red‑light running activity.
        /// </summary>
        public int ViolationTime { get; set; }

        /// <summary>
        /// Number of detector activations occurring during the yellow interval.  
        /// Useful for evaluating driver decision‑making in the dilemma zone and
        /// assessing whether yellow timing is appropriate.
        /// </summary>
        public int YellowActivations { get; set; }
    }

    /// <summary>
    /// Represents aggregated gap‑related metrics for a permissive or protected‑permissive
    /// left‑turn phase. These values help traffic engineers evaluate gap availability,
    /// driver gap‑acceptance behavior, and the adequacy of permissive operation.
    /// </summary>
    public partial class PhaseLeftTurnGapAggregation : AggregationApproachBase, ILocationPhaseLayer
    {
        /// <summary>
        /// Number of accepted or available gaps falling into bin 1.  
        /// Gap bins typically represent ranges of gap durations used to analyze driver behavior.
        /// </summary>
        public int GapCount1 { get; set; }

        /// <summary>
        /// Number of accepted or available gaps falling into bin 10.
        /// </summary>
        public int GapCount10 { get; set; }

        /// <summary>
        /// Number of accepted or available gaps falling into bin 11.
        /// </summary>
        public int GapCount11 { get; set; }

        /// <summary>
        /// Number of accepted or available gaps falling into bin 2.
        /// </summary>
        public int GapCount2 { get; set; }

        /// <summary>
        /// Number of accepted or available gaps falling into bin 3.
        /// </summary>
        public int GapCount3 { get; set; }

        /// <summary>
        /// Number of accepted or available gaps falling into bin 4.
        /// </summary>
        public int GapCount4 { get; set; }

        /// <summary>
        /// Number of accepted or available gaps falling into bin 5.
        /// </summary>
        public int GapCount5 { get; set; }

        /// <summary>
        /// Number of accepted or available gaps falling into bin 6.
        /// </summary>
        public int GapCount6 { get; set; }

        /// <summary>
        /// Number of accepted or available gaps falling into bin 7.
        /// </summary>
        public int GapCount7 { get; set; }

        /// <summary>
        /// Number of accepted or available gaps falling into bin 8.
        /// </summary>
        public int GapCount8 { get; set; }

        /// <summary>
        /// Number of accepted or available gaps falling into bin 9.
        /// </summary>
        public int GapCount9 { get; set; }

        /// <summary>
        /// The phase number associated with this left‑turn movement.  
        /// Corresponds to the controller’s configured phase for the approach.
        /// </summary>
        public int PhaseNumber { get; set; }

        /// <summary>
        /// Total duration, in seconds, of gaps categorized into duration bin 1.  
        /// Useful for understanding the distribution of shorter gaps.
        /// </summary>
        public double SumGapDuration1 { get; set; }

        /// <summary>
        /// Total duration, in seconds, of gaps categorized into duration bin 2.
        /// </summary>
        public double SumGapDuration2 { get; set; }

        /// <summary>
        /// Total duration, in seconds, of gaps categorized into duration bin 3.  
        /// Longer gaps often indicate favorable permissive conditions.
        /// </summary>
        public double SumGapDuration3 { get; set; }

        /// <summary>
        /// Total green time available for permissive left‑turn operations, in seconds.  
        /// Provides context for evaluating gap availability and utilization.
        /// </summary>
        public double SumGreenTime { get; set; }
    }
}