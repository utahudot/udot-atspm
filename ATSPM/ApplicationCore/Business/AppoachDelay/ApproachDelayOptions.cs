using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.AppoachDelay
{
    public class ApproachDelayOptions : OptionsBase
    {
        public int BinSize { get; set; }
        public bool GetPermissivePhase { get; set; }
        public bool GetVolume { get; set; } = true;
    }
}