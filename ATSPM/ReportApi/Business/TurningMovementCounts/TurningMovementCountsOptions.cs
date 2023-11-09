using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.TurningMovementCounts
{
    public class TurningMovementCountsOptions : OptionsBase
    {
        public int SelectedBinSize { get; set; }
        public int MetricTypeId { get; internal set; } = 5;
    }
}