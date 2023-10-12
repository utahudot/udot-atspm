using Reports.Business.Common;

namespace ATSPM.Application.Reports.Business.AppoachDelay
{
    public class ApproachDelayOptions : OptionsBase
    {
        public int BinSize { get; set; }
        public bool GetVolume { get; set; }
    }
}