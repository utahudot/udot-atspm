using ATSPM.Data.Models;
using Legacy.Common.Business;
using System.Collections.Generic;

namespace ATSPM.Application.Reports.Business.TimingAndActuation
{
    public class TimingAndActuationsForPhaseResult
    {
        public int PhaseNumber { get; set; }
        public bool PhaseOrOverlap { get; set; }
        public int ApproachId { get; set; }
        public List<Plan> Plans { get; set; }
        public string PhaseNumberSort { get; set; }
        public bool GetPermissivePhase { get; set; }
        public List<TimingAndActuationCycle> Cycles { get; set; }
        public List<ControllerEventLog> CycleDataEventLogs { get; set; }
        public List<ControllerEventLog> PedestrianIntervals { get; set; }
        public List<ControllerEventLog> ForceEventsForAllLanes { get; set; }
        public Dictionary<string, List<ControllerEventLog>> PedestrianEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> CycleAllEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> PedestrianAllEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> PedestrianAllIntervals { get; set; }
        public Dictionary<string, List<ControllerEventLog>> AdvanceCountEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> AdvancePresenceEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> StopBarEvents { get; set; }
        public Dictionary<string, List<ControllerEventLog>> LaneByLanes { get; set; }
        public Dictionary<string, List<ControllerEventLog>> PhaseCustomEvents { get; set; }
    }
}

