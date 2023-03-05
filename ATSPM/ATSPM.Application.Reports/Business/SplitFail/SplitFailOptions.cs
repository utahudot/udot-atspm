using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.SplitFail
{
    public class SplitFailOptions
    {

        [Required]
        [Display(Name = "First Seconds Of Red")]
        public int FirstSecondsOfRed { get; set; }

        [Display(Name = "Show Fail Lines")]
        public bool ShowFailLines { get; set; }

        [Display(Name = "Show Average Lines")]
        public bool ShowAvgLines { get; set; }

        [Display(Name = "Show Percent Fail Lines")]
        public bool ShowPercentFailLines { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int MetricTypeId { get; set; } = 12;
        public string SignalId { get; set; }
        public int ApproachId { get; set; }
        public bool UsePermissivePhase { get; set; }

        
    }
}