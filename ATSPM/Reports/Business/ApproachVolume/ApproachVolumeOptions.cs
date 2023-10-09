using ATSPM.Data.Enums;
using Reports.Business.Common;

namespace ATSPM.Application.Reports.Business.ApproachVolume
{
    public class ApproachVolumeOptions : OptionsBase
    {
        public int SelectedBinSize { get; set; }
        public bool ShowDirectionalSplits { get; set; }
        public bool ShowTotalVolume { get; set; }
        public bool ShowNbEbVolume { get; set; }
        public bool ShowSbWbVolume { get; set; }
        public bool ShowTMCDetection { get; set; }
        public bool ShowAdvanceDetection { get; set; }
        public DirectionTypes Direction { get; set; }
        public DetectionTypes DetectionType { get; set; }
        public int MetricTypeId { get; internal set; } = 7;
    }
}