using ATSPM.Application.Business.Common;
using ATSPM.Data.Enums;
using System.Collections.Generic;

namespace ATSPM.Application.Business.TimingAndActuation
{
    public class TimingAndActuationsOptions : BasePhaseOptions
    {
        public bool ShowPedestrianIntervals { get; set; }
        public bool ShowPedestrianActuation { get; set; }
        public bool ShowStopBarPresence { get; set; }
        public bool ShowLaneByLaneCount { get; set; }
        public bool ShowAdvancedDilemmaZone { get; set; }
        public bool ShowAdvancedCount { get; set; }
        public List<short>? GlobalEventCodesList { get; set; }
        public List<short>? GlobalEventParamsList { get; set; }
        public List<short>? PhaseEventCodesList { get; set; }
        public int GlobalEventCounter { get; set; }
    }
}
