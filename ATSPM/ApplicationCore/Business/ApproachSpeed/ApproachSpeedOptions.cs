using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.ApproachSpeed
{
    public class ApproachSpeedOptions : OptionsBase
    {
        public int BinSize { get; set; }
        public int MetricTypeId { get; } = 10;
    }
}