using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.GreenTimeUtilization
{
    public class GreenTimeUtilizationOptions : OptionsBase
    {
        public int MetricTypeId { get; set; } = 36;
        public int XAxisBinSize { get; set; }
        public int YAxisBinSize { get; set; }
    }
}