using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.ApproachVolume
{
    [DataContract]
    public class ApproachVolumeOptions
    {
        public List<MetricInfo> MetricInfoList;


        public ApproachVolumeOptions(
            string signalId,
            DirectionTypes direction,
            DetectionTypes detectionType,
            DateTime startDate,
            DateTime endDate,
            int binSize,
            bool showDirectionalSplits,
            bool showTotalVolume,
            bool showNbEbVolume,
            bool showSbWbVolume,
            bool showTmcDetection,
            bool showAdvanceDetection)
        {
            SignalId = signalId;
            Direction = direction;
            DetectionType = detectionType;
            Start = startDate;
            End = endDate;
            SelectedBinSize = binSize;
            ShowTotalVolume = showTotalVolume;
            ShowDirectionalSplits = showDirectionalSplits;
            ShowNbEbVolume = showNbEbVolume;
            ShowSbWbVolume = showSbWbVolume;
            ShowTMCDetection = showTmcDetection;
            ShowAdvanceDetection = showAdvanceDetection;
            MetricTypeId = 7;
        }

        [Required]
        [DataMember]
        [Display(Name = "Volume Bin Size")]
        public int SelectedBinSize { get; set; }

        [DataMember]
        [Display(Name = "Show Directional Splits")]
        public bool ShowDirectionalSplits { get; set; }

        [DataMember]
        [Display(Name = "Show Total Volume")]
        public bool ShowTotalVolume { get; set; }

        [DataMember]
        [Display(Name = "Show NB/EB Volume")]
        public bool ShowNbEbVolume { get; set; }

        [DataMember]
        [Display(Name = "Show SB/WB Volume")]
        public bool ShowSbWbVolume { get; set; }

        [DataMember]
        [Display(Name = "Show TMC Detection")]
        public bool ShowTMCDetection { get; set; }

        [DataMember]
        [Display(Name = "Show Advance Detection")]
        public bool ShowAdvanceDetection { get; set; }
        public int MetricTypeId { get; private set; }
        public string SignalId { get; private set; }
        public DirectionTypes Direction { get; }
        public DetectionTypes DetectionType { get; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
    }
}