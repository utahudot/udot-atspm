using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;

namespace ATSPM.Application.Reports.Business.TurningMovementCounts
{
    [DataContract]
    public class TurningMovementCountsOptions
    {
        private int MetricTypeID = 5;

        public TurningMovementCountsInfo TmcInfo;

        public TurningMovementCountsOptions(int approachId, DateTime startDate, DateTime endDate, double yAxisMax, double y2AxisMax,
            int binSize, bool showPlanStatistics, bool showVolumes, int metricTypeID, bool showLaneVolumes,
            bool showTotalVolumes)
        {
            ApproachId = approachId;
            Start = startDate;
            End = endDate;
            SelectedBinSize = binSize;
            MetricTypeID = metricTypeID;
            ShowLaneVolumes = showLaneVolumes;
            ShowTotalVolumes = showTotalVolumes;
        }

        [Required]
        [DataMember]
        [Display(Name = "Volume Bin Size")]
        public int SelectedBinSize { get; set; }

        [DataMember]
        [Display(Name = "Show MovementType Volume")]
        public bool ShowLaneVolumes { get; set; }

        [DataMember]
        [Display(Name = "Show Total Volume")]
        public bool ShowTotalVolumes { get; set; }

        [DataMember]
        [Display(Name = "Show Data Table")]
        public bool ShowDataTable { get; set; }
        public int ApproachId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public LaneTypes LaneType { get; set; }
        public List<MovementTypes> MovementTypes { get; set; }



    }
}