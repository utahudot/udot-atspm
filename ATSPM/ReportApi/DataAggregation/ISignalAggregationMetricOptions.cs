using ATSPM.Application.Business.Aggregation;
using ATSPM.Application.Business.Bins;
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using ATSPM.ReportApi.DataAggregation;

namespace MOE.Common.Business.WCFServiceLibrary
{
    public interface ISignalAggregationMetricOptions
    {
        List<AggregationResult> CreateMetric(AggregationOptions options);
        List<BinsContainer> GetBinsContainersByRoute(List<Location> signals, AggregationOptions options);
        List<DirectionTypes> GetFilteredDirections(AggregationOptions options);
        Series GetSignalsXAxisSignalSeries(List<Location> signals, string seriesName, AggregationOptions options);
        Series GetTimeXAxisRouteSeries(List<Location> signals, AggregationOptions options);
        Series GetTimeXAxisSignalSeries(Location signal, AggregationOptions options);
        void SetTimeAggregateSeries(Series series, List<BinsContainer> binsContainers, AggregationOptions options);
    }
}