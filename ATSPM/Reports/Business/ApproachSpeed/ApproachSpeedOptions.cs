using Reports.Business.Common;

namespace ATSPM.Application.Reports.Business.ApproachSpeed
{
    public class ApproachSpeedOptions : OptionsBase
    {
        public int SelectedBinSize { get; set; }
        public int MetricTypeId { get; } = 10;
    }
}