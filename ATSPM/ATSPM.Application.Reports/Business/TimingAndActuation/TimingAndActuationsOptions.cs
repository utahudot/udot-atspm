using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.TimingAndActuation
{
    [DataContract]
    public class TimingAndActuationsOptions
    {

        [Display(Name = "Extend Search (left) Minutes.decimal")]
        [Required]
        [DataMember]
        public double ExtendVsdSearch { get; set; }

        [Display(Name = "Vehicle Signal Display")]
        [Required]
        [DataMember]
        public bool ShowVehicleSignalDisplay { get; set; }

        [Display(Name = "Pedestrian Intervals")]
        [Required]
        [DataMember]
        public bool ShowPedestrianIntervals { get; set; }

        [Display(Name = "Pedestrian Actuations")]
        [Required]
        [DataMember]
        public bool ShowPedestrianActuation { get; set; }

        [Display(Name = "Extend Start/Stop Search Minutes.decimal")]
        [Required]
        [DataMember]
        public double ExtendStartStopSearch { get; set; }

        [Display(Name = "Stop Bar Presence")]
        [Required]
        [DataMember]
        public bool ShowStopBarPresence { get; set; }

        [Display(Name = "Lane-by-lane Count")]
        [Required]
        [DataMember]
        public bool ShowLaneByLaneCount { get; set; }

        [Display(Name = "Advanced Presence")]
        [Required]
        [DataMember]
        public bool ShowAdvancedDilemmaZone { get; set; }

        [Display(Name = "Advanced Count")]
        [Required]
        [DataMember]
        public bool ShowAdvancedCount { get; set; }

        [Display(Name = "All Lanes For Each Phase")]
        [Required]
        [DataMember]
        public bool ShowAllLanesInfo { get; set; }

        [Display(Name = "Show Permissive Phases")]
        [Required]
        [DataMember]
        public bool ShowPermissivePhases { get; set; }

        public List<int> GlobalEventCodesList { get; set; }
        public List<int> GlobalEventParamsList { get; set; }
        public List<int> PhaseEventCodesList { get; set; }
        public int GlobalEventCounter { get; set; }
        public DateTime End { get; set; }
        public DateTime Start { get; set; }
        public int PhaseNumber { get; set; }
        public bool PhaseOrOverlap { get; set; }
        public int ApproachId { get; set; }
        public bool GetPermissivePhase { get; set; }

       
    }
}
