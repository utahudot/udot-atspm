using ATSPM.Application.Business.Common;
using System.Runtime.Serialization;

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


    [KnownType(typeof(DataPointDateDouble))]
    public class Series
    {
        public Series()
        {
            DataPoints = new List<AggregationDataPoint>();
        }

        public string Identifier { get; set; }
        public List<AggregationDataPoint> DataPoints { get; set; }
    }
}
