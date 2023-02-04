using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.ApproachSpeed
{
    [DataContract]
    public class ApproachSpeedOptions
    {
        public ApproachSpeedOptions(
            int detectorId,
            DateTime startDate,
            DateTime endDate,
            int binSize,
            bool showPlanStatistics,
            int metricTypeID,
            bool showPostedSpeed,
            bool showAverageSpeed,
            bool show85Percentile,
            bool show15Percentile)
        {
            DetectorId = detectorId;
            StartDate = startDate;
            EndDate = endDate;
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
        public int MetricTypeId { get; private set; }
        public string SignalId { get; private set; }
        public int DetectorId { get; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
    }
}