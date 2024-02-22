using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.GreenTimeUtilization
{
    public class GreenTimeUtilizationOptions : OptionsBase
    {
        public int MetricTypeId { get; set; } = 36;
        public int XAxisBinSize { get; set; }
        public int YAxisBinSize { get; set; }
    }
}