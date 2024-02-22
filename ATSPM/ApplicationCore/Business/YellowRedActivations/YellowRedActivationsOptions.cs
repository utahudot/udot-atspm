using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.YellowRedActivations
{
    public class YellowRedActivationsOptions : OptionsBase
    {
        public double SevereLevelSeconds { get; set; }
        public int MetricTypeId { get; set; } = 11;
    }
}