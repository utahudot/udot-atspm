using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.SplitFail
{
    public class SplitFailOptions : OptionsBase
    {
        public int FirstSecondsOfRed { get; set; }
        public int MetricTypeId { get; set; } = 12;
        public bool GetPermissivePhase { get; set; }
    }
}