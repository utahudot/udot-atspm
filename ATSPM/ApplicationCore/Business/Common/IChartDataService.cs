using ATSPM.Application.Business.AppoachDelay;

namespace ATSPM.Application.Business.Common
{
    public interface IChartDataService
    {
        object GetChartData(ApproachDelayOptions options);
    }
}