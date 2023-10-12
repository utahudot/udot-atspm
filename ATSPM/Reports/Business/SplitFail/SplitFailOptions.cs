using Reports.Business.Common;

namespace ATSPM.Application.Reports.Business.SplitFail
{
    public class SplitFailOptions : OptionsBase
    {
        public int FirstSecondsOfRed { get; set; }
        public int MetricTypeId { get; set; } = 12;
        public bool UsePermissivePhase { get; set; }


    }
}