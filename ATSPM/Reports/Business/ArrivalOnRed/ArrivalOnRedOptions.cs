using Reports.Business.Common;

namespace ATSPM.Application.Reports.Business.ArrivalOnRed
{
    public class ArrivalOnRedOptions : OptionsBase
    {
        public int SelectedBinSize { get; set; }
        public bool ShowPlanStatistics { get; set; }
    }
}