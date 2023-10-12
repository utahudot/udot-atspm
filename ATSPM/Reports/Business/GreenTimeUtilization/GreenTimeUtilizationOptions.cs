using Reports.Business.Common;

namespace ATSPM.Application.Reports.Business.PerdueCoordinationDiagram
{
    public class GreenTimeUtilizationOptions : OptionsBase
    {
        public int MetricTypeId { get; set; } = 36;
        public int SelectedBinSize { get; set; }
    }
}