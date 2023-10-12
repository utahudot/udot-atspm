using Reports.Business.Common;

namespace ATSPM.Application.Reports.Business.TurningMovementCounts
{
    public class TurningMovementCountsOptions : OptionsBase
    {
        public int SelectedBinSize { get; set; }
        public int MetricTypeId { get; internal set; } = 5;
    }
}