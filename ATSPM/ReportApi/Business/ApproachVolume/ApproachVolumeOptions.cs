using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.ApproachVolume
{
    public class ApproachVolumeOptions : OptionsBase
    {
        public int BinSize { get; set; }
        public bool ShowDirectionalSplits { get; set; }
        public bool ShowTotalVolume { get; set; }
        public bool ShowNbEbVolume { get; set; }
        public bool ShowSbWbVolume { get; set; }
        public bool ShowTMCDetection { get; set; }
        public bool ShowAdvanceDetection { get; set; }
        public int MetricTypeId { get; internal set; } = 7;
    }
}