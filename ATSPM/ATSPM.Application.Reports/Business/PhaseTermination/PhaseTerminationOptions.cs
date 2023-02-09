using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.PhaseTermination
{
    [DataContract]
    public class PhaseTerminationOptions
    {      

        [Required]
        [DataMember]
        [Display(Name = "Consecutive Count")]
        public int SelectedConsecutiveCount { get; set; }

        [DataMember]
        [Display(Name = "Show Ped Activity")]
        public bool ShowPedActivity { get; set; }

        [DataMember]
        [Display(Name = "Show Plans")]
        public bool ShowPlanStripes { get; set; }

        [DataMember]
        public bool ShowArrivalsOnGreen { get; set; }
        public string SignalId { get; set; }
        public int MetricTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}