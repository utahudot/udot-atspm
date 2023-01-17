using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Legacy.Common.Business.WCFServiceLibrary
{
    [DataContract]
    public class ApproachSpeedOptions : MetricOptions
    {
        public ApproachSpeedOptions(string signalID, DateTime startDate, DateTime endDate, double yAxisMax,
            double yAxisMin,
            int binSize,bool showPlanStatistics, int metricTypeID, bool showPostedSpeed,
            bool showAverageSpeed, bool show85Percentile, bool show15Percentile)
        {
            SignalId = signalID;
            StartDate = startDate;
            EndDate = endDate;
            YAxisMax = yAxisMax;
            YAxisMin = yAxisMin;
            SelectedBinSize = binSize;
            ShowPlanStatistics = showPlanStatistics;
            ShowPostedSpeed = showPostedSpeed;
            ShowAverageSpeed = showAverageSpeed;
            Show85Percentile = show85Percentile;
            Show15Percentile = show15Percentile;
            MetricTypeId = metricTypeID;
        }


        public ApproachSpeedOptions()
        {
            MetricTypeId = 10;
            SetDefaults();
        }

        [Required]
        [DataMember]
        [Display(Name = "Volume Bin Size")]
        public int SelectedBinSize { get; set; }

        [DataMember]
        [Display(Name = "Show Plans")]
        public bool ShowPlanStatistics { get; set; }

        [DataMember]
        [Display(Name = "Show Posted Speed")]
        public bool ShowPostedSpeed { get; set; }

        [DataMember]
        [Display(Name = "Show Average Speed")]
        public bool ShowAverageSpeed { get; set; }

        [DataMember]
        [Display(Name = "Show 85% Speeds")]
        public bool Show85Percentile { get; set; }

        [DataMember]
        [Display(Name = "Show 15% Speeds")]
        public bool Show15Percentile { get; set; }

        
    }
}