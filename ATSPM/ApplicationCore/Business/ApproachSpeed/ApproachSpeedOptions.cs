using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.ApproachSpeed
{
    public class ApproachSpeedOptions : OptionsBase
    {
        public int BinSize { get; set; }
        public int MetricTypeId { get; } = 10;
    }
}