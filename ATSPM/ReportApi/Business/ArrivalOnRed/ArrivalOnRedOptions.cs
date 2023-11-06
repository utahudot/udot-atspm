using ATSPM.ReportApi.Business.Common;

namespace ATSPM.ReportApi.Business.ArrivalOnRed
{
    public class ArrivalOnRedOptions : OptionsBase
    {
        public int SelectedBinSize { get; set; }
        public bool ShowPlanStatistics { get; set; }
    }
}