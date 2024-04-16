namespace ATSPM.Application.Business.Common
{
    public class DataPointStringInt : IAggregationDataPoint
    {
        public string Identifier { get; set; }

        public int Value { get; set; }
    }
}
