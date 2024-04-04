namespace ATSPM.Application.Business.Common
{
    public class DataPointStringDouble : IAggregationDataPoint
    {
        public string Identifier { get; set; }

        public double Value { get; set; }
    }
}
