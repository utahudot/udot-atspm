using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.AppoachDelay
{
    public class ApproachDelayOptions : OptionsBase
    {
        public int BinSize { get; set; }
        public bool GetPermissivePhase { get; set; }
        public bool GetVolume { get; set; }
    }
}