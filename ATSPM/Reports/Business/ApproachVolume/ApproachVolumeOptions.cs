using ATSPM.Data.Enums;
using System;

namespace ATSPM.Application.Reports.Business.ApproachVolume
{
    public class ApproachVolumeOptions
    {
        public int SelectedBinSize { get; set; }
        public bool ShowDirectionalSplits { get; set; }
        public bool ShowTotalVolume { get; set; }
        public bool ShowNbEbVolume { get; set; }
        public bool ShowSbWbVolume { get; set; }
        public bool ShowTMCDetection { get; set; }
        public bool ShowAdvanceDetection { get; set; }
        public string SignalId { get; set; }
        public DirectionTypes Direction { get; set; }
        public DetectionTypes DetectionType { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int MetricTypeId { get; internal set; } = 7;
    }
}