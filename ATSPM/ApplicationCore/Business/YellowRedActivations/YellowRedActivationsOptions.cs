using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.YellowRedActivations
{
    public class YellowRedActivationsOptions : OptionsBase
    {
        public double SevereLevelSeconds { get; set; }
        public int MetricTypeId { get; set; } = 11;
    }
}