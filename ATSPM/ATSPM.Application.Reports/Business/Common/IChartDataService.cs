using ATSPM.Application.Reports.Business.AppoachDelay;
using ATSPM.Application.Reports.ViewModels.ApproachDelay;

namespace ATSPM.Application.Reports.Business.Common
{
    public interface IChartDataService
    {
        ApproachDelayResult GetChartData(ApproachDelayOptions options);
    }
}