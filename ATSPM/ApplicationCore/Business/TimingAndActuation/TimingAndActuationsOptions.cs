using ATSPM.ReportApi.Business.Common;
using System.Collections.Generic;

namespace ATSPM.ReportApi.Business.TimingAndActuation
{
    public class TimingAndActuationsOptions : BasePhaseOptions
    {
        public bool ShowPedestrianIntervals { get; set; }
        public bool ShowPedestrianActuation { get; set; }
        public bool ShowStopBarPresence { get; set; }
        public bool ShowLaneByLaneCount { get; set; }
        public bool ShowAdvancedDilemmaZone { get; set; }
        public bool ShowAdvancedCount { get; set; }
        public List<int>? GlobalEventCodesList { get; set; }
        public List<int>? GlobalEventParamsList { get; set; }
        public List<int>? PhaseEventCodesList { get; set; }
        public int GlobalEventCounter { get; set; }
    }
}
