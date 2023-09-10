using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.ArrivalOnRed
{
    [DataContract]
    public class ArrivalOnRedOptions
    {
        [Required]
        [DataMember]
        [Display(Name = "Volume Bin Size")]
        public int SelectedBinSize { get; set; }
        [DataMember]
        [Display(Name = "Show Plans")]
        public bool ShowPlanStatistics { get; set; }
        public string SignalIdentifier { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool UsePermissivePhase { get; set; }
    }
}