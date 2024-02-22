using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.TurningMovementCounts
{
    public class TurningMovementCountsOptions : OptionsBase
    {
        public int BinSize { get; set; }
        public int MetricTypeId { get; internal set; } = 5;
    }
}