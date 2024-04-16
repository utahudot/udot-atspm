namespace ATSPM.Application.Business.Common
{
    public class DataPointIntDouble : IAggregationDataPoint
    {
        public int Identifier { get; set; }

        public double Value { get; set; }
    }
}
