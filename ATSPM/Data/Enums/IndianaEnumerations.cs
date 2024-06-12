#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Enums/IndianaEnumerations.cs
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
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Enums
{
    /// <summary>
    /// Traffic Location Hi Resolution Data Logger Enumerations
    /// <seealso cref="Reference" href="https://docs.lib.purdue.edu/cgi/viewcontent.cgi?article=1002&context=jtrpdata"/>
    /// </summary>
    public enum IndianaEnumerations : ushort
    {
        #region Indiana Specification

        ///<summary>
        ///Set when NEMA Phase On becomes active, either upon start of green or walk interval, whichever occurs first
        ///</summary>
        [Display(Name = "Phase On")]
        PhaseOn = 0,

        ///<summary>
        ///Set when either solid or flashing green indication has begun.
        ///Do not set repeatedly during flashing operation
        ///</summary>
        [Display(Name = "Phase Begin Green")]
        PhaseBeginGreen = 1,

        ///<summary>
        ///Set when a conflicting call is registered against the active phase. (Marks beginning of MAX timing)
        ///</summary>
        [Display(Name = "Phase Check")]
        PhaseCheck = 2,

        ///<summary>
        ///Set when phase min timer expires
        ///</summary>
        [Display(Name = "Phase Min Complete")]
        PhaseMinComplete = 3,

        ///<summary>
        ///Phase termination due to gap out termination condition.
        ///Set once per phase when phase gaps out but may not necessarily occur upon phase termination
        ///</summary>
        [Display(Name = "Phase Gap Out")]
        PhaseGapOut = 4,

        ///<summary>
        ///Set when phase MAX timer expires but may not necessarily occur upon phase termination due to last car passage or other features
        ///</summary>
        [Display(Name = "Phase Max Out")]
        PhaseMaxOut = 5,

        ///<summary>
        ///Set when phase force off is applied by the coordinator to the active green phase
        ///</summary>
        [Display(Name = "Phase Force Off")]
        PhaseForceOff = 6,

        ///<summary>
        ///Set when phase green indications are terminated into either yellow change interval or permissive (FYA) movement
        ///</summary>
        [Display(Name = "Phase Green Termination")]
        PhaseGreenTermination = 7,

        ///<summary>
        ///Set when phase yellow indication becomes active and interval timer begins
        ///</summary>
        [Display(Name = "Phase Begin Yellow Change")]
        PhaseBeginYellowChange = 8,

        ///<summary>
        ///Set when phase yellow indication becomes inactive
        ///</summary>
        [Display(Name = "Phase End Yellow Change")]
        PhaseEndYellowChange = 9,

        ///<summary>
        ///Set only if phase red clearance is served. Set when red clearance timing begins
        ///</summary>
        [Display(Name = "Phase Begin Red Clearance")]
        PhaseBeginRedClearance = 10,

        ///<summary>
        ///Set only if phase red clearance is served. Set when red clearance timing concludes.
        ///This may not necessarily coincide with completion of the phase,
        ///especially during clearance of trailing overlaps, red revert timing, red rest, or delay for other ring terminations
        ///</summary>
        [Display(Name = "Phase End Red Clearance")]
        PhaseEndRedClearance = 11,

        ///<summary>
        ///Set when the phase is no longer active within the ring,
        ///including completion of any trailing overlaps or end of barrier delays for adjacent ring termination
        ///</summary>
        [Display(Name = "Phase Inactive")]
        PhaseInactive = 12,

        ///<summary>
        ///Set when phase extension timer gaps out
        ///</summary>
        [Display(Name = "Extension Timer Gap Out")]
        ExtensionTimerGapOut = 13,

        ///<summary>
        ///Set when phase in the programmed ring is skipped for any reason
        ///</summary>
        [Display(Name = "Phase Skipped")]
        PhaseSkipped = 14,

        ///<summary>
        ///Set when extension timer starts to reduce (the time before reduction)
        ///</summary>
        [Display(Name = "Extension Timer Reduction Start")]
        ExtensionTimerReductionStart = 15,

        ///<summary>
        ///Set when extension timer minimum is reached (after the time to reduce)
        ///</summary>
        [Display(Name = "Extension Timer Minimum Achieved")]
        ExtensionTimerMinimumAchieved = 16,

        ///<summary>
        ///Set when phase added initial timer expires
        ///</summary>
        [Display(Name = "Added Initial Complete")]
        AddedInitialComplete = 17,

        ///<summary>
        ///Set when the controller determines a phase will be next to begin green after the current active phase(s) end red clearance
        ///</summary>
        [Display(Name = "Next Phase Decision")]
        NextPhaseDecision = 18,

        ///<summary>
        ///Set when TSP early force off is applied to an active phase
        ///</summary>
        [Display(Name = "TSP Early Force Off")]
        TSPEarlyForceOffPhase = 19,

        ///<summary>
        ///Set when controller applies preemption force off to the active cycle
        ///</summary>
        [Display(Name = "Preemption Force Off")]
        PreemptionForceOffCycle = 20,

        ///<summary>
        ///Set when walk indication becomes active
        ///</summary>
        [Display(Name = "Pedestrian Begin Walk")]
        PedestrianBeginWalk = 21,

        ///<summary>
        ///Set when flashing don’t walk indication becomes active
        ///</summary>
        [Display(Name = "Pedestrian Begin Change Interval")]
        PedestrianBeginChangeInterval = 22,

        ///<summary>
        ///Set when don’t walk indication becomes solid (non-flashing) from either termination of pedestrian change interval,
        ///or head illumination after a pedestrian dark interval
        ///</summary>
        [Display(Name = "Pedestrian Begin Solid Don’t Walk")]
        PedestrianBeginSolidDontWalk = 23,

        ///<summary>
        ///Set when the pedestrian outputs are set off
        ///</summary>
        [Display(Name = "Pedestrian Dark")]
        PedestrianDark = 24,

        ///<summary>
        ///Set when extended pedestrian change interval is requested by pressing the pedestrian push button for two (2) seconds.
        ///See 2009 MUTCD Section 4E.13 - Accessible Pedestrian Locations and Detectors - Extended Pushbutton Press Features for more details
        ///</summary>
        [Display(Name = "Extended Pedestrian Change Interval")]
        ExtendedPedestrianChangeInterval = 25,

        ///<summary>
        ///Set when pedestrian phase is active beyond pedestrian change interval or force off point
        ///</summary>
        [Display(Name = "Oversized Pedestrian Served")]
        OversizedPedestrianServed = 26,

        //[Display(Name = "Pedestrian events reserved for future use.")]
        //Pedestrianeventsreservedforfutureuse = 27 - 30, //"

        ///<summary>
        ///Set when all active phases become inactive in the ring and cross barrier phases are next to be served
        ///</summary>
        [Display(Name = "Barrier Termination")]
        BarrierTermination = 31,

        ///<summary>
        ///Set when flashing yellow arrow becomes active
        ///</summary>
        [Display(Name = "FYA – Begin Permissive")]
        FYABeginPermissive = 32,

        ///<summary>
        ///Set when flashing yellow arrow becomes inactive through either clearance of the permissive movement or transition into a protected movement
        ///</summary>
        [Display(Name = "FYA – End Permissive")]
        FYAEndPermissive = 33,

        //[Display(Name = "Barrier events reserve for future use.")]
        //Barriereventsreserveforfutureuse = 34-40,  //"

        ///<summary>
        ///Set when phase hold is applied by the coordinator, preemptor, or external logic.
        ///Phase does not necessarily need to be actively timing for this event to occur
        ///</summary>
        [Display(Name = "Phase Hold Active")]
        PhaseHoldActive = 41,

        ///<summary>
        ///Set when phase hold is released by the coordinator, preemptor, or external logic.
        ///Phase does not necessarily need to be actively timing for this event to occur
        ///</summary>
        [Display(Name = "Phase Hold Released")]
        PhaseHoldReleased = 42,

        ///<summary>
        ///Call to service on a phase is registered by vehicular demand.
        ///This event will not be set if a recall exists on the phase
        ///</summary>
        [Display(Name = "Phase Call Registered")]
        PhaseCallRegistered = 43,

        ///<summary>
        ///Call to service on a phase is cleared by either service of the phase or removal of call
        ///</summary>
        [Display(Name = "Phase Call Dropped")]
        PhaseCallDropped = 44,

        ///<summary>
        ///Call to service on a phase is registered by pedestrian demand.
        ///This event will not be set if a recall exists on the phase
        ///</summary>
        [Display(Name = "Pedestrian Call Registered")]
        PedestrianCallRegistered = 45,

        ///<summary>
        ///Set when phase omit is applied by the coordinator, preemptor, or other dynamic sources.
        ///Phase does not necessarily need to be actively timing for this event to occur.
        ///This event is not set when phase is removed from the active sequence or other configuration-level change has occurred
        ///</summary>
        [Display(Name = "Phase Omit On")]
        PhaseOmitOn = 46,

        ///<summary>
        ///Set when phase omit is released by the coordinator, preemptor, or other dynamic sources.
        ///Phase does not necessarily need to be actively timing for this event to occur.
        ///This event is not set when phase is added from the active sequence or other configuration-level change has occurred
        ///</summary>
        [Display(Name = "Phase Omit Off")]
        PhaseOmitOff = 47,

        ///<summary>
        ///Set when pedestrian omit is applied by the coordinator, preemptor, or other dynamic sources.
        ///Phase does not necessarily need to be actively timing for this event to occur.
        ///This event is not set when phase is removed from the active sequence or other configuration-level change has occurred
        ///</summary>
        [Display(Name = "Pedestrian Omit On")]
        PedestrianOmitOn = 48,

        ///<summary>
        ///Set when pedestrian omit is released by the coordinator, preemptor, or other dynamic sources.
        ///Phase does not necessarily need to be actively timing for this event to occur.
        ///This event is not set when phase is added from the active sequence or other configuration-level change has occurred
        ///</summary>
        [Display(Name = "Pedestrian Omit Off")]
        PedestrianOmitOff = 49,

        ///<summary>
        ///Set when maximum green (MAX 1) interval is in-effect for the active phase
        ///</summary>
        [Display(Name = "MAX 1 In-Effect")]
        MAX1InEffect = 50,

        ///<summary>
        ///Set when maximum green (MAX 2) interval is in-effect for the active phase
        ///</summary>
        [Display(Name = "MAX 2 In-Effect")]
        MAX2InEffect = 51,

        ///<summary>
        ///Set when dynamic max interval is in-effect for the active phase.
        ///This event shall be populated upon termination of MAX green (MAX 1 or MAX 2) interval
        ///</summary>
        [Display(Name = "Dynamic MAX In-Effect")]
        DynamicMAXInEffect = 52,

        ///<summary>
        ///Set when dynamic max interval steps up for the active phase (initially after two consecutive phase max out events)
        ///</summary>
        [Display(Name = "Dynamic MAX Step Up")]
        DynamicMAXStepUp = 53,

        ///<summary>
        ///Set when dynamic max interval steps down for the active phase (initially after two consecutive phase gap out events)
        ///</summary>
        [Display(Name = "Dynamic MAX Step Down")]
        DynamicMAXStepDown = 54,

        ///<summary>
        ///Set when advance warning sign is on
        ///</summary>
        [Display(Name = "Advance Warning Sign On")]
        AdvanceWarningSignOn = 55,

        ///<summary>
        ///Set when advance warning sign is off
        ///</summary>
        [Display(Name = "Advance Warning Sign Off")]
        AdvanceWarningSignOff = 56,

        //[Display(Name = "Phase Control Events reserved for future use")]
        //PhaseControlEventsreservedforfutureuse = 57 - 60,   //"

        ///<summary>
        ///Set when overlap becomes green.
        ///Do not set repeatedly when overlap is flashing green.
        ///Note that overlap colors are consistent to the GYR intervals resultant from the controller programming and may not be indicative of actual Location head colors
        ///</summary>
        [Display(Name = "Overlap Begin Green")]
        OverlapBeginGreen = 61,

        ///<summary>
        ///Set when overlap is green and extension timers begin timing
        ///</summary>
        [Display(Name = "Overlap Begin Trailing Green (Extension)")]
        OverlapBeginTrailingGreenExtension = 62,

        ///<summary>
        ///Set when overlap is in a yellow change interval state
        ///</summary>
        [Display(Name = "Overlap Begin Yellow")]
        OverlapBeginYellow = 63,

        ///<summary>
        ///Set when overlap begins timing red clearance intervals
        ///</summary>
        [Display(Name = "Overlap Begin Red Clearance")]
        OverlapBeginRedClearance = 64,

        ///<summary>
        ///Set when overlap has completed all timing,
        ///allowing any conflicting phase next to begin service
        ///</summary>
        [Display(Name = "Overlap Off (Inactive with red indication)")]
        OverlapOffInactivewithredindication = 65,

        ///<summary>
        ///Set when overlap head is set dark (no active outputs).
        ///The end of this interval shall be recorded by either an overlap off state or other active overlap state
        ///</summary>
        [Display(Name = "Overlap Dark")]
        OverlapDark = 66,

        ///<summary>
        ///Set when walk indication becomes active
        ///</summary>
        [Display(Name = "Pedestrian Overlap Begin Walk")]
        PedestrianOverlapBeginWalk = 67,

        ///<summary>
        ///Set when flashing don’t walk indication becomes active
        ///</summary>
        [Display(Name = "Pedestrian Overlap Begin Clearance")]
        PedestrianOverlapBeginClearance = 68,

        ///<summary>
        ///Set when don’t walk indication becomes solid (non flashing) from either termination of ped clearance,
        ///or head illumination after a ped dark interval
        ///</summary>
        [Display(Name = "Pedestrian Overlap Begin Solid Don’t Walk")]
        PedestrianOverlapBeginSolidDontWalk = 69,

        ///<summary>
        ///Set when the pedestrian outputs are set off
        ///</summary>
        [Display(Name = "Pedestrian Overlap Dark")]
        PedestrianOverlapDark = 70,

        ///<summary>
        ///Set when advance warning sign becomes active
        ///</summary>
        [Display(Name = "Advance Warning Sign On")]
        AdvanceWarningSignActive = 71,

        ///<summary>
        ///Set when advance warning sign becomes inactive
        ///</summary>
        [Display(Name = "Advance Warning Sign Off")]
        AdvanceWarningSignInactive = 72,

        //[Display(Name = "Overlap events reserved for future use.")]
        //Overlapeventsreservedforfutureuse = 73 - 80,    //"

        ///<summary>
        ///Detector on and off events shall be triggered post any detector delay/extension processing
        ///</summary>
        [Display(Name = "Vehicle Detector Off")]
        VehicleDetectorOff = 81,

        ///<summary>
        ///Detector on and off events shall be triggered post any detector delay/extension processing
        ///</summary>
        [Display(Name = "Vehicle Detector On")]
        VehicleDetectorOn = 82,

        ///<summary>
        ///Detector restored to non-failed state by either manual restoration or re-enabling via continued diagnostics
        ///</summary>
        [Display(Name = "Detector Restored")]
        DetectorRestored = 83,

        ///<summary>
        ///Detector failure logged upon local controller diagnostics only (not system diagnostics)
        ///</summary>
        [Display(Name = "Detector Fault- Other")]
        DetectorFaultOther = 84,

        ///<summary>
        ///Detector failure logged upon local controller diagnostics only (not system diagnostics)
        ///</summary>
        [Display(Name = "Detector Fault- Watchdog Fault")]
        DetectorFaultWatchdogFault = 85,

        ///<summary>
        ///Detector failure logged upon local controller diagnostics only (not system diagnostics)
        ///</summary>
        [Display(Name = "Detector Fault- Open Loop Fault")]
        DetectorFaultOpenLoopFault = 86,

        ///<summary>
        ///Detector failure logged upon local controller diagnostics only (not system diagnostics)
        ///</summary>
        [Display(Name = "Detector Fault- Shorted Loop Fault")]
        DetectorFaultShortedLoopFault = 87,

        ///<summary>
        ///Detector failure logged upon local controller diagnostics only (not system diagnostics)
        ///</summary>
        [Display(Name = "Detector Fault- Excessive Change Fault")]
        DetectorFaultExcessiveChangeFault = 88,

        ///<summary>
        ///Ped detector events shall be triggered post any detector delay/extension processing and may be set multiple times for a single pedestrian call.
        ///(with future intent to eventually support ped presence and volume)
        ///</summary>
        [Display(Name = "PedDetector Off")]
        PedDetectorOff = 89,

        ///<summary>
        ///Ped detector events shall be triggered post any detector delay/extension processing and may be set multiple times for a single pedestrian call.
        ///(with future intent to eventually support ped presence and volume)
        ///</summary>
        [Display(Name = "PedDetector On")]
        PedDetectorOn = 90,

        ///<summary>
        ///Detector failure logged upon local controller diagnostics only (not system diagnostics)
        ///</summary>
        [Display(Name = "Pedestrian Detector Failed")]
        PedestrianDetectorFailed = 91,

        ///<summary>
        ///Detector failure logged upon local controller diagnostics only (not system diagnostics)
        ///</summary>
        [Display(Name = "Pedestrian Detector Restored")]
        PedestrianDetectorRestored = 92,

        ///<summary>
        ///TSP detector events shall be triggered post any detector delay/extension processing
        ///</summary>
        [Display(Name = "TSP Detector Off")]
        TSPDetectorOff = 93,

        ///<summary>
        ///TSP detector events shall be triggered post any detector delay/extension processing
        ///</summary>
        [Display(Name = "TSP Detector On")]
        TSPDetectorOn = 94,

        //[Display(Name = "Detector events reserved for future use.")]
        //Detectoreventsreservedforfutureuse = 95-100,   //"

        ///<summary>
        ///Set when preemption advance warning input is activated
        ///</summary>
        [Display(Name = "Preempt Advance Warning Input")]
        PreemptAdvanceWarningInput = 101,

        ///<summary>
        ///Set when preemption input is activated.
        ///(prior to preemption delay timing) May be set multiple times if input is intermittent during preemption service
        ///</summary>
        [Display(Name = "Preempt (Call) Input On")]
        PreemptCallInputOn = 102,

        ///<summary>
        ///Set when gate down input is received by the controller (if available)
        ///</summary>
        [Display(Name = "Preempt Gate Down Input Received")]
        PreemptGateDownInputReceived = 103,

        ///<summary>
        ///Set when preemption input is de-activated.
        ///May be set multiple times if input is intermittent preemption service
        ///</summary>
        [Display(Name = "Preempt (Call) Input Off")]
        PreemptCallInputOff = 104,

        ///<summary>
        ///Set when preemption delay expires,
        ///and controller begins transition timing (force off) to serve preemption
        ///</summary>
        [Display(Name = "Preempt Entry Started")]
        PreemptEntryStarted = 105,

        ///<summary>
        ///Set when track clearance phases are green and track clearance timing begins
        ///</summary>
        [Display(Name = "Preemption Begin Track Clearance")]
        PreemptionBeginTrackClearance = 106,

        ///<summary>
        ///Set when preemption dwell or limited service begins,
        ///or minimum dwell timer is reset due to call drop and reapplication
        ///</summary>
        [Display(Name = "Preemption Begin Dwell Service")]
        PreemptionBeginDwellService = 107,

        ///<summary>
        ///Set when linked preemptor input is applied from active preemptor
        ///</summary>
        [Display(Name = "Preemption Link Active On")]
        PreemptionLinkActiveOn = 108,

        ///<summary>
        ///Set when linked preemptor input is dropped from active preemptor
        ///</summary>
        [Display(Name = "Preemption Link Active Off")]
        PreemptionLinkActiveOff = 109,

        ///<summary>
        ///Set when preemption max presence timer is exceeded,
        ///and preemption input is released from service
        ///</summary>
        [Display(Name = "Preemption Max Presence Exceeded")]
        PreemptionMaxPresenceExceeded = 110,

        ///<summary>
        ///Set when preemption exit interval phases are green and exit timing begins
        ///</summary>
        [Display(Name = "Preemption Begin Exit Interval")]
        PreemptionBeginExitInterval = 111,

        ///<summary>
        ///Set when request for priority is received
        ///</summary>
        [Display(Name = "TSP Check In")]
        TSPCheckIn = 112,

        ///<summary>
        ///Set when controller is adjusting active cycle to accommodate early service to TSP phases
        ///</summary>
        [Display(Name = "TSP Adjustment to Early Green")]
        TSPAdjustmenttoEarlyGreen = 113,

        ///<summary>
        ///Set when controller is adjusting active cycle to accommodate extended service to TSP phases
        ///</summary>
        [Display(Name = "TSP Adjustment to Extend Green")]
        TSPAdjustmenttoExtendGreen = 114,

        ///<summary>
        ///Set when request for priority is retracted
        ///</summary>
        [Display(Name = "TSP Check Out")]
        TSPCheckOut = 115,

        ///<summary>
        ///Set when preemption force off is applied to the active cycle
        ///</summary>
        [Display(Name = "Preemption Force Off")]
        PreemptionForceOffPhase = 116,

        ///<summary>
        ///Set when TSP early force off is applied to the active cycle
        ///</summary>
        [Display(Name = "TSP Early Force Off")]
        TSPEarlyForceOffCycle = 117,

        ///<summary>
        ///Set when requested TSP service begins
        ///</summary>
        [Display(Name = "TSP Service Start")]
        TSPServiceStart = 118,

        ///<summary>
        ///Set when requested TSP service ends
        ///</summary>
        [Display(Name = "TSP Service End")]
        TSPServiceEnd = 119,

        //[Display(Name = "Preemption/TSP Events reserved for future use")]
        //PreemptionTSPEventsreservedforfutureuse = 120 - 130,    //"

        ///<summary>
        ///Coordination pattern that is actively running in the controller.
        ///(Highest priority of TOD, System or manual command). 
        ///This event will not be reapplied if coordination is temporarily suspended for preemption or other external control
        ///</summary>
        [Display(Name = "Coord Pattern Change")]
        CoordPatternChange = 131,

        ///<summary>
        ///This event shall be populated upon selection of a new coordination pattern change that selects a new cycle length.
        ///Cycle lengths in excess of 255 shall record this event with a 255 parameter, along with event code 156
        ///</summary>
        [Display(Name = "Cycle Length Change")]
        CycleLengthChange = 132,

        ///<summary>
        ///This event shall be populated upon selection of a new coordination pattern change that selects a new cycle length.
        ///Offsets in  excess of 255 shall record this event with a 255 parameter,
        ///requiring controller database lookup for this actual value
        ///</summary>
        [Display(Name = "Offset Length Change")]
        OffsetLengthChange = 133,

        ///<summary>
        ///Split change events shall be populated upon selection of a new coordination pattern
        ///as well as during a split change to an active pattern via Location Control and Prioritization (SCP),
        ///Adaptive Control System (ACS) Lite or other adaptive control system
        ///</summary>
        [Display(Name = "Split 1 Change")]
        Split1Change = 134,

        ///<summary>
        ///Split 2 Change
        ///</summary>
        [Display(Name = "Split 2 Change")]
        Split2Change = 135,

        ///<summary>
        ///Split 3 Change
        ///</summary>
        [Display(Name = "Split 3 Change")]
        Split3Change = 136,

        ///<summary>
        ///Split 4 Change
        ///</summary>
        [Display(Name = "Split 4 Change")]
        Split4Change = 137,

        ///<summary>
        ///Split 5 Change
        ///</summary>
        [Display(Name = "Split 5 Change")]
        Split5Change = 138,

        ///<summary>
        ///Split 6 Change
        ///</summary>
        [Display(Name = "Split 6 Change")]
        Split6Change = 139,

        ///<summary>
        ///Split 7 Change
        ///</summary>
        [Display(Name = "Split 7 Change")]
        Split7Change = 140,

        ///<summary>
        ///Split 8 Change
        ///</summary>
        [Display(Name = "Split 8 Change")]
        Split8Change = 141,

        ///<summary>
        ///Split 9 Change
        ///</summary>
        [Display(Name = "Split 9 Change")]
        Split9Change = 142,

        ///<summary>
        ///Split 10 Change
        ///</summary>
        [Display(Name = "Split 10 Change")]
        Split10Change = 143,

        ///<summary>
        ///Split 11 Change
        ///</summary>
        [Display(Name = "Split 11 Change")]
        Split11Change = 144,

        ///<summary>
        ///Split 12 Change
        ///</summary>
        [Display(Name = "Split 12 Change")]
        Split12Change = 145,

        ///<summary>
        ///Split 13 Change
        ///</summary>
        [Display(Name = "Split 13 Change")]
        Split13Change = 146,

        ///<summary>
        ///Split 14 Change
        ///</summary>
        [Display(Name = "Split 14 Change")]
        Split14Change = 147,

        ///<summary>
        ///Split 15 Change
        ///</summary>
        [Display(Name = "Split 15 Change")]
        Split15Change = 148,

        ///<summary>
        ///Split 16 Change
        ///</summary>
        [Display(Name = "Split 16 Change")]
        Split16Change = 149,

        ///<summary>
        ///Coord cycle state change
        ///</summary>
        [Display(Name = "Coord cycle state change")]
        Coordcyclestatechange = 150,

        ///<summary>
        ///Set once per cycle for each coordinated phase when controller suspends the coordinated phase
        ///</summary>
        [Display(Name = "Coordinated phase yield point")]
        Coordinatedphaseyieldpoint = 151,

        ///<summary>
        ///Set when coordinated phase begins
        ///</summary>
        [Display(Name = "Coordinated phase begin")]
        Coordinatedphasebegin = 152,

        ///<summary>
        ///Set when the logic statement evaluation changes from “False” to “True”
        ///</summary>
        [Display(Name = "Logic Statement True")]
        LogicStatementTrue = 153,

        ///<summary>
        ///Set when the logic statement evaluation changes from “True” to “False”
        ///</summary>
        [Display(Name = "Logic Statement False")]
        LogicStatementFalse = 154,

        ///<summary>
        ///See NTCIP 1202v0326 5.4.5 for definition
        ///</summary>
        [Display(Name = "Unit Control Status Change")]
        UnitControlStatusChange = 155,

        ///<summary>
        ///Set simultaneously with Event Code 132 when existing cycle length exceeds 255 seconds.
        ///When this Event Code 156 is used, Event Code 132 shall be 255, 
        ///where the value of this Event Code 156 will be the additional cycle length above 255 (in seconds).
        ///Cycle lengths longer than 510 seconds will require a controller look up
        ///</summary>
        [Display(Name = "Additional Cycle Length Change")]
        AdditionalCycleLengthChange = 156,

        //[Display(Name = "Coordination events reserved for future use.")]
        //Coordinationeventsreservedforfutureuse = 157 - 170,

        ///<summary>
        ///Cabinet test or special function input as defined by the local controller
        ///</summary>
        [Display(Name = "Test Input On")]
        TestInputOn = 171,

        ///<summary>
        ///Cabinet test or special function input as defined by the local controller
        ///</summary>
        [Display(Name = "Test Input Off")]
        TestInputOff = 172,

        ///<summary>
        ///See NTCIP 1202 2.4.6 for definition
        ///</summary>
        [Display(Name = "Unit Flash Status Change")]
        UnitFlashStatusChange = 173,

        ///<summary>
        ///See NTCIP 1202 2.4.8 for definition
        ///</summary>
        [Display(Name = "Unit Alarm Status 1 Change")]
        UnitAlarmStatus1Change = 174,

        ///<summary>
        ///See NTCIP 1202 2.4.12.2 for definition
        ///</summary>
        [Display(Name = "Alarm Group State Change")]
        AlarmGroupStateChange = 175,

        ///<summary>
        ///Special function output as defined by the local controller
        ///</summary>
        [Display(Name = "Special Function Output On")]
        SpecialFunctionOutputOn = 176,

        ///<summary>
        ///Special function output as defined by the local controller
        ///</summary>
        [Display(Name = "Special Function Output Off")]
        SpecialFunctionOutputOff = 177,

        ///<summary>
        ///Special function output as defined by the local controller
        ///</summary>
        [Display(Name = "Manual control enable On/Off")]
        ManualcontrolenableOnOff = 178,

        ///<summary>
        ///Leading edge on (1), lagging edge (0) optional
        ///</summary>
        [Display(Name = "Interval Advance On/Off")]
        IntervalAdvanceOnOff = 179,

        ///<summary>
        ///Set when stop time input is applied or removed, regardless of source of stop or state
        ///</summary>
        [Display(Name = "Stop Time Input On/Off")]
        StopTimeInputOnOff = 180,

        ///<summary>
        ///Set when the controller OS clock is adjusted via communications, OS command, or external input
        ///</summary>
        [Display(Name = "Controller Clock Updated")]
        ControllerClockUpdated = 181,

        ///<summary>
        ///Line voltage drops between 0-89 volts AC for more than 100ms
        ///</summary>
        [Display(Name = "Power Failure Detected")]
        PowerFailureDetected = 182,

        ///<summary>
        ///Line voltage applied/reapplied greater than 98 volts AC
        ///</summary>
        [Display(Name = "Power Restored")]
        PowerRestored = 184,

        ///<summary>
        ///Placeholder for generic failure/alarm types as defined by vendor
        ///</summary>
        [Display(Name = "Vendor Specific Alarm")]
        VendorSpecificAlarm = 185,

        //[Display(Name = "Cabinet/System events reserved for future use.")]
        //CabinetSystemeventsreservedforfutureuse = 186 - 199,

        ///<summary>
        ///Set when cabinet/system alarm is activated
        ///</summary>
        [Display(Name = "Alarm On")]
        AlarmOn = 200,

        ///<summary>
        ///Set when cabinet/system alarm is released
        ///</summary>
        [Display(Name = "Alarm Off")]
        AlarmOff = 201,

        ///<summary>
        ///Set when local controller aux switch is active (1) or inactive (0)
        ///</summary>
        [Display(Name = "Aux Switch On/Off")]
        AuxSwitchOnOff = 202,

        ///<summary>
        ///Split change events shall be populated upon selection of a new coordination pattern as well as during a split change to an active pattern via Location Control and Prioritization (SCP),
        ///Adaptive Control System (ACS) Lite or other adaptive control system
        ///</summary>
        [Display(Name = "Split 17 Change")]
        Split17Change = 203,

        ///<summary>
        ///Split 18 Change
        ///</summary>
        [Display(Name = "Split 18 Change")]
        Split18Change = 204,

        ///<summary>
        ///Split 19 Change
        ///</summary>
        [Display(Name = "Split 19 Change")]
        Split19Change = 205,

        ///<summary>
        ///Split 20 Change
        ///</summary>
        [Display(Name = "Split 20 Change")]
        Split20Change = 206,

        ///<summary>
        ///Split 21 Change
        ///</summary>
        [Display(Name = "Split 21 Change")]
        Split21Change = 207,

        ///<summary>
        ///Split 22 Change
        ///</summary>
        [Display(Name = "Split 22 Change")]
        Split22Change = 208,

        ///<summary>
        ///Split 23 Change
        ///</summary>
        [Display(Name = "Split 23 Change")]
        Split23Change = 209,

        ///<summary>
        ///Split 24 Change
        ///</summary>
        [Display(Name = "Split 24 Change")]
        Split24Change = 210,

        ///<summary>
        ///Split 25 Change
        ///</summary>
        [Display(Name = "Split 25 Change")]
        Split25Change = 211,

        ///<summary>
        ///Split 26 Change
        ///</summary>
        [Display(Name = "Split 26 Change")]
        Split26Change = 212,

        ///<summary>
        ///Split 27 Change
        ///</summary>
        [Display(Name = "Split 27 Change")]
        Split27Change = 213,

        ///<summary>
        ///Split 28 Change
        ///</summary>
        [Display(Name = "Split 28 Change")]
        Split28Change = 214,

        ///<summary>
        ///Split 29 Change
        ///</summary>
        [Display(Name = "Split 29 Change")]
        Split29Change = 215,

        ///<summary>
        ///Split 30 Change
        ///</summary>
        [Display(Name = "Split 30 Change")]
        Split30Change = 216,

        ///<summary>
        ///Split 31 Change
        ///</summary>
        [Display(Name = "Split 31 Change")]
        Split31Change = 217,

        ///<summary>
        ///Split 32 Change
        ///</summary>
        [Display(Name = "Split 32 Change")]
        Split32Change = 218,

        #endregion

        #region QFree Specification

        /*

        ///<summary>
        ///Set when NEMA Phase On becomes active, either upon start of green or walk interval, whichever occurs first.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase On")]
        PhaseOn = 0,

        ///<summary>
        ///Set when either steady or flashing green indication has begun.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Begin Green")]
        PhaseBeginGreen = 1,

        ///<summary>
        ///Set when a conflicting call is registered against the active phase.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Check")]
        PhaseCheck = 2,

        ///<summary>
        ///Set when phase min timer expires.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Min Complete")]
        PhaseMinComplete = 3,

        ///<summary>
        ///Phase termination due to gap out termination condition. Set once per phase when phase gaps out but may not necessarily occur upon phase termination.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Gap Out")]
        PhaseGapOut = 4,

        ///<summary>
        ///Set when phase max timer expires, but may not necessarily occur upon phase termination.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Max Out")]
        PhaseMaxOut = 5,

        ///<summary>
        ///Set when phase forceoff is applied by the coordinator to the active green phase.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Forceoff")]
        PhaseForceoff = 6,

        ///<summary>
        ///Set when phase green indications are terminated into either yellow change interval or permissive (FYA) movement.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Green Termination")]
        PhaseGreenTermination = 7,

        ///<summary>
        ///Set when phase yellow indication becomes active and interval timer begins.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Begin Yellow Clearance")]
        PhaseBeginYellowClearance = 8,

        ///<summary>
        ///Set when phase yellow indication become inactive.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase End Yellow Clearance")]
        PhaseEndYellowClearance = 9,

        ///<summary>
        ///Set only if phase red clearance is served.  Set when red clearance timing begins.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Begin Red Clearance")]
        PhaseBeginRedClearance = 10,

        ///<summary>
        ///Set only if phase red clearance is served.  Set when red clearance timing concludes.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase End Red Clearance")]
        PhaseEndRedClearance = 11,

        ///<summary>
        ///Set when the phase is no longer active within the ring, including completion of any trailing overlaps or end of barrier delays.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Inactive")]
        PhaseInactive = 12,

        ///<summary>
        ///Set when the extension timer gaps out.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Extension Timer Gap Out")]
        ExtensionTimerGapOut = 13,

        ///<summary>
        ///Set when phase is skipped.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Skipped")]
        PhaseSkipped = 14,

        ///<summary>
        ///Set when extension timer starts to reduce (the time before reduction).
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Extension Timer Reduction Start")]
        ExtensionTimerReductionStart = 15,

        ///<summary>
        ///Set when the extension timer minimum is reached (after the time to reduce).
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Extension Timer Minimum Achieved")]
        ExtensionTimerMinimumAchieved = 16,

        ///<summary>
        ///Set when added initial is achieved.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Added Initial Complete")]
        AddedInitialComplete = 17,

        ///<summary>
        ///Set when the controller determines a phase will be next to begin green after the current active phase(s) end red clearance.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Next Phase Decision")]
        NextPhaseDecision = 18,

        ///<summary>
        ///Set when TSP early forceoff is applied to an active phase.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "TSP Early Forceoff Phase")]
        TSPEarlyForceoffPhase = 19,

        ///<summary>
        ///Set when the controller applies preemption forceoff to the active cycle.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Preemption Forceoff Cycle")]
        PreemptionForceoffCycle = 20,

        ///<summary>
        ///Set when walk indication becomes active.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Pedestrian Begin Walk")]
        PedestrianBeginWalk = 21,

        ///<summary>
        ///Set when flashing don't walk indication becomes active.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Pedestrian Begin Clearance")]
        PedestrianBeginClearance = 22,

        ///<summary>
        ///Set when don't walk indication becomes steady (non flashing) from either termination of ped clearance, or after a dark interval.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Pedestrian Begin Steady Don't Walk")]
        PedestrianBeginSteadyDontWalk = 23,

        ///<summary>
        ///Set when the pedestrian outputs are set off.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Pedestrian Dark")]
        PedestrianDark = 24,

        ///<summary>
        ///Set when extended pedestrian change interval is requested.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Extended Pedestrian Change Interval")]
        ExtendedPedestrianChangeInterval = 25,

        ///<summary>
        ///Oversized ped served in coord.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Oversized Ped Served")]
        OversizedPedServed = 26,

        ///<summary>
        ///Set when all active phases become inactive in the ring and cross barrier phases are next to be served.
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Termination")]
        BarrierTermination = 31,

        ///<summary>
        ///Set when FYA becomes active.
        ///The <c>Parameter</c> is Overlap Number
        ///</summary>
        [Display(Name = "FYA - Begin Permissive")]
        FYABeginPermissive = 32,

        ///<summary>
        ///Set when FYA becomes inactive through either clearance of the permissive movement or transition into a protected movement.
        ///The <c>Parameter</c> is Overlap Number
        ///</summary>
        [Display(Name = "FYA - End Permissive")]
        FYAEndPermissive = 33,

        ///<summary>
        ///Set when phase hold is applied by the coordinator, preemptor, or external logic.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Hold Active")]
        PhaseHoldActive = 41,

        ///<summary>
        ///Set when phase hold is released by the coordinator, preemptor, or external logic.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Hold Released")]
        PhaseHoldReleased = 42,

        ///<summary>
        ///Call to service on a phase is registered by vehicular demand.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Call Registered")]
        PhaseCallRegistered = 43,

        ///<summary>
        ///Call to service on a phase is cleared by either service of the phase or removal of call.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Call Dropped")]
        PhaseCallDropped = 44,

        ///<summary>
        ///Call to service on a phase is registered by pedestrian demand.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Pedestrian Call Registered")]
        PedestrianCallRegistered = 45,

        ///<summary>
        ///Set when phase omit is applied by the coordinator, preemptor, or other dynamic sources.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Omit On")]
        PhaseOmitOn = 46,

        ///<summary>
        ///Set when phase omit is released by the coordinator, preemptor, or other dynamic sources.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Omit Off")]
        PhaseOmitOff = 47,

        ///<summary>
        ///Set when ped omit is applied by the coordinator, preemptor, or other dynamic sources.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Pedestrian Omit On")]
        PedestrianOmitOn = 48,

        ///<summary>
        ///Set when ped omit is released by the coordinator, preemptor, or other dynamic sources.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Pedestrian Omit Off")]
        PedestrianOmitOff = 49,

        ///<summary>
        ///Set when maximum green (MAX 1) interval is in-effect for the active phase.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "MAX 1 In-Effect")]
        MAX1InEffect = 50,

        ///<summary>
        ///Set when maximum green (MAX 2) interval is in-effect for the active phase.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "MAX 2 In-Effect")]
        MAX2InEffect = 51,

        ///<summary>
        ///Set when dynamic max interval is in-effect for the active phase. Populated upon termination if MAX green (MAX 1 or MAX 2) interval
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Dynamic MAX In-Effect")]
        DynamicMAXInEffect = 52,

        ///<summary>
        ///Set when dynamic max interval steps up for the active phase (initially after two consecutive phase max out events).
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Dynamic MAX Step Up")]
        DynamicMAXStepUp = 53,

        ///<summary>
        ///Set when dynamic max begins step down for the actuve phase (initially after two consecutive phase gap out events).
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Dynamic MAX Step Down")]
        DynamicMAXStepDown = 54,

        ///<summary>
        ///Set when advance warning sign is on.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Advance Warning Sign On")]
        PhaseAdvanceWarningSignOn = 55,

        ///<summary>
        ///Set when advance warning sign is off.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Phase Advance Warning Sign Off")]
        PhaseAdvanceWarningSignOff = 56,

        ///<summary>
        ///Set when overlap becomes green. Do not set repeatedly when overlap is flashing green.
        ///The <c>Parameter</c> is Overlap Number
        ///</summary>
        [Display(Name = "Overlap Begin Green")]
        OverlapBeginGreen = 61,

        ///<summary>
        ///Set when overlap is green and extension timers begin timing.
        ///The <c>Parameter</c> is Overlap Number
        ///</summary>
        [Display(Name = "Overlap Begin Trailing Green (Extension)")]
        OverlapBeginTrailingGreenExtension = 62,

        ///<summary>
        ///Set when overlap is in a yellow clearance state.
        ///The <c>Parameter</c> is Overlap Number
        ///</summary>
        [Display(Name = "Overlap Begin Yellow")]
        OverlapBeginYellow = 63,

        ///<summary>
        ///Set when overlap begins timing red clearance intervals.
        ///The <c>Parameter</c> is Overlap Number
        ///</summary>
        [Display(Name = "Overlap Begin Red Clearance")]
        OverlapBeginRedClearance = 64,

        ///<summary>
        ///Set when overlap has completed all timing, allowing any conflicting phase next to begin service.
        ///The <c>Parameter</c> is Overlap Number
        ///</summary>
        [Display(Name = "Overlap Off (Inactive with red indication)")]
        OverlapOffInactivewithredindication = 65,

        ///<summary>
        ///Set when overlap head is set dark (no active outputs).
        ///The <c>Parameter</c> is Overlap Number
        ///</summary>
        [Display(Name = "Overlap Dark")]
        OverlapDark = 66,

        ///<summary>
        ///Set when walk indication becomes active.
        ///The <c>Parameter</c> is Overlap Number
        ///</summary>
        [Display(Name = "Pedestrian Overlap Begin Walk")]
        PedestrianOverlapBeginWalk = 67,

        ///<summary>
        ///Set when flashing don't walk indication becomes active.
        ///The <c>Parameter</c> is Overlap Number
        ///</summary>
        [Display(Name = "Pedestrian Overlap Begin Clearance")]
        PedestrianOverlapBeginClearance = 68,

        ///<summary>
        ///Set when don't walk indication becomes steady (non flashing) from either termination of ped clearance, or after a dark interval.
        ///The <c>Parameter</c> is Overlap Number
        ///</summary>
        [Display(Name = "Pedestrian Overlap Begin Steady Don't Walk")]
        PedestrianOverlapBeginSteadyDontWalk = 69,

        ///<summary>
        ///Set when the pedestrian outputs are set off.
        ///The <c>Parameter</c> is Overlap Number
        ///</summary>
        [Display(Name = "Pedestrian Overlap Dark")]
        PedestrianOverlapDark = 70,

        ///<summary>
        ///Set when advance warning sign becomes active.
        ///The <c>Parameter</c> is Overlap Number
        ///</summary>
        [Display(Name = "Overlap Advance Warning Sign On")]
        OverlapAdvanceWarningSignOn = 71,

        ///<summary>
        ///Set when advance warning sign becomes inactive.
        ///The <c>Parameter</c> is Overlap Number
        ///</summary>
        [Display(Name = "Overlap Advance Warning Sign Off")]
        OverlapAdvanceWarningSignOff = 72,

        ///<summary>
        ///Vehicle detector has turned off. Detector on and off events are triggered post any detector delay/extension processing.
        ///The <c>Parameter</c> is Vehicle detector Number
        ///</summary>
        [Display(Name = "Vehicle Detector Off")]
        VehicleDetectorOff = 81,

        ///<summary>
        ///Vehicle detector has turned on. Detector on and off events are triggered post any detector delay/extension processing.
        ///The <c>Parameter</c> is Vehicle detector Number
        ///</summary>
        [Display(Name = "Vehicle Detector On")]
        VehicleDetectorOn = 82,

        ///<summary>
        ///Detector restored to non-failed state by either manual restoration or re-enabling via continued diagnostics.
        ///The <c>Parameter</c> is Vehicle detector Number
        ///</summary>
        [Display(Name = "Vehicle Detector Restored")]
        VehicleDetectorRestored = 83,

        ///<summary>
        ///Detector failure logged upon local controller diagnostics.
        ///The <c>Parameter</c> is Vehicle detector Number
        ///</summary>
        [Display(Name = "Vehicle Detector Fault- Other")]
        VehicleDetectorFaultOther = 84,

        ///<summary>
        ///Detector failure logged upon local controller diagnostics.
        ///The <c>Parameter</c> is Vehicle detector Number
        ///</summary>
        [Display(Name = "Vehicle Detector Fault- Watchdog")]
        VehicleDetectorFaultWatchdog = 85,

        ///<summary>
        ///Detector failure logged upon local controller diagnostics.
        ///The <c>Parameter</c> is Vehicle detector Number
        ///</summary>
        [Display(Name = "Vehicle Detector Fault- Open Loop")]
        VehicleDetectorFaultOpenLoop = 86,

        ///<summary>
        ///Detector failure logged upon local controller diagnostics.
        ///The <c>Parameter</c> is Vehicle detector Number
        ///</summary>
        [Display(Name = "Vehicle Detector Fault- Shorted Loop")]
        VehicleDetectorFaultShortedLoop = 87,

        ///<summary>
        ///Detector failure logged upon local controller diagnostics.
        ///The <c>Parameter</c> is Vehicle detector Number
        ///</summary>
        [Display(Name = "Vehicle Detector Fault- Excessive Change")]
        VehicleDetectorFaultExcessiveChange = 88,

        ///<summary>
        ///Ped detector turned off. Detector events are triggered post any detector delay/extension processing.
        ///The <c>Parameter</c> is Pedestrian detector Number
        ///</summary>
        [Display(Name = "Ped Detector Off")]
        PedDetectorOff = 89,

        ///<summary>
        ///Ped detector turned on. Detector events are triggered post any detector delay/extension processing.
        ///The <c>Parameter</c> is Pedestrian detector Number
        ///</summary>
        [Display(Name = "Ped Detector On")]
        PedDetectorOn = 90,

        ///<summary>
        ///Detector failure logged upon local controller diagnostics.
        ///The <c>Parameter</c> is Pedestrian detector Number
        ///</summary>
        [Display(Name = "Ped Detector Failed")]
        PedDetectorFailed = 91,

        ///<summary>
        ///Detector failure logged upon local controller diagnostics.
        ///The <c>Parameter</c> is Pedestrian detector Number
        ///</summary>
        [Display(Name = "Ped Detector Restored")]
        PedDetectorRestored = 92,

        ///<summary>
        ///Set when TSP detector becomes inactive. TSP detector event occurs post any detector delay/extension.
        ///The <c>Parameter</c> is TSP Number
        ///</summary>
        [Display(Name = "TSP Detector Off")]
        TSPDetectorOff = 93,

        ///<summary>
        ///Set when TSP detector is active. TSP detector event occurs post any detector delay/extension.
        ///The <c>Parameter</c> is TSP Number
        ///</summary>
        [Display(Name = "TSP Detector On")]
        TSPDetectorOn = 94,

        ///<summary>
        ///Set when preemption advance warning input is activated.
        ///The <c>Parameter</c> is Preempt Number
        ///</summary>
        [Display(Name = "Preempt Advance Warning Input")]
        PreemptAdvanceWarningInput = 101,

        ///<summary>
        ///Set when preemption input is activated.
        ///The <c>Parameter</c> is Preempt Number
        ///</summary>
        [Display(Name = "Preempt (Call) Input On")]
        PreemptCallInputOn = 102,

        ///<summary>
        ///Set when gate down input is received by the controller.
        ///The <c>Parameter</c> is Preempt Number
        ///</summary>
        [Display(Name = "Preempt Gate Down Input Received")]
        PreemptGateDownInputReceived = 103,

        ///<summary>
        ///Set when preemption input is de-activated.
        ///The <c>Parameter</c> is Preempt Number
        ///</summary>
        [Display(Name = "Preempt (Call) Input Off")]
        PreemptCallInputOff = 104,

        ///<summary>
        ///Set when preemption delay expires and controller begins transition timing (forceoff) to serve preemption.
        ///The <c>Parameter</c> is Preempt Number
        ///</summary>
        [Display(Name = "Preempt Entry Started")]
        PreemptEntryStarted = 105,

        ///<summary>
        ///Set when track clearance phases are green and track clearance timing begins.
        ///The <c>Parameter</c> is Preempt Number
        ///</summary>
        [Display(Name = "Preemption Begin Track Clearance")]
        PreemptionBeginTrackClearance = 106,

        ///<summary>
        ///Set when preemption dwell or limited service begins or minimum dwell timer is reset due to call drop and reapplication.
        ///The <c>Parameter</c> is Preempt Number
        ///</summary>
        [Display(Name = "Preemption Begin Dwell Service")]
        PreemptionBeginDwellService = 107,

        ///<summary>
        ///Set when linked preemptor input is applied from active preemptor.
        ///The <c>Parameter</c> is Preempt Number
        ///</summary>
        [Display(Name = "Preemption Link Active On")]
        PreemptionLinkActiveOn = 108,

        ///<summary>
        ///Set when linked preemptor input is dropped from active preemptor.
        ///The <c>Parameter</c> is Preempt Number
        ///</summary>
        [Display(Name = "Preemption Link Active Off")]
        PreemptionLinkActiveOff = 109,

        ///<summary>
        ///Set when preemption max presence timer is exceeded and preemption input is released from service.
        ///The <c>Parameter</c> is Preempt Number
        ///</summary>
        [Display(Name = "Preemption Max Presence Exceeded")]
        PreemptionMaxPresenceExceeded = 110,

        ///<summary>
        ///Set when preemption exit interval phases are green and exit timing begins.
        ///The <c>Parameter</c> is Preempt Number
        ///</summary>
        [Display(Name = "Preemption Begin Exit Interval")]
        PreemptionBeginExitInterval = 111,

        ///<summary>
        ///Set when request for priority is received.
        ///The <c>Parameter</c> is Prioritor Number
        ///</summary>
        [Display(Name = "TSP Check In")]
        TSPCheckIn = 112,

        ///<summary>
        ///Set when controller is adjusting active cycle to accommodate early service to TSP phases.
        ///The <c>Parameter</c> is Prioritor Number
        ///</summary>
        [Display(Name = "TSP Adjustment to Early Green")]
        TSPAdjustmenttoEarlyGreen = 113,

        ///<summary>
        ///Set when controller is adjusting active cycle to accommodate extended service to TSP phases.
        ///The <c>Parameter</c> is Prioritor Number
        ///</summary>
        [Display(Name = "TSP Adjustment to Extend Green")]
        TSPAdjustmenttoExtendGreen = 114,

        ///<summary>
        ///Set when the arrival time for the TSP event reaches zero.
        ///The <c>Parameter</c> is Prioritor Number
        ///</summary>
        [Display(Name = "TSP Check Out")]
        TSPCheckOut = 115,

        ///<summary>
        ///Set when preemption applies forceoffs to any phase in the active cycle.
        ///The <c>Parameter</c> is Preemptor Number
        ///</summary>
        [Display(Name = "Preemption Forceoff Phase")]
        PreemptionForceoffPhase = 116,

        ///<summary>
        ///Set when TSP early forceoff is applied to the active cycle.
        ///The <c>Parameter</c> is Prioritor Number
        ///</summary>
        [Display(Name = "TSP Early Forceoff Cycle")]
        TSPEarlyForceoffCycle = 117,

        ///<summary>
        ///Set when requested TSP service begins.
        ///The <c>Parameter</c> is Prioritor Number
        ///</summary>
        [Display(Name = "TSP Service Start")]
        TSPServiceStart = 118,

        ///<summary>
        ///Set when requested TSP service ends.
        ///The <c>Parameter</c> is Prioritor Number
        ///</summary>
        [Display(Name = "TSP Service End")]
        TSPServiceEnd = 119,

        ///<summary>
        ///Coordination pattern that is actively running in the controller.
        ///The <c>Parameter</c> is Pattern  Number
        ///</summary>
        [Display(Name = "Coord Pattern Change")]
        CoordPatternChange = 131,

        ///<summary>
        ///Set upon selection of a new coordination pattern change that selects a new cycle length.
        ///The <c>Parameter</c> is New Cycle Length in Seconds
        ///</summary>
        [Display(Name = "Cycle Length Change")]
        CycleLengthChange = 132,

        ///<summary>
        ///Set upon selection of a new coordination pattern change that selects a new cycle length. Cycle lengths in excess of 255 shall record this event with a 255 parameter, along with event code 156.
        ///The <c>Parameter</c> is New Offset in Seconds
        ///</summary>
        [Display(Name = "Offset Length Change")]
        OffsetLengthChange = 133,

        ///<summary>
        ///Programmed split time for phase 1 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 1 Change")]
        ProgrammedSplit1Change = 134,

        ///<summary>
        ///Programmed split time for phase 2 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 2 Change")]
        ProgrammedSplit2Change = 135,

        ///<summary>
        ///Programmed split time for phase 3 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 3 Change")]
        ProgrammedSplit3Change = 136,

        ///<summary>
        ///Programmed split time for phase 4 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 4 Change")]
        ProgrammedSplit4Change = 137,

        ///<summary>
        ///Programmed split time for phase 5 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 5 Change")]
        ProgrammedSplit5Change = 138,

        ///<summary>
        ///Programmed split time for phase 6 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 6 Change")]
        ProgrammedSplit6Change = 139,

        ///<summary>
        ///Programmed split time for phase 7 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 7 Change")]
        ProgrammedSplit7Change = 140,

        ///<summary>
        ///Programmed split time for phase 8 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 8 Change")]
        ProgrammedSplit8Change = 141,

        ///<summary>
        ///Programmed split time for phase 9 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 9 Change")]
        ProgrammedSplit9Change = 142,

        ///<summary>
        ///Programmed split time for phase 10 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 10 Change")]
        ProgrammedSplit10Change = 143,

        ///<summary>
        ///Programmed split time for phase 11 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 11 Change")]
        ProgrammedSplit11Change = 144,

        ///<summary>
        ///Programmed split time for phase 12 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 12 Change")]
        ProgrammedSplit12Change = 145,

        ///<summary>
        ///Programmed split time for phase 13 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 13 Change")]
        ProgrammedSplit13Change = 146,

        ///<summary>
        ///Programmed split time for phase 14 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 14 Change")]
        ProgrammedSplit14Change = 147,

        ///<summary>
        ///Programmed split time for phase 15 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 15 Change")]
        ProgrammedSplit15Change = 148,

        ///<summary>
        ///Programmed split time for phase 16 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 16 Change")]
        ProgrammedSplit16Change = 149,

        ///<summary>
        ///Cycle state has changed. Parameter: 0=Free, 1=In Step, 2=Transition-Add, 3=Transition-Subtract, 4 = Transition-Dwell, 5=Local Zero, 6=Begin Pickup, 7=Master Cycle Zero
        ///The <c>Parameter</c> is New Cycle State
        ///</summary>
        [Display(Name = "Coord cycle state change")]
        Coordcyclestatechange = 150,

        ///<summary>
        ///Set when a coordinated phase has reached the yield point.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Coordinated phase yield point")]
        Coordinatedphaseyieldpoint = 151,

        ///<summary>
        ///Set when coordinated phase begins.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Coordinated Phase Begin")]
        CoordinatedPhaseBegin = 152,

        ///<summary>
        ///Set when the logic statement evaluation changes from false to true.
        ///The <c>Parameter</c> is Logic Statement Number
        ///</summary>
        [Display(Name = "Logic Statement True")]
        LogicStatementTrue = 153,

        ///<summary>
        ///Set when logic statement evaluation changes from true to false.
        ///The <c>Parameter</c> is Logic Statement Number
        ///</summary>
        [Display(Name = "Logic Statement False")]
        LogicStatementFalse = 154,

        ///<summary>
        ///Set when control mode changes.
        ///The <c>Parameter</c> is 1 = Other, 2 = System Control, 3 = System Standby, 5 = Manual, 6 = Timebase, 7 = Interconnect, 8 = Interconnect Backup, 9 = Remote Manual Control, 10 = Local Manual Control.
        ///</summary>
        [Display(Name = "Unit Control Status Change")]
        UnitControlStatusChange = 155,

        ///<summary>
        ///Set simultaneously with Event Code 132 when existing cycle length exceeds 255 seconds. When this Event Code 156 is used, Event Code 132 shall be 255, where the value of this Event Code 156 will be the additional cycle length above 255 (in seconds). Cycle lengths longer than 510 seconds will require a controller look up.
        ///The <c>Parameter</c> is Seconds Number
        ///</summary>
        [Display(Name = "Additional Cycle Length Change")]
        AdditionalCycleLengthChange = 156,

        ///<summary>
        ///Test Input has turned on.
        ///The <c>Parameter</c> is Test Input Number
        ///</summary>
        [Display(Name = "Test Input On")]
        TestInputOn = 171,

        ///<summary>
        ///Test Input has turned off
        ///The <c>Parameter</c> is Test Input Number
        ///</summary>
        [Display(Name = "Test Input Off")]
        TestInputOff = 172,

        ///<summary>
        ///NTCIP Unit Flash Status has changed. Parameter: 1=Other, 2=Not Flash, 3=Automatic, 4=Local Manual, 5=Fault Monitor, 6=MMU, 7=Startup, 8=Preempt
        ///The <c>Parameter</c> is New Unit Flash Status value
        ///</summary>
        [Display(Name = "Unit Flash Status change")]
        UnitFlashStatuschange = 173,

        ///<summary>
        ///NTCIP Unit Alarm Status 1 has changed. Parameter: bit 0=Cycle Fault, 1=Coord Fault, 2=Coord Fail, 3=Cycle Fail, 4=MMU Flash, 5=Local Flash, 6=Local Free, 7=Coord Active
        ///The <c>Parameter</c> is New Alarm Status 1 value
        ///</summary>
        [Display(Name = "Unit Alarm Status 1 change")]
        UnitAlarmStatus1change = 174,

        ///<summary>
        ///NTCIP Alarm Group State has changed. Parameter: bit 0=Alarm Input 1, 1=Alarm Input 2, ..., 7=Alarm Input 8
        ///The <c>Parameter</c> is New Alarm Group State value
        ///</summary>
        [Display(Name = "Alarm Group change")]
        AlarmGroupchange = 175,

        ///<summary>
        ///Special function output has turned on.
        ///The <c>Parameter</c> is Special Function Number
        ///</summary>
        [Display(Name = "Special Function Output on")]
        SpecialFunctionOutputon = 176,

        ///<summary>
        ///Special function output has turned off.
        ///The <c>Parameter</c> is Special Function Number
        ///</summary>
        [Display(Name = "Special Function Output off")]
        SpecialFunctionOutputoff = 177,

        ///<summary>
        ///Manual Control Enable state has changed. Parameter: 0=Off, 1=On
        ///The <c>Parameter</c> is Manual Control Enable State
        ///</summary>
        [Display(Name = "Manual control enable off/on")]
        Manualcontrolenableoffon = 178,

        ///<summary>
        ///Interval Advance state has changed. Parameter: 0=Off, 1=On
        ///The <c>Parameter</c> is Interval Advance state
        ///</summary>
        [Display(Name = "Interval Advance off/on")]
        IntervalAdvanceoffon = 179,

        ///<summary>
        ///Set when stop time input is applied or removed, regardless of source of stop or state. Parameter: 0=Off, 1=On
        ///The <c>Parameter</c> is Stop Time state
        ///</summary>
        [Display(Name = "Stop Time off/on")]
        StopTimeoffon = 180,

        ///<summary>
        ///Set when the controller OS clock is adjusted via communications, OS command, or external input.
        ///The <c>Parameter</c> is Time correction in Seconds
        ///</summary>
        [Display(Name = "Controller Clock Updated")]
        ControllerClockUpdated = 181,

        ///<summary>
        ///Line voltage drops between 0-89 volts AC for more than 100 ms.
        ///The <c>Parameter</c> is 1
        ///</summary>
        [Display(Name = "Power Failure Detected")]
        PowerFailureDetected = 182,

        /////<summary>
        /////Line voltage applied/reapplied greater than 98 volts AC.
        /////The <c>Parameter</c> is Not used
        /////</summary>
        //[Display(Name = "Power Restored")]
        //PowerRestored = 183,

        ///<summary>
        ///Line voltage applied/reapplied greater than 98 volts AC.
        ///The <c>Parameter</c> is Not used
        ///</summary>
        [Display(Name = "Power Restored")]
        PowerRestored = 184,

        ///<summary>
        ///Vendor specific Alarm Event.
        ///The <c>Parameter</c> is Vendor Specific Alarm Type
        ///</summary>
        [Display(Name = "Vendor Specific Alarm")]
        VendorSpecificAlarm = 185,

        ///<summary>
        ///Set when cabinet/system alarm is activated.
        ///The <c>Parameter</c> is Alarm Number.
        ///</summary>
        [Display(Name = "Alarm On")]
        AlarmOn = 200,

        ///<summary>
        ///Set when cabinet/system alarm is deactivated.
        ///The <c>Parameter</c> is Alarm Number
        ///</summary>
        [Display(Name = "Alarm Off")]
        AlarmOff = 201,

        ///<summary>
        ///Set when local controller aux switch is active (1) or inactive (0).
        ///The <c>Parameter</c> is Aux Switch On(1), Off(0)
        ///</summary>
        [Display(Name = "Aux Switch On/Off")]
        AuxSwitchOnOff = 202,

        ///<summary>
        ///Programmed split time for phase 17 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 17 Change")]
        ProgrammedSplit17Change = 203,

        ///<summary>
        ///Programmed split time for phase 18 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 18 Change")]
        ProgrammedSplit18Change = 204,

        ///<summary>
        ///Programmed split time for phase 19 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 19 Change")]
        ProgrammedSplit19Change = 205,

        ///<summary>
        ///Programmed split time for phase 20 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 20 Change")]
        ProgrammedSplit20Change = 206,

        ///<summary>
        ///Programmed split time for phase 21 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 21 Change")]
        ProgrammedSplit21Change = 207,

        ///<summary>
        ///Programmed split time for phase 22 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 22 Change")]
        ProgrammedSplit22Change = 208,

        ///<summary>
        ///Programmed split time for phase 23 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 23 Change")]
        ProgrammedSplit23Change = 209,

        ///<summary>
        ///Programmed split time for phase 24 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 24 Change")]
        ProgrammedSplit24Change = 210,

        ///<summary>
        ///Programmed split time for phase 25 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 25 Change")]
        ProgrammedSplit25Change = 211,

        ///<summary>
        ///Programmed split time for phase 26 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 26 Change")]
        ProgrammedSplit26Change = 212,

        ///<summary>
        ///Programmed split time for phase 27 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 27 Change")]
        ProgrammedSplit27Change = 213,

        ///<summary>
        ///Programmed split time for phase 28 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 28 Change")]
        ProgrammedSplit28Change = 214,

        ///<summary>
        ///Programmed split time for phase 29 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 29 Change")]
        ProgrammedSplit29Change = 215,

        ///<summary>
        ///Programmed split time for phase 30 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 30 Change")]
        ProgrammedSplit30Change = 216,

        ///<summary>
        ///Programmed split time for phase 31 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 31 Change")]
        ProgrammedSplit31Change = 217,

        ///<summary>
        ///Programmed split time for phase 32 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Programmed Split 32 Change")]
        ProgrammedSplit32Change = 218,

        ///<summary>
        ///Actual split time for phase 1, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 1")]
        ActualSplit1 = 300,

        ///<summary>
        ///Actual split time for phase 2, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 2")]
        ActualSplit2 = 301,

        ///<summary>
        ///Actual split time for phase 3, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 3")]
        ActualSplit3 = 302,

        ///<summary>
        ///Actual split time for phase 4, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 4")]
        ActualSplit4 = 303,

        ///<summary>
        ///Actual split time for phase 5, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 5")]
        ActualSplit5 = 304,

        ///<summary>
        ///Actual split time for phase 6, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 6")]
        ActualSplit6 = 305,

        ///<summary>
        ///Actual split time for phase 7, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 7")]
        ActualSplit7 = 306,

        ///<summary>
        ///Actual split time for phase 8, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 8")]
        ActualSplit8 = 307,

        ///<summary>
        ///Actual split time for phase 9, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 9")]
        ActualSplit9 = 308,

        ///<summary>
        ///Actual split time for phase 10, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 10")]
        ActualSplit10 = 309,

        ///<summary>
        ///Actual split time for phase 11, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 11")]
        ActualSplit11 = 310,

        ///<summary>
        ///Actual split time for phase 12, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 12")]
        ActualSplit12 = 311,

        ///<summary>
        ///Actual split time for phase 13, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 13")]
        ActualSplit13 = 312,

        ///<summary>
        ///Actual split time for phase 14, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 14")]
        ActualSplit14 = 313,

        ///<summary>
        ///Actual split time for phase 15, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 15")]
        ActualSplit15 = 314,

        ///<summary>
        ///Actual split time for phase 16, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 16")]
        ActualSplit16 = 315,

        ///<summary>
        ///Actual Cycle Time, recorded at end of cycle during coordination.
        ///The <c>Parameter</c> is Cycle Length in Seconds
        ///</summary>
        [Display(Name = "Actual Cycle Length")]
        ActualCycleLength = 316,

        ///<summary>
        ///Actual Natural Cycle Time, recorded at reference point during free or transition.
        ///The <c>Parameter</c> is Cycle Length in Seconds
        ///</summary>
        [Display(Name = "Actual Natural Cycle Length")]
        ActualNaturalCycleLength = 317,

        ///<summary>
        ///Actual Cycle Offset, recorded at end of cycle during coordination or at reference point during free or transition.
        ///The <c>Parameter</c> is Offset Time in Seconds
        ///</summary>
        [Display(Name = "Actual Cycle Offset")]
        ActualCycleOffset = 318,

        ///<summary>
        ///Phase sequence change has been requested.
        ///The <c>Parameter</c> is Sequence Number
        ///</summary>
        [Display(Name = "Sequence Change Request")]
        SequenceChangeRequest = 319,

        ///<summary>
        ///Master Cycle Zero point
        ///The <c>Parameter</c> is Tine since last reference point.
        ///</summary>
        [Display(Name = "Master Cycle Zero")]
        MasterCycleZero = 320,

        ///<summary>
        ///Oversized ped served in coord
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Coord - Oversize Ped")]
        CoordOversizePed = 321,

        ///<summary>
        ///Tracks the time from TSP arrival to when TSP phase service begins.
        ///The <c>Parameter</c> is Time in Sec
        ///</summary>
        [Display(Name = "TSP Delay Time")]
        TSPDelayTime = 322,

        ///<summary>
        ///A new coord pattern has been selected
        ///The <c>Parameter</c> is Selected Pattern Number
        ///</summary>
        [Display(Name = "Coord Pattern Selected")]
        CoordPatternSelected = 323,

        ///<summary>
        ///Actual split time for phase 17, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 17")]
        ActualSplit17 = 324,

        ///<summary>
        ///Actual split time for phase 18, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 18")]
        ActualSplit18 = 325,

        ///<summary>
        ///Actual split time for phase 19, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 19")]
        ActualSplit19 = 326,

        ///<summary>
        ///Actual split time for phase 20, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 20")]
        ActualSplit20 = 327,

        ///<summary>
        ///Actual split time for phase 21, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 21")]
        ActualSplit21 = 328,

        ///<summary>
        ///Actual split time for phase 22, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 22")]
        ActualSplit22 = 329,

        ///<summary>
        ///Actual split time for phase 23, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 23")]
        ActualSplit23 = 330,

        ///<summary>
        ///Actual split time for phase 24, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 24")]
        ActualSplit24 = 331,

        ///<summary>
        ///Actual split time for phase 25, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 25")]
        ActualSplit25 = 332,

        ///<summary>
        ///Actual split time for phase 26, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 26")]
        ActualSplit26 = 333,

        ///<summary>
        ///Actual split time for phase 27, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 27")]
        ActualSplit27 = 334,

        ///<summary>
        ///Actual split time for phase 28, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 28")]
        ActualSplit28 = 335,

        ///<summary>
        ///Actual split time for phase 29, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 29")]
        ActualSplit29 = 336,

        ///<summary>
        ///Actual split time for phase 30, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 30")]
        ActualSplit30 = 337,

        ///<summary>
        ///Actual split time for phase 31, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 31")]
        ActualSplit31 = 338,

        ///<summary>
        ///Actual split time for phase 32, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 32")]
        ActualSplit32 = 339,

        ///<summary>
        ///Actual split time for phase 33, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 33")]
        ActualSplit33 = 340,

        ///<summary>
        ///Actual split time for phase 34, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 34")]
        ActualSplit34 = 341,

        ///<summary>
        ///Actual split time for phase 35, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 35")]
        ActualSplit35 = 342,

        ///<summary>
        ///Actual split time for phase 36, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 36")]
        ActualSplit36 = 343,

        ///<summary>
        ///Actual split time for phase 37, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 37")]
        ActualSplit37 = 344,

        ///<summary>
        ///Actual split time for phase 38, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 38")]
        ActualSplit38 = 345,

        ///<summary>
        ///Actual split time for phase 39, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 39")]
        ActualSplit39 = 346,

        ///<summary>
        ///Actual split time for phase 40, recorded at end of phase clearance.
        ///The <c>Parameter</c> is Split Time in Seconds
        ///</summary>
        [Display(Name = "Actual Split 40")]
        ActualSplit40 = 347,

        ///<summary>
        ///Programmed split time for phase 17 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 17 Change")]
        QfreeProgrammedSplit17Change = 348,

        ///<summary>
        ///Programmed split time for phase 18 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 18 Change")]
        QfreeProgrammedSplit18Change = 349,

        ///<summary>
        ///Programmed split time for phase 19 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 19 Change")]
        QfreeProgrammedSplit19Change = 350,

        ///<summary>
        ///Programmed split time for phase 20 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 20 Change")]
        QfreeProgrammedSplit20Change = 351,

        ///<summary>
        ///Programmed split time for phase 21 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 21 Change")]
        QfreeProgrammedSplit21Change = 352,

        ///<summary>
        ///Programmed split time for phase 22 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 22 Change")]
        QfreeProgrammedSplit22Change = 353,

        ///<summary>
        ///Programmed split time for phase 23 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 23 Change")]
        QfreeProgrammedSplit23Change = 354,

        ///<summary>
        ///Programmed split time for phase 24 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 24 Change")]
        QfreeProgrammedSplit24Change = 355,

        ///<summary>
        ///Programmed split time for phase 25 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 25 Change")]
        QfreeProgrammedSplit25Change = 356,

        ///<summary>
        ///Programmed split time for phase 26 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 26 Change")]
        QfreeProgrammedSplit26Change = 357,

        ///<summary>
        ///Programmed split time for phase 27 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 27 Change")]
        QfreeProgrammedSplit27Change = 358,

        ///<summary>
        ///Programmed split time for phase 28 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 28 Change")]
        QfreeProgrammedSplit28Change = 359,

        ///<summary>
        ///Programmed split time for phase 29 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 29 Change")]
        QfreeProgrammedSplit29Change = 360,

        ///<summary>
        ///Programmed split time for phase 30 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 30 Change")]
        QfreeProgrammedSplit30Change = 361,

        ///<summary>
        ///Programmed split time for phase 31 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 31 Change")]
        QfreeProgrammedSplit31Change = 362,

        ///<summary>
        ///Programmed split time for phase 32 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 32 Change")]
        QfreeProgrammedSplit32Change = 363,

        ///<summary>
        ///Programmed split time for phase 33 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 33 Change")]
        QfreeProgrammedSplit33Change = 364,

        ///<summary>
        ///Programmed split time for phase 34 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 34 Change")]
        QfreeProgrammedSplit34Change = 365,

        ///<summary>
        ///Programmed split time for phase 35 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 35 Change")]
        QfreeProgrammedSplit35Change = 366,

        ///<summary>
        ///Programmed split time for phase 36 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 36 Change")]
        QfreeProgrammedSplit36Change = 367,

        ///<summary>
        ///Programmed split time for phase 37 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 37 Change")]
        QfreeProgrammedSplit37Change = 368,

        ///<summary>
        ///Programmed split time for phase 38 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 38 Change")]
        QfreeProgrammedSplit38Change = 369,

        ///<summary>
        ///Programmed split time for phase 39 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 39 Change")]
        QfreeProgrammedSplit39Change = 370,

        ///<summary>
        ///Programmed split time for phase 40 has changed.
        ///The <c>Parameter</c> is New Split Time in Seconds
        ///</summary>
        [Display(Name = "Qfree Programmed Split 40 Change")]
        QfreeProgrammedSplit40Change = 371,

        ///<summary>
        ///Recorded controller time drift.
        ///The <c>Parameter</c> is Time drift in Seconds
        ///</summary>
        [Display(Name = "Time Drift")]
        TimeDrift = 400,

        ///<summary>
        ///Total number of communication attempts in last period. Recorded by Maxview.
        ///The <c>Parameter</c> is Number of attempts
        ///</summary>
        [Display(Name = "Total Comm Attempts")]
        TotalCommAttempts = 500,

        ///<summary>
        ///Number of failed communication attempts in last period. Recorded by Maxview.
        ///The <c>Parameter</c> is Number of attempts
        ///</summary>
        [Display(Name = "Failed Comm Attempts")]
        FailedCommAttempts = 501,

        ///<summary>
        ///Percentage of failed communication attempts in last period. Recorded by Maxview.
        ///The <c>Parameter</c> is Percent
        ///</summary>
        [Display(Name = "Percent Comm Loss")]
        PercentCommLoss = 502,

        ///<summary>
        ///Average communication response time during the last period. Recorded by Maxview.
        ///The <c>Parameter</c> is Time in ms
        ///</summary>
        [Display(Name = "Average Comm Response Time")]
        AverageCommResponseTime = 503,

        ///<summary>
        ///The amount of time the TSP request held the active phase past the programmed forceoff or max green time.
        ///The <c>Parameter</c> is Time in Sec
        ///</summary>
        [Display(Name = "TSP Hold Time")]
        TSPHoldTime = 517,

        ///<summary>
        ///The amount time the TSP request terminated the active phase before the programmed forceoff or max green time.
        ///The <c>Parameter</c> is Time in Sec
        ///</summary>
        [Display(Name = "TSP Early Term Time")]
        TSPEarlyTermTime = 518,

        ///<summary>
        ///Set when the TSP delay interval begins.
        ///The <c>Parameter</c> is Prioritor Number
        ///</summary>
        [Display(Name = "TSP Delay Begin")]
        TSPDelayBegin = 519,

        ///<summary>
        ///Set when the TSP delay interval ends.
        ///The <c>Parameter</c> is Prioritor Number
        ///</summary>
        [Display(Name = "TSP Delay End")]
        TSPDelayEnd = 520,

        ///<summary>
        ///Set when the TSP arrival time is expired.
        ///The <c>Parameter</c> is Prioritor Number
        ///</summary>
        [Display(Name = "TSP Arrival")]
        TSPArrival = 521,

        ///<summary>
        ///Set when a new sequence is started
        ///The <c>Parameter</c> is Sequence Number
        ///</summary>
        [Display(Name = "Sequence Entry")]
        SequenceEntry = 600,

        ///<summary>
        ///Set when pri/pre detector low priority call (pulsing) is received.
        ///The <c>Parameter</c> is Pri/Pre Detector Number
        ///</summary>
        [Display(Name = "Pri/Pre Det Low On")]
        PriPreDetLowOn = 601,

        ///<summary>
        ///Set when pri/pre detector low priority call (pulsing) is cleared.
        ///The <c>Parameter</c> is Pri/Pre Detector Number
        ///</summary>
        [Display(Name = "Pri/Pre Det Low Off")]
        PriPreDetLowOff = 602,

        ///<summary>
        ///Set when pri/pre detector high priority call (steady) is received
        ///The <c>Parameter</c> is Pri/Pre Detector Number
        ///</summary>
        [Display(Name = "Pri/Pre Det High On")]
        PriPreDetHighOn = 603,

        ///<summary>
        ///Set when pri/pre detector high priority call (steady) is cleared
        ///The <c>Parameter</c> is Pri/Pre Detector Number
        ///</summary>
        [Display(Name = "Pri/Pre Det High Off")]
        PriPreDetHighOff = 604,

        ///<summary>
        ///No Activity fault has occurred on a Pri/Pre detector.
        ///The <c>Parameter</c> is Pri/Pre Detector Number
        ///</summary>
        [Display(Name = "Pri/Pre Det Fault - No Activity")]
        PriPreDetFaultNoActivity = 605,

        ///<summary>
        ///Max Presence fault has occurred on a Pri/Pre detector.
        ///The <c>Parameter</c> is Pri/Pre Detector Number
        ///</summary>
        [Display(Name = "Pri/Pre Det Fault - Max Presence")]
        PriPreDetFaultMaxPresence = 606,

        ///<summary>
        ///Erratic Count fault has occurred on a Pri/Pre detector.
        ///The <c>Parameter</c> is Pri/Pre Detector Number
        ///</summary>
        [Display(Name = "Pri/Pre Det Fault - Erratic Count")]
        PriPreDetFaultErraticCount = 607,

        ///<summary>
        ///Set when all faults have been cleared on a Pri/Pre detector.
        ///The <c>Parameter</c> is Pri/Pre Detector Number
        ///</summary>
        [Display(Name = "Pri/Pre Det Fault - Cleared")]
        PriPreDetFaultCleared = 608,

        ///<summary>
        ///Set when any fault is active on a Pri/Pre detector.
        ///The <c>Parameter</c> is Pri/Pre Detector Number
        ///</summary>
        [Display(Name = "Pri/Pre Det Fault - Active")]
        PriPreDetFaultActive = 609,

        ///<summary>
        ///Prioritor service is started.
        ///The <c>Parameter</c> is Prioritor Number
        ///</summary>
        [Display(Name = "TSP Begin Service")]
        TSPBeginService = 610,

        ///<summary>
        ///Prioritor service has ended.
        ///The <c>Parameter</c> is Prioritor Number
        ///</summary>
        [Display(Name = "TSP End Service")]
        TSPEndService = 611,

        ///<summary>
        ///Recorded wait time for phase 1.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 1 Wait Time")]
        Phase1WaitTime = 612,

        ///<summary>
        ///Recorded wait time for phase 2.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 2 Wait Time")]
        Phase2WaitTime = 613,

        ///<summary>
        ///Recorded wait time for phase 3.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 3 Wait Time")]
        Phase3WaitTime = 614,

        ///<summary>
        ///Recorded wait time for phase 4.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 4 Wait Time")]
        Phase4WaitTime = 615,

        ///<summary>
        ///Recorded wait time for phase 5.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 5 Wait Time")]
        Phase5WaitTime = 616,

        ///<summary>
        ///Recorded wait time for phase 6.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 6 Wait Time")]
        Phase6WaitTime = 617,

        ///<summary>
        ///Recorded wait time for phase 7.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 7 Wait Time")]
        Phase7WaitTime = 618,

        ///<summary>
        ///Recorded wait time for phase 8.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 8 Wait Time")]
        Phase8WaitTime = 619,

        ///<summary>
        ///Recorded wait time for phase 9.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 9 Wait Time")]
        Phase9WaitTime = 620,

        ///<summary>
        ///Recorded wait time for phase 10.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 10 Wait Time")]
        Phase10WaitTime = 621,

        ///<summary>
        ///Recorded wait time for phase 11.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 11 Wait Time")]
        Phase11WaitTime = 622,

        ///<summary>
        ///Recorded wait time for phase 12.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 12 Wait Time")]
        Phase12WaitTime = 623,

        ///<summary>
        ///Recorded wait time for phase 13.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 13 Wait Time")]
        Phase13WaitTime = 624,

        ///<summary>
        ///Recorded wait time for phase 14.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 14 Wait Time")]
        Phase14WaitTime = 625,

        ///<summary>
        ///Recorded wait time for phase 15.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 15 Wait Time")]
        Phase15WaitTime = 626,

        ///<summary>
        ///Recorded wait time for phase 16.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 16 Wait Time")]
        Phase16WaitTime = 627,

        ///<summary>
        ///Recorded wait time for phase 17.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 17 Wait Time")]
        Phase17WaitTime = 628,

        ///<summary>
        ///Recorded wait time for phase 18.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 18 Wait Time")]
        Phase18WaitTime = 629,

        ///<summary>
        ///Recorded wait time for phase 19.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 19 Wait Time")]
        Phase19WaitTime = 630,

        ///<summary>
        ///Recorded wait time for phase 20.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 20 Wait Time")]
        Phase20WaitTime = 631,

        ///<summary>
        ///Recorded wait time for phase 21.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 21 Wait Time")]
        Phase21WaitTime = 632,

        ///<summary>
        ///Recorded wait time for phase 22.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 22 Wait Time")]
        Phase22WaitTime = 633,

        ///<summary>
        ///Recorded wait time for phase 23.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 23 Wait Time")]
        Phase23WaitTime = 634,

        ///<summary>
        ///Recorded wait time for phase 24.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 24 Wait Time")]
        Phase24WaitTime = 635,

        ///<summary>
        ///Recorded wait time for phase 25.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 25 Wait Time")]
        Phase25WaitTime = 636,

        ///<summary>
        ///Recorded wait time for phase 26.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 26 Wait Time")]
        Phase26WaitTime = 637,

        ///<summary>
        ///Recorded wait time for phase 27.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 27 Wait Time")]
        Phase27WaitTime = 638,

        ///<summary>
        ///Recorded wait time for phase 28.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 28 Wait Time")]
        Phase28WaitTime = 639,

        ///<summary>
        ///Recorded wait time for phase 29.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 29 Wait Time")]
        Phase29WaitTime = 640,

        ///<summary>
        ///Recorded wait time for phase 30.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 30 Wait Time")]
        Phase30WaitTime = 641,

        ///<summary>
        ///Recorded wait time for phase 31.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 31 Wait Time")]
        Phase31WaitTime = 642,

        ///<summary>
        ///Recorded wait time for phase 32.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 32 Wait Time")]
        Phase32WaitTime = 643,

        ///<summary>
        ///Recorded wait time for phase 33.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 33 Wait Time")]
        Phase33WaitTime = 644,

        ///<summary>
        ///Recorded wait time for phase 34.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 34 Wait Time")]
        Phase34WaitTime = 645,

        ///<summary>
        ///Recorded wait time for phase 35.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 35 Wait Time")]
        Phase35WaitTime = 646,

        ///<summary>
        ///Recorded wait time for phase 36.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 36 Wait Time")]
        Phase36WaitTime = 647,

        ///<summary>
        ///Recorded wait time for phase 37.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 37 Wait Time")]
        Phase37WaitTime = 648,

        ///<summary>
        ///Recorded wait time for phase 38.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 38 Wait Time")]
        Phase38WaitTime = 649,

        ///<summary>
        ///Recorded wait time for phase 39.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 39 Wait Time")]
        Phase39WaitTime = 650,

        ///<summary>
        ///Recorded wait time for phase 40.
        ///The <c>Parameter</c> is Wait Time in Seconds
        ///</summary>
        [Display(Name = "Phase 40 Wait Time")]
        Phase40WaitTime = 651,

        ///<summary>
        ///External Phase Min Call received.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "External Min Call On")]
        ExternalMinCallOn = 701,

        ///<summary>
        ///External Phase Min Call removed.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "External Min Call Off")]
        ExternalMinCallOff = 702,

        ///<summary>
        ///External Phase Max Call received.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "External Max Call On")]
        ExternalMaxCallOn = 703,

        ///<summary>
        ///External Phase Max Call removed.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "External Max Call Off")]
        ExternalMaxCallOff = 704,

        ///<summary>
        ///External Ped Call received.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "External Ped Call On")]
        ExternalPedCallOn = 705,

        ///<summary>
        ///External Ped Call removed.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "External Ped Call Off")]
        ExternalPedCallOff = 706,

        ///<summary>
        ///External Preempt Call received.
        ///The <c>Parameter</c> is Preempt Number
        ///</summary>
        [Display(Name = "External Preempt Call On")]
        ExternalPreemptCallOn = 707,

        ///<summary>
        ///External Preempt Call removed.
        ///The <c>Parameter</c> is Preempt Number
        ///</summary>
        [Display(Name = "External Preempt Call Off")]
        ExternalPreemptCallOff = 708,

        ///<summary>
        ///External Special Function Call received
        ///The <c>Parameter</c> is Special Function Number
        ///</summary>
        [Display(Name = "External Special Function Call On")]
        ExternalSpecialFunctionCallOn = 709,

        ///<summary>
        ///External Special Function Call removed.
        ///The <c>Parameter</c> is Special Function Number
        ///</summary>
        [Display(Name = "External Special Function Call Off")]
        ExternalSpecialFunctionCallOff = 710,

        ///<summary>
        ///Set when maximum green (MAX 3) interval is in-effect for the active phase.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Max 3 In-Effect")]
        Max3InEffect = 711,

        ///<summary>
        ///Set when maximum green (Conditional MAX) interval is in-effect for the active phase.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Conditional Max In-Effect")]
        ConditionalMaxInEffect = 712,

        ///<summary>
        ///Set when maximum green (Preempt Exit Max) interval is in-effect for the active phase.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Preempt Exit Max In-Effect")]
        PreemptExitMaxInEffect = 713,

        ///<summary>
        ///Set when maximum green (Coord Free Max) interval is in-effect for the active phase.
        ///The <c>Parameter</c> is Phase Number
        ///</summary>
        [Display(Name = "Coord Free Max In Effect")]
        CoordFreeMaxInEffect = 714,

        ///<summary>
        ///Set when maximum green (TSP Max) interval is in-effect for the active phase.
        ///The <c>Parameter</c> is Plan Number
        ///</summary>
        [Display(Name = "TSP Max In-Effect")]
        TSPMaxInEffect = 715,

        ///<summary>
        ///Set when coordinator is commanded to run a bad plan.
        ///The <c>Parameter</c> is Plan Number
        ///</summary>
        [Display(Name = "Coord Bad Plan Alarm On")]
        CoordBadPlanAlarmOn = 716,

        ///<summary>
        ///Set when coordinator switches from a bad plan to a valid plan.
        ///The <c>Parameter</c> is Plan Number
        ///</summary>
        [Display(Name = "Coord Bad Plan Alarm Off")]
        CoordBadPlanAlarmOff = 717,

        ///<summary>
        ///Set when an NTCIP 1211 Request message is received.
        ///The <c>Parameter</c> is Strategy, Request Id, ETA, ETD
        ///</summary>
        [Display(Name = "NTCIP 1211 Request Message")]
        NTCIP1211RequestMessage = 718,

        ///<summary>
        ///Set when an NTCIP 1211 Update message is received.
        ///The <c>Parameter</c> is Strategy, Request Id, ETA, ETD
        ///</summary>
        [Display(Name = "NTCIP 1211 Update Message")]
        NTCIP1211UpdateMessage = 719,

        ///<summary>
        ///Set when an NTCIP 1211 Cancel message is received.
        ///The <c>Parameter</c> is Request ID Number
        ///</summary>
        [Display(Name = "NTCIP 1211 Cancel Message")]
        NTCIP1211CancelMessage = 720,

        ///<summary>
        ///Set when an NTCIP 1211 clear message is received.
        ///The <c>Parameter</c> is Request Id Number
        ///</summary>
        [Display(Name = "NTCIP 1211 Clear Message")]
        NTCIP1211ClearMessage = 721,

        ///<summary>
        ///Non Concurrent Phases detected.
        ///The <c>Parameter</c> is Ring Number
        ///</summary>
        [Display(Name = "Non Concurrent Phases")]
        NonConcurrentPhases = 900,

        ///<summary>
        ///Barrier Entry Ring 1
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 1")]
        BarrierEntryRing1 = 901,

        ///<summary>
        ///Barrier Entry Ring 2
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 2")]
        BarrierEntryRing2 = 902,

        ///<summary>
        ///Barrier Entry Ring 3
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 3")]
        BarrierEntryRing3 = 903,

        ///<summary>
        ///Barrier Entry Ring 4
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 4")]
        BarrierEntryRing4 = 904,

        ///<summary>
        ///Barrier Entry Ring 5
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 5")]
        BarrierEntryRing5 = 905,

        ///<summary>
        ///Barrier Entry Ring 6
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 6")]
        BarrierEntryRing6 = 906,

        ///<summary>
        ///Barrier Entry Ring 7
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 7")]
        BarrierEntryRing7 = 907,

        ///<summary>
        ///Barrier Entry Ring 8
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 8")]
        BarrierEntryRing8 = 908,

        ///<summary>
        ///Barrier Entry Ring 9
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 9")]
        BarrierEntryRing9 = 909,

        ///<summary>
        ///Barrier Entry Ring 10
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 10")]
        BarrierEntryRing10 = 910,

        ///<summary>
        ///Barrier Entry Ring 11
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 11")]
        BarrierEntryRing11 = 911,

        ///<summary>
        ///Barrier Entry Ring 12
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 12")]
        BarrierEntryRing12 = 912,

        ///<summary>
        ///Barrier Entry Ring 13
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 13")]
        BarrierEntryRing13 = 913,

        ///<summary>
        ///Barrier Entry Ring 14
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 14")]
        BarrierEntryRing14 = 914,

        ///<summary>
        ///Barrier Entry Ring 15
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 15")]
        BarrierEntryRing15 = 915,

        ///<summary>
        ///Barrier Entry Ring 16
        ///The <c>Parameter</c> is Barrier Number
        ///</summary>
        [Display(Name = "Barrier Entry Ring 16")]
        BarrierEntryRing16 = 916,

        */

        #endregion

        #region QFree Ramp Metering

        ///<summary>
        ///Ramp Meter Lane Demand Detector Recalled
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLaneDemandDetectorRecalled = 1206,

        ///<summary>
        ///Ramp Meter Lane Demand Detector Working
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLaneDemandDetectorWorking = 1207,

        ///<summary>
        ///Ramp Meter Lane Demand Detector Other
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLaneDemandDetectorOther = 1208,

        ///<summary>
        ///Ramp Meter Lane Demand Detector EC
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLaneDemandDetectorEC = 1209,

        ///<summary>
        ///Ramp Meter Lane Demand Detector MP
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLaneDemandDetectorMP = 1210,

        ///<summary>
        ///Ramp Meter Lane Demand Detector NA
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLaneDemandDetectorNA = 1211,

        ///<summary>
        ///Ramp Meter Lane Demand Detector Error
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLaneDemandDetectorError = 1212,

        ///<summary>
        ///Ramp Meter Lane Demand Detector Dep NA
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLaneDemandDetectorDepNA = 1213,

        ///<summary>
        ///Ramp Meter Lane Demand Detector Dep MP
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLaneDemandDetectorDepMP = 1214,

        ///<summary>
        ///Ramp Meter Lane Passage Detector Recalled
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLanePassageDetectorRecalled = 1215,

        ///<summary>
        ///Ramp Meter Lane Passage Detector Working
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLanePassageDetectorWorking = 1216,

        ///<summary>
        ///Ramp Meter Lane Passage Detector Other
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLanePassageDetectorOther = 1217,

        ///<summary>
        ///Ramp Meter Lane Passage Detector EC
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLanePassageDetectorEC = 1218,

        ///<summary>
        ///Ramp Meter Lane Passage Detector MP
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLanePassageDetectorMP = 1219,

        ///<summary>
        ///Ramp Meter Lane Passage Detector NA
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLanePassageDetectorNA = 1220,

        ///<summary>
        ///Ramp Meter Lane Passage Detector Error
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLanePassageDetectorError = 1221,

        ///<summary>
        ///Ramp Meter Lane Passage Detector Dep NA
        ///The <c>Parameter</c> is used to indicate Lane Number
        ///</summary>
        RampMeterLanePassageDetectorDepNA = 1222,

        ///<summary>
        ///Ramp Meter Lane 1 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane1QueDetectorDisabled = 1223,

        ///<summary>
        ///Ramp Meter Lane 1 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane1QueDetectorWorking = 1224,

        ///<summary>
        ///Ramp Meter Lane 1 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane1QueDetectorOther = 1225,

        ///<summary>
        ///Ramp Meter Lane 1 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane1QueDetectorEC = 1226,

        ///<summary>
        ///Ramp Meter Lane 1 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane1QueDetectorMP = 1227,

        ///<summary>
        ///Ramp Meter Lane 1 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane1QueDetectorNA = 1228,

        ///<summary>
        ///Ramp Meter Lane 1 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane1QueDetectorError = 1229,

        ///<summary>
        ///Ramp Meter Lane 2 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane2QueDetectorDisabled = 1230,

        ///<summary>
        ///Ramp Meter Lane 2 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane2QueDetectorWorking = 1231,

        ///<summary>
        ///Ramp Meter Lane 2 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane2QueDetectorOther = 1232,

        ///<summary>
        ///Ramp Meter Lane 2 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane2QueDetectorEC = 1233,

        ///<summary>
        ///Ramp Meter Lane 2 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane2QueDetectorMP = 1234,

        ///<summary>
        ///Ramp Meter Lane 2 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane2QueDetectorNA = 1235,

        ///<summary>
        ///Ramp Meter Lane 2 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane2QueDetectorError = 1236,

        ///<summary>
        ///Ramp Meter Lane 3 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane3QueDetectorDisabled = 1237,

        ///<summary>
        ///Ramp Meter Lane 3 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane3QueDetectorWorking = 1238,

        ///<summary>
        ///Ramp Meter Lane 3 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane3QueDetectorOther = 1239,

        ///<summary>
        ///Ramp Meter Lane 3 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane3QueDetectorEC = 1240,

        ///<summary>
        ///Ramp Meter Lane 3 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane3QueDetectorMP = 1241,

        ///<summary>
        ///Ramp Meter Lane 3 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane3QueDetectorNA = 1242,

        ///<summary>
        ///Ramp Meter Lane 3 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane3QueDetectorError = 1243,

        ///<summary>
        ///Ramp Meter Lane 4 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane4QueDetectorDisabled = 1244,

        ///<summary>
        ///Ramp Meter Lane 4 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane4QueDetectorWorking = 1245,

        ///<summary>
        ///Ramp Meter Lane 4 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane4QueDetectorOther = 1246,

        ///<summary>
        ///Ramp Meter Lane 4 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane4QueDetectorEC = 1247,

        ///<summary>
        ///Ramp Meter Lane 4 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane4QueDetectorMP = 1248,

        ///<summary>
        ///Ramp Meter Lane 4 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane4QueDetectorNA = 1249,

        ///<summary>
        ///Ramp Meter Lane 4 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane4QueDetectorError = 1250,

        ///<summary>
        ///Ramp Meter Lane 5 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane5QueDetectorDisabled = 1251,

        ///<summary>
        ///Ramp Meter Lane 5 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane5QueDetectorWorking = 1252,

        ///<summary>
        ///Ramp Meter Lane 5 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane5QueDetectorOther = 1253,

        ///<summary>
        ///Ramp Meter Lane 5 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane5QueDetectorEC = 1254,

        ///<summary>
        ///Ramp Meter Lane 5 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane5QueDetectorMP = 1255,

        ///<summary>
        ///Ramp Meter Lane 5 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane5QueDetectorNA = 1256,

        ///<summary>
        ///Ramp Meter Lane 5 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane5QueDetectorError = 1257,

        ///<summary>
        ///Ramp Meter Lane 6 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane6QueDetectorDisabled = 1258,

        ///<summary>
        ///Ramp Meter Lane 6 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane6QueDetectorWorking = 1259,

        ///<summary>
        ///Ramp Meter Lane 6 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane6QueDetectorOther = 1260,

        ///<summary>
        ///Ramp Meter Lane 6 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane6QueDetectorEC = 1261,

        ///<summary>
        ///Ramp Meter Lane 6 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane6QueDetectorMP = 1262,

        ///<summary>
        ///Ramp Meter Lane 6 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane6QueDetectorNA = 1263,

        ///<summary>
        ///Ramp Meter Lane 6 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane6QueDetectorError = 1264,

        ///<summary>
        ///Ramp Meter Lane 7 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane7QueDetectorDisabled = 1265,

        ///<summary>
        ///Ramp Meter Lane 7 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane7QueDetectorWorking = 1266,

        ///<summary>
        ///Ramp Meter Lane 7 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane7QueDetectorOther = 1267,

        ///<summary>
        ///Ramp Meter Lane 7 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane7QueDetectorEC = 1268,

        ///<summary>
        ///Ramp Meter Lane 7 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane7QueDetectorMP = 1269,

        ///<summary>
        ///Ramp Meter Lane 7 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane7QueDetectorNA = 1270,

        ///<summary>
        ///Ramp Meter Lane 7 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane7QueDetectorError = 1271,

        ///<summary>
        ///Ramp Meter Lane 8 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane8QueDetectorDisabled = 1272,

        ///<summary>
        ///Ramp Meter Lane 8 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane8QueDetectorWorking = 1273,

        ///<summary>
        ///Ramp Meter Lane 8 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane8QueDetectorOther = 1274,

        ///<summary>
        ///Ramp Meter Lane 8 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane8QueDetectorEC = 1275,

        ///<summary>
        ///Ramp Meter Lane 8 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane8QueDetectorMP = 1276,

        ///<summary>
        ///Ramp Meter Lane 8 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane8QueDetectorNA = 1277,

        ///<summary>
        ///Ramp Meter Lane 8 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane8QueDetectorError = 1278,

        ///<summary>
        ///Ramp Meter Lane 9 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane9QueDetectorDisabled = 1279,

        ///<summary>
        ///Ramp Meter Lane 9 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane9QueDetectorWorking = 1280,

        ///<summary>
        ///Ramp Meter Lane 9 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane9QueDetectorOther = 1281,

        ///<summary>
        ///Ramp Meter Lane 9 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane9QueDetectorEC = 1282,

        ///<summary>
        ///Ramp Meter Lane 9 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane9QueDetectorMP = 1283,

        ///<summary>
        ///Ramp Meter Lane 9 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane9QueDetectorNA = 1284,

        ///<summary>
        ///Ramp Meter Lane 9 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane9QueDetectorError = 1285,

        ///<summary>
        ///Ramp Meter Lane 10 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane10QueDetectorDisabled = 1286,

        ///<summary>
        ///Ramp Meter Lane 10 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane10QueDetectorWorking = 1287,

        ///<summary>
        ///Ramp Meter Lane 10 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane10QueDetectorOther = 1288,

        ///<summary>
        ///Ramp Meter Lane 10 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane10QueDetectorEC = 1289,

        ///<summary>
        ///Ramp Meter Lane 10 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane10QueDetectorMP = 1290,

        ///<summary>
        ///Ramp Meter Lane 10 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane10QueDetectorNA = 1291,

        ///<summary>
        ///Ramp Meter Lane 10 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane10QueDetectorError = 1292,

        ///<summary>
        ///Ramp Meter Lane 11 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane11QueDetectorDisabled = 1293,

        ///<summary>
        ///Ramp Meter Lane 11 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane11QueDetectorWorking = 1294,

        ///<summary>
        ///Ramp Meter Lane 11 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane11QueDetectorOther = 1295,

        ///<summary>
        ///Ramp Meter Lane 11 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane11QueDetectorEC = 1296,

        ///<summary>
        ///Ramp Meter Lane 11 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane11QueDetectorMP = 1297,

        ///<summary>
        ///Ramp Meter Lane 11 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane11QueDetectorNA = 1298,

        ///<summary>
        ///Ramp Meter Lane 11 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane11QueDetectorError = 1299,

        ///<summary>
        ///Ramp Meter Lane 12 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane12QueDetectorDisabled = 1300,

        ///<summary>
        ///Ramp Meter Lane 12 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane12QueDetectorWorking = 1301,

        ///<summary>
        ///Ramp Meter Lane 12 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane12QueDetectorOther = 1302,

        ///<summary>
        ///Ramp Meter Lane 12 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane12QueDetectorEC = 1303,

        ///<summary>
        ///Ramp Meter Lane 12 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane12QueDetectorMP = 1304,

        ///<summary>
        ///Ramp Meter Lane 12 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane12QueDetectorNA = 1305,

        ///<summary>
        ///Ramp Meter Lane 12 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane12QueDetectorError = 1306,

        ///<summary>
        ///Ramp Meter Lane 13 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane13QueDetectorDisabled = 1307,

        ///<summary>
        ///Ramp Meter Lane 13 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane13QueDetectorWorking = 1308,

        ///<summary>
        ///Ramp Meter Lane 13 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane13QueDetectorOther = 1309,

        ///<summary>
        ///Ramp Meter Lane 13 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane13QueDetectorEC = 1310,

        ///<summary>
        ///Ramp Meter Lane 13 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane13QueDetectorMP = 1311,

        ///<summary>
        ///Ramp Meter Lane 13 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane13QueDetectorNA = 1312,

        ///<summary>
        ///Ramp Meter Lane 13 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane13QueDetectorError = 1313,

        ///<summary>
        ///Ramp Meter Lane 14 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane14QueDetectorDisabled = 1314,

        ///<summary>
        ///Ramp Meter Lane 14 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane14QueDetectorWorking = 1315,

        ///<summary>
        ///Ramp Meter Lane 14 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane14QueDetectorOther = 1316,

        ///<summary>
        ///Ramp Meter Lane 14 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane14QueDetectorEC = 1317,

        ///<summary>
        ///Ramp Meter Lane 14 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane14QueDetectorMP = 1318,

        ///<summary>
        ///Ramp Meter Lane 14 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane14QueDetectorNA = 1319,

        ///<summary>
        ///Ramp Meter Lane 14 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane14QueDetectorError = 1320,

        ///<summary>
        ///Ramp Meter Lane 15 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane15QueDetectorDisabled = 1321,

        ///<summary>
        ///Ramp Meter Lane 15 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane15QueDetectorWorking = 1322,

        ///<summary>
        ///Ramp Meter Lane 15 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane15QueDetectorOther = 1323,

        ///<summary>
        ///Ramp Meter Lane 15 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane15QueDetectorEC = 1324,

        ///<summary>
        ///Ramp Meter Lane 15 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane15QueDetectorMP = 1325,

        ///<summary>
        ///Ramp Meter Lane 15 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane15QueDetectorNA = 1326,

        ///<summary>
        ///Ramp Meter Lane 15 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane15QueDetectorError = 1327,

        ///<summary>
        ///Ramp Meter Lane 16 Que Detector Disabled
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane16QueDetectorDisabled = 1328,

        ///<summary>
        ///Ramp Meter Lane 16 Que Detector Working
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane16QueDetectorWorking = 1329,

        ///<summary>
        ///Ramp Meter Lane 16 Que Detector Other
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane16QueDetectorOther = 1330,

        ///<summary>
        ///Ramp Meter Lane 16 Que Detector EC
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane16QueDetectorEC = 1331,

        ///<summary>
        ///Ramp Meter Lane 16 Que Detector MP
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane16QueDetectorMP = 1332,

        ///<summary>
        ///Ramp Meter Lane 16 Que Detector NA
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane16QueDetectorNA = 1333,

        ///<summary>
        ///Ramp Meter Lane 16 Que Detector Error
        ///The <c>Parameter</c> is used to indicate 1 - Intermediate Queue or 2 - Excessive Queue
        ///</summary>
        RampMeterLane16QueDetectorError = 1334

        #endregion
    }
}

