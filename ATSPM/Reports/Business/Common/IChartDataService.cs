using ATSPM.Application.Reports.Business.AppoachDelay;

namespace ATSPM.Application.Reports.Business.Common
{
    public interface IChartDataService
    {
        object GetChartData(ApproachDelayOptions options);
    }
}