#region license
// Copyright 2025 Utah Departement of Transportation
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
        /// <summary>
        /// Identifer of the location
        /// </summary>
        [JsonIgnore]
        public string LocationIdentifier { get; set; }
    }

    public abstract class  AggregationApproachBase : AggregationModelBase, ILocationApproachLayer
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


    /// <summary>
    /// Represents aggregated split‑monitoring metrics for a signalized phase.  
    /// These values help traffic engineers evaluate phase utilization, skipped service,
    /// and how actual green time compares to typical operating conditions.
    /// </summary>
    public partial class PhaseSplitMonitorAggregation : AggregationModelBase, ILocationPhaseLayer
    {
        /// <summary>
        /// The 85th percentile split time, in seconds, for the phase.  
        /// Useful for understanding upper‑range green durations and identifying phases
        /// that frequently require more time than the median cycle provides.
        /// </summary>
        public int EightyFifthPercentileSplit { get; set; }

        /// <summary>
        /// The phase number associated with this movement.  
        /// Corresponds to the controller’s configured phase for the approach.
        /// </summary>
        public int PhaseNumber { get; set; }

        /// <summary>
        /// Number of cycles in which the phase was skipped.  
        /// Helps identify low‑demand phases or potential detection issues.
        /// </summary>
        public int SkippedCount { get; set; }
    }

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
    }

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

    /// <summary>
    /// Represents the total number of signal controller events recorded for a location
    /// during the aggregation period.  
    /// This metric helps traffic engineers assess controller activity levels,
    /// detect unusual event patterns, and validate system communication.
    /// </summary>
    public partial class SignalEventCountAggregation : AggregationModelBase, ILocationLayer
    {
        /// <summary>
        /// Total number of signal controller events captured.  
        /// Useful for monitoring controller health, logging frequency,
        /// and identifying periods of abnormal activity.
        /// </summary>
        public int EventCount { get; set; }
    }

    /// <summary>
    /// Represents aggregated information for a specific signal timing plan.  
    /// Timing plans define coordinated operation, cycle length, offsets, and splits,
    /// and this model allows performance data to be grouped by the active plan.
    /// </summary>
    public partial class SignalPlanAggregation : AggregationModelBase, IPlanLayer
    {
        /// <summary>
        /// The identifier of the signal timing plan active during the aggregation period.  
        /// Used to associate performance metrics with specific coordination patterns
        /// or time‑of‑day schedules.
        /// </summary>
        public int PlanNumber { get; set; }
    }
}