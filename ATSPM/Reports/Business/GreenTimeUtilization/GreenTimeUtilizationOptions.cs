using Reports.Business.Common;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram
{
    public class GreenTimeUtilizationOptions : OptionsBase
    {
        public int MetricTypeId { get; set; } = 36;
        public int XAxisBinSize { get; set; }
        public int YAxisBinSize { get; set; }
    }
}