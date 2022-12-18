using System.Collections.Generic;

namespace Legacy.Common.Business.ApproachVolume
{
    public class ApproachVolumeViewModel
    {
        public IEnumerable<MetricInfo> InfoList { get; set; }
        public string ShowMetricUrlJavascript { get; set; } = string.Empty;
    }
}