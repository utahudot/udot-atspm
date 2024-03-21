using System;

namespace ATSPM.Application.Business.Common
{
    public class DataPointDateInt : IAggregationDataPoint
    {
        public DataPointDateInt(DateTime start, int value)
        {
            Start = start;
            Value = value;
        }

        public DateTime Start { get; set; }

        public int Value { get; set; }
    }
}
