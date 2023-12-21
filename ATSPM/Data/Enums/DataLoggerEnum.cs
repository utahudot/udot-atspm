using System;
using System.ComponentModel.DataAnnotations;


namespace ATSPM.Data.Enums
{
    /// <summary>
    /// Traffic Location Hi Resolution Data Logger Enumerations
    /// <seealso cref="Reference" href="https://docs.lib.purdue.edu/cgi/viewcontent.cgi?article=1002&context=jtrpdata"/>
    /// </summary>
    public enum DataLoggerEnum
    {
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
        PreemptionForceOff20 = 20,

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
        [Display(Name = "Detector Off")]
        DetectorOff = 81,

        ///<summary>
        ///Detector on and off events shall be triggered post any detector delay/extension processing
        ///</summary>
        [Display(Name = "Detector On")]
        DetectorOn = 82,

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
        PreemptionForceOff116 = 116,

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
        Split32Change = 218
    }
}
