using ATSPM.ReportApi.Business.AppoachDelay;

namespace ATSPM.ReportApi.Business.Common
{
    public interface IChartDataService
    {
        object GetChartData(ApproachDelayOptions options);
    }
}