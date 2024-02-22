using ATSPM.Application.Business.Common;

namespace ATSPM.Application.Business.ArrivalOnRed
{
    public class ArrivalOnRedOptions : OptionsBase
    {
        public int BinSize { get; set; }
        public bool GetPermissivePhase { get; set; }
    }
}