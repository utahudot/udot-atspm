using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.ArrivalOnRed
{
    public class ArrivalOnRedOptions : OptionsBase
    {
        public int BinSize { get; set; }
        public bool GetPermissivePhase { get; set; }
    }
}