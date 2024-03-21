using ATSPM.Application.Business.Common;

namespace ATSPM.ReportApi.DataAggregation
{
    public class AggregationResult
    {
        public AggregationResult()
        {
            Series = new List<Series>();
        }

        public AggregationResult(string identifier, List<Series> series)
        {
            Identifier = identifier;
            Series = series;
        }

        public string Identifier { get; set; }
        public List<Series> Series { get; set; }
    }

    public class Series
    {
        public Series()
        {
            DataPoints = new List<IAggregationDataPoint>();
        }

        public string Identifier { get; set; }
        public List<IAggregationDataPoint> DataPoints { get; set; }
    }
}
