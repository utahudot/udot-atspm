using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.LeftTurnGapAnalysis
{
    [DataContract]
    public class LeftTurnGapAnalysisOptions
    {
        public const int EVENT_GREEN = 1;
        public const int EVENT_RED = 10;
        public const int EVENT_DET = 81;



        public LeftTurnGapAnalysisOptions()
        {
        }

        [DataMember]
        [Display(Name = "Gap 1 Minimum (seconds) ")]
        public double Gap1Min { get; set; } = 0;

        [DataMember]
        [Display(Name = "Gap 1 Maximum (seconds)")]
        public double Gap1Max { get; set; } = 1;

        [DataMember]
        [Display(Name = "Gap 2 Minimum (seconds) ")]
        public double Gap2Min { get; set; } = 1;

        [DataMember]
        [Display(Name = "Gap 2 Maximum (seconds)")]
        public double Gap2Max { get; set; } = 3.3;

        [DataMember]
        [Display(Name = "Gap 3 Minimum (seconds) ")]
        public double Gap3Min { get; set; } = 3.3;

        [DataMember]
        [Display(Name = "Gap 3 Maximum (seconds)")]
        public double Gap3Max { get; set; } = 3.7;

        [DataMember]
        [Display(Name = "Gap 4 Minimum (seconds) ")]
        public double Gap4Min { get; set; } = 3.7;

        [DataMember]
        [Display(Name = "Gap 4 Maximum (seconds) ")]
        public double? Gap4Max { get; set; } = 3.9;

        [DataMember]
        [Display(Name = "Gap 5 Minimum (seconds) ")]
        public double? Gap5Min { get; set; } = 3.9;

        [DataMember]
        [Display(Name = "Gap 5 Maximum (seconds) ")]
        public double? Gap5Max { get; set; } = 4.1;

        [DataMember]
        [Display(Name = "Gap 6 Minimum (seconds) ")]
        public double? Gap6Min { get; set; } = 4.1;

        [DataMember]
        [Display(Name = "Gap 6 Maximum (seconds) ")]
        public double? Gap6Max { get; set; } = 5.3;
        [DataMember]
        [Display(Name = "Gap 7 Minimum (seconds) ")]
        public double? Gap7Min { get; set; } = 5.3;

        [DataMember]
        [Display(Name = "Gap 7 Maximum (seconds) ")]
        public double? Gap7Max { get; set; } = 5.5;
        [DataMember]
        [Display(Name = "Gap 8 Minimum (seconds) ")]
        public double? Gap8Min { get; set; } = 5.5;

        [DataMember]
        [Display(Name = "Gap 8 Maximum (seconds) ")]
        public double? Gap8Max { get; set; } = 6.5;

        [DataMember]
        [Display(Name = "Gap 9 Minimum (seconds) ")]
        public double? Gap9Min { get; set; } = 6.5;

        [DataMember]
        [Display(Name = "Gap 9 Maximum (seconds) ")]
        public double? Gap9Max { get; set; } = 6.9;

        [DataMember]
        [Display(Name = "Gap 10 Minimum (seconds) ")]
        public double? Gap10Min { get; set; } = 6.9;

        [DataMember]
        [Display(Name = "Gap 10 Maximum (seconds) ")]
        public double? Gap10Max { get; set; } = 7.4;

        [DataMember]
        [Display(Name = "Sum Duration Gap 1 (seconds) ")]
        public double? SumDurationGap1 { get; set; } = 4.1;

        [DataMember]
        [Display(Name = "Sum Duration Gap 2 (seconds) ")]
        public double? SumDurationGap2 { get; set; } = 5.3;

        [DataMember]
        [Display(Name = "Sum Duration Gap 3 (seconds) ")]
        public double? SumDurationGap3 { get; set; } = 7.4;
        [DataMember]
        public double TrendLineGapThreshold { get; set; } = 7.4;

        [DataMember]
        public double BinSize { get; set; }
        public string SignalIdentifier { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }


    }
}
