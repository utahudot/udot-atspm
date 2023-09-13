using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.TimingAndActuation
{
    [DataContract]
    public class TimingAndActuationsOptions
    {
        public double ExtendVsdSearch { get; set; }
        public bool ShowVehicleSignalDisplay { get; set; }
        public bool ShowPedestrianIntervals { get; set; }
        public bool ShowPedestrianActuation { get; set; }
        public double ExtendStartStopSearch { get; set; }
        public bool ShowStopBarPresence { get; set; }
        public bool ShowLaneByLaneCount { get; set; }
        public bool ShowAdvancedDilemmaZone { get; set; }
        public bool ShowAdvancedCount { get; set; }
        public bool ShowAllLanesInfo { get; set; }
        public bool ShowPermissivePhases { get; set; }
        public List<int> GlobalEventCodesList { get; set; }
        public List<int> GlobalEventParamsList { get; set; }
        public List<int> PhaseEventCodesList { get; set; }
        public int GlobalEventCounter { get; set; }
        public DateTime End { get; set; }
        public DateTime Start { get; set; }
        public string SignalIdentifier { get; set; }
    }
}
