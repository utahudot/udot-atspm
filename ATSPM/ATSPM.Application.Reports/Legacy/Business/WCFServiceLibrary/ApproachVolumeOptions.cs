using Legacy.Common.Business.ApproachVolume;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Legacy.Common.Business.WCFServiceLibrary
{
    [DataContract]
    public class ApproachVolumeOptions
    {
        public List<MetricInfo> MetricInfoList;


        public ApproachVolumeOptions(string signalId, DateTime startDate, DateTime endDate, double? yAxisMax,
            int binSize, bool showDirectionalSplits, bool showTotalVolume, bool showNbEbVolume, bool showSbWbVolume, bool showTmcDetection,
            bool showAdvanceDetection)
        {
            SignalId = signalId;
            StartDate = startDate;
            EndDate = endDate;
            SelectedBinSize = binSize;
            ShowTotalVolume = showTotalVolume;
            ShowDirectionalSplits = showDirectionalSplits;
            ShowNbEbVolume = showNbEbVolume;
            ShowSbWbVolume = showSbWbVolume;
            ShowTMCDetection = showTmcDetection;
            ShowAdvanceDetection = showAdvanceDetection;
            MetricTypeId = 7;
        }

        public ApproachVolumeOptions()
        {
            BinSizeList = new List<int>() { 5, 15 };
        }

        [Required]
        [DataMember]
        [Display(Name = "Volume Bin Size")]
        public int SelectedBinSize { get; set; }

        [DataMember]
        public List<int> BinSizeList { get; set; }

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
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
    }
}